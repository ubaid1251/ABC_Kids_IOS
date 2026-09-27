using System.Collections;
using System.Collections.Generic;
using KGF.Coloring;
using UnityEngine;
using DG.Tweening;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
// using JetBrains.Annotations;
// using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Handler : MonoBehaviour
{
    public static Handler MyHandler;
    public PaintistaManager Outline, DummyForInner;

    [Header("-------------------- Final Animation Screen --------------------")]
    public GameObject canvasForAds;

    public GameObject AnimationBG;
    public Vector3 DefaultPosBG, CamPosAnimation;
    public string AnimationPath;
    public GameObject AnimateObject_Resource, /*AnimatedObject,*/ AnimationPanel;

    [Header("-------------------- Full Body [White Image's] --------------------")]
    public List<PaintistaManager> Inner;

    public Texture2D[] WhiteSprite;
    public SpriteRenderer[] WhiteSpriteTemp;

    [Header("-------------------- After Completion Outline --------------------")]
    public Camera_Pos campos;

    public bool OutlineCompleted = false;
    float CameraSize;
    public Vector3 CamPosition;
    public Camera HandlerCamera;
    public bool paint = false;
    public PaintistaManager Selected;
    public float raycastVal = 100;
    public RectTransform reference;
    public GameObject NextBtn, PaintingPanel, delBtn, UndoBtn;
    public float timeWithoutTouch = 0;
    float RealtimeWithoutTouch = 0;
    public bool ForIndication = true;
    public bool MouseDown = false;
    public int PencilSizeForOutline = 40, PencilSizeForInner = 70;
    public int BrushSizeForOutline = 80, BrushSizeForInner = 120;

    public ToolGrid switchToolsSubGrid;

    public GameObject stickerButton;

    public Camer2Helper camer2Helper;

    public GameObject AdPanel;
    public RectTransform GridHolder;
    public RectTransform SafePanel;
    public GameObject events;
    public GameObject loading;
    public AudioClip[] VoiceOver;
    public AudioClip[] CompleteS;
    float delayTime = 1;
    public string DrawingPath;
    public string PrefabObjPath;
    private void Awake()
    {
        MyHandler = this;
        PlayerPrefs.SetInt("fromGP",1);
        if (PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            AdPanel.SetActive(false);
            GridHolder.DOAnchorPosY(0, 0);
            GridHolder.DOScale(1, 0);
            SafePanel.DOAnchorPosY(170, 0);
        }
        else
        {
            AdPanel.SetActive(true);
            //GridHolder.DOScale(.85f, 0);
            //GridHolder.DOAnchorPosY(-50, 0);
            //SafePanel.DOAnchorPosY(300, 0);
        }
    }

    private void Start()
    {
        //eraserButton.transform.position = stickerButton.transform.position;
        StartCoroutine(ShowAdd());
        stickerButton.SetActive(false);

        Assign_Sprite(); //Assign Texture2D
        Assign_SpriteTemp(); //Assign Sprite renderer
        AnimationPath = ServiceManager.instance.selectedCharacter.AnimationPath;
        campos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();
        DrawingPath = campos.DrawingPath;
        PrefabObjPath = campos.PrefabObjPath;
        CamPosAnimation = campos.CameraFinalPosition;
        //if (campos.EligibleForBanner)
        //{
        //    canvasForAds.SetActive(true);
        //}
        //else
        //{
        //    canvasForAds.SetActive(false);
        //}
        DefaultPosBG = AnimationBG.transform.localPosition;
        ReassignCharacterWhiteSprite();
    }

    IEnumerator ShowAdd()
    {
        yield return new WaitForSeconds(0.5f);
        Intitializeabc.instance.ShowBanner(); //remove after
    }

    private void Update()
    {
        if (!MouseDown)
        {
            timeWithoutTouch = Time.deltaTime + timeWithoutTouch;
            RealtimeWithoutTouch = Time.deltaTime + RealtimeWithoutTouch;
        }
    }

    public void PointerDownFunc()
    {
        timeWithoutTouch = 0;
        ForIndication = true;
    }

    public void CallAllIndicationOff()
    {
        for (int i = 0; i < Inner.Count; i++)
        {
            Inner[i].Indication();
        }
    }

    public void GenerateInner()
    {
        events.SetActive(false);
        
        for (int i = 0; i < WhiteSprite.Length; i++)
        {
            Debug.Log("Instantiate Error");
            var refer = Instantiate(reference, reference.transform.parent);
            Renderer whiteimage = Instantiate(DummyForInner).GetComponent<Renderer>();
            whiteimage.GetComponent<PaintistaManager>().referenceArea = reference;
            whiteimage.name = "inner" + i.ToString();
            //whiteimage.gameObject.layer = LayerMask.NameToLayer("Painted");
            whiteimage.GetComponent<PaintistaManager>().referenceArea = refer;
            whiteimage.GetComponent<PaintistaManager>().Self_Index = i;
            whiteimage.GetComponent<PaintistaManager>().White_image = WhiteSprite[i];
            whiteimage.GetComponent<PaintistaManager>().myRenderer.material.SetTexture("_MainTexture", WhiteSprite[i]);
            whiteimage.gameObject.SetActive(true);
            Inner.Add(whiteimage.GetComponent<PaintistaManager>());
            //  Inner[i].drawMode= PaintistaManager.DrawMode.CustomBrush;
            whiteimage.GetComponent<MeshRenderer>().enabled = true;
            //whiteimage.GetComponent<MeshRenderer>().sortingOrder = campos.Inner_part[i].sortingOrder;
        }

        for (int x = 0; x < campos.objects.Length; x++)
        {
            campos.objects[x].sortingLayerName = "Default";
        }

        delBtn.SetActive(true);
        UndoBtn.SetActive(false);
    }

    public void OffInnerCollider(int selfIndex)
    {
        for (int i = 0; i < Inner.Count; i++)
        {
            if (Inner[i] && i != selfIndex)
            {
                if (Inner[i].GetComponent<Collider>())
                {
                    Inner[i].GetComponent<Collider>().enabled = false;
                }

                if (Inner[i].GetComponent<PaintistaManager>())
                {
                    Inner[i].GetComponent<PaintistaManager>().enabled = false;
                }
            }
        }
    }

    public void OnInnerCollider()
    {
        for (int i = 0; i < Inner.Count; i++)
        {
            if (Inner[i].GetComponent<Collider>())
            {
                Inner[i].GetComponent<Collider>().enabled = true;
            }

            if (Inner[i].GetComponent<PaintistaManager>())
            {
                Inner[i].GetComponent<PaintistaManager>().enabled = true;
            }
        }
    }

    public void Assign_Sprite()
    {
        Camera_Pos cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();
        WhiteSprite = new Texture2D[cam_pos.WhiteSprite.Length];
        for (int x = 0; x < cam_pos.WhiteSprite.Length; x++)
        {
            if (cam_pos.WhiteSprite[x] != null)
            {
                WhiteSprite[x] = cam_pos.WhiteSprite[x].texture;
            }
            else
            {
                WhiteSprite[x] = null; // or any other desired action
            }
        }
    }

    public void CameraFinalPosition()
    {
        Debug.Log("set camera position");
        Camera_Pos cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();
        if (ResCheck.ResolutionType != ResType.tab)
        {
            CamPosition = cam_pos.CameraFinalPosition;
            CameraSize = cam_pos.CamSize;
        }
        else
        {
            CamPosition = cam_pos.CameraFinalPositionTab;
            CameraSize = cam_pos.CamSizeTab;
        }

        CamPosition = new Vector3(CamPosition.x, CamPosition.y, -10);
        HandlerCamera.DOOrthoSize(CameraSize, 1f);
        HandlerCamera.transform.DOMove(CamPosition, 1.5f).OnComplete(delegate
        {
            if (campos.CompleteClip)
            {
                //SoundManager.instance.PlayCompleteSound(campos.CompleteClip);
                SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
            }

            Invoke(nameof(resetTool), 1);
        });
    }

    void resetTool()
    {
        ToolGrid.FromEnable = true;
        switchToolsSubGrid.SelectTool(0);
        SetButtonsPos();
    }

    void SetButtonsPos()
    {
        //eraserButton.transform.GetComponent<RectTransform>().DOAnchorPosY(-303, 0);
        stickerButton.SetActive(true);
        events.gameObject.SetActive(true);
    }

    public Canvas mainCanvas;

    public void Home()
    {
        SubSelection.cameFrom = "GamePlay";
        // InitializeFirebase_CB._Instance.LogFirebaseEvent(SceneManager.GetActiveScene().name + "_IsCompleted");
        // InitializeFirebase_CB._Instance.LogFirebaseEvent("SubSelection_IsLoading");
        for (int i = 0; i < Inner.Count; i++)
        {
            if (Inner[i] != null)
            {
                Inner[i].gameObject.SetActive(false);
            }
        }

        ServiceManager.instance.selectedCharacter.CurrentCharacter_new.SetActive(false);
        var p = Path.Combine(Application.persistentDataPath,
            ServiceManager.instance.selectedCharacter.CharacterLocalPath);
        if (Directory.Exists(p))
        {
            Directory.Delete(p, true);
        }
        
        // Loading.ShowLoading = true;
        // Loading.cameFrom = "GamePlay";
        mainCanvas.GetComponent<CanvasGroup>().DOKill();
        DOTween.KillAll();
        Resources.UnloadUnusedAssets();
        SceneManager.LoadSceneAsync("ColorSubSelection");
    }

    public void RateUpdate()
    {
        PlayerPrefs.SetInt("Completed", 1);
        PlayerPrefs.SetInt("RateCounter", PlayerPrefs.GetInt("RateCounter") + 1);
        Debug.Log("rateUs Issue");
    }

    public void CheckPlayedCharacter()
    {
        //for(int i=0; i< LoadAllCharacters.instance.PrefsToCheck.Count; i++)
        {
            if (LoadAllCharacters.instance.PrefsToCheck.Contains(ServiceManager.instance.selectedCharacter.PrefName))
            {
                Debug.Log("Challll Bhai: " +
                          LoadAllCharacters.instance.PrefsToCheck.IndexOf(ServiceManager.instance.selectedCharacter
                              .PrefName));

                LoadAllCharacters.instance.AllCharactersTexture.RemoveAt(
                    LoadAllCharacters.instance.PrefsToCheck.IndexOf(ServiceManager.instance.selectedCharacter
                        .PrefName));
                LoadAllCharacters.instance.PrefsToCheck.Remove(ServiceManager.instance.selectedCharacter.PrefName);

                //LoadAllCharacters.instance.LoadCharacters();
            }
        }
    }
     public void OnSelectionCharacter()
        {
            if (ServiceManager.instance.selectedCharacter.CurrentCharacter_new != null)
            {
                Destroy(ServiceManager.instance.selectedCharacter.CurrentCharacter_new.gameObject);
            }
            if (DrawingPath.Length<1)
            {
                Home();
                return;
            }
            ServiceManager.instance.selectedCharacter.CharacterBeenDrawn = Resources.Load<GameObject>(PrefabObjPath);
            ServiceManager.instance.selectedCharacter.CurrentCharacter_new =
                Instantiate(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn
                    ,ServiceManager.instance.selectedCharacter.transform);
            //ServiceManager.instance.selectedCharacter.CurrentCharacter_new.transform.localPosition = new Vector3(0, -2, 0);
           Camera_Pos cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();

            // InitializeFirebase_CB._Instance.LogFirebaseEvent(ServiceManager.instance.selectedCharacter.CharacterBeenDrawn.name + "_SelectedCharacter");
            
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
            // InitializeFirebase_CB._Instance.LogFirebaseEvent(SceneManager.GetActiveScene().name+ "_IsCompleted");
            // InitializeFirebase_CB._Instance.LogFirebaseEvent("PaintScene_IsLoading");
            // Loading.cameFrom = "SubSelection";
            SceneManager.LoadScene("ColorGamePlay");//loading
        }
    public void PlayAnimation()
    {
        // if (PlayerPrefs.GetInt("RemoveAds") == 0)
        // {
        //     IntitializeAdmob.instance.HideBanner();//remove after
        // }
        SoundHandler.instance.PlayTap();
        MouseDown = true;
        timeWithoutTouch = 0;
        PlayerPrefs.SetInt(ServiceManager.instance.selectedCharacter.PrefName, 1);
        PaintingPanel.SetActive(false);
        for (int i = 0; i < Inner.Count; i++)
        {
            Inner[i].Save();
        }

        ActiveAniamtionPanel();
        loading.SetActive(true);
        StartCoroutine(waitForAnimation());
        AnimationBG.SetActive(true);
        SoundHandler.instance.bgm.volume = .2f;
        //float cameSize = 7;
        //if (splash.IsTablet && !campos.OverWriteTab)
        //{
        //    cameSize = campos.CamSizeTab;
        //}

        //HandlerCamera.DOOrthoSize(cameSize, 1f).OnComplete(delegate
        //{
        //    for (int i = 0; i < Inner.Count; i++)
        //    {
        //        Inner[i].Save();
        //    }

        //    ActiveAniamtionPanel();
        //});
        //if (!splash.IsTablet || campos.OverWriteTab)
        //{
        //    HandlerCamera.transform.DOMove(CamPosAnimation, 1.5f);
        //}
        //else
        //{
        //    if (!campos.OverWriteTab)
        //    {
        //        HandlerCamera.transform.DOMove(campos.CameraFinalPositionTab, 1.5f);
        //    }
        //}

    }

    IEnumerator waitForAnimation()
    {
        yield return new WaitForSeconds(1);
        SoundHandler.instance.PlaySource(CompleteS[UnityEngine.Random.Range(0, CompleteS.Length)]);
        
        camer2Helper.BorderImage.GetComponent<CenterGameObjectOnImages>().SetImage();
    }


    public void BackToPaint()
    {
        MouseDown = false;
        ForIndication = true;
        //AnimationBG.transform.DOLocalMove(DefaultPosBG, 1f).OnComplete(() =>
        //{
        //    if (PlayerPrefs.GetInt("RemoveAds") == 0)
        //    {
        //        IntitializeAdmob.instance.ShowBanner();//Lock
        //    }
        //});

        loading.SetActive(true);
        CameraFinalPosition();
        Invoke(nameof(ActivePaintingPanel), .5f);
    }

    public void ActiveAniamtionPanel()
    {
        /*if (AssignAdIds_CB.instance)
            AssignAdIds_CB.instance.HideBanner();*/
        canvasForAds.SetActive(false);
        AnimationPanel.SetActive(true);
        // AnimatedObject.SetActive(true);
        // ServiceManager.instance.selectedCharacter.CurrentCharacter_new.SetActive(false);
    }

    public void ActivePaintingPanel()
    {
        SoundHandler.instance.bgm.volume = 1f;
        AnimationBG.SetActive(false);
        PaintingPanel.SetActive(true);
        PaintingPanel.transform.parent.GetComponent<CanvasGroup>().alpha = 1f;
        AnimationPanel.SetActive(false);
        // AnimatedObject.SetActive(false);
        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            canvasForAds.SetActive(true);
        }

        //Destroy(AnimatedObject);
        AnimateObject_Resource = null;
        ServiceManager.instance.selectedCharacter.CurrentCharacter_new.SetActive(true);
    }

    //public void SwitchAnimation(string ClipName)
    //{
    //    var p = AnimatedObject.GetComponent<Painted_Sprite>();
    //    if (ClipName == "idle")
    //    {
    //        p.source.clip = p.clips[0];
    //        p.source.Play();
    //    }
    //    else if (ClipName == "happy")
    //    {
    //        p.source.clip = p.clips[1];
    //        p.source.Play();
    //    }
    //    else if (ClipName == "happy2")
    //    {
    //        p.source.clip = p.clips[2];
    //        p.source.Play();
    //    }

    //    if (!AnimatedObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(ClipName))
    //    {
    //        AnimatedObject.GetComponent<Animator>().Play(ClipName, 0);
    //    }
    //}

    public void AllDel()
    {
        PlayerPrefs.SetInt(campos.name, 0);
        var p = Path.Combine(Application.persistentDataPath,
            ServiceManager.instance.selectedCharacter.CharacterLocalPath);
        if (Directory.Exists(p))
        {
            if (PlayerPrefs.GetInt(campos.name) == 0)
            {
                Directory.Delete(p, true);
            }
        }

        for (int i = 0; i < campos.WhiteImagePosition.Length; i++)
        {
            campos.WhiteImagePosition[i].gameObject.SetActive(false);
            if (campos.WhiteImagePosition[i].transform.GetSiblingIndex() == 1)
            {
                campos.WhiteImagePosition[i].transform.parent.GetChild(0).gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < campos.Camera_Position.Length; i++)
        {
            for (int j = 0; j < campos.Camera_Position[i].transform.childCount; j++)
            {
                campos.Camera_Position[i].transform.GetChild(j).gameObject.SetActive(false);
            }
        }

        DOTween.KillAll();
        Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReassignCharacterWhiteSprite()
    {
        for (int i = 0; i < WhiteSpriteTemp.Length; i++)
        {
            Sprite sprite = Sprite.Create(WhiteSprite[i], new Rect(0, 0, WhiteSprite[i].width, WhiteSprite[i].height),
                new Vector2(0.5f, 0.5f));
            WhiteSpriteTemp[i].sprite = sprite;
        }

        Resources.UnloadUnusedAssets();
    }

    void Assign_SpriteTemp()
    {
        Camera_Pos cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();
        WhiteSpriteTemp = new SpriteRenderer[cam_pos.Inner_part.Length];
        for (int x = 0; x < cam_pos.Inner_part.Length; x++)
        {
            if (cam_pos.Inner_part[x] != null)
            {
                WhiteSpriteTemp[x] = cam_pos.Inner_part[x];
            }
            else
            {
                WhiteSpriteTemp[x] = null; // or any other desired action
            }
        }
    }
}