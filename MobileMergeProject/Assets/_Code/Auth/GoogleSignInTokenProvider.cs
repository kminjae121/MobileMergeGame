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
        private GoogleSignInConfiguration _configuration;
#if !UNITY_EDITOR
        private bool _isSignInRunning;
#endif
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
#if UNITY_EDITOR
            Debug.Log("Google Sign-In SDK is enabled. Build and Run on an Android device to test Google login.");
#else
            if (!TryPrepareSignIn())
                return;

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(HandleSignInFinished);
#endif
#else
            Debug.LogWarning("Google Sign-In SDK is not enabled. Import the SDK and add CATBLAST_GOOGLE_SIGN_IN to Scripting Define Symbols.");
#endif
        }

#if CATBLAST_GOOGLE_SIGN_IN
#if !UNITY_EDITOR
        private bool TryPrepareSignIn()
        {
            if (_isSignInRunning)
            {
                Debug.Log("Google sign-in is already running.");
                return false;
            }

            if (loginManager == null)
            {
                Debug.LogWarning("GoogleLoginManager is missing.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(webClientId))
            {
                Debug.LogWarning("Google Web Client ID is empty.");
                return false;
            }

            _configuration ??= new GoogleSignInConfiguration
            {
                WebClientId = webClientId,
                UseGameSignIn = false,
                RequestEmail = true,
                RequestIdToken = true
            };

            GoogleSignIn.Configuration = _configuration;
            _isSignInRunning = true;
            return true;
        }
#endif

        private void HandleSignInFinished(Task<GoogleSignInUser> task)
        {
            EnqueueOnMainThread(() =>
            {
#if !UNITY_EDITOR
                _isSignInRunning = false;
#endif

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
