using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SendGridSharp.Core
{
    public class SendGridRetryPolicy
    {
        public int MaxCount { get; } = 3;

        public TimeSpan InitialDelay { get; } = TimeSpan.FromSeconds(5);

        public double Backoff { get; } = 2.0;


        public SendGridRetryPolicy()
        {
        }

        public SendGridRetryPolicy(int maxCount)
        {
            if (maxCount <= 0)
                throw new ArgumentNullException(nameof(maxCount));

            MaxCount = maxCount;
        }

        public SendGridRetryPolicy(int maxCount, TimeSpan initialDelay, double backoff)
        {
            if (maxCount <= 0)
                throw new ArgumentNullException(nameof(maxCount));

            if (initialDelay == null)
                throw new ArgumentNullException(nameof(initialDelay));

            if (backoff <= 0)
                throw new ArgumentNullException(nameof(backoff));

            MaxCount = maxCount;
            InitialDelay = initialDelay;
            Backoff = backoff;
        }

        internal TimeSpan CalcWaitTimeSpan(int currentStage)
        {
            // y = ax^2+b, a = Backoff, b = InitialDelay, x = currentStage
            return TimeSpan.FromMilliseconds(Backoff * (currentStage ^ 2) + InitialDelay.TotalMilliseconds);
        }

        internal bool IsRetryOver(int currentRetry)
        {
            return currentRetry > MaxCount - 1;
        }
    }
}
