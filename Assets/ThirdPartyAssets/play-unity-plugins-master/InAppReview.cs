using Google.Play.Review;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InAppReview : MonoBehaviour
{

    public static InAppReview Instance;

    private void Awake()
    {
        Instance = this;
    }

    // public static int ShowINAppReview;

    ReviewManager _reviewManager;

    // Start is called before the first frame update
    void Start()
    {
        //ShowINAppReview++;

        //if (ShowINAppReview == 2)
        //{
        //    StartCoroutine(RequestAppReview());

        //}
    }

    public void ShowInGameRating()
    {
        ////AdmobAdsManager.blockAppOpen = true;

        StartCoroutine(RequestAppReview());
    }


    IEnumerator RequestAppReview()
    {
       
        _reviewManager = new ReviewManager();
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)// if error occured
        {
            yield break;
        }
        var _playReviewInfo = requestFlowOperation.GetResult();
         
        
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
         

        yield return launchFlowOperation;

        _playReviewInfo = null;


        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {

            yield break;
        }
        else
        {
            Debug.Log("In app Review Successfull!");
        }


        //Invoke(nameof(UnblockAppOpen) , 1);

    }

    void UnblockAppOpen()
    {
        ////AdmobAdsManager.blockAppOpen = false;
    }
}
