using Serilog.Core;
using Serilog.Events;

namespace WorkerServiceDemo.Helpers
{
    public class SourceEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            //write to the log event column
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SoURcE", "Source"));
        }
    }
}
