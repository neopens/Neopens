using System;
using System.Collections.Generic;
using System.Text;

namespace Neopens.FrameworkLite.Interfaces
{
    public interface IAppConfig
    {
        string GetString(string key);
        string GetStringOrDefault(string key, string defaultValue);
        bool GetBoolean(string key);
        bool GetBooleanOrDefault(string key, bool defaultValue);
        int GetInt(string key);
        int GetIntOrDefault(string key, int defaultValue);
        float GetSingle(string key);        
        float GetSingleOrDefault(string key, float defaultValue);
    }
}
