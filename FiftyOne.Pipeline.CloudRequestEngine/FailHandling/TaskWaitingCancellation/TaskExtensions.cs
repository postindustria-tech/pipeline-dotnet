using System;
using System.Threading;
using System.Threading.Tasks;

namespace FiftyOne.Pipeline.CloudRequestEngine.FailHandling.TaskWaitingCancellation
{
    /// <summary>
    /// Task extension using implementation from
    /// https://stackoverflow.com/a/73207811
    /// i.e.
    /// https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#cancelling-uncancellable-operations
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Wait for either task to finish,
        /// or the token to get tripped.
        /// </summary>
        /// <typeparam name="T">
        /// Result type of the initial task.
        /// </typeparam>
        /// <param name="task">
        /// Task to wait for result from.
        /// </param>
        /// <param name="cancellationToken">
        /// Token to cancel the waiting.
        /// </param>
        /// <returns>
        /// The result from initial task.
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Token was tripped before task finished.
        /// </exception>
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            // There's no way to dispose of the registration
            var delayTask = Task.Delay(-1, cancellationToken);

            var resultTask = await Task.WhenAny(task, delayTask);
            if (resultTask == delayTask)
            {
                // Operation cancelled
                throw new OperationCanceledException();
            }

            return await task;
        }
    }
}