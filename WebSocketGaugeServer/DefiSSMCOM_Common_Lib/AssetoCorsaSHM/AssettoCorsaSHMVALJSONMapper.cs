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
 
        private void putVal(ValueJSONFormat valJSON, string code, int val)
        {
            valJSON.val.Add(code, val.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, float val)
        {
            valJSON.val.Add(code, val.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, int[] valArray)
        {
            for (int i = 0; i < valArray.Length; i++)
                valJSON.val.Add(code + "_" + i.ToString("00"), valArray[i].ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, float[] valArray)
        {
            for (int i = 0; i < valArray.Length; i++)
                valJSON.val.Add(code + "_" + i.ToString("00"), valArray[i].ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, string val)
        {
            valJSON.val.Add(code, val.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, Coordinates val)
        {
            valJSON.val.Add(code + "_" + "X", val.X.ToString());
            valJSON.val.Add(code + "_" + "Y", val.Y.ToString());
            valJSON.val.Add(code + "_" + "Z", val.Z.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, Coordinates[] val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                valJSON.val.Add(code + "_" + i.ToString("00") + "_" + "X", val[i].X.ToString());
                valJSON.val.Add(code + "_" + i.ToString("00") + "_" + "Y", val[i].Y.ToString());
                valJSON.val.Add(code + "_" + i.ToString("00") + "_" + "Z", val[i].Z.ToString());
            }
        }

        private void putVal(ValueJSONFormat valJSON, string code, AC_STATUS val)
        {
            valJSON.val.Add(code, val.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, AC_SESSION_TYPE val)
        {
            valJSON.val.Add(code, val.ToString());
        }

        private void putVal(ValueJSONFormat valJSON, string code, AC_FLAG_TYPE val)
        {
            valJSON.val.Add(code, val.ToString());
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
                        case AssettoCorsaSHMPhysicsParameterCode.Gas: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Gas)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Brake: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Brake)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Fuel: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Fuel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Gear: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Gear)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Rpms: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Rpms)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SteerAngle: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.SteerAngle)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SpeedKmh: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.SpeedKmh)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Velocity: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Velocity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AccG: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.AccG)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelSlip: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.WheelSlip)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelLoad: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.WheelLoad)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelsPressure: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.WheelsPressure)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelAngularSpeed: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.WheelAngularSpeed)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreWear: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreWear)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreDirtyLevel: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreDirtyLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreCoreTemperature: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreCoreTemperature)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CamberRad: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.CamberRad)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SuspensionTravel: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.SuspensionTravel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Drs: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Drs)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TC: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TC)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Heading: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Heading)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Pitch: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Pitch)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Roll: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Roll)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CgHeight: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.CgHeight)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CarDamage: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.CarDamage)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.NumberOfTyresOut: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.NumberOfTyresOut)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PitLimiterOn: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.PitLimiterOn)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Abs: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Abs)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCharge: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.KersCharge)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersInput: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.KersInput)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AutoShifterOn: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.AutoShifterOn)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RideHeight: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.RideHeight)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TurboBoost: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TurboBoost)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Ballast: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Ballast)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirDensity: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.AirDensity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirTemp: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.AirTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RoadTemp: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.RoadTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalAngularVelocity: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.LocalAngularVelocity)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.FinalFF: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.FinalFF)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PerformanceMeter: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.PerformanceMeter)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.EngineBrake: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.EngineBrake)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsRecoveryLevel: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.ErsRecoveryLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsPowerLevel: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.ErsPowerLevel)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsHeatCharging: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.ErsHeatCharging)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsisCharging: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.ErsisCharging)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCurrentKJ: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.KersCurrentKJ)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsAvailable: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.DrsAvailable)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsEnabled: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.DrsEnabled)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeTemp: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.BrakeTemp)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Clutch: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.Clutch)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempI: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreTempI)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempM: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreTempM)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempO: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreTempO)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.IsAIControlled: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.IsAIControlled)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactPoint: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreContactPoint)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactNormal: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreContactNormal)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactHeading: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.TyreContactHeading)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeBias: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.BrakeBias)); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalVelocity: valJSONsrc.val.Add(cd.ToString(), putVal(physicsSHM.LocalVelocity)); break;
                        
                        //Custom calced physics parameters
                        case AssettoCorsaSHMPhysicsParameterCode.ManifoldPressure:
                            float manifoldPres;
                            {
                                float boost = physicsSHM.TurboBoost;
                                float throttleClose = 1 - physicsSHM.Gas;
                                float rpm = (physicsSHM.Rpms>10000)?10000:physicsSHM.Rpms;

                                float throttleVacuum = 0.8F * throttleClose * throttleClose;
                                float pumpingVacuum = 0.15F * rpm / 10000F;
                                float vacuum = throttleVacuum + pumpingVacuum;

                                const float boostClip = 0.2F;

                                if (boost > boostClip)
                                    manifoldPres = boost;
                                else
                                    manifoldPres = boost*(boost/boostClip) - vacuum*(1-boost/boostClip);
                            }
                            valJSONsrc.val.Add(cd.ToString(), putVal(manifoldPres));
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
                        case AssettoCorsaSHMGraphicsParameterCode.Status: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.Status)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Session: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.Session)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.CurrentTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.LastTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.BestTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.BestTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Split: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.Split)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CompletedLaps: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.CompletedLaps)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Position: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.Position)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iCurrentTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.iCurrentTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iLastTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.iLastTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iBestTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.iBestTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SessionTimeLeft: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.SessionTimeLeft)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.DistanceTraveled: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.DistanceTraveled)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPit: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.IsInPit)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentSectorIndex: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.CurrentSectorIndex)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastSectorTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.LastSectorTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NumberOfLaps: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.NumberOfLaps)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.TyreCompound: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.TyreCompound)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.ReplayTimeMultiplier: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.ReplayTimeMultiplier)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NormalizedCarPosition: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.NormalizedCarPosition)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CarCoordinates: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.CarCoordinates)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.PenaltyTime: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.PenaltyTime)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Flag: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.Flag)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IdealLineOn: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.IdealLineOn)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPitLane: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.IsInPitLane)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SurfaceGrip: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.SurfaceGrip)); break;
                        case AssettoCorsaSHMGraphicsParameterCode.MandatoryPitDone: valJSONsrc.val.Add(cd.ToString(), putVal(graphicsSHM.MandatoryPitDone)); break;

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
                        case AssettoCorsaSHMStaticInfoParameterCode.SMVersion: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.SMVersion)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ACVersion: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.ACVersion)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumberOfSessions: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.NumberOfSessions)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumCars: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.NumCars)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarModel: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.CarModel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.Track: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.Track)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerName: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PlayerName)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerSurname: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PlayerSurname)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerNick: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PlayerNick)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SectorCount: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.SectorCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTorque: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.MaxTorque)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxPower: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.MaxPower)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxRpm: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.MaxRpm)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxFuel: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.MaxFuel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SuspensionMaxTravel: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.SuspensionMaxTravel)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TyreRadius: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.TyreRadius)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTurboBoost: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.MaxTurboBoost)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PenaltiesEnabled: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PenaltiesEnabled)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidFuelRate: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidFuelRate)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidTireRate: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidTireRate)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidMechanicalDamage: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidMechanicalDamage)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAllowTyreBlankets: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidAllowTyreBlankets)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidStability: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidStability)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoClutch: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidAutoClutch)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoBlip: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.AidAutoBlip)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasDRS: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.HasDRS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasERS: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.HasERS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasKERS: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.HasKERS)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.KersMaxJoules: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.KersMaxJoules)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.EngineBrakeSettingsCount: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.EngineBrakeSettingsCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsPowerControllerCount: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.ErsPowerControllerCount)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackSPlineLength: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.TrackSPlineLength)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackConfiguration: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.TrackConfiguration)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsMaxJ: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.ErsMaxJ)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.IsTimedRace: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.IsTimedRace)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasExtraLap: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.HasExtraLap)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarSkin: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.CarSkin)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ReversedGridPositions: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.ReversedGridPositions)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowStart: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PitWindowStart)); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowEnd: valJSONsrc.val.Add(cd.ToString(), putVal(staticInfoSHM.PitWindowEnd)); break;

                        default:
                            throw new InvalidProgramException("Cannot map Graphics parameter code.");
                    }
                }
            }
            return valJSONsrc;
        } 
    }
}
