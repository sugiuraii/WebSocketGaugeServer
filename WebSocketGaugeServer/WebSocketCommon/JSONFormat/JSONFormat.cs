using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Arduino;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.Defi;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.SSM;

namespace SZ2.WebSocketGaugeServer.WebSocketCommon.JSONFormat
{
    public abstract class JSONFormat
	{
        public string mode { get; set; }

		public string Serialize ()
		{
			return JsonConvert.SerializeObject(this);
		}
		public abstract void Validate ();
	}

	public class JSONFormatsException : Exception
	{
		public JSONFormatsException(){
		}
		public JSONFormatsException(string message): base(message){
		}
		public JSONFormatsException(string message, Exception inner) : base(message) { }
	}

    public class ResetJSONFormat : JSONFormat
    {
        public const string ModeCode = "RESET";
        ResetJSONFormat()
        {
            mode = ModeCode;
        }

        public override void Validate()
        {
            if (mode != ModeCode)
            {
                throw new JSONFormatsException("mode property of " + this.GetType().ToString() + " packet is not valid.");
            } 
        }
    }

	public class ValueJSONFormat : JSONFormat
	{
		public Dictionary<string,string> val;
        public const string ModeCode = "VAL";

		public ValueJSONFormat()
		{
			mode = ModeCode;
			val = new Dictionary<string, string> ();
		}
		public override void Validate ()
		{
			try{
				if (mode != ModeCode) {
					throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
				}
				else{
					foreach (var key in val.Keys) {
						//Numeric case
						if (Enum.IsDefined (typeof(DefiParameterCode), key) || Enum.IsDefined (typeof(SSMParameterCode), key)
                                            || Enum.IsDefined(typeof(ArduinoParameterCode), key) || Enum.IsDefined (typeof(OBDIIParameterCode), key)){
							double val_result;
							if(!double.TryParse(val[key], out val_result))
								throw new JSONFormatsException("Value of " + key + "is not numeric.");
							else 
								return;
						}
						else if(Enum.IsDefined(typeof(SSMSwitchCode),key)){
							bool val_result;
							if(!bool.TryParse(val[key],out val_result))
								throw new JSONFormatsException("Value of " + key + " is not a flag (true or false).");
							else
								return;
						}
						else{
							throw new JSONFormatsException ("Parameter_Code property of VAL packet is not valid.");
						}
					}
				}
			}
			catch (ArgumentException ex) {
				throw new JSONFormatsException ("Invalid argument is used in ValueJSONFormat :" + ex.Message, ex); 
			}
		}
	}
	public class ErrorJSONFormat : JSONFormat
	{
        public const string ModeCode = "ERR";
		public ErrorJSONFormat()
		{
			mode = ModeCode;
		}
		public override void Validate()
		{
			if (mode != ModeCode) {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
		public string msg;
	}

	public class ResponseJSONFormat : JSONFormat
	{
        public const string ModeCode = "RES";
		public ResponseJSONFormat()
		{
			mode = ModeCode;
		}
		public override void Validate()
		{
			if (mode != ModeCode) {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}

		public string msg;
	}
}

