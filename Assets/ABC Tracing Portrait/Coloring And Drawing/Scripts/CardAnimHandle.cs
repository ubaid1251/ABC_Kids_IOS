using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardAnimHandle : MonoBehaviour
{
    public RectTransform content;
    public float startX, endX,st = 0,anchorPosY=0;
    public int range = 0;
    private void OnEnable()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 0 && ResCheck.ResolutionType == ResType.tab)
        {
            content.DOAnchorPosY(anchorPosY, 0);
        }
        else if (PlayerPrefs.GetInt("RemoveAds") == 1 && ResCheck.ResolutionType == ResType.tab)
        {
            content.DOAnchorPosY(anchorPosY+50, 0);
        }
        int index = PlayerPrefs.GetInt("Character_Index");
        //if (PlayerPrefs.GetInt("fromGP") == 1)
        //{
        //    PlayerPrefs.SetInt("fromGP", 0);
        //    float p = PlayerPrefs.GetFloat("pos");
        //    st = index < range ? endX : startX;

        //    content.DOAnchorPosY(st, 0).OnComplete(() =>
        //    {
        //        content.DOAnchorPosY(p, 1).OnComplete(() =>
        //        {
        //            GetComponent<ScrollRect>().enabled = true;
        //           if(RateUsHandler.Instance.CheckRateCondition())
        //               RateUsHandler.Instance.rate.SetActive(true);
        //        });
        //    });
        //}
        //else
        {
            content.DOAnchorPosY(endX, 0).OnComplete(() =>
            {
                content.DOAnchorPosY(startX, 1f).OnComplete(() =>
                {
                    GetComponent<ScrollRect>().enabled = true;
                    if (RateUsHandler.Instance.CheckRateCondition())
                    {
                        RateUsHandler.Instance.rate.SetActive(true);
                    }
                });
            });
        }
    }
}
