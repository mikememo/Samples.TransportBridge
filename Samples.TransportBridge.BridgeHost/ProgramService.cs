namespace Samples.TransportBridge.BridgeHost
{
    using System;
    using System.ComponentModel;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Bridge;
    using NServiceBus.Logging;

    [DesignerCategory("Code")]
    class ProgramService : ServiceBase
    {
        private IBridge _bridge;
        static readonly ILog Logger = LogManager.GetLogger<ProgramService>();

        static void Main()
        {
            Console.Title = "Samples.TransportBridge.BridgeHost";

            using (var service = new ProgramService())
            {
                // to run interactive from a console or as a windows service
                if (Environment.UserInteractive)
                {
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        service.OnStop();
                    };
                    service.OnStart(null);
                    Console.WriteLine("\r\nPress enter key to stop program\r\n");
                    Console.Read();
                    service.OnStop();
                    return;
                }

                Run(service);
            }
        }

        protected override void OnStart(string[] args)
        {
            AsyncOnStart().GetAwaiter().GetResult();
        }

        async Task AsyncOnStart()
        {
            try
            {
                var bridgeConfiguration = Bridge
                    .Between<MsmqTransport>(endpointName: "TransportBridge.MsmqBank")
                    .And<RabbitMQTransport>(
                        endpointName: "TransportBridge.RabbitBank",
                        customization: transportExtensions =>
                        {
                            transportExtensions.ConnectionString("host=localhost");
                        });

                bridgeConfiguration.UseSubscriptionPersistece<InMemoryPersistence>((e, c) => { });
            
                bridgeConfiguration.AutoCreateQueues();
                _bridge = bridgeConfiguration.Create();
                await _bridge.Start().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.Fatal("Failed to start", exception);
                Environment.FailFast("Failed to start", exception);
            }
        }

        protected override void OnStop()
        {
            _bridge?.Stop().GetAwaiter().GetResult();
        }
    }
}