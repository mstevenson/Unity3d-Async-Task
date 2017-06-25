using System;
using System.Collections;

namespace Foundation.Tasks
{
    /// <summary>
    /// A task encapsulates future work that may be waited on.
    /// - Support running actions in background threads 
    /// - Supports running coroutines with return results
    /// - Use the WaitForRoutine method to wait for the task in a coroutine
    /// </summary>
    /// <example>
    /// <code>
    ///     var task = Task.Run(() =>
    ///     {
    ///        //Debug.Log does not work in
    ///        Debug.Log("Sleeping...");
    ///        Task.Delay(2000);
    ///        Debug.Log("Slept");
    ///    });
    ///    // wait for it
    ///    yield return StartCoroutine(task.WaitRoutine());
    ///
    ///    // check exceptions
    ///    if(task.IsFaulted)
    ///        Debug.LogException(task.Exception)
    ///</code>
    ///</example>
    public partial class UnityTask
    {
        #region Task
        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask Run(Action action)
        {
            var task = new UnityTask(action);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnMain(Action action)
        {
            var task = new UnityTask(action, TaskStrategy.MainThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnCurrent(Action action)
        {
            var task = new UnityTask(action, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask Run<TParam>(Action<TParam> action, TParam param)
        {
            var task = new UnityTask(action, param, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnMain<TParam>(Action<TParam> action, TParam param)
        {
            var task = new UnityTask(action, param, TaskStrategy.MainThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunOnCurrent<TParam>(Action<TParam> action, TParam param)
        {
            var task = new UnityTask(action, param, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        #endregion
        
        #region Coroutine

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunCoroutine(IEnumerator function)
        {
            var task = new UnityTask(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask RunCoroutine(Func<IEnumerator> function)
        {
            var task = new UnityTask(function());
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task which passes the task as a parameter
        /// </summary>
        public static UnityTask RunCoroutine(Func<UnityTask, IEnumerator> function)
        {
            var task = new UnityTask();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task which passes the task as a parameter, and one additional parameter
        /// </summary>
        public static UnityTask RunCoroutine<TParam>(Func<UnityTask, TParam, IEnumerator> function, TParam param)
        {
            var task = new UnityTask();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task, param);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task which passes the task as a parameter, and two additional parameters
        /// </summary>
        public static UnityTask RunCoroutine<TParam1, TParam2>(Func<UnityTask, TParam1, TParam2, IEnumerator> function, TParam1 param1, TParam2 param2)
        {
            var task = new UnityTask();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task, param1, param2);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task which passes the task as a parameter, and three additional parameters
        /// </summary>
        public static UnityTask RunCoroutine<TParam1, TParam2, TParam3>(Func<UnityTask, TParam1, TParam2, TParam3, IEnumerator> function, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            var task = new UnityTask();
            task.Strategy = TaskStrategy.Coroutine;
            task._routine = function(task, param1, param2, param3);
            task.Start();
            return task;
        }
        #endregion

        #region Task With Result
        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> Run<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> Run<TParam, TResult>(Func<TParam, TResult> function, TParam param)
        {
            var task = new UnityTask<TResult>(function, param);
            task.Start();
            return task;
        }
        /// <summary>
        /// Creates a new running task
        /// </summary>
        public static UnityTask<TResult> RunOnMain<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function, TaskStrategy.MainThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task on the main thread that returns a result
        /// </summary>
        public static UnityTask<TResult> RunOnMain<TParam, TResult>(Func<TParam, TResult> function, TParam param)
        {
            var task = new UnityTask<TResult>(function, param, TaskStrategy.MainThread);
            task.Start();
            return task;
        } 
        
        /// <summary>
        /// Creates a new running task on the current thread that returns a result
        /// </summary>
        public static UnityTask<TResult> RunOnCurrent<TResult>(Func<TResult> function)
        {
            var task = new UnityTask<TResult>(function, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task on the current thread that returns a result
        /// </summary>
        public static UnityTask<TResult> RunOnCurrent<TParam, TResult>(Func<TParam, TResult> function, TParam param)
        {
            var task = new UnityTask<TResult>(function, param, TaskStrategy.CurrentThread);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a new running task that returns a result
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TResult>(IEnumerator function)
        {
            var task = new UnityTask<TResult>(function);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a task which returns a result and passes the task as a parameter
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TResult>(Func<UnityTask<TResult>, IEnumerator> function)
        {
            var task = new UnityTask<TResult>();
            task.Strategy = TaskStrategy.Coroutine;
            task.Paramater = task;
            task._routine = function(task);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a task which returns a result, passes the task as a parameter, and passes one additional parameter
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TParam, TResult>(Func<UnityTask<TResult>, TParam, IEnumerator> function, TParam param)
        {
            var task = new UnityTask<TResult>();
            task.Strategy = TaskStrategy.Coroutine;
            task.Paramater = task;
            task._routine = function(task, param);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a task which returns a result, passes the task as a parameter, and passes two additional parameters
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TParam1, TParam2, TResult>(Func<UnityTask<TResult>, TParam1, TParam2, IEnumerator> function, TParam1 param1, TParam2 param2)
        {
            var task = new UnityTask<TResult>();
            task.Strategy = TaskStrategy.Coroutine;
            task.Paramater = task;
            task._routine = function(task, param1, param2);
            task.Start();
            return task;
        }

        /// <summary>
        /// Creates a task which returns a result, passes the task as a parameter, and passes three additional parameters
        /// </summary>
        public static UnityTask<TResult> RunCoroutine<TParam1, TParam2, TParam3, TResult>(Func<UnityTask<TResult>, TParam1, TParam2, TParam3, IEnumerator> function, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            var task = new UnityTask<TResult>();
            task.Strategy = TaskStrategy.Coroutine;
            task.Paramater = task;
            task._routine = function(task, param1, param2, param3);
            task.Start();
            return task;
        }
        #endregion

        #region success / fails

        /// <summary>
        /// A default task in the success state
        /// </summary>
        static UnityTask _successTask = new UnityTask(TaskStrategy.Custom) { Status = TaskStatus.Success };

        /// <summary>
        /// A default task in the success state
        /// </summary>
        public static UnityTask<T> SuccessTask<T>(T result)
        {
            return new UnityTask<T>(TaskStrategy.Custom) { Status = TaskStatus.Success, Result = result };
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask SuccessTask()
        {
            return _successTask;
        }


        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask FailedTask(string exception)
        {
            return FailedTask(new Exception(exception));
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask FailedTask(Exception ex)
        {
            return new UnityTask(TaskStrategy.Custom) { Status = TaskStatus.Faulted, Exception = ex };
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(string exception)
        {
            return FailedTask<T>(new Exception(exception));
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(Exception ex)
        {
            return new UnityTask<T>(TaskStrategy.Custom) { Status = TaskStatus.Faulted, Exception = ex };
        }


        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(string exception, T result)
        {
            return FailedTask(new Exception(exception), result);
        }

        /// <summary>
        /// A default task in the faulted state
        /// </summary>
        public static UnityTask<T> FailedTask<T>(Exception ex, T result)
        {
            return new UnityTask<T>(TaskStrategy.Custom) { Status = TaskStatus.Faulted, Exception = ex, Result = result };
        }
        #endregion

    }
}
