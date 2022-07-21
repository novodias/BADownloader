namespace MinimalLog
{
    public interface ILogger
    {
        void Log<T>(Logging logenum, ReadOnlySpan<char> log);
        void Log(Logging logenum, ReadOnlySpan<char> id, ReadOnlySpan<char> log);
    }
}