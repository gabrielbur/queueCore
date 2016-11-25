using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMBCQueue.Interfaces;

namespace VMBCQueue.Interfaces
{
    public interface IQueue<T>:IDisposable where T : class
    {
        string Name { get; set; }
        void AddHandler(IMessageHandler<T> handler);

        bool Push(T message);
        T Pop();
        List<T> List();
        int Count();
        T Top(); 
           
    }
}
