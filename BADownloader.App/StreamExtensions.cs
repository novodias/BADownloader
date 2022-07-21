using System;

namespace BADownloader.App
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsyncWithProgress(this Stream source, Stream destination, long length, string message, (int x, int y) position, CancellationToken cancellationToken = default) 
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));

            var infoProgress = new ProgressInfo(position, length, message);

            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0) 
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);

                infoProgress = Progress.Report(infoProgress, destination.Length);
            }

            // while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) 
            // {
            //     await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

            //     infoProgress = Progress.Report(infoProgress, destination.Length);
            // }
        }
    }
    
}