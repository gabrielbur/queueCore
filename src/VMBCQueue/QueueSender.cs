using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMBCQueue.Interfaces;

namespace VMBCQueue
{
    public class QueueSender<T> where T:class
    {
        IQueue<T> _queue;
        public QueueSender(IQueue<T> queue)
        {
            _queue = queue;
        }
        public bool Send(T message)
        {
            return _queue.Push(message);
        }
    }
}
