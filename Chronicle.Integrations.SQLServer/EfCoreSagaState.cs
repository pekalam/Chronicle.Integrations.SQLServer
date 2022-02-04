namespace Chronicle.Integrations.SQLServer
{
    internal class DbSagaState
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public SagaStates State { get; set; }

        public string Data { get; set; }
    }

    internal class EfCoreSagaState : ISagaState
    {
        public SagaId Id { get; set; }

        public Type Type { get; set; }

        public SagaStates State { get; set; }

        public object Data { get; set; }

        public void Update(SagaStates state, object data = null)
        {
            State = state;
            Data = data;
        }
    }

    internal class EfCoreSagaStateAssembler
    {
        private readonly ISagaDataSerialization _sagaDataSerialization;
        private readonly GetSagaType _getSagaType;

        public EfCoreSagaStateAssembler(ISagaDataSerialization sagaDataSerialization, GetSagaType getSagaType)
        {
            _sagaDataSerialization = sagaDataSerialization;
            _getSagaType = getSagaType;
        }

        public EfCoreSagaState FromDb(DbSagaState dbSagaState)
        {
            return new EfCoreSagaState
            {
                Id = dbSagaState.Id,
                State = dbSagaState.State,
                Data = _sagaDataSerialization.DeserializeSagaData(dbSagaState.Data, dbSagaState.Type),
                Type = _getSagaType(dbSagaState.Type),
            };
        }

        public DbSagaState ToDb(ISagaState sagaState)
        {
            return new DbSagaState
            {
                Id = sagaState.Id,
                Type = sagaState.Type.Name,
                Data = _sagaDataSerialization.SerializeSagaData(sagaState.Data),
                State = sagaState.State,
            };
        }
    }
}