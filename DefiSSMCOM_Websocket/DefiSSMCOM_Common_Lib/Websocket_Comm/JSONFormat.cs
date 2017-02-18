using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using DefiSSMCOM.Defi;
using DefiSSMCOM.SSM;

namespace DefiSSMCOM.WebSocket.JSON
{
	public abstract class JSONFormats
	{
		public string mode;

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

	public class ValueJSONFormat : JSONFormats
	{
		public Dictionary<string,string> val;

		public ValueJSONFormat()
		{
			mode = "VAL";
			val = new Dictionary<string, string> ();
		}
		public override void Validate ()
		{
			try{
				if (mode != "VAL") {
					throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
				}
				else{
					foreach (var key in val.Keys) {
						//Numeric case
						if ((Enum.IsDefined (typeof(Defi_Parameter_Code), key)) || (Enum.IsDefined (typeof(SSM_Parameter_Code), key))){
							double val_result;
							if(!double.TryParse(val[key], out val_result))
								throw new JSONFormatsException("Value of " + key + "is not numeric.");
							else 
								return;
						}
						else if(Enum.IsDefined(typeof(SSM_Switch_Code),key)){
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
		public ErrorJSONFormat()
		{
			mode = "ERR";
		}
		public override void Validate()
		{
			if (mode != "ERR") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}
		public string msg;
	}
	public class ResponseJSONFormat : JSONFormats
	{
		public ResponseJSONFormat()
		{
			mode = "RES";
		}
		public override void Validate()
		{
			if (mode != "RES") {
				throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
			}
		}

		public string msg;
	}
	public class Defi_WS_SendJSONFormat : JSONFormats
	{
		public Defi_WS_SendJSONFormat()
		{
			mode = "DEFI_WS_SEND";
		}
		public string code;
		public bool flag;

		public override void Validate()
		{
			try
			{
				if (mode != "DEFI_WS_SEND") {
					throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
				}
				else{
					if (!(Enum.IsDefined (typeof(Defi_Parameter_Code), code)))
						throw new JSONFormatsException ("Defi_Parameter_Code property of DEFI_WS_SEND packet is not valid.");
					if( flag != true && flag != false)
						throw new JSONFormatsException ("flag of DEFI_WS_SEND packet is not valid.");
				}
			}
			catch(ArgumentNullException ex) {
				throw new JSONFormatsException ("Null is found in DEFI_WS_SEND packet.", ex);
			}
		}
	}

	public class Defi_WS_IntervalJSONFormat : JSONFormats
	{
		public int interval;
		public Defi_WS_IntervalJSONFormat()
		{
			mode = "DEFI_WS_INTERVAL";
		}

		public override void Validate()
		{
			if (mode != "DEFI_WS_INTERVAL") {
				throw new JSONFormatsException ("mode property is not valid.");
			}
			else{
				if(interval < 0)
					throw new JSONFormatsException ("interval property of DEFI_WS_SEND packet is less than 0.");
			}
		}
	}

	public class SSM_COM_ReadJSONFormat : JSONFormats
	{
		public SSM_COM_ReadJSONFormat()
		{
			mode = "SSM_COM_READ";
		}
		public string code;
		public string read_mode;
		public bool flag;

		public override void Validate()
		{
			try
			{
				if (mode != "SSM_COM_READ") {
					throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
				}
				else{
					if (!(Enum.IsDefined (typeof(SSM_Parameter_Code), code)))
						throw new JSONFormatsException ("SSM_Parameter_Code property of SSM_COM_READ packet is not valid.");
					if( read_mode != "FAST" && read_mode != "SLOW")
						throw new JSONFormatsException ("read_mode of SSM_COM_READ packet is not valid (Should be SLOW or FAST).");
					if( flag != true && flag != false)
						throw new JSONFormatsException ("flag of SSM_COM_READ packet is not valid.");
				}
			}
			catch(ArgumentNullException ex) {
				throw new JSONFormatsException ("Null is found in SSM_COM_READ packet.", ex);
			}
		}
	}

	public class SSM_SLOWREAD_IntervalJSONFormat : JSONFormats
	{
		public int interval;
		public SSM_SLOWREAD_IntervalJSONFormat()
		{
			mode = "SSM_SLOWREAD_INTERVAL";
		}

		public override void Validate()
		{
			if (mode != "SSM_SLOWREAD_INTERVAL") {
				throw new JSONFormatsException ("mode property is not valid.");
			}
			else{
				if(interval < 0)
					throw new JSONFormatsException ("interval property of SSM_SLOWREAD_INTERVAL packet is less than 0.");
			}
		}
	}
	/* not used
	public class SSM_WS_SendJSONFormat : JSONFormats
	{
		public SSM_WS_SendJSONFormat()
		{
			mode = "SSM_WS_SEND";
		}
		public string code;
		public bool flag;

		public override void Validate()
		{
			try
			{
				if (mode != "SSM_WS_SEND") {
					throw new JSONFormatsException ("mode property of " + this.GetType().ToString() + " packet is not valid.");
				}
				else{
					if (!(Enum.IsDefined (typeof(SSM_Parameter_Code), code)))
						throw new JSONFormatsException ("SSM_Parameter_Code property of SSM_WS_SEND packet is not valid.");
					if( flag != true && flag != false)
						throw new JSONFormatsException ("flag of SSM_WS_SEND packet is not valid.");
				}
			}
			catch(ArgumentNullException ex) {
				throw new JSONFormatsException ("Null is found in SSM_WS_SEND packet.", ex);
			}
		}
	}
	*/

}

