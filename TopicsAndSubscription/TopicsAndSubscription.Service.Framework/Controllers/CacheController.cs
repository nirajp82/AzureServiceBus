using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TopicsAndSubscription.Service.Framework.Service;

namespace TopicsAndSubscription.Service.Framework.Controllers
{
    public class CacheController : ApiController
    {
        // GET api/values
        public object Get()
        {
            return CacheContainerService.GetCachedData();
        }
    }
}
