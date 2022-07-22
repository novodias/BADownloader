using System.Text;

namespace MinimalLog
{
    internal class Logger : ILogger
    {
        private readonly static object _writeLock = new();
        internal Logging[]? Logtypes { get; init; }
        readonly FileInfo? _fileInfo;
        readonly bool _writable;

        internal Logger(IEnumerable<Logging>? logtypes = null, bool writable = false)
        {
            this._writable = writable;

            if ( writable )
            {
                string date = $"{DateTime.Now.ToShortDateString().Replace('/', '-')}";
                string path = $"log-{date}.txt";
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                this._fileInfo = new(path);
                if ( !_fileInfo.Exists ) _fileInfo.Create();
            }

            if ( logtypes is not null )
            {
                int count = logtypes.Count();
                int index = 0;
                this.Logtypes = new Logging[count];
                foreach (var log in logtypes)
                {
                    this.Logtypes[index++] = log;
                }
            }
        }

        public void Log<T>(Logging logenum, ReadOnlySpan<char> log)
        {
            string type = typeof(T).Name;
            string message = $"[{type}][{logenum}]: {log}";

            if ( this.Logtypes is not null && this.Logtypes.Contains(logenum) )
                Console.WriteLine(message);

            Write(message);
        }

        public void Log(Logging logenum, ReadOnlySpan<char> id, ReadOnlySpan<char> log)
        {
            string message = $"[{id}][{logenum}]: {log}";

            if ( this.Logtypes is not null && this.Logtypes.Contains(logenum) )
                Console.WriteLine(message);

            Write(message);
        }

        internal void Write(ReadOnlySpan<char> log)
        {
            lock(_writeLock)
            {
                if ( this._writable && this._fileInfo is not null && this._fileInfo.Exists )
                {
                    using var sw = new StreamWriter(this._fileInfo.OpenWrite(), Encoding.UTF8);

                    sw.WriteLine(log);
                    sw.Flush();
                }
            }
        }

    }

    public enum Logging
    {
        Information = 0,
        Debug = 1,
        Warning = 2,
        Error = 3,
    }
    
}