using AssettoCorsaSharedMemory;
using DefiSSMCOM.WebSocket.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DefiSSMCOM.Websocket.AssettoCorsaSHM;

namespace DefiSSMCOM.AssetoCorsaSHM
{
    public class AssettoCorsaSHMVALJSONMapper
    {
        public ValueJSONFormat Create(AssettoCorsaWebsocketSessionParam sessionParam, Physics physicsSHM, Graphics graphicsSHM, StaticInfo staticInfoSHM)
        {
            return Create(sessionParam.PhysicsDataSendList, sessionParam.GraphicsDataSendList, sessionParam.StaticInfoDataSendList, physicsSHM, graphicsSHM, staticInfoSHM);
        }

        public ValueJSONFormat Create(Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool> physicsSendList,
                                     Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool> graphicsSendList,
                                     Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool> staticInfoSendList,
                                     Physics physicsSHM, Graphics graphicsSHM, StaticInfo staticInfoSHM)
        {
            ValueJSONFormat valJSON = new ValueJSONFormat();
            valJSON = CreatePhysicsParameterValueJSON(valJSON, physicsSendList, physicsSHM);
            valJSON = CreateGraphicsParameterValueJSON(valJSON, graphicsSendList, graphicsSHM);
            valJSON = CreateStaticInfoParameterValueJSON(valJSON, staticInfoSendList, staticInfoSHM);

            return valJSON;
        }
 
        private string ValStrConv(int val)
        {
            return val.ToString();
        }

        private string ValStrConv(float val)
        {
            return val.ToString();
        }

        private string ValStrConv(int[] val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private string ValStrConv(float[] val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private string ValStrConv(string val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private string ValStrConv(Coordinates val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private string ValStrConv(Coordinates[] val)
        {
            return JsonConvert.SerializeObject(val);
        }

        private string ValStrConv(AC_STATUS val)
        {
            return val.ToString();
        }

        private string ValStrConv(AC_SESSION_TYPE val)
        {
            return val.ToString();
        }

        private string ValStrConv(AC_FLAG_TYPE val)
        {
            return val.ToString();
        }

        public ValueJSONFormat CreatePhysicsParameterValueJSON(Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool> physicsSendList, Physics physicsSHM)
        {
            return CreatePhysicsParameterValueJSON(new ValueJSONFormat(), physicsSendList, physicsSHM);
        }

        private ValueJSONFormat CreatePhysicsParameterValueJSON(ValueJSONFormat valJSONsrc, Dictionary<AssettoCorsaSHMPhysicsParameterCode, bool> physicsSendList, Physics physicsSHM)
        {
            foreach (AssettoCorsaSHMPhysicsParameterCode cd in physicsSendList.Keys)
            {
                if (physicsSendList[cd])
                {
                    switch (cd)
                    {
                        case AssettoCorsaSHMPhysicsParameterCode.Gas: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Gas)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Brake: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Brake)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Fuel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Fuel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Gear: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Gear)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Rpms: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Rpms)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SteerAngle: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.SteerAngle)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SpeedKmh: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.SpeedKmh)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Velocity: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Velocity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AccG: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.AccG)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelSlip: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.WheelSlip)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelLoad: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.WheelLoad)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelsPressure: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.WheelsPressure)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelAngularSpeed: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.WheelAngularSpeed)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreWear: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreWear)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreDirtyLevel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreDirtyLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreCoreTemperature: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreCoreTemperature)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CamberRad: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.CamberRad)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SuspensionTravel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.SuspensionTravel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Drs: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Drs)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TC: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TC)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Heading: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Heading)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Pitch: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Pitch)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Roll: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Roll)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CgHeight: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.CgHeight)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CarDamage: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.CarDamage)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.NumberOfTyresOut: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.NumberOfTyresOut)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PitLimiterOn: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.PitLimiterOn)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Abs: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Abs)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCharge: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.KersCharge)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersInput: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.KersInput)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AutoShifterOn: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.AutoShifterOn)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RideHeight: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.RideHeight)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TurboBoost: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TurboBoost)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Ballast: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Ballast)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirDensity: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.AirDensity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirTemp: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.AirTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RoadTemp: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.RoadTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalAngularVelocity: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.LocalAngularVelocity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.FinalFF: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.FinalFF)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PerformanceMeter: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.PerformanceMeter)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.EngineBrake: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.EngineBrake)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsRecoveryLevel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.ErsRecoveryLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsPowerLevel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.ErsPowerLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsHeatCharging: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.ErsHeatCharging)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsisCharging: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.ErsisCharging)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCurrentKJ: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.KersCurrentKJ)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsAvailable: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.DrsAvailable)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsEnabled: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.DrsEnabled)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeTemp: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.BrakeTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Clutch: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.Clutch)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempI: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreTempI)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempM: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreTempM)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempO: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreTempO)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.IsAIControlled: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.IsAIControlled)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactPoint: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreContactPoint)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactNormal: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreContactNormal)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactHeading: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.TyreContactHeading)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeBias: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.BrakeBias)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalVelocity: valJSONsrc.val.Add(cd.ToString(), ValStrConv(physicsSHM.LocalVelocity)); break;
                        
                        //Custom calced physics parameters
                        case AssettoCorsaSHMPhysicsParameterCode.ManifoldPressure:
                            float manifoldPres;
                            {
                                float boost = physicsSHM.TurboBoost;
                                float throttleClose = 1 - physicsSHM.Gas;
                                float rpm = (physicsSHM.Rpms>10000)?10000:physicsSHM.Rpms;

                                float throttleVacuum = (float)0.8 * throttleClose * throttleClose;
                                float pumpingVacuum = (float)0.15 * rpm / (float)10000;
                                float vacuum = throttleVacuum + pumpingVacuum;

                                manifoldPres = (boost > 0)?boost:-vacuum;
                            }
                            valJSONsrc.val.Add(cd.ToString(), ValStrConv(manifoldPres));
                            break;

                        default:
                            throw new InvalidProgramException("Cannot map Physics parameter code.");
                    }
                }
            }
            return valJSONsrc;
        }

        public ValueJSONFormat CreateGraphicsParameterValueJSON(Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool> graphicsSendList, Graphics graphicsSHM)
        {
            return CreateGraphicsParameterValueJSON(new ValueJSONFormat(), graphicsSendList, graphicsSHM);
        }

        private ValueJSONFormat CreateGraphicsParameterValueJSON(ValueJSONFormat valJSONsrc, Dictionary<AssettoCorsaSHMGraphicsParameterCode, bool> graphicsSendList, Graphics graphicsSHM)
        {
            foreach (AssettoCorsaSHMGraphicsParameterCode cd in graphicsSendList.Keys)
            {
                if (graphicsSendList[cd])
                {
                    switch (cd)
                    {
                        case AssettoCorsaSHMGraphicsParameterCode.Status: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.Status)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Session: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.Session)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.CurrentTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.LastTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.BestTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.BestTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Split: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.Split)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CompletedLaps: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.CompletedLaps)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Position: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.Position)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iCurrentTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.iCurrentTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iLastTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.iLastTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iBestTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.iBestTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SessionTimeLeft: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.SessionTimeLeft)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.DistanceTraveled: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.DistanceTraveled)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPit: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.IsInPit)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentSectorIndex: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.CurrentSectorIndex)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastSectorTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.LastSectorTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NumberOfLaps: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.NumberOfLaps)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.TyreCompound: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.TyreCompound)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.ReplayTimeMultiplier: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.ReplayTimeMultiplier)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NormalizedCarPosition: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.NormalizedCarPosition)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CarCoordinates: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.CarCoordinates)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.PenaltyTime: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.PenaltyTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Flag: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.Flag)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IdealLineOn: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.IdealLineOn)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPitLane: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.IsInPitLane)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SurfaceGrip: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.SurfaceGrip)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.MandatoryPitDone: valJSONsrc.val.Add(cd.ToString(), ValStrConv(graphicsSHM.MandatoryPitDone)); break;

                        default:
                            throw new InvalidProgramException("Cannot map Graphics parameter code.");
                    }
                }
            }
            return valJSONsrc;
        }

        public ValueJSONFormat CreateStaticInfoParameterValueJSON(Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool> staticInfoSendList, StaticInfo staticInfoSHM)
        {
            return CreateStaticInfoParameterValueJSON(new ValueJSONFormat(), staticInfoSendList, staticInfoSHM);
        }

        private ValueJSONFormat CreateStaticInfoParameterValueJSON(ValueJSONFormat valJSONsrc, Dictionary<AssettoCorsaSHMStaticInfoParameterCode, bool> staticInfoSendList, StaticInfo staticInfoSHM)
        {
            foreach (AssettoCorsaSHMStaticInfoParameterCode cd in staticInfoSendList.Keys)
            {
                if (staticInfoSendList[cd])
                {
                    switch (cd)
                    {
                        case AssettoCorsaSHMStaticInfoParameterCode.SMVersion: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.SMVersion)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ACVersion: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.ACVersion)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumberOfSessions: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.NumberOfSessions)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumCars: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.NumCars)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarModel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.CarModel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.Track: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.Track)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerName: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PlayerName)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerSurname: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PlayerSurname)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerNick: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PlayerNick)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SectorCount: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.SectorCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTorque: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.MaxTorque)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxPower: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.MaxPower)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxRpm: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.MaxRpm)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxFuel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.MaxFuel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SuspensionMaxTravel: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.SuspensionMaxTravel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TyreRadius: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.TyreRadius)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTurboBoost: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.MaxTurboBoost)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PenaltiesEnabled: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PenaltiesEnabled)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidFuelRate: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidFuelRate)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidTireRate: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidTireRate)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidMechanicalDamage: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidMechanicalDamage)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAllowTyreBlankets: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidAllowTyreBlankets)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidStability: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidStability)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoClutch: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidAutoClutch)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoBlip: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.AidAutoBlip)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasDRS: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.HasDRS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasERS: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.HasERS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasKERS: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.HasKERS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.KersMaxJoules: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.KersMaxJoules)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.EngineBrakeSettingsCount: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.EngineBrakeSettingsCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsPowerControllerCount: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.ErsPowerControllerCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackSPlineLength: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.TrackSPlineLength)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackConfiguration: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.TrackConfiguration)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsMaxJ: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.ErsMaxJ)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.IsTimedRace: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.IsTimedRace)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasExtraLap: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.HasExtraLap)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarSkin: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.CarSkin)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ReversedGridPositions: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.ReversedGridPositions)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowStart: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PitWindowStart)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowEnd: valJSONsrc.val.Add(cd.ToString(), ValStrConv(staticInfoSHM.PitWindowEnd)); break;

                        default:
                            throw new InvalidProgramException("Cannot map Graphics parameter code.");
                    }
                }
            }
            return valJSONsrc;
        } 
    }
}
