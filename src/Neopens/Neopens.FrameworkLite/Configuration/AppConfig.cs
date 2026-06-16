using Neopens.FrameworkLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Neopens.FrameworkLite.Configuration
{
    internal class AppConfig : IAppConfig
    {
        public AppConfig()
        {
            
        }

        public bool GetBoolean(string key)
        {
            throw new NotImplementedException();
        }

        public bool GetBooleanOrDefault(string key, bool defaultValue)
        {
            throw new NotImplementedException();
        }

        public int GetInt(string key)
        {
            throw new NotImplementedException();
        }

        public int GetIntOrDefault(string key, int defaultValue)
        {
            throw new NotImplementedException();
        }

        public float GetSingle(string key)
        {
            throw new NotImplementedException();
        }

        public float GetSingleOrDefault(string key, float defaultValue)
        {
            throw new NotImplementedException();
        }

        public string GetString(string key)
        {
            throw new NotImplementedException();
        }

        public string GetStringOrDefault(string key, string defaultValue)
        {
            throw new NotImplementedException();
        }
    }
}
