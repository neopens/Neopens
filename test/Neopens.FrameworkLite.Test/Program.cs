using Neopens.FrameworkLite.Core;
using Neopens.FrameworkLite.Logging;


const string TestService = nameof(TestService);

ServiceManager.Instance.Initialize(null);

ServiceManager.Instance.Register("Neopens.FrameworkLite.Test.dll", "TestService01");
ServiceManager.Instance.Register("Neopens.FrameworkLite.Test.dll", TestService);


var result = ServiceManager.Instance.SendMessage("TestService", "Echo", "Hello World!!!");

Console.WriteLine(result);

var r2 = ServiceManager.Instance.SendMessage("TestService", "Echo01", "Hello World!!!");

Console.WriteLine(r2);


Console.Read();

//Console.WriteLine("Hello, World!");
