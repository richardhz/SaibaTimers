using System;
using System.Threading;

namespace SaibaTimers
{
    public class RepeatableProcess
    {
        private Timer processTimer;
        private int _delaySeconds;
        private CancellationTokenSource _source;
        private CancellationToken _token;
        private Action _processToRun;
        private bool _canStart = true;
        private int? _executionHourOfTheDay;

        public DateTime LastRunDate { get; set; }

        public RepeatableProcess(int? delaySeconds, Action process, int? executionHourOfTheDay = null)
        {
            _executionHourOfTheDay = executionHourOfTheDay;
            if (_executionHourOfTheDay.HasValue)
            {
                _delaySeconds = 60_000;
            }
            else
            {
                _delaySeconds = delaySeconds.Value;
            }
            _processToRun = process;
            LastRunDate = DateTime.Now.AddDays(-1);

        }

        public void Start()
        {
            if (_canStart)
            {
                _canStart = false;
                _source = new CancellationTokenSource();
                _token = _source.Token;
                processTimer = new Timer(TimedProcess, _token, Timeout.Infinite, Timeout.Infinite);
                processTimer.Change(_delaySeconds, Timeout.Infinite);
            }
        }

        public void Stop()
        {
            _source.Cancel();
        }

        public void TimedProcess(object state)
        {

            CancellationToken ct = (CancellationToken)state;
            if (ct.IsCancellationRequested)
            {
                processTimer.Dispose();
                _canStart = true;
            }
            else
            {
                var executeProcessNow = true;

                if (_executionHourOfTheDay.HasValue)
                {
                    if (DateTime.Now.Hour != _executionHourOfTheDay || (LastRunDate.Date == DateTime.Now.Date))
                    {
                        executeProcessNow = false;
                    }
                }

                if (executeProcessNow)
                {
                    if (_executionHourOfTheDay.HasValue)
                    {
                        LastRunDate = DateTime.Now;
                    }
                    _processToRun.Invoke();
                }

                processTimer.Change(_delaySeconds, Timeout.Infinite);
            }
        }
    }
}
