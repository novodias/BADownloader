using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace BADownloader
{
    public class BADHttp
    {
        public static readonly HttpClient Client = new
        ( 
            new HttpClientHandler() 
            {
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = false,
            }
        );

        public static async Task<HttpResponseMessage> SendAsync( string url, Dictionary<string, string>? headers = null, string accept = "*/*" )
        {
            var request = new HttpRequestMessage( HttpMethod.Get, url );

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

            if ( Program.IsDebugMode )
            {
                var header = response.Headers;
                foreach (var pair in header)
                {
                    Console.Write("[" + pair.Key + ": ");
                    var sb = new StringBuilder();
                    sb.AppendJoin(',', pair.Value);
                    Console.Write(sb.ToString() + "]\n");
                }
                System.Console.WriteLine( "[" + nameof(SendAsync) + "] Status Code: " + response.StatusCode.ToString());
                System.Console.WriteLine( "[" + nameof(SendAsync) + "] Is Success Status Code: " + response.IsSuccessStatusCode);
            }

            return response;
        }

        public static async Task DownloadFileAsync( string downloadurl, FileStream file )
        {
            using var response = await Client.GetAsync( downloadurl, HttpCompletionOption.ResponseHeadersRead );

            if ( Program.IsDebugMode )
            {
                System.Console.WriteLine( "[" + nameof(DownloadFileAsync) + "] Status Code: " + response.StatusCode);
                System.Console.WriteLine( "[" + nameof(DownloadFileAsync) + "] Is Success Status Code: " + response.IsSuccessStatusCode);
            }

            response.EnsureSuccessStatusCode();
            
            long mb = 1000 * 1000;
            long? tamanho = response.Content.Headers.ContentLength / mb;
            Console.WriteLine( $"Tamanho: {tamanho} MB" );

            using ( var stream = await response.Content.ReadAsStreamAsync() )
            {
                await stream.CopyToAsync( file );

                await stream.DisposeAsync();
            }

            response.Dispose();
        }

        public static void Dispose()
        {
            Client.Dispose();
        }
    }
}