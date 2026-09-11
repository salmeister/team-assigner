namespace TeamAssigner.Services
{
    using System.Collections.Specialized;
    using System.Net;
    using System.Text;

    internal static class RESTUtil
    {
        // GitHub-hosted HttpClient sends no User-Agent; Akamai on site.api.espn.com often 403s that.
        // A spoofed Chrome UA can also 403 from some cloud IPs, so we try an app-style UA first
        // and fall back to a browser UA (and a short product token) on 403.
        internal const string DefaultUserAgent =
            "Mozilla/5.0 (compatible; TeamAssigner/1.0; +https://github.com/salmeister/team-assigner)";

        private static readonly string[] UserAgentFallbacks =
        [
            DefaultUserAgent,
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
            "TeamAssigner/1.0"
        ];

        private static readonly HttpClient Client = CreateClient();

        private static HttpClient CreateClient()
        {
            var handler = new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(10)
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            return client;
        }

        public static string Get(NameValueCollection? headers, string url)
        {
            return Send(HttpMethod.Get, url, headers, contentBody: null);
        }

        public static string Put(NameValueCollection? headers, string url, string package)
        {
            return Send(HttpMethod.Put, url.TrimEnd('/'), headers, contentBody: package);
        }

        private static string Send(HttpMethod method, string url, NameValueCollection? headers, string? contentBody)
        {
            Exception? lastError = null;

            foreach (string userAgent in UserAgentFallbacks)
            {
                using var request = new HttpRequestMessage(method, url);
                request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
                ApplyHeaders(request, headers);
                if (contentBody != null)
                {
                    request.Content = new StringContent(contentBody, Encoding.UTF8, "application/json");
                }

                using HttpResponseMessage response = Client.Send(request);
                string body = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    if (userAgent != UserAgentFallbacks[0])
                    {
                        Console.WriteLine($"Request succeeded after User-Agent fallback ({userAgent}) for {url}");
                    }
                    return body;
                }

                lastError = new Exception(
                    $"{response.RequestMessage?.RequestUri} returned: Status Code \"{response.StatusCode}\" with User-Agent \"{userAgent}\" and Reason \"{Truncate(body)}\"");

                if (response.StatusCode == HttpStatusCode.Forbidden && userAgent != UserAgentFallbacks[^1])
                {
                    Console.WriteLine($"Warning: {url} returned 403 with User-Agent '{userAgent}'. Retrying with a different User-Agent.");
                    continue;
                }

                throw lastError;
            }

            throw lastError ?? new Exception($"{url} failed with no response.");
        }

        private static void ApplyHeaders(HttpRequestMessage request, NameValueCollection? headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (string? key in headers.AllKeys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                request.Headers.TryAddWithoutValidation(key, headers[key]);
            }
        }

        private static string Truncate(string? text, int maxLength = 400)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            string flattened = string.Join(' ', text.Split(default(char[]), StringSplitOptions.RemoveEmptyEntries));
            return flattened.Length <= maxLength ? flattened : flattened[..maxLength] + "...";
        }
    }
}
