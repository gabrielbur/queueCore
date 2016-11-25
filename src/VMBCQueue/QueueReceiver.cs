using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMBCQueue.Interfaces;

namespace VMBCQueue
{
    public class QueueReceiver<T>:IDisposable where T:class
    {
        private List<IQueue<T>> _queues;
        private Dictionary<string, IMessageHandler<T>> _handlers;

        public bool AddQueue(IQueue<T> queue, IMessageHandler<T> handler)
        {
            if (_queues.Any(q => q.Name == queue.Name))
                return false;
            _queues.Add(queue);
            _handlers.Add(queue.Name, handler);
            queue.AddHandler(handler);
            return true;
        }

        public void Dispose()
        {
            
        }
    }
}
