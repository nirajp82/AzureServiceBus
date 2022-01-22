﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TopicsAndSubscription.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheController : ControllerBase
    {

        private readonly ILogger<CacheController> _logger;
        private readonly ICacheContainerService _cacheContainerService;
        private readonly ICacheSynchronizerService _cacheSynchronizerService;

        public CacheController(ILogger<CacheController> logger, ICacheContainerService cacheContainerService, ICacheSynchronizerService cacheSynchronizerService)
        {
            _logger = logger;
            _cacheContainerService = cacheContainerService;
            _cacheSynchronizerService = cacheSynchronizerService;
        }

        [HttpGet]
        public object Get()
        {
            return _cacheContainerService.GetCachedData();
        }
    }
}
