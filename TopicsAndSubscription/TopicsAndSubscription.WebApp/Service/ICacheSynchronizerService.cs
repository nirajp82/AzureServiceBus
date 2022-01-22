using System;
using System.Threading.Tasks;

namespace TopicsAndSubscription.WebApp
{
    public interface ICacheSynchronizerService: IDisposable
    {
        Task SendSyncRequestAsync(string clientName);
    }
}