using Axis.Luna.Operation;

namespace Axis.Proteus.Metrics.Service
{
    /// <summary>
    /// Service that facilitates recording metrics
    /// </summary>
    public interface IMetricRecorder
    {
        /// <summary>
        /// Record the duration of an event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="duration">The event duration</param>
        /// <param name="unit">Duration unit, e.g: milliseconds, seconds, etc</param>
        /// <returns>An <see cref="Operation"/> object encapsulating the recording process. Recommended that it is <c>asynchronious</c></returns>
        Operation RecordDuration(string eventName, long duration, Units.Duration unit);

        /// <summary>
        /// Record the occurence count of an event
        /// </summary>
        /// <param name="eventName">The event name</param>
        /// <param name="count">Number of times the event occured</param>
        /// <returns>An <see cref="Operation"/> object encapsulating the recording process. Recommended that it is <c>asynchronious</c></returns>
        Operation RecordCount(string eventName, long count);
    }
}
