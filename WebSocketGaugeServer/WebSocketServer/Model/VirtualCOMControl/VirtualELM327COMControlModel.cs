using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Reactive.Linq;
using SZ2.WebSocketGaugeServer.WebSocketServer.Service;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Model.VirtualCOMControl
{
    public class VirtualELM327COMControlModel : ReactivePropertyBlazorModelBase, IDisposable
    {
        private readonly ILogger logger;
        private readonly ELM327COMService Service;
        public ReactivePropertySlim<OBDIIParameterCode> ParameterCodeToSet { get; private set; }
        public ReactivePropertySlim<uint> SetValue { get; private set; }
        public ReadOnlyReactivePropertySlim<uint> MaxValue { get; private set; }
        public ReadOnlyReactivePropertySlim<double> PhysicalValue {get; private set;}
        public ReadOnlyReactivePropertySlim<string> PhysicalUnit {get; private set;}
        
        public VirtualELM327COMControlModel(ELM327COMService serivce, ILogger<VirtualELM327COMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var vElm327COM = serivce.VirtualELM327COM;

            this.ParameterCodeToSet = GetDefaultReactivePropertySlim<OBDIIParameterCode>(OBDIIParameterCode.Engine_Load, "ParameterCodeToSet");
            this.SetValue = GetDefaultReactivePropertySlim<uint>(0, "SetValue");
            this.SetValue.Subscribe(v => vElm327COM.SetRawValue(ParameterCodeToSet.Value, v));

            this.MaxValue= ParameterCodeToSet.Select(code => ((1U << (vElm327COM.get_Value_ByteLength(code)*8)) - 1)).ToReadOnlyReactivePropertySlim();
            this.PhysicalUnit = ParameterCodeToSet.Select(code => vElm327COM.get_unit(code)).ToReadOnlyReactivePropertySlim();
            
            this.ParameterCodeToSet.Subscribe(cd => SetValue.Value = vElm327COM.get_raw_value(cd));
            this.PhysicalValue = SetValue.Select(_ => vElm327COM.get_value(ParameterCodeToSet.Value)).ToReadOnlyReactivePropertySlim();   
        }

        public void Dispose()
        {
            this.ParameterCodeToSet.Dispose();
            this.MaxValue.Dispose();
            this.SetValue.Dispose();
            this.PhysicalUnit.Dispose();
            this.PhysicalValue.Dispose();
        }
    }
}