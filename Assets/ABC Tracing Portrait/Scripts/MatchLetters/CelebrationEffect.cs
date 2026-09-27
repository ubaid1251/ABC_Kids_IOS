using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CelebrationEffect : MonoBehaviour
{
   public Image dialogue;
   public Sprite[] allSp;
   public AudioClip[] allClips;
   public GameObject loadingH;
   private void OnEnable()
   {
      int i = Random.Range(0, allSp.Length);
      dialogue.sprite = allSp[i];
      //dialogue.SetNativeSize();
      if (GetComponent<AudioSource>().enabled)
         GetComponent<AudioSource>().PlayOneShot(allClips[i]);
      Invoke(nameof(MoveToSelection),5);
   }

   void MoveToSelection()
   {
      DOTween.KillAll(false);
     //if (Intitializeabc.instance.IsInterAvailable() || Intitializeabc.instance.IsStaticInterAvailable())
     // {
     //    gameObject.SetActive(false);
     //    loadingH.SetActive(true);
     // }
     // else
      {
         print(PlayerPrefs.GetString("ReloadScene")+" scene loaded celebration panel");
         SceneManager.LoadScene(PlayerPrefs.GetString("ReloadScene"));
      }
   }
}
