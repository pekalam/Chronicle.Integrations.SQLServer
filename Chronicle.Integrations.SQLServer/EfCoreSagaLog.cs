using Microsoft.EntityFrameworkCore;

namespace Chronicle.Integrations.SQLServer
{
    internal class EfCoreSagaLog : ISagaLog
    {
        private readonly EfCoreDbContext _dbContext;
        private readonly EfCoreSagaLogDataAssembler _assembler;

        public EfCoreSagaLog(EfCoreDbContext dbContext, EfCoreSagaLogDataAssembler assembler)
        {
            _dbContext = dbContext;
            _assembler = assembler;
        }

        public async Task<IEnumerable<ISagaLogData>> ReadAsync(SagaId id, Type type)
        {
            var dbSagaLogDatas = await _dbContext.SagaLogDatas
                .Where(d => d.SagaId == id)
                .ToListAsync();
            return dbSagaLogDatas.Select(_assembler.FromDb);
        }

        public Task WriteAsync(ISagaLogData message)
        {
            var dbSagaLog = _assembler.ToDb(message);
            _dbContext.SagaLogDatas.Add(dbSagaLog);
            return _dbContext.SaveChangesAsync();
        }
    }
}