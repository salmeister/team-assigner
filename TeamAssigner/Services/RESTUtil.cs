namespace TeamAssigner.Services
{
    using System;
    using System.Collections.Specialized;
    using System.Net.Http.Headers;
    using System.Net;
    using System.Text;

    internal class RESTUtil
    {
        public static string Get(NameValueCollection headers, string url)
        {
            HttpClientHandler handler = new();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            using HttpClient client = new(handler);
            client.DefaultRequestHeaders.Accept.Add(new
            MediaTypeWithQualityHeaderValue("application/json"));
            if (headers != null)
            {
                foreach (string header in headers.Keys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }
            }
            var data = client.GetAsync(url).Result;
            if (data.IsSuccessStatusCode)
            {
                return data.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Exception ex = new($"{data.RequestMessage?.RequestUri} returned: Status Code \"{data.StatusCode}\" with Reason \"{data.Content.ReadAsStringAsync().Result}\"");
                throw ex;
            }
        }

        public static string Put(NameValueCollection headers, string url, string package)
        {
            HttpClientHandler handler = new();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            using var client = new HttpClient(handler);
            client.BaseAddress = new Uri(url.TrimEnd('/'));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (headers != null)
            {
                foreach (string header in headers.Keys)
                {
                    client.DefaultRequestHeaders.Add(header, headers[header]);
                }
            }
            HttpResponseMessage response = client.PutAsync(url.TrimEnd('/'), new StringContent(package, Encoding.UTF8, "application/json")).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                Exception ex = new($"{response.RequestMessage.RequestUri} returned: Status Code \"{response.StatusCode}\" with Content \"{response.Content.ReadAsStringAsync().Result}\"");
                throw ex;
            }
        }
    }
}
