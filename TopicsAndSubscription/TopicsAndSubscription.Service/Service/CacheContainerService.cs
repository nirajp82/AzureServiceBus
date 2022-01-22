using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TopicsAndSubscription.Service
{
    public class CacheContainerService : ICacheContainerService
    {
        static object _cacheData = new { RefreshDate = DateTime.UtcNow.ToString(), ProcessId = Process.GetCurrentProcess().Id, Info = "Default" };

        public void RefreshCache(string cacheRefreshRequest)
        {
            _cacheData = new
            {
                RefreshDate = DateTime.UtcNow.ToString(),
                ProcessId = Process.GetCurrentProcess().Id,
                Info = cacheRefreshRequest
            };
        }

        public object GetCachedData() => _cacheData;
    }
}
