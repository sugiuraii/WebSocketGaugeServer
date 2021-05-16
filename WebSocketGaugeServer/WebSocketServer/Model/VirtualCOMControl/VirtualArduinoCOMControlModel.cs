using System;
using System.ComponentModel;
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

        public ReactivePropertySlim<uint> ManifoldAbsolutePressureValue { get; private set; }
        public ReadOnlyReactivePropertySlim<double> ManifoldAbsolutePressurePhysicalValue { get; private set; }
        public string ManifoldAbsolutePressurePhysicalUnit {get; private set;}
        public VirtualArduinoCOMControlModel(ArduinoCOMService serivce, ILogger<VirtualArduinoCOMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var virtualArduinoCOM = serivce.VirtualArduinoCOM;

            this.ManifoldAbsolutePressureValue = GetDefaultReactivePropertySlim<uint>(0, "ManifoldAbsolutePressureValue");
            this.ManifoldAbsolutePressureValue.Subscribe(v => virtualArduinoCOM.SetRawValue(ArduinoParameterCode.Manifold_Absolute_Pressure, v));
            this.ManifoldAbsolutePressurePhysicalValue = this.ManifoldAbsolutePressureValue.Select(_ => virtualArduinoCOM.get_value(ArduinoParameterCode.Manifold_Absolute_Pressure)).ToReadOnlyReactivePropertySlim();
            this.ManifoldAbsolutePressurePhysicalUnit = virtualArduinoCOM.get_unit(ArduinoParameterCode.Manifold_Absolute_Pressure);
        }

        public void Dispose()
        {
            this.ManifoldAbsolutePressureValue.Dispose();
            this.ManifoldAbsolutePressurePhysicalValue.Dispose();
        }
    }
}