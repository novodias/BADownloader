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
        public static ILogger SetUpLogger(int level = -1, bool writable = false) 
        { 
            if (_log is null)
            {
                lock (_logRoot)
                {
                    if (_log is null)
                    {
                        var logs = MinimalLogger.GetLoggings(level);
                        _log = new Logger(logs, writable);
                    }
                }
            }

            return _log;
        }

        private static IReadOnlyCollection<Logging>? GetLoggings(int level)
        {
            if ( level < 0 ) return null;
            Logging[] loggings = new Logging[level];

            int current_index = 0;
            for (int i = level; i < 4; i++) {
                loggings[current_index++] = (Logging)current_index;
            }

            return loggings;
        }

        public static void Log<T>(Logging logenum, ReadOnlySpan<char> log) => _log?.Log<T>(logenum, log);
        public static void Log(Logging logenum, ReadOnlySpan<char> id, ReadOnlySpan<char> log) => _log?.Log(logenum, id, log);
    }
}