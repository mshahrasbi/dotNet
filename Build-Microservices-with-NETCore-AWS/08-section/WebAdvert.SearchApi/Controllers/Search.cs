using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAdvert.SearchApi.Models;
using WebAdvert.SearchApi.Services;

namespace WebAdvert.SearchApi.Controllers
{
    [Route("search/v1")]
    [ApiController]
    [Produces("application/json")]
    public class Search : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<Search> _logger;

        public Search(ISearchService searchService, ILogger<Search> logger)
        {
            this._searchService = searchService;
            this._logger = logger;
        }

        [HttpGet]
        [Route("{keyword}")]
        public async Task<List<AdvertType>> Get(string keyword)
        {
            this._logger.LogInformation("Search method was called");
            return await _searchService.Search(keyword);
        }
    }
}
