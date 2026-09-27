using System;
//using DG.Tweening.Plugins.Core.PathCore;
//using Spine;
//using Spine.Unity;
//using Spine.Unity.AttachmentTools;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Linq;
//using System.Security.Policy;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KGF.Coloring
{
    public class CharacterSelection : MonoBehaviour
    {
        [HideInInspector]
        public int NumberOfImages;

        [HideInInspector]
        public bool IsCompleted = false;

        public string PrefName;
        public string DrawingPath;
        public string PrefabObjPath;
        public bool IsCharacterSetup = false;
        public bool showIconImage = true;

        [HideInInspector]
        public AudioClip voiceOver;

        float delayTime = 1;

        


        void Awake()
        {
            /*var child = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<RawImage>();

            Holder = child;

            if (PlayerPrefs.GetInt(PrefName) == 1)
            {
                LoadTheCharacter();
            }
            else
            {
                Holder.gameObject.SetActive(false);
            }*/
        }
        public IEnumerator setupCharacterEnumerator()
        {
            if (!IsCharacterSetup)
            {
                ServiceManager.instance.selectedCharacter.CharacterBeenDrawn = Resources.Load<GameObject>(PrefabObjPath);
                var childobj = Instantiate(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn, this.transform);
                childobj.transform.localScale = new Vector3(5, 5, 1);
                childobj.transform.localPosition = new Vector3(0, 0, -20);
                print(childobj.name+"selected");
                var animator = childobj.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.enabled = true;
                }
                IsCharacterSetup = true;
            }
            yield return null;
        }


        
        
        public void OnSelectionCharacter(AudioClip c)
        {
            if (c != null)
                SoundHandler.instance.PlaySource(c);
            float pos = transform.parent.GetComponent<RectTransform>().anchoredPosition.x;
            PlayerPrefs.SetFloat("pos", pos);
            PlayerPrefs.SetFloat("ContentPos", pos);
            SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);

            PlayerPrefs.SetInt("Character_Index", transform.GetSiblingIndex());
            Debug.Log(transform.GetSiblingIndex()+" Sin");

            //cardhandler.instance.selectedCharacterIndex = transform.GetSiblingIndex();
            if (ServiceManager.instance.selectedCharacter.CurrentCharacter_new != null)
            {
                Destroy(ServiceManager.instance.selectedCharacter.CurrentCharacter_new.gameObject);
            }
            EventSystem.current.gameObject.SetActive(false);
            ServiceManager.instance.selectedCharacter.CharacterBeenDrawn = Resources.Load<GameObject>(PrefabObjPath);
            ServiceManager.instance.selectedCharacter.CurrentCharacter_new =
                Instantiate(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn
                    ,ServiceManager.instance.selectedCharacter.transform);
            //ServiceManager.instance.selectedCharacter.CurrentCharacter_new.transform.localPosition = new Vector3(0, -2, 0);
           Camera_Pos cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();

           // InitializeFirebase_CB._Instance.LogFirebaseEvent(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn.name + "_SelectedCharacter");
            InitializeFi._Instance.LogFi();

            ServiceManager.instance.selectedCharacter.images = new Texture2D[cam_pos.Character_Sprite.Length];
            ServiceManager.instance.selectedCharacter.White_Image = new Texture2D[cam_pos.WhiteSprite.Length];
            
            for (int i = 0; i < cam_pos.Character_Sprite.Length; i++)
            {
                ServiceManager.instance.selectedCharacter.images[i] = cam_pos.Character_Sprite[i].texture;
            }
            
            for (int i = 0; i < cam_pos.WhiteSprite.Length; i++)
            {
                ServiceManager.instance.selectedCharacter.White_Image[i] = cam_pos.WhiteSprite[i].texture;
            }
            ServiceManager.instance.selectedCharacter.NumberOfImages = cam_pos.Character_Sprite.Length;
            ServiceManager.instance.selectedCharacter.CharacterLocalPath = DrawingPath;
            // ServiceManager.instance.selectedCharacter.AnimationPath = AnimationPath;//remove for anim
            ServiceManager.instance.selectedCharacter.PrefName = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>().gameObject.name;
            //PlayerPrefs.SetInt(ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>().name, 1);
            var p = Path.Combine(Application.persistentDataPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
            if (Directory.Exists(p))
            {
                if (PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0)
                {
                    Directory.Delete(p, true);
                }
            }
            StartCoroutine(LoadDrawingScene());
        }
        
        public IEnumerator LoadDrawingScene()
        {
            yield return new WaitForSeconds(delayTime);
          //  InitializeFirebase_CB._Instance.LogFirebaseEvent(SceneManager.GetActiveScene().name+ "_IsCompleted");
        //    InitializeFirebase_CB._Instance.LogFirebaseEvent("PaintScene_IsLoading");
            InitializeFi._Instance.LogFi();
            // Loading.cameFrom = "SubSelection";
            SceneManager.LoadScene("ColorGamePlay");//loading
        }
        public void OnSelectionCharacterNew()
        {
            ServiceManager.instance.selectedCharacter.CharacterBeenDrawn = Resources.Load<GameObject>(PrefabObjPath);
        }

        public void LoadTheCharacter(RawImage Holder,List<Texture2D>tex,List<string>Pref)
        {
            if (DrawingPath != null)
            {
                var localPath = Application.persistentDataPath + "/Assets/Outputs/" + PrefName + ".png";
                print(localPath);
                if (File.Exists(localPath))
                {
                    if (!showIconImage)
                    {
                        transform.GetChild(0).GetComponent<Image>().enabled = false;
                    }
                    Holder.gameObject.SetActive(true);
                    Texture2D mtexture = new Texture2D(10, 10);
                    mtexture.LoadImage(File.ReadAllBytes(localPath));
                    tex.Add(mtexture);
                    Pref.Add(PrefName);
                    Holder.texture = mtexture;
                    Holder.SetNativeSize();
                }
            }

        }

        public void OpenDrawOrAnimateCanvas()
        {
            ServiceManager.instance.referenceManager.DrawOrAnimateCanvas.SetActive(true);
        }
    }
}
