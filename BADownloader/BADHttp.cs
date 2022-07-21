using System.Net;
using System.Text;
using System.Net.Http.Headers;

namespace BADownloader
{
    public static class BADHttp
    {
        public static readonly HttpClient Client = new
        ( 
            new HttpClientHandler() 
            {
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = true,
            }
        );

        public static async Task<HttpResponseMessage> SendAsync( string requestUri, Dictionary<string, string>? headers = null, string accept = "*/*" )
        {
            var request = new HttpRequestMessage( HttpMethod.Get, requestUri );

            if ( accept.Contains(',') )
            {
                string[] strArray = accept.Split(',');
                foreach (var str in strArray)
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(str));
                }
            }
            else
                request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue(accept) );
            
            if ( headers is not null )
            {
                foreach ( var item in headers )
                {
                    request.Headers.Add( item.Key, item.Value );
                }
            }

            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.60 Safari/537.36");

            var response = await Client.SendAsync( request );
            response.EnsureSuccessStatusCode();

            // var header = response.Headers;
            // foreach (var pair in header)
            // {
            //     var sb = new StringBuilder()
            //         .AppendJoin(',', pair.Value)
            //         .ToString();
            // }

            return response;
        }

        public static async Task<HttpResponseMessage> DownloadFileAsync( string downloadurl )
        {
            var response = await Client.GetAsync(downloadurl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return response;
            
            // using var response = await Client.GetAsync( downloadurl, HttpCompletionOption.ResponseHeadersRead );

            // response.EnsureSuccessStatusCode();

            // long mb = 1000 * 1000;
            // long? tamanho = response.Content.Headers.ContentLength / mb;
            // Console.WriteLine( $"Tamanho: {tamanho} MB" );

            // using ( var stream = await response.Content.ReadAsStreamAsync() )
            // {
            //     await stream.CopyToAsyncWithProgress( file );

            //     await stream.DisposeAsync();
            // }

            // response.Dispose();
        }

        // public static async Task DownloadFileAsync( string downloadurl, FileStream file, IProgress<float>? progress = null, CancellationToken cancellationToken = default )
        // {
        //     using var response = await Client.GetAsync( downloadurl, HttpCompletionOption.ResponseHeadersRead, cancellationToken );

        //     MinimalLogger.Log<BADHttp>(Logging.Information, $"DownloadFileAsync - URL: {downloadurl}");
        //     MinimalLogger.Log<BADHttp>(Logging.Information, $"DownloadFileAsync - Status Code: {response.StatusCode}");
        //     MinimalLogger.Log<BADHttp>(Logging.Information, $"DownloadFileAsync - Is Success Status Code: {response.IsSuccessStatusCode}");

        //     response.EnsureSuccessStatusCode();
            
        //     long mb = 1000 * 1000;
        //     long? contentLength = response.Content.Headers.ContentLength;
        //     Console.WriteLine( $"Tamanho: [{contentLength / mb} MB]" );

        //     using ( var download = await response.Content.ReadAsStreamAsync(cancellationToken) )
        //     {
        //         if (progress == null || !contentLength.HasValue) 
        //         {
        //             await download.CopyToAsync(file, cancellationToken);
        //             return;
        //         }
                
        //         var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
        //         await download.CopyToAsync(file, 81920, relativeProgress, cancellationToken);
        //         progress.Report(1);
        //     }

        //     response.Dispose();
        // }

        public static void Dispose()
        {
            Client.Dispose();
        }
    }
}