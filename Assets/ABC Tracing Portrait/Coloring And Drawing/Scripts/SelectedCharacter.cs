using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


namespace KGF.Coloring
{
    public class SelectedCharacter : MonoBehaviour
    {
        public GameObject CurrentCharacter_new;
        public string CharacterLocalPath; //store path
        public string AnimationPath;
        public string PrefName; //for Setting Prefab 
        public string CharacterIndexInCategroy; // Name of selected character
        public int animIndex;



        public Texture2D[] images;
        public Texture2D[] White_Image;
        public int NumberOfImages;
        [HideInInspector] public bool IsCompleted;
        [HideInInspector] public string DrawingPath;

        public GameObject CharacterBeenDrawn;

        void Awake()
        {
//            NumberOfImages = images.Length;
            Application.backgroundLoadingPriority = ThreadPriority.Low;
            if (ServiceManager.instance.selectedCharacter == this)
            {
                DontDestroyOnLoad(this);
                
            }
            else
            {
                /*if (gameObject.transform.childCount > 0)
                {
                    Destroy(transform.GetChild(0).gameObject);
                }*/
                Destroy(gameObject);
                
                return;
            }
            //audioSource = GetComponent<AudioSource>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        //private void OnEnable()
        //{
        //    if (PlayerPrefs.GetInt("AudioSetiings") == 1)
        //    {
        //        audioSourceBG.mute = true;
        //    }
        //    else
        //    {
        //        audioSourceBG.mute = false;
        //    }
        //}
        public IEnumerator WaitWhile(Action CallBack, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            CallBack?.Invoke();
        }

        public void PlayCharAudio(AudioClip charAudio, bool isLoop = false, float volume = 0.4f)
        {
            AudioSource source;

            if (!gameObject.TryGetComponent<AudioSource>(out source))
                source = gameObject.AddComponent<AudioSource>();

            source.volume = volume;
            source.loop = isLoop;
            source.clip = charAudio;
            source.Play();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "Paintista Scene":
                   // PlayCharAudio(PaintingAudio, true, 0.47f);
                    Debug.Log("I'm loading Paintista Scene");
                    break;
                case "SubSelection":

                    if (CurrentCharacter_new != null)
                    {
                        Destroy(CurrentCharacter_new.gameObject);
                        CurrentCharacter_new = null;
                    }
                    //audioSourceBG = gameObject.transform.GetChild(0).GetComponent<AudioSource>();
                    //audioSourceBG.volume = 0.5f;
                   // audioSourceBG.clip = SelectionAudio;
                    //audioSourceBG.loop = true;
                    //audioSourceBG.Play();
                    break;

                case "CharacterAnimation":
                    //var temp = UnityEngine.Random.Range(0, AnimationAudio.Length);

                    //PlayCharAudio(AnimationAudio[temp], true, 0.47f);

                    break;
            }
        }

        private void OnSceneUnloaded(Scene current)
        {
            switch (current.name)
            {
                case "CharacterAnimation":
                    if (CurrentCharacter_new != null)
                    {
                        Destroy(CurrentCharacter_new.gameObject);
                        CurrentCharacter_new = null;
                    }

                    //PlayCharAudio(SelectionAudio, false, 0.075f);
                    break;
                case "Paintista Scene":
                   // PlayCharAudio(PaintingAudio, false, 0f);
                    Destroy(GameObject.Find("root"));
                    break;
                case "Selection Scene":
                    var audioSource = gameObject.transform.GetChild(0).GetComponent<AudioSource>();
                    audioSource.Stop();
                    break;
            }
        }

        public void LoadTextures()
        {
            images = new Texture2D[NumberOfImages];
            StartCoroutine(LoadDrawingScene());
        }

        public IEnumerator LoadDrawingScene()
        {
            yield return new WaitForSeconds(0.4f);

            SceneManager.LoadScene("Loading");
        }
        public IEnumerator LoadAnimationScene()
        {
            yield return new WaitForSeconds(0.3f);
            yield return new WaitForSeconds(0.4f);
            SceneManager.LoadScene("CharacterAnimation");
        }

        public void OpenDrawOrAnimateCanvas()
        {
            ServiceManager.instance.referenceManager.DrawOrAnimateCanvas.SetActive(true);
        }

        public void PanelNumTeller(Toggle toggle)
        {
            //if (toggle.isOn)
            //{
            //    panelNumber = toggle.GetComponent<btnscrollAudio>().panelNumber;
            //    Debug.Log("I'm Clicked " + panelNumber);
            //}
        }
    }
}