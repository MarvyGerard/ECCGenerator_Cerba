using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GenerateECCDocument
{
	public class ApiHelper
	{
		public static HttpClient ApiClient { get; set; } = new HttpClient();
		public static void InitializeClient(string BaseAddress, string ApplicationUnderTest, string PATToken,
            bool enableProxy)
		{
			try
			{
                WebProxy proxy = null;
                if (enableProxy)
                {
                    proxy = new WebProxy
                    {
                        Address = new Uri($"http://bel01spprox01.barcapp.org:3128"),
                    };
                }

                ServicePointManager.Expect100Continue = false;
				ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				HttpClientHandler clientHandler = new HttpClientHandler()
				{
					AllowAutoRedirect = true,
					AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
					Proxy = proxy
				};

				ApiClient = new HttpClient(clientHandler);

				ApiClient.BaseAddress = new Uri($"{BaseAddress}/{ApplicationUnderTest}/_apis/");

				ApiClient.DefaultRequestHeaders.Accept.Clear();
				ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
					Convert.ToBase64String(
						System.Text.ASCIIEncoding.ASCII.GetBytes(
							string.Format("{0}:{1}", "", PATToken))));

				//ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", PATToken);
				ApiClient.DefaultRequestHeaders.Add("api-version", "6.0");
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

	}
}
