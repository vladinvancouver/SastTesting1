using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Company.DataAccess
{

    public class Benchmark : IDisposable
    {
        private readonly ILogger _logger;
        private string _message = String.Empty;
        private int _thresholdInMilliseconds = 0;
        private Stopwatch _stopwatch;

        public Benchmark(ILogger logger, string message) : this(logger, message, thresholdInMilliseconds: 0)
        {

        }

        public Benchmark(ILogger logger, string message, int thresholdInMilliseconds)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _message = message;
            _thresholdInMilliseconds = thresholdInMilliseconds;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            if (_thresholdInMilliseconds <= 0)
            {
                return;
            }

            if (_stopwatch.ElapsedMilliseconds >= _thresholdInMilliseconds)
            {
                _logger.LogWarning($"{_message}; Duration {_stopwatch.ElapsedMilliseconds}ms.");
            }
        }
    }
}
