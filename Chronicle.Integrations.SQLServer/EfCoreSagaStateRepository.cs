using Microsoft.EntityFrameworkCore;

namespace Chronicle.Integrations.SQLServer
{
    internal class EfCoreSagaStateRepository : ISagaStateRepository
    {
        private readonly EfCoreDbContext _dbContext;
        private readonly EfCoreSagaStateAssembler _assembler;

        public EfCoreSagaStateRepository(EfCoreDbContext dbContext, EfCoreSagaStateAssembler assembler)
        {
            _dbContext = dbContext;
            _assembler = assembler;
        }

        public async Task<ISagaState> ReadAsync(SagaId id, Type type)
        {
            var dbSagaState = await _dbContext.SagaStates.FindAsync(id.Id);
            if (dbSagaState == null)
            {
                return null!;
            }
            return _assembler.FromDb(dbSagaState);
        }

        public Task WriteAsync(ISagaState state)
        {
            var dbSagaState = _assembler.ToDb(state);
            var local = _dbContext.SagaStates.Local.FirstOrDefault(s => s.Id == dbSagaState.Id);

            if (local != default)
            {
                _dbContext.Entry(local).State = EntityState.Detached;
                _dbContext.Entry(dbSagaState).State = EntityState.Modified;
            }
            else
            {
                _dbContext.SagaStates.Add(dbSagaState);
            }

            return _dbContext.SaveChangesAsync();
        }
    }
}