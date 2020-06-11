using ParkyWeb.Models;
using ParkyWeb.repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ParkyWeb.repository
{
    public class TrailRepository: Repository<Trail>, ITrailRepository
    {
        private IHttpClientFactory _clientFactory;

        public TrailRepository(IHttpClientFactory clientFactory): base(clientFactory)
        {
            this._clientFactory = clientFactory;
        }
    }
}
