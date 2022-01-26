using System;
using System.Threading.Tasks;

namespace TopicsAndSubscription.WebApp
{
    public interface ICacheSynchronizerService: IDisposable
    {
        Task CreateTopicAsync();
        Task SendSyncRequestAsync(string clientName);
    }
}