public interface ISqsMessageHandler<T>
{
    Task HandleAsync(string messageBody, string messageId);
}
