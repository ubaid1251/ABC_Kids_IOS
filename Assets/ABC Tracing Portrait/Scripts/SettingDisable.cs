using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingDisable : MonoBehaviour
{
    public void Deactive()
    {
        if (SceneManager.GetActiveScene().name != "Gameplay")
        {
            if (PlayerPrefs.GetInt("RemoveAds") == 0)
            {
                Intitializeabc.instance.ShowBanner();//remove later
            }
        }
        gameObject.SetActive(false);
    }
    public void PlayAudio()
    {
        AudioSource s = transform.GetChild(0).GetComponent<AudioSource>();
        if (s.enabled == true)
        {
            s.Play();
        }
    }
}
