using System;

namespace Axis.Proteus.Metrics.EventAttributes
{
    /// <summary>
    /// Attribute used in decorating methods for which their execution duration is to be recorded via interception
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RecordDurationAttribute: Attribute
    {
        /// <summary>
        /// The unit of measurement for the duration, e.g: milliseconds, nanoseconds, days, etc.
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// The name given to the event represented by the method being recorded
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Creates a new Attribute
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="unit">The unit; defaults to "ms" or milliseconds</param>
        public RecordDurationAttribute(string eventName, string unit = "ms")
        {
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }
    }
}
