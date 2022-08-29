using System;

namespace Axis.Proteus.Metrics.EventAttributes
{
    /// <summary>
    /// Attribute used in decorating methods for which their execution call-count is to be recorded via interception
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RecordCountAttribute: Attribute
    {
        /// <summary>
        /// The default value to record for each call
        /// </summary>
        public long DefaultValue { get; }

        /// <summary>
        /// The name given to the event represented by the method being recorded
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Creates a new Attribute
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="unit">The default value; defaults to 1/param>
        public RecordCountAttribute(string eventName, long defaultValue = 1)
        {
            DefaultValue = defaultValue;
            EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }
    }
}
