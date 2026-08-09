using System.Collections.Generic;
using UnityEngine;

#if CATBLAST_GOOGLE_SIGN_IN
using System;
using System.Threading.Tasks;
using Google;
#endif

namespace _Code.Auth
{
    public sealed class GoogleSignInTokenProvider : MonoBehaviour
    {
        [SerializeField] private GoogleLoginManager loginManager;
        [SerializeField] private string webClientId;

#if CATBLAST_GOOGLE_SIGN_IN
        private readonly object _sync = new object();
        private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
#endif

        private void Awake()
        {
            if (loginManager == null)
                loginManager = GetComponent<GoogleLoginManager>();
        }

#if CATBLAST_GOOGLE_SIGN_IN
        private void Update()
        {
            while (true)
            {
                Action action;

                lock (_sync)
                {
                    if (_mainThreadActions.Count == 0)
                        return;

                    action = _mainThreadActions.Dequeue();
                }

                action?.Invoke();
            }
        }
#endif

        public void SignIn()
        {
#if CATBLAST_GOOGLE_SIGN_IN
            if (loginManager == null)
            {
                Debug.LogWarning("GoogleLoginManager is missing.");
                return;
            }

            if (string.IsNullOrWhiteSpace(webClientId))
            {
                Debug.LogWarning("Google Web Client ID is empty.");
                return;
            }

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                RequestEmail = true,
                RequestIdToken = true
            };

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(HandleSignInFinished);
#else
            Debug.LogWarning("Google Sign-In SDK is not enabled. Import the SDK and add CATBLAST_GOOGLE_SIGN_IN to Scripting Define Symbols.");
#endif
        }

#if CATBLAST_GOOGLE_SIGN_IN
        private void HandleSignInFinished(Task<GoogleSignInUser> task)
        {
            EnqueueOnMainThread(() =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogWarning("Google sign-in was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogWarning($"Google sign-in failed. {task.Exception}");
                    return;
                }

                loginManager.LoginWithGoogleIdToken(task.Result.IdToken);
            });
        }

        private void EnqueueOnMainThread(Action action)
        {
            lock (_sync)
            {
                _mainThreadActions.Enqueue(action);
            }
        }
#endif
    }
}
