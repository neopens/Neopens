using Neopens.FrameworkLite.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neopens.FrameworkLite.Test.BusinessServices
{
    internal class TestService:BusinessService
    {

        protected override void OnCreated(object[] args)
        {
            base.OnCreated(args);
        }

        public static string Echo(string message)
        {
            return $"Echo: {message}";
        }
    }
}
