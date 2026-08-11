using System.Threading.Tasks;
using Firebase.Auth;
using SFS.Core;
using UnityEngine;

public static class FbAuth
{
	private static FirebaseAuth auth;

	public static bool Authenticated => auth.CurrentUser != null;

	public static FirebaseUser User => auth.CurrentUser;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		auth = FirebaseAuth.DefaultInstance;
	}

	public static void SignIn(AuthCallback callback = null)
	{
		GoogleSignInBridge.SignIn(delegate(GoogleSignInBridge.Result result)
		{
			Debug.Log($"Callback from GSIB: {result.success}; {result.error}");
			if (!result.success)
			{
				callback?.Invoke(authed: false, null);
			}
			else
			{
				Credential credential = GoogleAuthProvider.GetCredential(result.token, null);
				auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(delegate(Task<AuthResult> t)
				{
					Debug.Log("Regular sign-in");
					if (!t.IsCompletedSuccessfully)
					{
						Debug.Log($"ERROR: {t.Exception}");
						ActionQueue.main.QueueAction(delegate
						{
							callback?.Invoke(authed: false, null);
						});
					}
					else
					{
						ActionQueue.main.QueueAction(delegate
						{
							callback?.Invoke(authed: true, t.Result.User);
						});
					}
				});
			}
		});
	}

	public static void NewSignIn(AuthCallback callback)
	{
		SignOut();
		SignIn(callback);
	}

	public static void SignOut()
	{
		GoogleSignInBridge.SignOut();
		auth.SignOut();
	}
}
