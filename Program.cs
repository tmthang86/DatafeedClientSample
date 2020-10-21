using QuickFix;
using QuickFix.Transport;
using System;

namespace DatafeedClient
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionSettings settings = new SessionSettings("SessionConfig.cfg"); //load config file
            IApplication myApp = new MyFixApplication();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            SocketInitiator fixClient = new SocketInitiator(
                myApp,
                storeFactory,
                settings,
                logFactory);            
            fixClient.Start(); // start connecting to FIX Aceptor            
            Console.ReadLine();//press Enter to stop Fix Client
            fixClient.Stop();


        }
    }
}
