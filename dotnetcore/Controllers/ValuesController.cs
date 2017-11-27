using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace dotnetcore.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private static readonly HttpClient Client = new HttpClient();
        private readonly IConfiguration _configuration;

        public ValuesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new [] { "StorageConnectionString", _configuration["StorageConnectionString"]?.Length.ToString() };
        }

        // GET api/values/5
        [HttpGet("{resource}")]
        public object Get(string resource)
        {
            if (resource == "httprequest")
                return ReadUrl();

            return _configuration.AsEnumerable().OrderBy(k => k.Key).Select(k => k.Key);
        }

        private async Task<string> ReadUrl()
        {
            var data = "";
            try
            {
                using (var s = await Client.GetStreamAsync("https://5564qhte39.execute-api.eu-west-1.amazonaws.com/prod/circuits"))
                using (var sr = new StreamReader(s))
                {
                    data = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                data = ex.Message;
            }
            return data;
        }
    }
}