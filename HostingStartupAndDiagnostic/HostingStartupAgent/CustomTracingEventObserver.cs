using System;
using System.Collections.Generic;
using System.Text;

namespace HostingStartupAgent
{
    class CustomTracingEventObserver : IObserver<KeyValuePair<string, object>>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(KeyValuePair<string, object> evnt)
        {
            Console.WriteLine($"Listener Received Event {evnt.Key} with payload {evnt.Value}");
        }
    }
}
