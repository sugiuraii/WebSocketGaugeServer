using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Reactive.Bindings;
using System.Reactive.Linq;
using SZ2.WebSocketGaugeServer.WebSocketServer.Service;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Model.VirtualCOMControl
{
    public class VirtualSSMCOMControlModel : ReactivePropertyBlazorModelBase, IDisposable
    {
        private readonly ILogger logger;
        private readonly SSMCOMService Service;
        public ReactivePropertySlim<SSMParameterCode> ParameterCodeToSet { get; private set; }
        public ReactivePropertySlim<uint> SetValue { get; private set; }
        public ReadOnlyReactivePropertySlim<uint> MaxValue { get; private set; }
        public ReadOnlyReactivePropertySlim<double> PhysicalValue { get; private set; }
        public ReadOnlyReactivePropertySlim<string> PhysicalUnit { get; private set; }
        public ReactivePropertySlim<SSMSwitchCode> SwitchCodeToSet { get; private set; }
        public ReactivePropertySlim<bool> SetSwitch { get; private set; }

        public VirtualSSMCOMControlModel(SSMCOMService serivce, ILogger<VirtualSSMCOMControlModel> logger)
        {
            this.logger = logger;
            this.Service = serivce;
            var vSSMCOM = serivce.VirtualSSMCOM;

            this.ParameterCodeToSet = GetDefaultReactivePropertySlim<SSMParameterCode>(SSMParameterCode.Engine_Load, "ParameterCodeToSet");
            this.SetValue = GetDefaultReactivePropertySlim<uint>(0, "SetValue");
            this.SetValue.Subscribe(v => 
            {
                var code = ParameterCodeToSet.Value;
                if(code.ToString().Contains("Switch_P0x"))
                {
                    this.logger.LogWarning("Value modificationof Switch code from parameter code is prohibited. This action will be ignored.");
                    return;
                }
                else
                    vSSMCOM.SetRawValue(ParameterCodeToSet.Value, v);
            });

            this.MaxValue = ParameterCodeToSet.Select(code => ((1U << (vSSMCOM.get_raw_value_ByteLength(code) * 8)) - 1)).ToReadOnlyReactivePropertySlim();
            this.PhysicalUnit = ParameterCodeToSet.Select(code => vSSMCOM.get_unit(code)).ToReadOnlyReactivePropertySlim();

            this.ParameterCodeToSet.Subscribe(cd => SetValue.Value = vSSMCOM.get_raw_value(cd));
            this.PhysicalValue = SetValue.Select(_ => vSSMCOM.get_value(ParameterCodeToSet.Value)).ToReadOnlyReactivePropertySlim();

            this.SwitchCodeToSet = GetDefaultReactivePropertySlim<SSMSwitchCode>(SSMSwitchCode.AT_Vehicle_ID, "SwitchCodeToSet");
            this.SetSwitch = GetDefaultReactivePropertySlim<bool>(false, "SwtSwitch");
            this.SetSwitch.Subscribe(v => vSSMCOM.SetSwitch(SwitchCodeToSet.Value, v));
            this.SwitchCodeToSet.Subscribe(cd => SetSwitch.Value = vSSMCOM.get_switch(cd));
        }

        public void Dispose()
        {
            this.ParameterCodeToSet.Dispose();
            this.MaxValue.Dispose();
            this.SetValue.Dispose();
            this.PhysicalUnit.Dispose();
            this.PhysicalValue.Dispose();
            this.SwitchCodeToSet.Dispose();
            this.SetSwitch.Dispose();
        }
    }
}