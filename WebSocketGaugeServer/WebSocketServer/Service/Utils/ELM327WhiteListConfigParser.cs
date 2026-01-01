using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SZ2.WebSocketGaugeServer.ECUSensorCommunication.ELM327.Config;

namespace SZ2.WebSocketGaugeServer.WebSocketServer.Service.Utils
{
    public class ELM327PIDWhiteListConfigParser {
        public static ELM327PIDWhiteListConfig parse(IConfigurationSection configSection) {
            var mode = Enum.Parse<ELM327PIDWhiteListMode>(configSection["mode"] ?? throw new ArgumentNullException("ELM327WhiteListConfig mode is null. Maybe not defined."), true);
            switch(mode) {
                case ELM327PIDWhiteListMode.Query:
                case ELM327PIDWhiteListMode.AllPass:
                    return new ELM327PIDWhiteListConfig(mode, []); // Skip parse customlist
                case ELM327PIDWhiteListMode.Custom:
                    var customPIDStrList = (configSection.GetSection("customlist").Get<string[]>() ?? throw new ArgumentNullException("ELM327 custom white list config is null. Maybe not defined."));
                    var customPIDByteList = customPIDStrList.Select(pidByteStr => Convert.ToByte(pidByteStr.Trim(), 16)).ToArray();
                    return new ELM327PIDWhiteListConfig(mode, customPIDByteList);
                default:
                    throw new InvalidProgramException("Enum of ELM327PIDWhiteListMode is set to undefined value.");
            }
        }
    }
}