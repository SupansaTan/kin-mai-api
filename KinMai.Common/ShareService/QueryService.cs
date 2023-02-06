using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinMai.Common.ShareService
{
    public class QueryService
    {
        private static readonly string filePath = AppDomain.CurrentDomain.BaseDirectory + "/QueryCommand/";

        public static string GetCommand(string command, params ParamCommand[] queparams)
        {
            try
            {
                if (command.IndexOf('.') == -1) command = command + ".sql";
                var result = File.ReadAllText(filePath + command);
                if (queparams != null)
                    foreach (var kv in queparams)
                        result = result.Replace(kv.Key, kv.Value);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }
    }

    public class ParamCommand
    {
        public string Key;
        public string Value;

        public ParamCommand() { }

        public ParamCommand(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
