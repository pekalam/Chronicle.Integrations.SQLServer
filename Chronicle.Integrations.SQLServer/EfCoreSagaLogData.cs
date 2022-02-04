using System.ComponentModel.DataAnnotations.Schema;

namespace Chronicle.Integrations.SQLServer
{
    internal class DbSagaLogData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string SagaId { get; set; }

        public string Type { get; set; }

        public long CreatedAt { get; set; }

        public string Message { get; set; }
        public string MessageType { get; set; }

    }

    internal class EfCoreSagaLogData : ISagaLogData
    {
        public SagaId Id { get; set; }

        public Type Type { get; set; }

        public long CreatedAt { get; set; }

        public object Message { get; set; }

    }



    internal class EfCoreSagaLogDataAssembler
    {
        private readonly ISagaLogDataSerialization _serialization;
        private readonly GetSagaType _getSagaType;

        public EfCoreSagaLogDataAssembler(ISagaLogDataSerialization serialization, GetSagaType getSagaType)
        {
            _serialization = serialization;
            _getSagaType = getSagaType;
        }

        public EfCoreSagaLogData FromDb(DbSagaLogData dbSagaLogData)
        {
            return new EfCoreSagaLogData
            {
                Id = dbSagaLogData.SagaId,
                CreatedAt = dbSagaLogData.CreatedAt,
                Message = _serialization.DeserializeMessage(dbSagaLogData.MessageType, dbSagaLogData.Message),
                Type = _getSagaType(dbSagaLogData.Type),
            };
        }

        public DbSagaLogData ToDb(ISagaLogData sagaLogData)
        {
            return new DbSagaLogData
            {
                SagaId = sagaLogData.Id,
                CreatedAt = sagaLogData.CreatedAt,
                Message = _serialization.SerializeMessage(sagaLogData.Message),
                Type = sagaLogData.Type.Name,
                MessageType = sagaLogData.Message.GetType().Name,
            };
        }
    }
}