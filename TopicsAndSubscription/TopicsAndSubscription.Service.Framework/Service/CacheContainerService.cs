using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace TopicsAndSubscription.Service.Framework.Service
{
    public class CacheContainerService
    {
        static object _cacheData = new { RefreshDate = DateTime.UtcNow.ToString(), ProcessId = Process.GetCurrentProcess().Id, Info = "Default" };
        static int _cnt = 1;

        public static void RefreshCache(string cacheRefreshRequest)
        {
            _cacheData = new
            {
                RefreshDate = DateTime.UtcNow.ToString(),
                ProcessId = Process.GetCurrentProcess().Id,
                Info = cacheRefreshRequest,
                Counter = _cnt + 1
            };
        }

        public static object GetCachedData() => _cacheData;
    }
}