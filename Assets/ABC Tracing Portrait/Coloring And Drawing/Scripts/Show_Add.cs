using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Show_Add : MonoBehaviour
{
    public bool show = false;
    public bool dis_Show = false;

    /*    [System.Obsolete]
        private void OnEnable()
        {
            if (AssignAdIds_CB.instance)
            {
                if (show)
                {
                    AssignAdIds_CB.instance.ShowBanner();
                }
                else
                {
                    AssignAdIds_CB.instance.HideBanner();
                }
            }
        }*/
    [Obsolete]
    private void Start()
    {
        if (Intitializeabc.instance) //remove after
        {
            if (show)
            {
                StartCoroutine(ShowAdd());
            }
            else
            {
                Intitializeabc.instance.HideBanner();//remove after
            }
        }
    }

    IEnumerator ShowAdd()
    {
        yield return new WaitForSeconds(0.5f);
        //Debug.LogError("Show Loading: " + Loading.ShowLoading);
        // if (!Loading.ShowLoading)
        {
            Intitializeabc.instance.ShowBanner();//remove after
        }
        // else
        // {
        //     StartCoroutine(WaitForLoadingToStop());
        // }
    }

    IEnumerator WaitForLoadingToStop()
    {
        yield return new WaitForSeconds(3.25f);
        Intitializeabc.instance.ShowBanner();//remove after
    }


    [System.Obsolete]
    private void OnDisable()
    {
        if (dis_Show)
        {
            if (Intitializeabc.instance)//remove after
                Intitializeabc.instance.ShowBanner();//remove after
        }
        else
        {
            if (SceneManager.GetActiveScene().name != "MainSelection")
            {
                if (Intitializeabc.instance) //remove after
                    Intitializeabc.instance.HideBanner();//remove after
            }
        }
    }
}