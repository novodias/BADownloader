namespace MinimalLog
{
    public class MinimalLogger
    {
        private static ILogger? _log;
        private static readonly object _logRoot = new();
        
        /// <summary>
        /// Sets up the logger in a thread-safe way. Use it in a public static field somewhere.
        /// </summary>
        /// <param name="logsenabled">Array of Logging types that should show in the console output.</param>
        /// <param name="writable">If true, logs will be saved in a text file in the app directory.</param>
        /// <returns>Logger</returns>
        public static ILogger SetUpLogger(IReadOnlyCollection<Logging>? logsenabled = null, bool writable = false) 
        { 
            if (_log is null)
            {
                lock (_logRoot)
                {
                    if (_log is null)
                    {
                        _log = new Logger(logsenabled, writable);
                    }
                }
            }

            return _log;
        }

        public static void Log<T>(Logging logenum, ReadOnlySpan<char> log) => _log?.Log<T>(logenum, log);
        public static void Log(Logging logenum, ReadOnlySpan<char> id, ReadOnlySpan<char> log) => _log?.Log(logenum, id, log);
    }
}