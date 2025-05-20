using Serilog.Core;
using Serilog.Events;
using System;

namespace WorkerServiceDemo.Helpers
{
    public class SourceEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("dateProperty", DateTime.Now));

            if (logEvent.Exception != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("logProperty",
                    logEvent.Exception.Message));
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("exProperty",
                logEvent.MessageTemplate.Text));

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("srcProperty",
                logEvent.Properties["RequestPath"].ToString()));
        }
    }
}
