using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TopicsAndSubscription.WebApp
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ICacheSynchronizerService _cacheSynchronizerService;

        public AdminController(ICacheSynchronizerService cacheSynchronizerService)
        {
            this._cacheSynchronizerService = cacheSynchronizerService;
        }

        [HttpPost]
        [Route("RefreshCache")]
        public async Task RefreshCache([FromBody] string clientName)
        {
            await _cacheSynchronizerService.SendSyncRequestAsync(clientName);
        }
    }
}
