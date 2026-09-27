using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMessage : MonoBehaviour {

	public static ToastMessage Instance;
	// Use this for initialization
		void Start () {

		Instance = this;
		DontDestroyOnLoad(gameObject);
//		ShowToastMessage ("It Worked!");

		}

		string toastString;
		AndroidJavaObject currentActivity;

	public void ShowToastMessage (string Message)
		{
			if (Application.platform == RuntimePlatform.Android) {
				showToastOnUiThread (Message);
		}
//		else showToastOnUiThread ("It Worked!");
		}

		void showToastOnUiThread(string toastString){
			AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

			currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			this.toastString = toastString;

			currentActivity.Call ("runOnUiThread", new AndroidJavaRunnable (showToast));
		}

		void showToast(){
			Debug.Log ("Running on UI thread");
			AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
			AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
			AndroidJavaObject javaString=new AndroidJavaObject("java.lang.String",toastString);
			AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject> ("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
			toast.Call ("show");
		}
		
}
