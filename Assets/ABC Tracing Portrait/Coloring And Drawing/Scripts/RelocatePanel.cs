using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RelocatePanel : MonoBehaviour
{
    public RectTransform title,cardCon,content;
    public float contentH;
    // public float cardHeight,cardPosY;
    public float[] cardHeight, cardPosY, contentHTab;
    //private void Start()
    //{
    //    if (ResCheck.instance.resType == ResType.tab)
    //    {
    //        if (Mathf.Approximately(ResCheck.instance.aspect, 1.33f))
    //        {
    //            cardCon.DOAnchorPosY(cardPosY[0], 0);
    //            cardCon.sizeDelta = new Vector2(cardCon.sizeDelta.x, cardHeight[0]);
    //            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHTab[0]);
    //        }
    //        else if (Mathf.Approximately(ResCheck.instance.aspect, 1.43f))
    //        {
    //            // content.DOAnchorPosY(530, 0);
    //            cardCon.DOAnchorPosY(cardPosY[1], 0);
    //            cardCon.sizeDelta = new Vector2(cardCon.sizeDelta.x, cardHeight[1]);
    //            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHTab[1]);
    //        }
    //        else if (Mathf.Approximately(ResCheck.instance.aspect, 1.44f))
    //        {
    //            // content.DOAnchorPosY(550, 0);
    //            cardCon.DOAnchorPosY(cardPosY[2], 0);
    //            cardCon.sizeDelta = new Vector2(cardCon.sizeDelta.x, cardHeight[2]);
    //            content.sizeDelta = new Vector2(content.sizeDelta.x, contentHTab[2]);
    //        }
    //        else if (Mathf.Approximately(ResCheck.instance.aspect, 1.6f))
    //        {
    //            content.DOAnchorPosY(480, 0);
    //        }
    //        else
    //        {
    //            content.DOAnchorPosY(480, 0);
    //        }
    //    }
    //}

    // private void OnEnable()
    // {
    //     if (PlayerPrefs.GetInt("RemoveAds") == 1)
    //     {
    //         title.DOAnchorPosY(-310, 0);
    //         cardCon.DOAnchorPosY(-163.37f, 0);
    //         Vector2 size = cardCon.sizeDelta;
    //         size.y = 941.56f; // Set the height
    //         cardCon.sizeDelta = size;
    //
    //         Vector2 sizeC = content.sizeDelta;
    //         sizeC.y = contentH; // Set the height
    //         content.sizeDelta = sizeC;
    //     }
    //     
    // }
}
