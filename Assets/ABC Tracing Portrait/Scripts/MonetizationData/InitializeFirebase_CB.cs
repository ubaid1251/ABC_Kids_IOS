using Firebase;
using Firebase.Extensions;
//using Firebase.RemoteConfig;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Firebase.Analytics;
#if !UNITY_EDITOR
//using Firebase.Crashlytics;
#endif
public class InitializeFirebase_CB : MonoBehaviour
{
    [HideInInspector]
    public bool firebaseInitialized = false;
    public static InitializeFirebase_CB _Instance;
    public bool isBanner = false,removeInter=false;
  
    private void Start()
    {
        _Instance = this;
        print("firebaseInitialized");
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }
    void InitializeFirebase()
    {
        // Initialize Firebase
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
             //   FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
#if !UNITY_EDITOR
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
#endif
                Debug.Log("Enabling firebase Analytics");
                Dictionary<string, object> defaults = new Dictionary<string, object>();
                // defaults.Add("RemoveAds", "0");
                defaults.Add("DoubleBanner", "0");
                defaults.Add("RemoveInter", "0");
                
                //if (!PlayerPrefs.HasKey("Get_Value"))
                {
                    //FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults).ContinueWithOnMainThread(task =>
                    //{
                    //    Debug.Log("RemoteConfig configured and ready!");
                    //    FetchCompletePopUp();
                    //});
                }
                firebaseInitialized = true;
            });
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message.ToString());
        }
    }
    string TestA,TestB, TestC;
    void FetchCompletePopUp()
    {
        
        //FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync().ContinueWithOnMainThread(task =>
        //{
        //     TestA = FirebaseRemoteConfig.DefaultInstance.GetValue("DoubleBanner").StringValue;
        //     TestB = FirebaseRemoteConfig.DefaultInstance.GetValue("DoubleBanner").StringValue;
        //    Debug.Log("after Get Value => " + TestA);
        //    Debug.Log("after Get Value => " + TestB);
        //   // TestB = FirebaseRemoteConfig.DefaultInstance.GetValue("InterstitalDelay").StringValue;
        //   // Debug.Log("Direct Remote Value => " + FirebaseRemoteConfig.DefaultInstance.GetValue("RemoveAds").StringValue);
        //  // Debug.Log("Direct Remote Value => " + FirebaseRemoteConfig.DefaultInstance.GetValue("InterstitalDelay").StringValue);
        //    // PlayerPrefs.SetString("RemoveAds", TestA);
        //    //PlayerPrefs.SetString("DoubleBanner",TestA);
        //    PlayerPrefs.SetInt("DoubleBanner", int.Parse(TestA));
        //    PlayerPrefs.SetInt("RemoveInter", int.Parse(TestB));
        //    if (int.Parse(TestA)==1)
        //    {
        //        isBanner=true;
        //    }
        //    if (int.Parse(TestB)==1)
        //    {
        //        removeInter=true;
        //    }
        //    print("FetchCompletePopUp");
        //    print(PlayerPrefs.GetInt("DoubleBanner")+" DoubleBannerVal");
        //    IntitializeAdmob.instance.RequestConsent();
        //});
         
    }
    public void LogFirebaseEvent(string CustomEvent)
    {
        if(IsValidEventName(CustomEvent))
        {
          //  FirebaseAnalytics.LogEvent(CustomEvent);
            Debug.Log("Firebase Event Logged "+CustomEvent);
        }
        else
        {
            Debug.Log("Firebase Event Error: "+CustomEvent);
        }

    }
     public static bool IsValidEventName(string eventName)
     {
         //Debug.Log("FireBase " + eventName);
         // Check if the event name is empty or exceeds 40 characters
         if (string.IsNullOrEmpty(eventName) || eventName.Length > 40)
             return false;

         // Check if the event name starts with an alphabetic character
         if (!char.IsLetter(eventName[0]))
             return false;

         // Check if the event name contains only alphanumeric characters and underscores
         if (!Regex.IsMatch(eventName, @"^\w+$"))
             return false;

         // Check if the event name is not one of the reserved names
         string[] reservedNames = {
             "ad_activeview", "ad_click", "ad_exposure", "ad_impression", "ad_query",
             "ad_reward", "adunit_exposure", "app_background", "app_clear_data",
             "app_exception", "app_remove", "app_store_refund", "app_store_subscription_cancel",
             "app_store_subscription_convert", "app_store_subscription_renew", "app_update",
             "app_upgrade", "dynamic_link_app_open", "dynamic_link_app_update", "dynamic_link_first_open",
             "error", "first_open", "first_visit", "in_app_purchase", "notification_dismiss",
             "notification_foreground", "notification_open", "notification_receive", "os_update",
             "session_start", "session_start_with_rollout", "user_engagement"
         };

         
         if (reservedNames.Contains(eventName, StringComparer.OrdinalIgnoreCase))
             return false;

         // Check if the event name starts with reserved prefixes
         string reservedPrefixesPattern = "^(firebase_|google_|ga_)";
         if (Regex.IsMatch(eventName, reservedPrefixesPattern, RegexOptions.IgnoreCase))
             return false;

         return true;
     }

    public void CustomAdEvent(string evt, string placement)
    {
        if (!firebaseInitialized)
            return;

        LogFirebaseEvent(evt + "_" + placement);
    }
}