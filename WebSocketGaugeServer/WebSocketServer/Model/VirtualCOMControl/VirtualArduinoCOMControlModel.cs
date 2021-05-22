using System;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Reactive.Linq;
using SZ2.WebSocketGaugeServer.WebSocketServer.Service;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Model.VirtualCOMControl
{
    public class VirtualArduinoCOMControlModel : ReactivePropertyBlazorModelBase, IDisposable
    {
        private readonly ILogger logger;
        private readonly ArduinoCOMService Service;
        
        public ReactivePropertySlim<double> EngineSpeed {get; private set;}
        public readonly string EngineSpeedUnit;
        public readonly (double Min, double Max) EngineSpeedRange = (Min : 0.0, Max : 20000.0);        
        public ReactivePropertySlim<double> VehicleSpeed {get; private set;}
        public readonly string VehicleSpeedUnit;
        public readonly (double Min, double Max) VehicleSpeedRange = (Min : 0.0, Max : 400.0);
        public ReactivePropertySlim<double> ManifoldAbsolutePressure { get; private set; }
        public readonly string ManifoldAbsolutePressureUnit;
        public readonly (double Min, double Max) ManifoldAbsolutePressureRange = (Min : 0, Max : 400);
        public ReactivePropertySlim<double> CoolantTemperature { get; private set; }
        public readonly string CoolantTemperatureUnit;
        public readonly (double Min, double Max) CoolantTemperatureRange = (Min : -40, Max : 200);      
        public ReactivePropertySlim<double> OilTemperature { get; private set; }
        public readonly string OilTemperatureUnit;
        public readonly (double Min, double Max) OilTemperatureRange = (Min : -40, Max : 200);
        public ReactivePropertySlim<double> OilTemperature2 { get; private set; }
        public readonly string OilTemperatureUnit2;
        public readonly (double Min, double Max) OilTemperature2Range = (Min : -40, Max : 200);
        public ReactivePropertySlim<double> OilPressure { get; private set; }
        public readonly string OilPressureUnit;
        public readonly (double Min, double Max) OilPressureRange = (Min : 0, Max : 10);
        public ReactivePropertySlim<double> FuelRailPressure { get; private set; }
        public readonly string FuelRailPressureUnit;
        public readonly (double Min, double Max) FuelRailPressureRange = (Min : 0, Max : 6);

        public VirtualArduinoCOMControlModel(ArduinoCOMService serivce, ILogger<VirtualArduinoCOMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var virtualArduinoCOM = serivce.VirtualArduinoCOM;
            
            this.EngineSpeed = GetDefaultReactivePropertySlim<double>(0, "EngineSpeed");
            this.EngineSpeed.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Engine_Speed, v));
            this.EngineSpeedUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Engine_Speed);

            this.VehicleSpeed = GetDefaultReactivePropertySlim<double>(0, "VehicleSpeed");
            this.VehicleSpeed.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Vehicle_Speed, v));
            this.VehicleSpeedUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Vehicle_Speed);
            
            this.ManifoldAbsolutePressure = GetDefaultReactivePropertySlim<double>(0, "ManifoldAbsolutePressure");
            this.ManifoldAbsolutePressure.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Manifold_Absolute_Pressure, v));
            this.ManifoldAbsolutePressureUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Manifold_Absolute_Pressure);

            this.CoolantTemperature = GetDefaultReactivePropertySlim<double>(0, "CoolantTemperature");
            this.CoolantTemperature.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Coolant_Temperature, v));
            this.CoolantTemperatureUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Coolant_Temperature);

            this.OilTemperature = GetDefaultReactivePropertySlim<double>(0, "OilTemperature");
            this.OilTemperature.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Oil_Temperature, v));
            this.OilTemperatureUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Oil_Temperature);

            this.OilTemperature2 = GetDefaultReactivePropertySlim<double>(0, "OilTemperature2");
            this.OilTemperature2.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Oil_Temperature2, v));
            this.OilTemperatureUnit2 = virtualArduinoCOM.get_unit(ArduinoParameterCode.Oil_Temperature2);

            this.OilPressure = GetDefaultReactivePropertySlim<double>(0, "OilPressure");
            this.OilPressure.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Oil_Pressure, v));
            this.OilPressureUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Oil_Pressure);

            this.FuelRailPressure = GetDefaultReactivePropertySlim<double>(0, "FuelRailPressure");
            this.FuelRailPressure.Subscribe(v => virtualArduinoCOM.SetValue(ArduinoParameterCode.Fuel_Rail_Pressure, v));
            this.FuelRailPressureUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Fuel_Rail_Pressure);            
        }

        public void Dispose()
        {
            EngineSpeed.Dispose();
            VehicleSpeed.Dispose();
            ManifoldAbsolutePressure.Dispose();
            CoolantTemperature.Dispose();
            OilTemperature.Dispose();
            OilTemperature2.Dispose();
            OilPressure.Dispose();
            FuelRailPressure.Dispose();
        }
    }
}