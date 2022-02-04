using Chronicle;
using Chronicle.Integrations.SQLServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    [JsonSerializable(typeof(SagaData))]
    [JsonSerializable(typeof(Message1))]
    [JsonSerializable(typeof(Message2))]
    public partial class MyJsonContext : JsonSerializerContext
    {
    }

    public class SagaLogDataSerialization : ISagaLogDataSerialization
    {
        public object DeserializeMessage(string messageType, string serializedMessage)
        {
            if (messageType == typeof(Message1).Name)
            {
                return JsonSerializer.Deserialize(serializedMessage, MyJsonContext.Default.Message1)!;
            }
            if (messageType == typeof(Message2).Name)
            {
                return JsonSerializer.Deserialize(serializedMessage, MyJsonContext.Default.Message2)!;
            }
            Debug.Assert(false);
            throw new();
        }


        string ISagaLogDataSerialization.SerializeMessage(object message)
        {
            return JsonSerializer.Serialize(message);
        }
    }

    public class SagaDataSerialization : ISagaDataSerialization
    {
        public object DeserializeSagaData(string serializedData, string sagaType)
        {
            if (sagaType == typeof(SampleSaga).Name)
            {
                return JsonSerializer.Deserialize(serializedData, MyJsonContext.Default.SagaData);
            }
            Debug.Assert(false);
            throw new();
        }

        public string SerializeSagaData(object data)
        {
            return JsonSerializer.Serialize(data);
        }
    }

    public static class SagaTypeSerialization
    {
        public static Type GetSagaType(string typeName)
        {
            return typeof(SampleSaga);
        }
    }

    public class Message1
    {
        public string Text { get; set; }
    }

    public class Message2
    {
        public string Text { get; set; }
    }

    public class SagaData
    {
        public bool IsMessage1Received { get; set; }
        public bool IsMessage2Received { get; set; }
    }

    public class SampleSaga : Saga<SagaData>, ISagaStartAction<Message1>, ISagaAction<Message2>
    {
        public static bool Completed = false;

        public Task HandleAsync(Message1 message, ISagaContext context)
        {
            Data.IsMessage1Received = true;
            Console.WriteLine($"Received message1 with message: {message.Text}");
            CompleteSaga();
            return Task.CompletedTask;
        }

        public Task HandleAsync(Message2 message, ISagaContext context)
        {
            Data.IsMessage2Received = true;
            Console.WriteLine($"Received message2 with message: {message.Text}");
            CompleteSaga();
            return Task.CompletedTask;
        }

        public Task CompensateAsync(Message1 message, ISagaContext context)
            => Task.CompletedTask;

        public Task CompensateAsync(Message2 message, ISagaContext context)
        {
            return Task.CompletedTask;
        }

        private void CompleteSaga()
        {
            if (Data.IsMessage1Received && Data.IsMessage2Received)
            {
                Complete();
                Completed = true;
            }
        }
    }

    public class EfCorePersistence_Tests
    {
        private readonly IServiceProvider serviceProvider;

        public EfCorePersistence_Tests()
        {
            var services = new ServiceCollection();

            services.AddChronicle(opt =>
            {
                opt.UseEfCorePersistence<SagaLogDataSerialization, SagaDataSerialization>(services,
                    SagaTypeSerialization.GetSagaType, opt =>
                    {
                        opt.UseSqlServer("Data Source=127.0.0.1;Initial Catalog=AuctionhouseDatabase;User ID=sa;Password=Qwerty1234");
                        //opt.UseInMemoryDatabase("test");
                    });
            });

            serviceProvider = services.BuildServiceProvider();
            ChronicleEfCoreIntegrationInitializer.Initialize(serviceProvider);
        }

        [Fact]
        public async Task Test_saga_completion()
        {
            var id = Guid.NewGuid();
            var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();

            var context = SagaContext
                .Create().WithSagaId(id.ToString()).Build();
            await coordinator.ProcessAsync(new Message1 { Text = "Hello" }, context);
            await coordinator.ProcessAsync(new Message2 { Text = "Hello" }, context);

            Assert.True(SampleSaga.Completed);
        }
    }
}