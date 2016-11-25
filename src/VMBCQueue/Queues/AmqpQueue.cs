using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VMBCQueue.Interfaces;
using Amqp;
using Amqp.Framing;
using Amqp.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VMBCQueue.Queues
{
    public class AmqpQueue<T>: IQueue<T> where T: class
    {
        private string _connectionString;
        private ILogger _logger;
        private SenderLink _sender;
        private Session _amqpSession;
        private Connection _connection;
        private ReceiverLink _consumer;
        public string Name { get; set; }

        public AmqpQueue(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            Trace.WriteLine(Amqp.TraceLevel.Information, "Establishing a connection...");
            // Create the AMQP connection
            _connection = new Connection(new Address(_connectionString));

            Trace.WriteLine(Amqp.TraceLevel.Information, "Creating a session...");
            // Create the AMQP session
            _amqpSession = new Session(_connection);

            // Give a name to the sender
            var senderSubscriptionId = "devqueue.amqp.sender";
            // Give a name to the receiver
            var receiverSubscriptionId = "devqueue.amqp.receiver";

            // Name of the topic you will be sending messages
            var topic = "devqueue";

            // Name of the subscription you will receive messages from
            //var subscription = "codeityourself.listener";

            Trace.WriteLine(Amqp.TraceLevel.Information, "Creating a sender link...");
            // Create the AMQP sender
            _sender = new SenderLink(_amqpSession, senderSubscriptionId, topic);

            Trace.WriteLine(Amqp.TraceLevel.Information, "Creating a receiver link...");
            _consumer = new ReceiverLink(_amqpSession, receiverSubscriptionId, $"{topic}");
            _consumer.Start(0, onMessage);

        }
        IMessageHandler<T> _handler;
        public void AddHandler(IMessageHandler<T> handler)
        {
            _handler = handler;
        }
        private void onMessage(ReceiverLink receiver, Message message)
        {
            try
            {

                // Variable to save the body of the message.
                string body = string.Empty;

                // Get the body
                var rawBody = message.GetBody<object>();

                // If the body is byte[] assume it was sent as a BrokeredMessage  
                // and deserialize it using a XmlDictionaryReader
                if (rawBody is byte[])
                {
                    using (var reader = XmlDictionaryReader.CreateBinaryReader(
                        new MemoryStream(rawBody as byte[]),
                        null,
                        XmlDictionaryReaderQuotas.Max))
                    {
                        var doc = new XmlDocument();
                        doc.Load(reader);
                        body = doc.InnerText;
                    }
                }
                else // Asume the body is a string
                {
                    body = rawBody.ToString();
                }

                // Write the body to the Console.
                Console.WriteLine(body);

                // Accept the messsage.
                receiver.Accept(message);
                if (_handler!=null)
                    _handler.ReceiveMessage(body);
            }
            catch (Exception ex)
            {
                receiver.Reject(message);
                Console.WriteLine(ex);
            }

        }




        public int Count()
        {
            throw new NotImplementedException();
        }

        public List<T> List()
        {
            throw new NotImplementedException();
        }

        private T Receive(bool acceptance)
        {
            var msg = _consumer.Receive();
            try
            {

                // Variable to save the body of the message.
                string body = string.Empty;

                // Get the body
                var rawBody = msg.GetBody<object>();

                // If the body is byte[] assume it was sent as a BrokeredMessage  
                // and deserialize it using a XmlDictionaryReader
                if (rawBody is byte[])
                {
                    //using (var reader = XmlDictionaryReader.CreateBinaryReader(
                    //    new MemoryStream(rawBody as byte[]),
                    //    null,
                    //    XmlDictionaryReaderQuotas.Max))
                    //{
                    //    var doc = new XmlDocument();
                    //    doc.Load(reader);
                    //    body = doc.InnerText;
                    //}
                }
                else // Asume the body is a string
                {
                    body = rawBody.ToString();
                }

                // Write the body to the Console.
                Console.WriteLine(body);
                if(acceptance)
                    // Accept the messsage.
                    _consumer.Accept(msg);
                else
                    _consumer.Reject(msg);
            }
            catch (Exception ex)
            {
                _consumer.Reject(msg);
                Console.WriteLine(ex);
            }


            return null;
        }

        public T Pop()
        {
            return Receive(true);
        }
        public T Top()
        {
            return Receive(false);
        }
        public bool Push(T message)
        {
            try
            {
                // Create message
                var _message = new Message(message);

                // Add a meesage id
                _message.Properties = new Properties() { MessageId = Guid.NewGuid().ToString() };

                // Add some message properties
                _message.ApplicationProperties = new ApplicationProperties();
                _message.ApplicationProperties["Message.Type.FullName"] = typeof(string).FullName;

                // Send message
                _sender.Send(_message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
          
        }

       

        public void Dispose()
        {
            Trace.WriteLine(Amqp.TraceLevel.Information, "Closing sender...");

            _sender.Close();
            _consumer.Close();
            _amqpSession.Close();
            _connection.Close();
        }
    }
}
