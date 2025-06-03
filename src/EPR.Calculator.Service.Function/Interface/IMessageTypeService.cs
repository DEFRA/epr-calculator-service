namespace EPR.Calculator.Service.Function.Interface
{
    public interface IMessageTypeService
    {
        MessageBase DeserializeMessage(string json);
    }
}
