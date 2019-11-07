﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefiSSMCOM_Common_Lib.AssetoCorsaSHM
{
    public enum AssettoCorsaSHMPhysicsParameterCode
    {
        PacketId,
        Gas,
        Brake,
        Fuel,
        Gear,
        Rpms,
        SteerAngle,
        SpeedKmh,
        Velocity,
        AccG,
        WheelSlip,
        WheelLoad,
        WheelsPressure,
        WheelAngularSpeed,
        TyreWear,
        TyreDirtyLevel,
        TyreCoreTemperature,
        CamberRad,
        SuspensionTravel,
        Drs,
        TC,
        Heading,
        Pitch,
        Roll,
        CgHeight,
        CarDamage,

        NumberOfTyresOut,
        PitLimiterOn,
        Abs,

        KersCharge,
        KersInput,
        AutoShifterOn,
        RideHeight,

        TurboBoost,
        Ballast,
        AirDensity,
        AirTemp,
        RoadTemp,
        LocalAngularVelocity,
        FinalFF,
        PerformanceMeter,
        EngineBrake,
        ErsRecoveryLevel,
        ErsPowerLevel,
        ErsHeatCharging,
        ErsisCharging,
        KersCurrentKJ,
        DrsAvailable,
        DrsEnabled,
        BrakeTemp,
        Clutch,

        TyreTempI,
        TyreTempM,
        TyreTempO,
        IsAIControlled,
        TyreContactPoint,
        TyreContactNormal,
        TyreContactHeading,
        BrakeBias,

        LocalVelocity
    }
}
