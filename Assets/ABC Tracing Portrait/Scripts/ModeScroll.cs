using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class ModeScroll : MonoBehaviour
{
    public RectTransform content;
    public Ease Ease;
    public float startF, EndAt, duration = 1.25f;
    public SoundHandler handler;

    private void OnEnable()
    {
        if (PlayerPrefs.GetInt("RemoveAds") == 1&& ResCheck.ResolutionType != ResType.tab)
        {
            content.DOAnchorPosY(450, 0);
        }
        if (PlayerPrefs.GetInt("RemoveAds") == 0&& ResCheck.ResolutionType == ResType.tab)
        {
            content.DOAnchorPosY(500, 0);
        }
        else if (PlayerPrefs.GetInt("RemoveAds") == 1 && ResCheck.ResolutionType == ResType.tab)
        {
            content.DOAnchorPosY(600, 0);
        }
        handler.bgm = GetComponent<AudioSource>();
        //  if (SubSelection.cameFrom == "MainSelection")
        // {
        if (GetComponent<ScrollRect>() != null)
        {
            GetComponent<ScrollRect>().enabled = false;
            content.DOAnchorPosY(startF, 0f).OnComplete(() =>
            {
                content.DOAnchorPosY(EndAt, duration).SetEase(Ease).OnComplete(() =>
                {
                    if (RateUsHandler.Instance.CheckRateCondition())
                    {
                        RateUsHandler.Instance.rate.SetActive(true);
                    }
                    GetComponent<ScrollRect>().enabled = true;
                });
            });
        }
        //else
        //{
        //    float f = PlayerPrefs.GetFloat("ContentPos");
        //    GetComponent<ScrollRect>().enabled = false;
        //    content.DOAnchorPosY(startF, 0f).OnComplete(() =>
        //    {
        //        content.DOAnchorPosY(f, duration).SetEase(Ease).OnComplete(() =>
        //        {
        //            if (RateUsHandler.Instance.CheckRateCondition())
        //            {
        //                RateUsHandler.Instance.rate.SetActive(true);
        //            }
        //            GetComponent<ScrollRect>().enabled = true;
        //        });
        //    });
        //}
        handler.CheckSounds();
    }
}
