using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HostingStartupAgent
{
    class AspNetCoreTracingDiagnosticObserver : IObserver<DiagnosticListener>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == "Microsoft.AspNetCore")
            {
                var observer = new BeforeOnActionExecutingObserver();

                Predicate<string> predicate = (string eventName) =>
                {
                    Console.WriteLine(eventName);
                    return eventName == "Microsoft.AspNetCore.Mvc.BeforeOnActionExecuting";
                };

                listener.Subscribe(observer, predicate);
            }
        }
    }
}
