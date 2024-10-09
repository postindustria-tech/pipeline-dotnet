using FiftyOne.Pipeline.CloudRequestEngine.FailHandling.Throttling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling
{
    internal class LazyFailable<T>
    {
        public readonly T FallbackValue;
        public event Action<Exception> OnMainFuncException;

        private readonly Func<T> _mainFunc;
        private readonly IFailThrottlingStrategy _failThrottlingStrategy;

        private readonly object _taskLock = new object();
        private Task<T> _activeTask;
        private T _result;
        private bool _hasResult;

        public LazyFailable(
            Func<T> mainFunc, 
            T fallbackValue,
            IFailThrottlingStrategy failThrottlingStrategy)
        {
            if (mainFunc is null)
            {
                throw new ArgumentNullException(nameof(mainFunc));
            }
            if (failThrottlingStrategy is null)
            {
                throw new ArgumentNullException(nameof(failThrottlingStrategy));
            }
            _mainFunc = mainFunc;
            FallbackValue = fallbackValue;
            _failThrottlingStrategy = failThrottlingStrategy;
        }

        public async Task<T> GetValueAsync(CancellationToken token)
        {
            if (_hasResult)
            {
                return _result;
            }
            if (GetOrBuildActiveTask() is Task<T> activeTask)
            {
                if (activeTask.IsCompleted)
                {
                    return activeTask.Result;
                }
                return await TaskWithCancellation(activeTask, token);
            }
            return FallbackValue;
        }

        private Task<T> GetOrBuildActiveTask()
        {
            lock (_taskLock)
            {
                if (_activeTask is object)
                {
                    return _activeTask;
                }
                if (!_failThrottlingStrategy.MayTryNow())
                {
                    return null;
                }
                _activeTask = Task.Run(TryGetNewValue);
                return _activeTask;
            }
        }

        private T TryGetNewValue()
        {
            try
            {
                _result = _mainFunc();
                _hasResult = true;
                return _result;
            }
            catch (Exception e)
            {
                _failThrottlingStrategy.RecordFailure();
                lock (_taskLock)
                {
                    _activeTask = null;
                }
                try
                {
                    OnMainFuncException?.Invoke(e);
                }
                catch
                {
                    // nop -- do not report errors on reporting callback
                }
                throw;
            }
        }

        // modified from
        // https://stackoverflow.com/a/73207811
        // i.e.
        // https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#cancelling-uncancellable-operations
        private static async Task<R> TaskWithCancellation<R>(Task<R> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);

            // This disposes the registration as soon as one of the tasks trigger
            using (cancellationToken.Register(state =>
            {
                ((TaskCompletionSource<object>)state).TrySetResult(null);
            },
            tcs))
            {
                var resultTask = await Task.WhenAny(task, tcs.Task);
                if (resultTask == tcs.Task)
                {
                    // Operation cancelled
                    throw new OperationCanceledException(cancellationToken);
                }

                return await task;
            }
        }
    }
}
