namespace Samples.TransportBridge.Publisher
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using Shared;

    internal static class Program
    {
        private static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        private static async Task AsyncMain()
        {
            Console.Title = "Samples.TransportBridge.Publisher";
            var endpointConfiguration = new EndpointConfiguration("Samples.TransportBridge.Publisher");

            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.EnableInstallers();

            var endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            await Start(endpointInstance).ConfigureAwait(false);

            await endpointInstance.Stop().ConfigureAwait(false);
        }

        private static async Task Start(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Press '1' to publish the OrderReceived event");
            Console.WriteLine("Press any other key to exit");

            #region PublishLoop

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                var orderReceivedId = Guid.NewGuid();
                if (key.Key == ConsoleKey.D1)
                {
                    var orderReceived = new OrderReceived
                    {
                        OrderId = orderReceivedId
                    };
                    await endpointInstance.Publish(orderReceived)
                        .ConfigureAwait(false);
                    Console.WriteLine($"Published OrderReceived Event with Id {orderReceivedId}.");
                }
                else
                {
                    return;
                }
            }

            #endregion
        }
    }
}