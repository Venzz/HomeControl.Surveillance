using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Venz.UI.Xaml
{
    public sealed class ApplicationDispatcher
    {
        public TaskFactory TaskFactory { get; }
        public Boolean IsInContext => Window.Current?.Dispatcher?.HasThreadAccess == true;



        internal ApplicationDispatcher()
        {
            TaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task RunAsync(Action action)
        {
            if (!IsInContext)
            {
                if (TaskFactory != null)
                    await TaskFactory.StartNew(action).ConfigureAwait(false);
                else
                    throw new InvalidOperationException("Synchronization Context is not captured.");
            }
            else
            {
                action.Invoke();
            }
        }

        public async Task<TResult> RunAsync<TResult>(Func<TResult> actionWithResult)
        {
            if (!IsInContext)
            {
                if (TaskFactory != null)
                    return await TaskFactory.StartNew<TResult>(actionWithResult).ConfigureAwait(false);
                else
                    throw new InvalidOperationException("Synchronization Context is not captured.");
            }
            else
            {
                return actionWithResult.Invoke();
            }
        }

        public async Task RunAsync(Func<Task> asyncAction)
        {
            if (!IsInContext)
            {
                if (TaskFactory != null)
                {
                    var actionProxy = await TaskFactory.StartNew<Task>(asyncAction);
                    await actionProxy.ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException("Synchronization Context is not captured.");
                }
            }
            else
            {
                await asyncAction.Invoke().ConfigureAwait(false);
            }
        }

        public async Task<TResult> RunAsync<TResult>(Func<Task<TResult>> asyncActionWithResult)
        {
            if (!IsInContext)
            {
                if (TaskFactory != null)
                {
                    var actionProxy = await TaskFactory.StartNew<Task<TResult>>(asyncActionWithResult);
                    return await actionProxy.ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException("Synchronization Context is not captured.");
                }
            }
            else
            {
                return await asyncActionWithResult.Invoke().ConfigureAwait(false);
            }
        }
    }
}
