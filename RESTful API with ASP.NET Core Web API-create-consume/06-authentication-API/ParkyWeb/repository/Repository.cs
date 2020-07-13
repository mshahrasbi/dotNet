using Newtonsoft.Json;
using ParkyWeb.repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ParkyWeb.repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IHttpClientFactory _clientFactory;

        public Repository(IHttpClientFactory clientFactory)
        {
            this._clientFactory = clientFactory;
        }

        public async Task<bool> CreateAsync(string url, T objToCreate, string token = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            if (objToCreate != null)
            {
                request.Content = new StringContent(
                        JsonConvert.SerializeObject(objToCreate), 
                        encoding: Encoding.UTF8, 
                        "application/json");
            } else
            {
                return false;
            }

            var client = this._clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token) && token.Length != 0)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string url, int Id, string token = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url + Id);

            var client = this._clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token) && token.Length != 0)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string url, string token = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var client = this._clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token) && token.Length != 0)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
            }

            return null;
        }

        public async Task<T> GetAsync(string url, int Id, string token = "")
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url + Id);

            var client = this._clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token) && token.Length != 0)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonString);
            }

            return null;
        }

        public async Task<bool> updateAsync(string url, T objToUpdate, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);

            if (objToUpdate != null)
            {
                request.Content = new StringContent(
                        JsonConvert.SerializeObject(objToUpdate),
                        encoding: Encoding.UTF8,
                        "application/json");
            }
            else
            {
                return false;
            }

            var client = this._clientFactory.CreateClient();
            if (!string.IsNullOrEmpty(token) && token.Length != 0)
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
