using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMBCQueue.Interfaces
{
    public interface IMessageHandler<T>:IDisposable  where T : class
    {
        T ReceiveMessage(string message);

    }
}
