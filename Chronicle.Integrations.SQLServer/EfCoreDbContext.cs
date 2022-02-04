using Microsoft.EntityFrameworkCore;

namespace Chronicle.Integrations.SQLServer
{
    internal class EfCoreDbContext : DbContext
    {
        public EfCoreDbContext(DbContextOptions<EfCoreDbContext> options) : base(options)
        {
        }

        public DbSet<DbSagaState> SagaStates { get; set; } = null!;
        public DbSet<DbSagaLogData> SagaLogDatas { get; set; } = null!;
    }
}