using Neopens.FrameworkLite.Core;
using Neopens.FrameworkLite.Logging;
using Neopens.FrameworkLite.Test;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;




const string TestService = nameof(TestService);

ServiceManager.Instance.Initialize(null);

ServiceManager.Instance.Register("Neopens.FrameworkLite.Test.dll", "TestService01");
ServiceManager.Instance.Register("Neopens.FrameworkLite.Test.dll", TestService);


var result = ServiceManager.Instance.SendMessage("TestService", "Echo", "Hello World!!!");

Console.WriteLine(result);

var r2 = ServiceManager.Instance.SendMessage("TestService", "Echo01", "Hello World!!!");

Console.WriteLine(r2);


LoggerFactory.Default.Debug($"this is Debug");
LoggerFactory.Default.Info($"this is Info");
LoggerFactory.Default.Warn($"this is Warn"); 

LoggerFactory.Default.Error($"this is Error");

Console.ReadLine( );

//Console.WriteLine("Hello, World!");
