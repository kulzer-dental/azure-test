public class ConnectionStringNotFoundException : Exception
{
    public ConnectionStringNotFoundException()
    {
    }

    public ConnectionStringNotFoundException(string message)
        : base(message)
    {
    }

    public ConnectionStringNotFoundException(string message, Exception inner)
        : base(message, inner)
    {
    }
}