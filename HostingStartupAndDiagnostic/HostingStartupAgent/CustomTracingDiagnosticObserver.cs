using System;
using System.Diagnostics;

namespace HostingStartupAgent
{
    class CustomTracingDiagnosticObserver : IObserver<DiagnosticListener>
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
            if (listener.Name == "CustomTracingDiagnostic")
            {
                var observer = new CustomTracingEventObserver();

                Predicate<string> predicate = (string eventName) =>
                {
                    return true;
                };

                listener.Subscribe(observer, predicate);
            }
        }
    }
}
