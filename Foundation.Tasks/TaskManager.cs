using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Foundation.Tasks
{
    /// <summary>
    /// Manager for running coroutines and scheduling actions to runs in the main thread.
    /// </summary>
    /// <remarks>
    /// Self instantiating. No need to add to scene.
    /// </remarks>
    [AddComponentMenu("Foundation/TaskManager")]
    [ExecuteInEditMode]
    public partial class TaskManager : MonoBehaviour
    {

        #region sub
        /// <summary>
        /// Thread Safe logger command
        /// </summary>
        public struct LogCommand
        {
            /// <summary>
            /// Color Code
            /// </summary>
            public LogType Type;
            /// <summary>
            /// Text
            /// </summary>
            public object Message;
        }

        /// <summary>
        /// Thread safe coroutine command
        /// </summary>
        public struct CoroutineCommand
        {
            /// <summary>
            /// The IEnumerator Coroutine
            /// </summary>
            public IEnumerator Coroutine;
            /// <summary>
            /// Called on complete
            /// </summary>
            public Action OnComplete;
        }
        #endregion
        
        /// <summary>
        /// Static Accessor
        /// </summary>
        public static TaskManager Instance
        {
            get
            {
                ConfirmInit();
                return _instance;
            }
        }

        /// <summary>
        /// Confirms the instance is ready for use
        /// </summary>
        public static void ConfirmInit()
        {
            if (_instance == null)
            {
                var old = FindObjectsOfType<TaskManager>();
                foreach (var manager in old)
                {
                    if (Application.isEditor)
                        DestroyImmediate(manager.gameObject);
                    else
                        Destroy(manager.gameObject);
                }


                var go = new GameObject("_TaskManager");
				if (Application.isPlaying) {
					DontDestroyOnLoad(go);
				}
                _instance = go.AddComponent<TaskManager>();

                MainThread = CurrentThread;
            }

        }

        /// <summary>
        /// Scheduled the routine to run (on the main thread)
        /// </summary>
        public static Coroutine WaitForSeconds(int seconds)
        {
            return Instance.StartCoroutine(Instance.WaitForSecondsInternal(seconds));
        }

        /// <summary>
        /// Scheduled the routine to run (on the main thread)
        /// </summary>
        public static Coroutine StartRoutine(IEnumerator coroutine)
        {
			UnityEngine.Assertions.Assert.IsFalse (UnityTask.DisableCoroutineYields, "TaskManager.StartRoutine is not compatible with UnityTask.DisableCoroutineYields");

            if (IsApplicationQuit)
                return null;

            //Make sure we are in the main thread
            if (!IsMainThread)
            {
                lock (syncRoot)
                {
                    PendingAdd.Add(coroutine);

                    //Debug.LogWarning("Running coroutines from background thread are not awaitable. Use CoroutineInfo");
                    return null;
                }
            }

            return Instance.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Scheduled the routine to run (on the main thread)
        /// </summary>
        public static void StartRoutine(CoroutineCommand info)
        {
            if (IsApplicationQuit)
                return;

            //Make sure we are in the main thread
            if (!IsMainThread)
            {
                lock (syncRoot)
                {
                    PendingCoroutineInfo.Add(info);
                }
            }
            else
            {
				if (UnityTask.DisableCoroutineYields) {
					RunCoroutineWithoutYields (Instance.RunCoroutineInfo(info));
				} else {
					Instance.StartCoroutine(Instance.RunCoroutineInfo(info));
				}
            }
        }

        /// <summary>
        /// Scheduled the routine to run (on the main thread)
        /// </summary>
        public static void StopRoutine(IEnumerator coroutine)
        {
            if (IsApplicationQuit)
                return;

            //Make sure we are in the main thread
            if (!IsMainThread)
            {
                lock (syncRoot)
                {
                    PendingRemove.Add(coroutine);
                }
            }
            else
            {
                Instance.StartCoroutine(coroutine);
            }
        }

        /// <summary>
        /// Schedules the action to run on the main thread
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnMainThread(Action action)
        {
            if (IsApplicationQuit)
                return;

            //Make sure we are in the main thread
            if (!IsMainThread)
            {
                lock (syncRoot)
                {
                    PendingActions.Add(action);
                }
            }
            else
            {
                action();
            }

        }

        /// <summary>
        /// A thread safe logger
        /// </summary>
        /// <param name="m"></param>
        public static void Log(LogCommand m)
        {
            if (!IsMainThread)
            {
                lock (syncRoot)
                {
                    PendingLogs.Add(m);
                }
            }
            else
            {
                Write(m);
            }

        }

        static void Write(LogCommand m)
        {
            switch (m.Type)
            {
                case LogType.Warning:
                    Debug.LogWarning(m.Message);
                    break;
                case LogType.Error:
                case LogType.Exception:
                    Debug.LogError(m.Message);
                    break;
                case LogType.Log:
                case LogType.Assert:
                    Debug.Log(m.Message);
                    break;
            }
        }

        private static TaskManager _instance;
        private static object syncRoot = new object();
        protected static readonly List<CoroutineCommand> PendingCoroutineInfo = new List<CoroutineCommand>();
        protected static readonly List<IEnumerator> PendingAdd = new List<IEnumerator>();
        protected static readonly List<IEnumerator> PendingRemove = new List<IEnumerator>();
        protected static readonly List<Action> PendingActions = new List<Action>();
        protected static readonly List<LogCommand> PendingLogs = new List<LogCommand>();
        protected static bool IsApplicationQuit;

        protected void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        protected void Update()
        {
            if (IsApplicationQuit)
                return;

            if (PendingAdd.Count == 0 && PendingRemove.Count == 0 && PendingActions.Count == 0 && PendingLogs.Count == 0 && PendingCoroutineInfo.Count == 0)
                return;

            lock (syncRoot)
            {
                for (int i = 0;i < PendingLogs.Count;i++)
                {
                    Write(PendingLogs[i]);
                }
                for (int i = 0;i < PendingAdd.Count;i++)
                {
                    StartCoroutine(PendingAdd[i]);
                }
                for (int i = 0;i < PendingRemove.Count;i++)
                {
                    StopCoroutine(PendingRemove[i]);
                }
                for (int i = 0;i < PendingCoroutineInfo.Count;i++)
                {
                    StartCoroutine(RunCoroutineInfo(PendingCoroutineInfo[i]));
                }
                for (int i = 0;i < PendingActions.Count;i++)
                {
                    PendingActions[i]();
                }
                PendingAdd.Clear();
                PendingRemove.Clear();
                PendingActions.Clear();
                PendingLogs.Clear();
                PendingCoroutineInfo.Clear();
            }
        }

        IEnumerator RunCoroutineInfo(CoroutineCommand info)
        {
			if (UnityTask.DisableCoroutineYields) {
				// HACK
				RunCoroutineWithoutYields (info.Coroutine);
				if (info.OnComplete != null)
					info.OnComplete();
				yield break;
			} else {
				yield return StartCoroutine(info.Coroutine);
				if (info.OnComplete != null)
					info.OnComplete();
			}
        }

		/// <summary>
		/// Execute an entire Unity Coroutine in one frame.
		/// This is useful for testing coroutines with NUnit.
		/// 
		/// The process will bail out after a fixed number of yields to avoid
		/// looping infinitely.
		/// 
		/// Calling StartCoroutine from inside an IEnumerator is not supported
		/// because there is no way to access the IEnumerator object created
		/// by StartCoroutine.
		/// 
		/// Returns true if the coroutine execution finished, otherwise false if it
		/// bailed early after reaching the maximum number of yields.
		/// </summary>
		static bool RunCoroutineWithoutYields (IEnumerator enumerator, int maxYields = 1000)
		{
			Stack<IEnumerator> enumStack = new Stack<IEnumerator> ();
			enumStack.Push (enumerator);

			int step = 0;
			while (enumStack.Count > 0) {
				IEnumerator activeEnum = enumStack.Pop ();
				while (activeEnum.MoveNext ()) {
					if (activeEnum.Current is IEnumerator) {
						enumStack.Push (activeEnum);
						activeEnum = (IEnumerator)activeEnum.Current;
					} else if (activeEnum.Current is Coroutine) {
						throw new System.NotSupportedException ("RunCoroutineWithoutYields can not be used with an IEnumerator that calls StartCoroutine inside itself.");
					}
					step += 1;
					if (step >= maxYields) {
						return false;
					}
				}
			}
			return true;
		}

        protected void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

        IEnumerator WaitForSecondsInternal(int seconds)
        {
            if(seconds <= 0)
                yield break;

            var delta = 0f;

            while (delta < seconds)
            {
                delta += Time.unscaledDeltaTime;
                yield return 1;
            }
        }
    }
}