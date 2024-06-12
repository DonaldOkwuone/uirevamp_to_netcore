using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.utils
{
    public static class Settings
    {
        private static Dictionary<string, string> MySettings;
        
       

        public static string Get(string key, string otherwise = "")
        {
            if (MySettings == null) return otherwise;
            return MySettings.ContainsKey(key) ? MySettings[key] : otherwise;
        }

        public static void Initiate(Dictionary<string, string> mySettings)
        {
            MySettings = mySettings;
        }


        public static int GetInt(string key, int otherwise = 0)
        {
            var str = Get(key);
            if (string.IsNullOrEmpty(str))
            {
                return otherwise;
            }
            var val = otherwise;
            try
            {
                val = Convert.ToInt32(str);
            }
            catch
            {
            }
            return val;
        }

        public static long GetLong(string key, long otherwise = 0)
        {
            var str = Get(key);
            if (string.IsNullOrEmpty(str))
            {
                return otherwise;
            }
            var val = otherwise;
            try
            {
                val = Convert.ToInt64(str);
            }
            catch
            {
            }
            return val;
        }
        public static bool GetBool(string key, bool otherwise = false)
        {
            var str = Get(key);
            if (string.IsNullOrEmpty(str))
            {
                return otherwise;
            }
            var val = otherwise;
            try
            {
                val = Convert.ToBoolean(str);
            }
            catch
            {
            }
            return val;
        }
    }
}
