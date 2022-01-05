using System;
using System.Text;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service.FUELTripCalculator;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.SessionItems;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Settings;
using SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.JSONFormat;
using Microsoft.Extensions.Hosting;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SZ2.WebSocketGaugeServer.WebSocketCommon.Utils;

namespace SZ2.WebSocketGaugeServer.WebSocketDataLogger.FUELTRIPLogger.Service
{
    public class FUELTRIPService : IDisposable
    {
        private readonly ILogger logger;
        private readonly WebSocketClients wsClients;
        private readonly FuelTripCalculator fuelTripCalc;
        private readonly Dictionary<Guid, (WebSocket WebSocket, FUELTRIPWebSocketSessionParam SessionParam)> WebSocketDictionary = new Dictionary<Guid, (WebSocket WebSocket, FUELTRIPWebSocketSessionParam SessionParam)>();
        private readonly AsyncSemaphoreLock WebSocketDictionaryLock = new AsyncSemaphoreLock();
        private readonly FUELTRIPLoggerSettings appSettings;
        public FUELTRIPLoggerSettings AppSettings { get => appSettings; }

        public async Task AddWebSocketAsync(Guid sessionGuid, WebSocket websocket)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Add(sessionGuid, (websocket, new FUELTRIPWebSocketSessionParam()));
            }
        }

        public async Task RemoveWebSocketAsync(Guid sessionGuid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                this.WebSocketDictionary.Remove(sessionGuid);
            }
        }

        public async Task<FUELTRIPWebSocketSessionParam> GetSessionParamAsync(Guid guid)
        {
            using (await WebSocketDictionaryLock.LockAsync())
            {
                return this.WebSocketDictionary[guid].SessionParam;
            }
        }

        public FuelTripCalculator FUELTripCalculator { get { return this.fuelTripCalc; } }

        public FUELTRIPService(IConfiguration configuration, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<FUELTRIPService> logger)
        {
            this.logger = logger;
            this.appSettings = JsonConvert.DeserializeObject<FUELTRIPLoggerSettings>(File.ReadAllText("./fueltriplogger_settings.jsonc"));

            var logStoreFolderPath = configuration.GetSection("ServiceConfig")["FuelTripLogStoreFolderPath"];
            logger.LogInformation("Fuel trip log store folder path is set to : " + logStoreFolderPath);
            if (!Directory.Exists(logStoreFolderPath))
            {
                logger.LogWarning("Fuel trip log store folder path : " + logStoreFolderPath + " does not exist.");
                logStoreFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                logger.LogWarning("Default fuel trip log store folder path : " + logStoreFolderPath + " will be used instead.");
            }

            this.fuelTripCalc = new FuelTripCalculator(appSettings.Calculation.CalculationOption, appSettings.Calculation.FuelCalculationMethod, logStoreFolderPath, logger);

            //Websocket clients setup
            this.wsClients = new WebSocketClients(appSettings, loggerFactory);

            var cancellationToken = lifetime.ApplicationStopping;

            this.wsClients.VALMessageParsed += async (sender, args) =>
            {
                try
                {
                    fuelTripCalc.update(wsClients.EngineRev, wsClients.VehicleSpeed, wsClients.InjPulseWidth, wsClients.MassAirFlow, wsClients.AFRatio, wsClients.FuelRate);
                    using (await WebSocketDictionaryLock.LockAsync())
                    {
                        foreach (var session in this.WebSocketDictionary)
                        {
                            var guid = session.Key;
                            var websocket = session.Value.WebSocket;
                            var sessionparam = session.Value.SessionParam;

                            // Construct momentun fuel trip data message.
                            string msg = this.constructMomentumFUELTRIPDataMessage(fuelTripCalc).Serialize();
                            // Send message,
                            byte[] buf = Encoding.UTF8.GetBytes(msg);
                            await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                        }

                    }
                }
                catch (TimeoutException ex)
                {
                    logger.LogWarning("TimeOutException is occured on FUELTRIP calcilation. FUELTRIP calutaion is skipped on this tick. ");
                    logger.LogWarning(ex.Message);
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };

            this.fuelTripCalc.SectFUELTRIPUpdated += async (sender, e) =>
            {
                try
                {
                    using (await WebSocketDictionaryLock.LockAsync())
                    {
                        foreach (var session in this.WebSocketDictionary)
                        {
                            var guid = session.Key;
                            var websocket = session.Value.WebSocket;
                            var sessionparam = session.Value.SessionParam;

                            // Construct section fuel trip data message.
                            string msg = this.constructSectionFUELTRIPDataMessage(fuelTripCalc).Serialize();
                            // Send message,
                            byte[] buf = Encoding.UTF8.GetBytes(msg);
                            await websocket.SendAsync(new ArraySegment<byte>(buf), WebSocketMessageType.Text, true, cancellationToken);
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    logger.LogWarning(ex.GetType().FullName + " : " + ex.Message + " : Error code : " + ex.ErrorCode.ToString());
                    logger.LogWarning(ex.StackTrace);
                }
            };

            this.fuelTripCalc.loadTripFuel();
            this.wsClients.start();

            logger.LogInformation("Websocket server is started");
        }

        public void Dispose()
        {
            fuelTripCalc.saveTripFuel();
            logger.LogInformation("FUELTRIP data is saved.");
            var stopTask = Task.Run(() => this.wsClients.stop());
            Task.WhenAny(stopTask, Task.Delay(10000));
            logger.LogInformation("Websocket server is stopped");
        }

        private FUELTRIPJSONFormat constructMomentumFUELTRIPDataMessage(FuelTripCalculator fuelTripCalculator)
        {
            var fueltrip_json = new FUELTRIPJSONFormat();
            fueltrip_json.moment_gasmilage = fuelTripCalculator.MomentaryTripPerFuel;
            fueltrip_json.total_gas = fuelTripCalculator.TotalFuelConsumption;
            fueltrip_json.total_trip = fuelTripCalculator.TotalTrip;
            fueltrip_json.total_gasmilage = fuelTripCalculator.TotalTripPerFuel;

            return fueltrip_json;
        }

        private SectFUELTRIPJSONFormat constructSectionFUELTRIPDataMessage(FuelTripCalculator fuelTripCalculator)
        {
            var sectfueltrip_json = new SectFUELTRIPJSONFormat();
            sectfueltrip_json.sect_gas = fuelTripCalculator.SectFuelArray;
            sectfueltrip_json.sect_trip = fuelTripCalculator.SectTripArray;
            sectfueltrip_json.sect_gasmilage = fuelTripCalculator.SectTripPerFuelArray;
            sectfueltrip_json.sect_span = fuelTripCalculator.SectSpan;

            return sectfueltrip_json;
        }
    }
}
