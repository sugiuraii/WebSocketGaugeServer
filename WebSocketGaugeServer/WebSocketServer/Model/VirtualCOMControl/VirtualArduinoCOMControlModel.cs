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

        public ReactivePropertySlim<uint> ManifoldAbsolutePressureValue { get; set; }
        public VirtualArduinoCOMControlModel(ArduinoCOMService serivce, ILogger<VirtualArduinoCOMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var virtualArduinoCOM = serivce.VirtualArduinoCOM;

            this.ManifoldAbsolutePressureValue = GetDefaultReactivePropertySlim<uint>(0, "ManifoldAbsolutePressureValue");
            this.ManifoldAbsolutePressureValue.Subscribe(v => virtualArduinoCOM.SetRawValue(ArduinoParameterCode.Manifold_Absolute_Pressure, v));
        }

        public void Dispose()
        {
            this.ManifoldAbsolutePressureValue.Dispose();
        }
    }
}