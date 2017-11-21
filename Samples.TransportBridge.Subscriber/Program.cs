namespace Samples.TransportBridge.Subscriber
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using NServiceBus;
    using Shared;

    static class Program
    {
        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.Title = "Samples.TransportBridge.Subscriber";
            var endpointConfiguration = new EndpointConfiguration("Samples.TransportBridge.Subscriber");

            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
           
            var routing = transport.Routing();
            var bridge = routing.ConnectToBridge("TransportBridge.MsmqBank");
            bridge.RegisterPublisher(typeof(OrderReceived), "Samples.TransportBridge.Publisher");
            
            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            await endpointInstance.Subscribe(typeof(OrderReceived));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}