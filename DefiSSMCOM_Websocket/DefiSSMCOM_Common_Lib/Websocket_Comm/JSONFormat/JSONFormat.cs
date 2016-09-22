﻿using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DefiSSMCOM.WebSocket.JSON
{
	public abstract class JSONFormats
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

    public class ResetJSONFormat : JSONFormats
    {
        public const string ModeCode = "RES";
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

	public class ValueJSONFormat : JSONFormats
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
						if ((Enum.IsDefined (typeof(DefiParameterCode), key)) || (Enum.IsDefined (typeof(SSMParameterCode), key))){
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
	public class ErrorJSONFormat : JSONFormats
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

	public class ResponseJSONFormat : JSONFormats
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
