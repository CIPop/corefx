// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics.Tracing
{
    public sealed class ConsoleEventListener : EventListener
    {
        private readonly string [] _eventFilters;
        private object _lock = new object();

        public ConsoleEventListener() : this(string.Empty) { }

        public ConsoleEventListener(string filter)
        {
            _eventFilters = new string[1];

            _eventFilters[0] = filter ?? throw new ArgumentNullException(nameof(filter));

            foreach (EventSource source in EventSource.GetSources())
                EnableEvents(source, EventLevel.LogAlways);
        }

        public ConsoleEventListener(string [] filters)
        {
            if (filters.Length == 0) throw new ArgumentException("Filters cannot be empty");
            foreach (string filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter))
                {
                    throw new ArgumentNullException(nameof(filters));
                }
            }

            _eventFilters = filters;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
#if NET451
            EnableEvents(eventSource, EventLevel.LogAlways);
#else
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
#endif
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            lock (_lock)
            {
                string text = $"[{eventData.EventSource.Name}-{eventData.EventId}]{(eventData.Payload != null ? $" ({string.Join(", ", eventData.Payload)})." : "")}";

                bool shouldDisplay = false;

                if (_eventFilters?.Length == 1 && text.Contains(_eventFilters[0]))
                {
                    shouldDisplay = true;
                }
                else
                {
                    foreach (string filter in _eventFilters)
                    {
                        if (_eventFilters != null && text.Contains(filter))
                        {
                            shouldDisplay = true;
                        }
                    }
                }

                if (shouldDisplay)
                {
                    ConsoleColor origForeground = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(text);
                    Debug.WriteLine(text);
                    Console.ForegroundColor = origForeground;
                }
            }
        }
    }
}
