﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TopicsAndSubscription.Service
{
    public class CacheContainerService : ICacheContainerService
    {
        static object _cacheData = new { RefreshDate = DateTime.UtcNow.ToString(), ProcessId = Process.GetCurrentProcess().Id, Info = "Default" };
        static int _cnt = 1;

        public void RefreshCache(string cacheRefreshRequest)
        {
            _cacheData = new
            {
                RefreshDate = DateTime.UtcNow.ToString(),
                ProcessId = Process.GetCurrentProcess().Id,
                Info = cacheRefreshRequest,
                Counter = _cnt + 1
            };
        }

        public object GetCachedData() => _cacheData;
    }
}