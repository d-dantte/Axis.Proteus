using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;
using Axis.Luna.Extensions;
using Axis.Luna.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axis.Proteus.Metrics.Service
{
    public class MetricRecorder : IMetricRecorder
    {
        private Dictionary<string, CounterOptions> CounterOptions = new Dictionary<string, CounterOptions>();
        private Dictionary<string, TimerOptions> TimerOptions = new Dictionary<string, TimerOptions>();
        private IMetrics Metrics;

        public MetricRecorder(IMetrics metrics, IEnumerable<MetricValueOptionsBase> metricOptions = null)
        {
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            metricOptions?.ForAll(option =>
            {
                switch(option)
                {
                    case CounterOptions opt:
                        CounterOptions.Add(opt.Name, opt);
                        break;

                    case TimerOptions opt:
                        TimerOptions.Add(opt.Name, opt);
                        break;

                    // ignore other options
                    default: break;
                }
            });
        }

        public MetricRecorder(IMetrics metrics, params MetricValueOptionsBase[] metricOptions)
            : this(metrics, metricOptions as IEnumerable<MetricValueOptionsBase>)
        { }

        public Operation RecordCount(string eventName, long count) => Operation.Try(() =>
        {
        });

        public Operation RecordDuration(string eventName, long duration, Units.Duration unit) => Operation.Try(() =>
        {
            var option = TimerOptions.GetOrAdd(eventName, name => new TimerOptions
            {
                DurationUnit = ToTimeUnit(unit),
                //Context = "",
                Name = eventName,
                MeasurementUnit = Unit.Calls,
                RateUnit = ToTimeUnit(unit),
                //ResetOnReporting = true
            });

            Metrics.Measure.Timer.Time(option, duration);
        });

        private static TimeUnit ToTimeUnit(Units.Duration unit)
        {
            return unit switch
            {
                Units.Duration.Days => TimeUnit.Days,
                Units.Duration.Hours => TimeUnit.Hours,
                Units.Duration.Microseconds => TimeUnit.Microseconds,
                Units.Duration.Milliseconds => TimeUnit.Milliseconds,
                Units.Duration.Minutes => TimeUnit.Minutes,
                Units.Duration.Nanoseconds => TimeUnit.Nanoseconds,
                Units.Duration.Seconds => TimeUnit.Seconds,
                _ => throw new ArgumentException("Invalid unit: " + unit)
            };
        }
    }
}
