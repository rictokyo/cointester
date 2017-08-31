
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class Receive
{
    public static void Main()
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

        var cm = new CommandManager();

        Task.Factory.StartNew(() =>
        {
            cm.ReceiveCommands();
        });

        Console.WriteLine(" Press [enter] to exit.");
        Console.TreatControlCAsInput = false;
        CommandManager.AutoEvent.WaitOne();
    }
}

