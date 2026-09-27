using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideBanner : MonoBehaviour
{
    // Start is called before the first frame update
    bool HideBannerCheck = false;
    public bool makeItShow = true;
    private void OnEnable()
    {
        Intitializeabc.instance.HideBanner();//remove after
    }

    [System.Obsolete]
    /*private void Update()
    {
        if (HideBannerCheck && AssignAdIds_CB.instance)
        {
            AssignAdIds_CB.instance.HideBanner();
        }
        
    }*/
    private void OnDisable()
    {
        if (makeItShow)
        {
            HideBannerCheck = false;
        }
        if (makeItShow && !HideBannerCheck)
        {
            Intitializeabc.instance.ShowBanner();
        }
    }
}
