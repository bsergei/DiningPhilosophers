using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DiningPhilosophers.Utils
{
    public static class SourceBlockExtensions
    {
        /// <summary>
        /// Returns null when timeout happened.
        /// </summary>
        public static async Task<bool?> OutputAvailableAsync<T>(
            this ISourceBlock<T> sourceBlock, 
            int timeoutMilliseconds)
        {
            using (var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMilliseconds)))
            {
                try
                {
                    return await sourceBlock.OutputAvailableAsync(timeoutTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }
    }
}