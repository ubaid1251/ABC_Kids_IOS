using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundHandler : MonoBehaviour
{
    public AudioSource[] sfx;
    public AudioSource bgm;
    [HideInInspector]
    public AudioSource mySource;
    public GameObject sfxOn,sfxOff, bgmOn,bgmOff,vibOn,VibOff;
    public AudioClip selectCh,click,tap,camera;
    public AudioSource paintS;
    public static SoundHandler instance;
    private void Awake()
    {
        mySource = GetComponent<AudioSource>();
        CheckSounds();
        instance = this;
    }

    public void PlaySource(AudioClip c)
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            mySource?.PlayOneShot(c);
        }
    }
    public void playPaint()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            paintS.Play();
        }
    }
    public void stopPaint()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            paintS.Stop();
        }
    }
    
    public void PlayClick()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            PlaySource(click);
        }
    }

    public void PlayTap()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            PlaySource(tap);
        }
    }

    public void CheckSounds()
    {
        if (PlayerPrefs.GetInt("sfx") == 1 && sfx.Length > 0)
        {
            for (int i = 0; i < sfx.Length; i++)
            {
                sfx[i].enabled = false;
            }
            if (sfxOn && sfxOff)
            {
                sfxOn.SetActive(false);
                sfxOff.SetActive(true);
            }
        }
        if (PlayerPrefs.GetInt("bgm") == 1 && bgm != null)
        {
            bgm.enabled = false;
            if (bgmOn && bgmOff)
            {
                bgmOn.SetActive(false);
                bgmOff.SetActive(true);
            }
        }
        if (PlayerPrefs.GetInt("vibr") == 1)
        {
            if (vibOn && VibOff)
            {
                vibOn.SetActive(false);
                VibOff.SetActive(true);
            }
        }
    }
    public void toggleSFX()
    {
        if (PlayerPrefs.GetInt("sfx") == 0)
        {
            PlayerPrefs.SetInt("sfx", 1);
            for (int i = 0; i < sfx.Length; i++)
            {
                sfx[i].enabled = false;
            }
            sfxOn.SetActive(false);
            sfxOff.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("sfx", 0);
            for (int i = 0; i < sfx.Length; i++)
            {
                sfx[i].enabled = true;
            }
            sfxOn.SetActive(true);
            sfxOff.SetActive(false);
           
        }
        if (mySource.enabled)
            PlayClick();
    }
    public void toggleBGM()
    {
        if (mySource.enabled)
            PlayClick();
        if (PlayerPrefs.GetInt("bgm") == 0)
        {
            PlayerPrefs.SetInt("bgm", 1);
           bgm.enabled = false;

            bgmOn.SetActive(false);
            bgmOff.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("bgm", 0);
            bgm.enabled = true;
            bgmOn.SetActive(true);
            bgmOff.SetActive(false);
        }
    }
    public void toggleVibr()
    {
        if (mySource.enabled)
            PlayClick();
        if (PlayerPrefs.GetInt("vibr") == 0)
        {
            PlayerPrefs.SetInt("vibr", 1);
            //            bgm.enabled = false;

            vibOn.SetActive(false);
            VibOff.SetActive(true);
        }
        else
        {
            PlayerPrefs.SetInt("vibr", 0);
            //    bgm.enabled = true;
            vibOn.SetActive(true);
            VibOff.SetActive(false);
        }
    }
}
