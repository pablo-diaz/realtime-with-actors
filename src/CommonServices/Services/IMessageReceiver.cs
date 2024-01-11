using System;
using System.Threading.Tasks;

namespace Services
{
    public interface IMessageReceiver: IDisposable
    {
        Task StartReceivingMessages<T>(Func<T, Task> messageHandlerAsyncFn);
    }
}
