using System;

namespace SZ2.WebSocketGaugeServer.Special.AssettoCorsaSharedMemoryWebSocketServer.SharedMemoryCommunication
{
    public enum AssettoCorsaSHMPhysicsParameterCode
    {
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

        LocalVelocity,

        // Custom parameter
        ManifoldPressure
    }

    public enum AssettoCorsaSHMGraphicsParameterCode
    {
        Status,
        Session,
        CurrentTime,
        LastTime,
        BestTime,
        Split,
        CompletedLaps,
        Position,
        iCurrentTime,
        iLastTime,
        iBestTime,
        SessionTimeLeft,
        DistanceTraveled,
        IsInPit,
        CurrentSectorIndex,
        LastSectorTime,
        NumberOfLaps,
        TyreCompound,

        ReplayTimeMultiplier,
        NormalizedCarPosition,
        CarCoordinates,

        PenaltyTime,
        Flag,
        IdealLineOn,

        IsInPitLane,
        SurfaceGrip,
    
        MandatoryPitDone
    }

    public enum AssettoCorsaSHMStaticInfoParameterCode
    {
        SMVersion,
        ACVersion,

        NumberOfSessions,
        NumCars,
        CarModel,
        Track,
        PlayerName,
        PlayerSurname,
        PlayerNick,

        SectorCount,

        MaxTorque,
        MaxPower,
        MaxRpm,
        MaxFuel,
        SuspensionMaxTravel,
        TyreRadius,

        MaxTurboBoost,
        PenaltiesEnabled,
        AidFuelRate,
        AidTireRate,
        AidMechanicalDamage,
        AidAllowTyreBlankets,
        AidStability,
        AidAutoClutch,
        AidAutoBlip,

        HasDRS,
        HasERS,
        HasKERS,
        KersMaxJoules,
        EngineBrakeSettingsCount,
        ErsPowerControllerCount,

        TrackSPlineLength,
        TrackConfiguration,

        ErsMaxJ,

        IsTimedRace,
        HasExtraLap,
        CarSkin,
        ReversedGridPositions,
        PitWindowStart,
        PitWindowEnd
    }
}
