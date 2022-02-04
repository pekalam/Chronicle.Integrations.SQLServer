namespace Chronicle.Integrations.SQLServer
{
    public interface ISagaDataSerialization
    {
        string SerializeSagaData(object data);
        object DeserializeSagaData(string serializedData, string sagaType);
    }

    public delegate Type GetSagaType(string sagaTypeName);

    public interface ISagaLogDataSerialization
    {
        string SerializeMessage(object message);
        object DeserializeMessage(string messageType, string serializedMessage);
    }
}