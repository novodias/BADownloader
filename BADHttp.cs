using System.Net.Http.Headers;

namespace BADownloader
{
    public class BADHttp
    {
        public static readonly HttpClient Client = new();

        public static async Task<HttpResponseMessage> GetResponseMessageAsync( string url, Dictionary<string, string> headers )
        {
            var request = new HttpRequestMessage( HttpMethod.Get, url );

            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue("*/*") );
            
            foreach ( var item in headers )
            {
                request.Headers.Add( item.Key, item.Value );
            }

            var response = await Client.SendAsync( request );
            response.EnsureSuccessStatusCode();

            return response;
        }

        public static async Task DownloadFileAsync( string downloadurl, FileStream file )
        {
            using var response = await Client.GetAsync( downloadurl, HttpCompletionOption.ResponseHeadersRead );
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