/*
    Lock object for async method
    https://www.hanselman.com/blog/comparing-two-techniques-in-net-asynchronous-coordination-primitives
*/

using System;
using System.Threading;
using System.Threading.Tasks;

namespace SZ2.WebSocketGaugeServer.WebSocketCommon.Utils
{
    public sealed class AsyncSemaphoreLock
    {
        private readonly SemaphoreSlim m_semaphore
          = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> m_releaser;

        public AsyncSemaphoreLock()
        {
            m_releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                    m_releaser :
                    wait.ContinueWith(
                      (_, state) => (IDisposable)state,
                      m_releaser.Result,
                      System.Threading.CancellationToken.None,
                      TaskContinuationOptions.ExecuteSynchronously,
                      TaskScheduler.Default
                    );
        }
        private sealed class Releaser : IDisposable
        {
            private readonly AsyncSemaphoreLock m_toRelease;
            internal Releaser(AsyncSemaphoreLock toRelease) { m_toRelease = toRelease; }
            public void Dispose() { m_toRelease.m_semaphore.Release(); }
        }
    }
}