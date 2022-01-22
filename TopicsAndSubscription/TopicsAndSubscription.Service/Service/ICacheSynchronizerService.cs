using System;
using System.Threading.Tasks;

namespace TopicsAndSubscription.Service
{
    public interface ICacheSynchronizerService : IDisposable
    {
        Task StartAsync();
        Task StopAsync();
    }
}