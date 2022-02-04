using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Chronicle.Integrations.SQLServer
{
    public static class ChronicleEfCoreIntegrationInstaller
    {
        public static void UseEfCorePersistence<TSagaLogDataSerialization, TSagaDataSerialization>(this IChronicleBuilder builder,
            IServiceCollection services, GetSagaType getSagaTypeDelegate, Action<DbContextOptionsBuilder> dbContextOptionBuilderAction)
            where TSagaLogDataSerialization : class, ISagaLogDataSerialization
            where TSagaDataSerialization : class, ISagaDataSerialization
        {
            services.AddHostedService<ChronicleEfCoreIntegrationInitializerService>();
            services.AddSingleton(getSagaTypeDelegate);
            services.AddTransient<ISagaLogDataSerialization, TSagaLogDataSerialization>();
            services.AddTransient<ISagaDataSerialization, TSagaDataSerialization>();
            services.AddTransient<EfCoreSagaLogDataAssembler>();
            services.AddTransient<EfCoreSagaStateAssembler>();

            builder.UseSagaStateRepository<EfCoreSagaStateRepository>();
            builder.UseSagaLog<EfCoreSagaLog>();
            services.AddDbContext<EfCoreDbContext>(dbContextOptionBuilderAction);
        }
    }

    internal class ChronicleEfCoreIntegrationInitializerService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public ChronicleEfCoreIntegrationInitializerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            ChronicleEfCoreIntegrationInitializer.Initialize(_serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public static class ChronicleEfCoreIntegrationInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<EfCoreDbContext>();
            dbContext.Database.Migrate();
//            dbContext.Database.ExecuteSqlRaw(@"
//BEGIN TRANSACTION;
//GO

//IF OBJECT_ID(N'[SagaLogDatas]') IS NULL
//BEGIN

//CREATE TABLE [SagaLogDatas] (
//    [Id] int NOT NULL IDENTITY,
//    [SagaId] nvarchar(max) NOT NULL,
//    [Type] nvarchar(max) NOT NULL,
//    [CreatedAt] bigint NOT NULL,
//    [Message] nvarchar(max) NOT NULL,
//    [MessageType] nvarchar(max) NOT NULL,
//    CONSTRAINT [PK_SagaLogDatas] PRIMARY KEY ([Id])
//);
//END;
//GO

//IF OBJECT_ID(N'[SagaStates]') IS NULL
//BEGIN
//CREATE TABLE [SagaStates] (
//    [Id] nvarchar(450) NOT NULL,
//    [Type] nvarchar(max) NOT NULL,
//    [State] tinyint NOT NULL,
//    [Data] nvarchar(max) NOT NULL,
//    CONSTRAINT [PK_SagaStates] PRIMARY KEY ([Id])
//);
//END;
//GO

//COMMIT;
//GO
//");
        }
    }
}
