using AssettoCorsaSharedMemory;
using System;
using System.Collections.Generic;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.AssettoCorsaSHM;
using SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.SessionItems;
using SZ2.WebSocketGaugeServer.WebSocketServer.WebSocketCommon.JSONFormat;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.AssettoCorsaSharedMemoryWebSocketServer.Service
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
                        case AssettoCorsaSHMPhysicsParameterCode.Gas: putVal(valJSONsrc, cd.ToString(), physicsSHM.Gas); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Brake: putVal(valJSONsrc, cd.ToString(), physicsSHM.Brake); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Fuel: putVal(valJSONsrc, cd.ToString(), physicsSHM.Fuel); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Gear: putVal(valJSONsrc, cd.ToString(), physicsSHM.Gear); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Rpms: putVal(valJSONsrc, cd.ToString(), physicsSHM.Rpms); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SteerAngle: putVal(valJSONsrc, cd.ToString(), physicsSHM.SteerAngle); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SpeedKmh: putVal(valJSONsrc, cd.ToString(), physicsSHM.SpeedKmh); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Velocity: putVal(valJSONsrc, cd.ToString(), physicsSHM.Velocity); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AccG: putVal(valJSONsrc, cd.ToString(), physicsSHM.AccG); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelSlip: putVal(valJSONsrc, cd.ToString(), physicsSHM.WheelSlip); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelLoad: putVal(valJSONsrc, cd.ToString(), physicsSHM.WheelLoad); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelsPressure: putVal(valJSONsrc, cd.ToString(), physicsSHM.WheelsPressure); break;
                        case AssettoCorsaSHMPhysicsParameterCode.WheelAngularSpeed: putVal(valJSONsrc, cd.ToString(), physicsSHM.WheelAngularSpeed); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreWear: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreWear); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreDirtyLevel: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreDirtyLevel); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreCoreTemperature: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreCoreTemperature); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CamberRad: putVal(valJSONsrc, cd.ToString(), physicsSHM.CamberRad); break;
                        case AssettoCorsaSHMPhysicsParameterCode.SuspensionTravel: putVal(valJSONsrc, cd.ToString(), physicsSHM.SuspensionTravel); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Drs: putVal(valJSONsrc, cd.ToString(), physicsSHM.Drs); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TC: putVal(valJSONsrc, cd.ToString(), physicsSHM.TC); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Heading: putVal(valJSONsrc, cd.ToString(), physicsSHM.Heading); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Pitch: putVal(valJSONsrc, cd.ToString(), physicsSHM.Pitch); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Roll: putVal(valJSONsrc, cd.ToString(), physicsSHM.Roll); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CgHeight: putVal(valJSONsrc, cd.ToString(), physicsSHM.CgHeight); break;
                        case AssettoCorsaSHMPhysicsParameterCode.CarDamage: putVal(valJSONsrc, cd.ToString(), physicsSHM.CarDamage); break;
                        case AssettoCorsaSHMPhysicsParameterCode.NumberOfTyresOut: putVal(valJSONsrc, cd.ToString(), physicsSHM.NumberOfTyresOut); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PitLimiterOn: putVal(valJSONsrc, cd.ToString(), physicsSHM.PitLimiterOn); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Abs: putVal(valJSONsrc, cd.ToString(), physicsSHM.Abs); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCharge: putVal(valJSONsrc, cd.ToString(), physicsSHM.KersCharge); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersInput: putVal(valJSONsrc, cd.ToString(), physicsSHM.KersInput); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AutoShifterOn: putVal(valJSONsrc, cd.ToString(), physicsSHM.AutoShifterOn); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RideHeight: putVal(valJSONsrc, cd.ToString(), physicsSHM.RideHeight); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TurboBoost: putVal(valJSONsrc, cd.ToString(), physicsSHM.TurboBoost); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Ballast: putVal(valJSONsrc, cd.ToString(), physicsSHM.Ballast); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirDensity: putVal(valJSONsrc, cd.ToString(), physicsSHM.AirDensity); break;
                        case AssettoCorsaSHMPhysicsParameterCode.AirTemp: putVal(valJSONsrc, cd.ToString(), physicsSHM.AirTemp); break;
                        case AssettoCorsaSHMPhysicsParameterCode.RoadTemp: putVal(valJSONsrc, cd.ToString(), physicsSHM.RoadTemp); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalAngularVelocity: putVal(valJSONsrc, cd.ToString(), physicsSHM.LocalAngularVelocity); break;
                        case AssettoCorsaSHMPhysicsParameterCode.FinalFF: putVal(valJSONsrc, cd.ToString(), physicsSHM.FinalFF); break;
                        case AssettoCorsaSHMPhysicsParameterCode.PerformanceMeter: putVal(valJSONsrc, cd.ToString(), physicsSHM.PerformanceMeter); break;
                        case AssettoCorsaSHMPhysicsParameterCode.EngineBrake: putVal(valJSONsrc, cd.ToString(), physicsSHM.EngineBrake); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsRecoveryLevel: putVal(valJSONsrc, cd.ToString(), physicsSHM.ErsRecoveryLevel); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsPowerLevel: putVal(valJSONsrc, cd.ToString(), physicsSHM.ErsPowerLevel); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsHeatCharging: putVal(valJSONsrc, cd.ToString(), physicsSHM.ErsHeatCharging); break;
                        case AssettoCorsaSHMPhysicsParameterCode.ErsisCharging: putVal(valJSONsrc, cd.ToString(), physicsSHM.ErsisCharging); break;
                        case AssettoCorsaSHMPhysicsParameterCode.KersCurrentKJ: putVal(valJSONsrc, cd.ToString(), physicsSHM.KersCurrentKJ); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsAvailable: putVal(valJSONsrc, cd.ToString(), physicsSHM.DrsAvailable); break;
                        case AssettoCorsaSHMPhysicsParameterCode.DrsEnabled: putVal(valJSONsrc, cd.ToString(), physicsSHM.DrsEnabled); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeTemp: putVal(valJSONsrc, cd.ToString(), physicsSHM.BrakeTemp); break;
                        case AssettoCorsaSHMPhysicsParameterCode.Clutch: putVal(valJSONsrc, cd.ToString(), physicsSHM.Clutch); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempI: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreTempI); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempM: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreTempM); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreTempO: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreTempO); break;
                        case AssettoCorsaSHMPhysicsParameterCode.IsAIControlled: putVal(valJSONsrc, cd.ToString(), physicsSHM.IsAIControlled); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactPoint: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreContactPoint); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactNormal: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreContactNormal); break;
                        case AssettoCorsaSHMPhysicsParameterCode.TyreContactHeading: putVal(valJSONsrc, cd.ToString(), physicsSHM.TyreContactHeading); break;
                        case AssettoCorsaSHMPhysicsParameterCode.BrakeBias: putVal(valJSONsrc, cd.ToString(), physicsSHM.BrakeBias); break;
                        case AssettoCorsaSHMPhysicsParameterCode.LocalVelocity: putVal(valJSONsrc, cd.ToString(), physicsSHM.LocalVelocity); break;
                        
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
                            putVal(valJSONsrc, cd.ToString(), manifoldPres);
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
                        case AssettoCorsaSHMGraphicsParameterCode.Status: putVal(valJSONsrc, cd.ToString(), graphicsSHM.Status); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Session: putVal(valJSONsrc, cd.ToString(), graphicsSHM.Session); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.CurrentTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.LastTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.BestTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.BestTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Split: putVal(valJSONsrc, cd.ToString(), graphicsSHM.Split); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CompletedLaps: putVal(valJSONsrc, cd.ToString(), graphicsSHM.CompletedLaps); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Position: putVal(valJSONsrc, cd.ToString(), graphicsSHM.Position); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iCurrentTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.iCurrentTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iLastTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.iLastTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.iBestTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.iBestTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SessionTimeLeft: putVal(valJSONsrc, cd.ToString(), graphicsSHM.SessionTimeLeft); break;
                        case AssettoCorsaSHMGraphicsParameterCode.DistanceTraveled: putVal(valJSONsrc, cd.ToString(), graphicsSHM.DistanceTraveled); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPit: putVal(valJSONsrc, cd.ToString(), graphicsSHM.IsInPit); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CurrentSectorIndex: putVal(valJSONsrc, cd.ToString(), graphicsSHM.CurrentSectorIndex); break;
                        case AssettoCorsaSHMGraphicsParameterCode.LastSectorTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.LastSectorTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NumberOfLaps: putVal(valJSONsrc, cd.ToString(), graphicsSHM.NumberOfLaps); break;
                        case AssettoCorsaSHMGraphicsParameterCode.TyreCompound: putVal(valJSONsrc, cd.ToString(), graphicsSHM.TyreCompound); break;
                        case AssettoCorsaSHMGraphicsParameterCode.ReplayTimeMultiplier: putVal(valJSONsrc, cd.ToString(), graphicsSHM.ReplayTimeMultiplier); break;
                        case AssettoCorsaSHMGraphicsParameterCode.NormalizedCarPosition: putVal(valJSONsrc, cd.ToString(), graphicsSHM.NormalizedCarPosition); break;
                        case AssettoCorsaSHMGraphicsParameterCode.CarCoordinates: putVal(valJSONsrc, cd.ToString(), graphicsSHM.CarCoordinates); break;
                        case AssettoCorsaSHMGraphicsParameterCode.PenaltyTime: putVal(valJSONsrc, cd.ToString(), graphicsSHM.PenaltyTime); break;
                        case AssettoCorsaSHMGraphicsParameterCode.Flag: putVal(valJSONsrc, cd.ToString(), graphicsSHM.Flag); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IdealLineOn: putVal(valJSONsrc, cd.ToString(), graphicsSHM.IdealLineOn); break;
                        case AssettoCorsaSHMGraphicsParameterCode.IsInPitLane: putVal(valJSONsrc, cd.ToString(), graphicsSHM.IsInPitLane); break;
                        case AssettoCorsaSHMGraphicsParameterCode.SurfaceGrip: putVal(valJSONsrc, cd.ToString(), graphicsSHM.SurfaceGrip); break;
                        case AssettoCorsaSHMGraphicsParameterCode.MandatoryPitDone: putVal(valJSONsrc, cd.ToString(), graphicsSHM.MandatoryPitDone); break;

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
                        case AssettoCorsaSHMStaticInfoParameterCode.SMVersion: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.SMVersion); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ACVersion: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.ACVersion); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumberOfSessions: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.NumberOfSessions); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.NumCars: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.NumCars); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarModel: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.CarModel); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.Track: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.Track); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerName: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PlayerName); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerSurname: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PlayerSurname); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PlayerNick: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PlayerNick); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SectorCount: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.SectorCount); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTorque: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.MaxTorque); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxPower: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.MaxPower); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxRpm: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.MaxRpm); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxFuel: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.MaxFuel); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.SuspensionMaxTravel: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.SuspensionMaxTravel); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TyreRadius: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.TyreRadius); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.MaxTurboBoost: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.MaxTurboBoost); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PenaltiesEnabled: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PenaltiesEnabled); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidFuelRate: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidFuelRate); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidTireRate: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidTireRate); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidMechanicalDamage: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidMechanicalDamage); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAllowTyreBlankets: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidAllowTyreBlankets); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidStability: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidStability); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoClutch: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidAutoClutch); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.AidAutoBlip: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.AidAutoBlip); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasDRS: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.HasDRS); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasERS: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.HasERS); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasKERS: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.HasKERS); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.KersMaxJoules: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.KersMaxJoules); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.EngineBrakeSettingsCount: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.EngineBrakeSettingsCount); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsPowerControllerCount: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.ErsPowerControllerCount); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackSPlineLength: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.TrackSPlineLength); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.TrackConfiguration: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.TrackConfiguration); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ErsMaxJ: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.ErsMaxJ); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.IsTimedRace: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.IsTimedRace); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.HasExtraLap: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.HasExtraLap); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.CarSkin: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.CarSkin); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.ReversedGridPositions: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.ReversedGridPositions); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowStart: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PitWindowStart); break;
                        case AssettoCorsaSHMStaticInfoParameterCode.PitWindowEnd: putVal(valJSONsrc, cd.ToString(), staticInfoSHM.PitWindowEnd); break;

                        default:
                            throw new InvalidProgramException("Cannot map Graphics parameter code.");
                    }
                }
            }
            return valJSONsrc;
        } 
    }
}
