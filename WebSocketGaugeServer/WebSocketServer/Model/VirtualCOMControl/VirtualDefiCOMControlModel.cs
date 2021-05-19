using System;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Reactive.Linq;
using SZ2.WebSocketGaugeServer.WebSocketServer.Service;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Model.VirtualCOMControl
{
    public class VirtualDefiCOMControlModel : ReactivePropertyBlazorModelBase, IDisposable
    {
        private readonly ILogger logger;
        private readonly DefiCOMService Service;
        public readonly (uint Min, uint Max) ValueRange = (Min : 0, Max : 2464);        
        public ReactivePropertySlim<uint> ManifoldAbsolutePressure { get; private set; }
        public ReadOnlyReactivePropertySlim<double> ManifoldAbsolutePressurePhysicalValue { get; private set; }
        public readonly string ManifoldAbsolutePressureUnit;        
        public ReactivePropertySlim<uint> EngineSpeed {get; private set;}
        public ReadOnlyReactivePropertySlim<double> EngineSpeedPhysicalValue {get; private set;}
        public readonly string EngineSpeedUnit;
        public ReactivePropertySlim<uint> OilPressure { get; private set; }
        public ReadOnlyReactivePropertySlim<double> OilPressurePhysicalValue { get; private set; }
        public readonly string OilPressureUnit;
        public ReactivePropertySlim<uint> FuelRailPressure { get; private set; }
        public ReadOnlyReactivePropertySlim<double> FuelRailPressurePhysicalValue { get; private set; }
        public readonly string FuelRailPressureUnit;
        public ReactivePropertySlim<uint> ExhaustGasTemperature { get; private set; }
        public ReadOnlyReactivePropertySlim<double> ExhaustGasTemperaturePhysicalValue { get; private set; }
        public readonly string ExhaustGasTemperatureUnit;
        public ReactivePropertySlim<uint> OilTemperature { get; private set; }
        public ReadOnlyReactivePropertySlim<double> OilTemperaturePhysicalValue { get; private set; }
        public readonly string OilTemperatureUnit;
        public ReactivePropertySlim<uint> CoolantTemperature { get; private set; }
        public ReadOnlyReactivePropertySlim<double> CoolantTemperaturePhysicalValue { get; private set; }
        public readonly string CoolantTemperatureUnit;

        public VirtualDefiCOMControlModel(DefiCOMService serivce, ILogger<VirtualDefiCOMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var virtualDefiCOM = serivce.VirtualDefiCOM;

            this.ManifoldAbsolutePressure = GetDefaultReactivePropertySlim<uint>(0, "ManifoldAbsolutePressure");
            this.ManifoldAbsolutePressure.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Manifold_Absolute_Pressure, v));
            this.ManifoldAbsolutePressureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Manifold_Absolute_Pressure);
            this.ManifoldAbsolutePressurePhysicalValue = ManifoldAbsolutePressure.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Manifold_Absolute_Pressure)).ToReadOnlyReactivePropertySlim();
            
            this.EngineSpeed = GetDefaultReactivePropertySlim<uint>(0, "EngineSpeed");
            this.EngineSpeed.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Engine_Speed, v));
            this.EngineSpeedUnit = virtualDefiCOM.get_unit(DefiParameterCode.Engine_Speed);
            this.EngineSpeedPhysicalValue = EngineSpeed.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Engine_Speed)).ToReadOnlyReactivePropertySlim();
            
            this.OilPressure = GetDefaultReactivePropertySlim<uint>(0, "OilPressure");
            this.OilPressure.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Oil_Pressure, v));
            this.OilPressureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Oil_Pressure);
            this.OilPressurePhysicalValue = OilPressure.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Oil_Pressure)).ToReadOnlyReactivePropertySlim();

            this.FuelRailPressure = GetDefaultReactivePropertySlim<uint>(0, "FuelRailPressure");
            this.FuelRailPressure.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Fuel_Rail_Pressure, v));
            this.FuelRailPressureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Fuel_Rail_Pressure);
            this.FuelRailPressurePhysicalValue = FuelRailPressure.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Fuel_Rail_Pressure)).ToReadOnlyReactivePropertySlim();

            this.ExhaustGasTemperature = GetDefaultReactivePropertySlim<uint>(0, "ExhaustGasTemperature");
            this.ExhaustGasTemperature.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Exhaust_Gas_Temperature, v));
            this.ExhaustGasTemperatureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Exhaust_Gas_Temperature); 
            this.ExhaustGasTemperaturePhysicalValue = ExhaustGasTemperature.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Exhaust_Gas_Temperature)).ToReadOnlyReactivePropertySlim();

            this.OilTemperature = GetDefaultReactivePropertySlim<uint>(0, "OilTemperature");
            this.OilTemperature.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Oil_Temperature, v));
            this.OilTemperatureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Oil_Temperature);
            this.OilTemperaturePhysicalValue = OilTemperature.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Oil_Temperature)).ToReadOnlyReactivePropertySlim();

            this.CoolantTemperature = GetDefaultReactivePropertySlim<uint>(0, "CoolantTemperature");
            this.CoolantTemperature.Subscribe(v => virtualDefiCOM.SetRawValue(DefiParameterCode.Coolant_Temperature, v));
            this.CoolantTemperatureUnit = virtualDefiCOM.get_unit(DefiParameterCode.Coolant_Temperature);
            this.CoolantTemperaturePhysicalValue = CoolantTemperature.Select(_ => virtualDefiCOM.get_value(DefiParameterCode.Coolant_Temperature)).ToReadOnlyReactivePropertySlim();
        }

        public void Dispose()
        {
            ManifoldAbsolutePressure.Dispose();
            ManifoldAbsolutePressurePhysicalValue.Dispose();
            EngineSpeed.Dispose();
            EngineSpeedPhysicalValue.Dispose();
            OilPressure.Dispose();
            OilPressurePhysicalValue.Dispose();
            FuelRailPressure.Dispose();
            FuelRailPressurePhysicalValue.Dispose();
            ExhaustGasTemperature.Dispose();
            ExhaustGasTemperaturePhysicalValue.Dispose();
            OilTemperature.Dispose();
            OilPressurePhysicalValue.Dispose();
            CoolantTemperature.Dispose();
            CoolantTemperaturePhysicalValue.Dispose();
        }
    }
}