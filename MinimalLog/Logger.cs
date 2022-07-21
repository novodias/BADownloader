using System.Text;

namespace MinimalLog
{
    internal class Logger : ILogger
    {
        public Logging[]? Logtypes { get; init; }
        readonly FileStream? _filestream;
        readonly bool _writable;

        internal Logger(IEnumerable<Logging>? logtypes = null, bool writable = false)
        {
            this._writable = writable;

            if ( writable )
            {
                string date = $"{DateTime.Now.ToShortDateString().Replace('/', '-')}";
                string path = $"log-{date}.txt";
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                this._filestream = File.Create(path);
                this._filestream.Close();
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
            if ( this._writable && (this._filestream is not null && this._filestream.CanWrite) )
            {
                using var sw = new StreamWriter(this._filestream, Encoding.UTF8);

                sw.WriteLine(log);
                sw.Flush();
            }
        }

        internal void Dispose()
        {
            this._filestream?.Dispose();
        }

    }

    public enum Logging
    {
        Debug,
        Warning,
        Information,
        Error
    }
    
}