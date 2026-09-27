using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolGrid : MonoBehaviour
{
    public RectTransform mainHighLighter,subToolPanel,mainToolPanel;
    public RectTransform[] mainToolItem;
    public GameObject[] subToolPanels;
    public EventSystem eve;
    public static int SelectedTool = 0;
    public ParticleSystem part;
    [HideInInspector]
    public int pencil = 0,brush = 0,bucket = 0,texture = 0,sticker = 0;
    public RectTransform[] allPencil, allBrush, allBucket, allTexture, allStickers;
    public RectTransform bucketHigh, textureHigh, stickersHigh;
    public static bool FromEnable=false;
    private void Start()
    {
        if (ResCheck.ResolutionType == ResType.tab)
        {
            mainToolPanel.DOScale(1, 0);
        }
        FromEnable=false;
        eve.gameObject.SetActive(false);
        mainToolPanel.gameObject.SetActive(true);
        subToolPanels[0].SetActive(true);
        mainToolPanel.DOAnchorPosY(100, 1f).OnComplete(()=>
        {
            subToolPanel.DOAnchorPosY(213, 1f).OnComplete(() =>
            {
                part.Play();
                eve.gameObject.SetActive(true);
            });
        });
        
    }

    public void SelectTool(int index)
    {
        GameObject v = null;

        int t = SelectedTool;
        SelectedTool = index;
        if (!FromEnable)
        {
            v = eve.currentSelectedGameObject; 
        }
        else
        {
            v = mainToolItem[index].gameObject;
            FromEnable = false;
        }
        eve.gameObject.SetActive(false);
        v.GetComponent<RectTransform>()
            .DOShakeScale(0.1f, .3f, 5, 45);
        
            subToolPanel.DOAnchorPosX(150, .5f).OnComplete(() =>
            {
                if (t != 5)
                    subToolPanels[t].SetActive(false);
                t = index;
                if (index != 5)
                {
                    subToolPanels[t].SetActive(true);
                    subToolPanel.DOAnchorPosX(0, .5f).OnComplete(() => { eve.gameObject.SetActive(true); });
                }
                else
                {
                    eve.gameObject.SetActive(true);
                }
            });

        mainHighLighter.DOAnchorPosX(v.GetComponent<RectTransform>().anchoredPosition.x, .5f);
        switch (index)
        {

            case 0:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(0);
                }

                break;
            case 1:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(1);
                }

                break;
            case 2:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(3);
                }

                break;
            case 3:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(2);
                }

                break;
            case 4:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(7);
                }

                break;
            case 5:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(5);
                }

                break;
            default:
                for (int i = 0; i < Handler.MyHandler.Inner.Count; i++)
                {
                    Handler.MyHandler.Inner[i].SetDrawMode(1);
                }

                break;
        }
    }

    public void SelectPencil(int index)
    {
        eve.gameObject.SetActive(false);
        //allPencil[pencil].DOAnchorPosX(146, .5f);
        pencil = index;
        //allPencil[pencil].DOAnchorPosX(110, .5f).OnComplete(() =>
        //{
            eve.gameObject.SetActive(true);
        //});
    }
    public void SelectBrush(int index)
    {
        eve.gameObject.SetActive(false);
      //  allBrush[brush].DOAnchorPosX(210, .5f);
        brush = index;
       // allBrush[brush].DOAnchorPosX(170, .5f).OnComplete(() =>
       // {
            eve.gameObject.SetActive(true);
       // });
    }
    public void SelectBucket(float place)
    {
        GameObject v = null;
        int index = 0;
        if (!FromEnable)
        {
            v = eve.currentSelectedGameObject; 
            index = v.transform.GetSiblingIndex()-1;
        }
        else
        {
            FromEnable = false;
        }
        eve.gameObject.SetActive(false);
        ////allBucket[bucket].DOAnchorPosX(80, .5f);
        bucket = index;
     //   bucketHigh.DOAnchorPosY(place, .5f);
        //allBucket[bucket].DOAnchorPosX(50, .5f).OnComplete(() =>
      //  {
            eve.gameObject.SetActive(true);
        //});
    }
    public void SelectTexture(float place)
    {
        GameObject v = null;
        int index = 0;
        if (!FromEnable)
        {
            v = eve.currentSelectedGameObject; 
            index = v.transform.GetSiblingIndex()-1;
        }
        else
        {
            FromEnable = false;
        }
        eve.gameObject.SetActive(false);
       // allTexture[texture].DOAnchorPosX(80, .5f);
        texture = index;
       // textureHigh.DOAnchorPosY(place, .5f);
      //  allTexture[texture].DOAnchorPosX(50, .5f).OnComplete(() =>
       // {
            eve.gameObject.SetActive(true);
     //   });
    }
    public void SelectSticker(float place)
    {
        GameObject v = null;
        int index = 0;
        if (!FromEnable)
        {
            v = eve.currentSelectedGameObject; 
            index = v.transform.GetSiblingIndex()-1;
        }
        else
        {
            FromEnable = false;
        }
        eve.gameObject.SetActive(false);
        //allStickers[sticker].DOAnchorPosX(80, .5f);
        sticker = index;
       // stickersHigh.DOAnchorPosY(place, .5f);
       // allStickers[sticker].DOAnchorPosX(50, .5f).OnComplete(() =>
     //   {
            eve.gameObject.SetActive(true);
     //   });
    }
}
