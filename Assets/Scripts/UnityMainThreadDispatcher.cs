using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace PimDeWitte.UnityMainThreadDispatcher {

	public class UnityMainThreadDispatcher : MonoBehaviour {

		private static readonly Queue<Action> _executionQueue = new Queue<Action>();

		public void Update() {
			lock(_executionQueue) {
				while (_executionQueue.Count > 0) {
					_executionQueue.Dequeue().Invoke();
				}
			}
		}

		public void Enqueue(IEnumerator action) {
			lock (_executionQueue) {
				_executionQueue.Enqueue(() => {
					StartCoroutine(action);
				});
			}
		}

		public void Enqueue(Action action) {
			Enqueue(ActionWrapper(action));
		}

		public Task EnqueueAsync(Action action) {
			var tcs = new TaskCompletionSource<bool>();

			void WrappedAction() {
				try {
					action();
					tcs.TrySetResult(true);
				} catch (Exception ex) {
					tcs.TrySetException(ex);
				}
			}

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;
		}

		IEnumerator ActionWrapper(Action a) {
			a();
			yield return null;
		}

		private static UnityMainThreadDispatcher _instance = null;

		public static bool Exists() {
			return _instance != null;
		}

		public static UnityMainThreadDispatcher Instance() {
			if (!Exists()) {
				throw new Exception("UnityMainThreadDispatcher could not find the UnityMainThreadDispatcher object. Please ensure you have added the MainThreadExecutor Prefab to your scene.");
			}
			return _instance;
		}

		void Awake() {
			if (_instance == null) {
				_instance = this;
				DontDestroyOnLoad(this.gameObject);
				Debug.Log("[UnityMainThreadDispatcher] Instance created and DontDestroyOnLoad set.");
			} else if (_instance != this) {
				Debug.LogWarning("[UnityMainThreadDispatcher] Duplicate instance found. Destroying this one.");
				Destroy(gameObject);  // Destroy duplicate dispatcher GameObject
			}
		}

		void OnDestroy() {
			if (_instance == this) {
				Debug.Log("[UnityMainThreadDispatcher] Instance destroyed.");
				_instance = null;
			}
		}
	}
}
