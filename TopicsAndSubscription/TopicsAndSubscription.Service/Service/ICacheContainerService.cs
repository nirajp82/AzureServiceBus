namespace TopicsAndSubscription.Service
{
    public interface ICacheContainerService
    {
        object GetCachedData();
        void RefreshCache(string cacheRefreshRequest);
    }
}