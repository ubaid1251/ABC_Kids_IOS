using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
// using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// using Utilities;

namespace KGF.Coloring
{
    [Serializable]
    class SaveData
    {
        public string path;

        public byte[] savedPixels;
        public byte[] savedPixelsOnlyDrawn;
        public byte[] mtexture;
        public float targetCamPositionX;
        public float targetCamPositionY;
        public float targetCamPositionZ;
        public float targetCamSize;


        public byte[] texture
        {
            get { return mtexture; }
            set
            {
                mtexture = value;
                PathUtil.SaveTexture(path, value);
                MonoBehaviour.print("SAve Path: " + path);
            }
        }

        public Sprite GetSprite
        {
            get
            {
                Texture2D mtexture = PathUtil.ReadAllByt(path);
                return Sprite.Create(mtexture, new Rect(0, 0, mtexture.width, mtexture.height), new Vector2(.5f, .5f),
                    100, 1, SpriteMeshType.FullRect);
            }
        }
    }
    //[Serializable]
    //class SaveAtlas
    //{
    //    public Skin drawnSkin;
    //}

    public struct Point
    {
        public short x;
        public short y;

        public Point(short aX, short aY)
        {
            x = aX;
            y = aY;
        }

        public Point(int aX, int aY) : this((short)aX, (short)aY)
        {
        }
    }

    public struct FloodLine
    {
        public int startPixel;

        public int endPixel;

        public string comeFrom;

        public FloodLine(int startPixel, int endPixel, string comeFrom)
        {
            this.startPixel = startPixel;
            this.endPixel = endPixel;
            this.comeFrom = comeFrom;
        }
    }

    public class PaintistaManager : MonoBehaviour
    {
        //public /*static*/ PaintistaManager paintistaManager;

        public enum DrawMode
        {
            Default,
            CustomBrush,
            FloodFill,
            Pattern,
            ShapeLines,
            Eraser,
            FloodFillPattern,
            Sticker
        }

        public Camera CurrentCam;
        public Camera RefCam;
        public GameObject canvas;
        public Renderer myRenderer;

        private RaycastHit hit;
        private RaycastHit[] hits;
        private bool IsHitMissingPaintingLayer;
        private Vector2 pixelUV; // with mouse
        private Vector2 pixelUVOld; // with mouse

        private Vector2[] pixelUVs; // mobiles
        private Vector2[] pixelUVOlds; // mobiles

        private bool textureNeedsUpdate = false; // if we have modified texture

        public bool
            realTimeTexUpdate =
                true; // if set to true, ignore textureUpdateSpeed, and always update when textureNeedsUpdate gets set to true when drawing

        public float
            textureUpdateSpeed = 0.1f; // how often texture should be updated (0 = no delay, 1 = every one seconds)

        private float nextTextureUpdate = 0;

        private Texture2D
            drawingTexture; // texture that we paint into (it gets updated from pixels[] array when painted)

        public string targetTexture = "_MainTex"; // target texture for this material shader (usually _MainTex)

        private bool
            usingClearingImage = false; // did we have initial texture as maintexture, then use it as clear pixels array

        public FilterMode filterMode = FilterMode.Point;

        //	Pixels to Make Core Things work
        private byte[] pixels; // byte array for texture painting, this is the image that we paint into.
        private byte[] maskPixels; // byte array for mask texture
        private byte[] clearPixels; // byte array for clearing texture
        private byte[] floodFillPatternPixels; // byte array for clearing texture
        private byte[] toSavePixels; // byte array for clearing texture

        private byte[] toSaveClearPixels; // byte array for clearing texture
        //private byte[] actualPixels; // byte array for clearing texture
        // private byte[] actualClearPixels; // byte array for clearing texture

        [Header("Brush Settings")]
        //	*** Default settings ***
        public Color32 paintColor = new Color32(255, 0, 0, 255);

        //public int brushSize = 40; // default brush size
        //public int brushSizeMin = 1; // default min brush size
        //public int brushSizeMax = 180; // default max brush size

        // cached calculations
        public bool hiQualityBrush = false; // Draw more brush strokes when moving NOTE: this is slow on mobiles!
        private int brushSizeX1 = 48; // << 1
        private int brushSizeXbrushSize = 576; // x*x
        private int brushSizeX4 = 96; // << 2
        private int brushSizeDiv4 = 6; // >> 2 == /4

        [Header("Overrides")]
        public float resolutionScaler = 1.0f; // 1 means screen resolution, 0.5f means half the screen resolution

        public bool overrideResolution = false;
        public int overrideWidth = 1280;
        public int overrideHeight = 720;

        private float scaleAdjust = 1.0f;
        private const float BASE_WIDTH = 800;
        private const float BASE_HEIGHT = 480;

        public bool createCanvasMesh = true; // default canvas is full screen quad, if disabled existing mesh is used

        public Vector2
            canvasSizeAdjust =
                new Vector2(0,
                    0); // this means, "ScreenResolution.xy+screenSizeAdjust.xy" (use only minus values, to add un-drawable border on right or bottom)

        // canvas clear color
        public Color32 clearColor = new Color32(255, 255, 255, 255);

        public bool
            canDrawOnBlack = true; // to stop filling on mask black lines, FIXME: not working if its not pure black..

        public RectTransform referenceArea; // we will match the size of this reference object
        private float canvasScaleFactor = 1; // canvas scaling factor (will be taken from Canvas)

        private int texWidth;
        private int texHeight;

        [Header("Options")]
        public DrawMode drawMode = DrawMode.CustomBrush; // drawing modes: 0 = Default, 1 = custom brush, 2 = floodfill

        public bool useLockArea = false; // locking mask: only paint in area of the color that your click first

        public bool
            useMaskLayerOnly = false; // if true, only check pixels from mask layer, not from the painted texture

        public bool smoothenMaskEdges = false; // less white edges with mask
        public bool useThreshold = false;
        public byte paintThreshold = 128; // 0 = only exact match, 255 = match anything

        // AREA FILL CALCULATIONS
        [Space(10)] public bool
            getAreaSize =
                false; // NOTE: to use this, someone has to listen the event AreaWasPainted (see scene "scene_MobilePaint_LockingMaskWithAreaCalculation")

        int initialX = 0;
        int initialY = 0;

        public delegate void AreaWasPainted(int fullArea, int filledArea, float percentageFilled, Vector3 point);

        public event AreaWasPainted AreaPaintedEvent;
        private byte[] lockMaskPixels; // locking mask pixels


        [Header("Custom Brushes")] public bool useCustomBrushes = false;
        public Texture2D[] customBrushes;
        public Texture2D[] RotatedTextures;
        public int[,] DRotatedTextures = new int[20, 5460];
        int BrushTextureRotator = 0;
        int BrushTextureRotatorLimit = 10;
        public bool overrideCustomBrushColor = false; // uses paint color instead of brush texture color

        public bool
            useCustomBrushAlpha = true; // true = use alpha from brush, false = use alpha from current paint color

        public int selectedBrush = 0; // currently selected brush index

        private float brushAlphaLerpVal = 0.1f;

        //private Color[] customBrushPixels;
        private byte[] customBrushBytes;
        private int customBrushWidth;
        private int customBrushHeight;

        private int customBrushWidthHalf;

        //		private int customBrushHeightHalf;
        private int texWidthMinusCustomBrushWidth;
        private int texHeightMinusCustomBrushHeight;
        private float CurrentCounter = 0f;
        public float currentCounterSpeed = 5f;

        [Space(10)] public LayerMask PaintingLayer;
        float Degree = 0;
        public bool MultiColor = true;

        [Header("Mask/Overlay")] public bool useMaskImage = false;
        public Texture2D maskTex;
        public string SavePath;

        public string DrawingPath;

        //public SaveDataScriptable SavedImage;
        //[HideInInspector]
        public int ShapeIndex = 0;

        // [HideInInspector]
        public int maxShapeIndex = 0;
        public Texture2D[] images;
        public Texture2D White_image;

        private int totalBlackPixels;
        private int totalWhitePixels;
        private int totalcolorPixels;
        private int totalLightBluePixels;
        public Text BlackPixelsText;
        public Text totalPixelsText;
        public GameObject selectedObj;
        public GameObject character;
        private bool SetNextCanvas = false;
        public Vector3 targetCamPosition = new Vector3(0, 0, -10f);

        public float targetCamSize = 3f;

        //public float camSizeConstant = 166f;
        private Vector2 PrevHitPosition;
        public float nextPicRemainingTime = 0;
        public float timeWithoutTouch = 0;
        float RealtimeWithoutTouch = 0;
        public bool LoadNextImage = false;
        private Animator animator;
        public bool isBackButtonUsed = false;
        public Camera_Pos cam_pos;
        private bool isTargetCamSizeForAnim;

        public GameObject objToFollow;
        public Vector3[] path;

        [Header("Custom Patterns")] public bool useCustomPatterns = false;
        private int customPatternWidth;
        private int customPatternHeight;
        public Texture2D[] customPatterns;
        public Texture2D RainbowPattern;
        public Texture2D currentPattern;
        public int selectedPattern = 0;

        public Texture2D[] Stickers;
        public int CurrentSticker;

        Color fadeInAndFadeOutColor;

        byte[] temparrayforPattern;
        bool isTablet = false;
        bool isGoingBack = false;
        bool isGoingForward = false;
        public Vector3 currentAttachmentPos;
        public GameObject particleEffect;
        public GameObject UndoBtn;
        public GameObject RedoBtn;
        private bool IsAbleToSaveGame = false;
        public GameObject handIndicatorOnNext;

        static int FILL_VALUE_ID = Shader.PropertyToID("_FillValue");
        static int MOUSE_VALUE_ID = Shader.PropertyToID("_Mouse");
        float fillValue;
        bool fillAnimate;
        Texture2D tex;
        public GameObject MatObj;
        Material radialMat;
        public Mesh floodFillMesh;
        public Shader radialFillShader;
        public Texture2D radialFillTexture;

        public Texture2D gradientTextureTemp;

        // public AudioSource ColorCompSound;
        public ParticleSystem colorfill_complete, DragP;
        public bool isComplete = false;
        public bool isLast = false;
        public GameObject NextBtn;

        //public string AnimationPath;
        //public GameObject Final_Character;
        //public Transform AnimationPosition;
        // public GameObject ColoringPanel;
        // public GameObject AnimationPanel;
        public int BrushSize_Custom = 70;
        public int Self_Index;

        bool sticker;

        // Effect
        // public List<GameObject> ButtonEffect;
        public GameObject eventS;

        bool btnDown = false;

        public float timer = 0;

        /*private void Start()
        {
            /*Degree = UnityEngine.Random.Range(0, 360);
#if UNITY_ANDROID
            //AdsManager.instance.ShowBanner();
#endif
            sticker = false;#1#
        }*/

        // public IEnumerator StartButtonEffectB()
        // {
        //     yield return new WaitForSeconds(0.2f);
        //     for (int i = 6; i < ButtonEffect.Count; i++)
        //     {
        //         ButtonEffect[i].transform.DOScale(Vector3.one * 0.8f, 0.2f).SetDelay((i - 6) * 0.05f);
        //         if (i == ButtonEffect.Count - 1)
        //         {
        //             ButtonEffect[i].transform.DOScale(Vector3.one, 0.2f).SetDelay(((i - 6) * 0.05f) + 0.2f).OnComplete(
        //                 () =>
        //                 {
        //                     pencilMagicSubBtn.transform.DOLocalMoveX(-61f, 0.35f);
        //                     particleEffect.GetComponent<ParticleSystem>().Play();
        //
        //                     pencilMainBtn.transform.DOLocalMoveX(-19f, 0.35f);
        //                 });
        //         }
        //         else
        //         {
        //             ButtonEffect[i].transform.DOScale(Vector3.one, 0.2f).SetDelay(((i - 6) * 0.05f) + 0.2f);
        //         }
        //     }
        // }
        //
        // public IEnumerator StartButtonEffect()
        // {
        //     yield return new WaitForSeconds(0f);
        //     //Camera.main.GetComponent<PlayAudio>().PlayPaintistaStartAudio();
        //     //StartCoroutine(Camera.main.GetComponent<PlayAudio>().WaitWhile(() =>
        //     //{
        //     //    StartCoroutine(fadeInCurrentSprite());
        //     //    Debug.Log("Fade In Current Sprite)");
        //     //}, 1f));
        //     for (int i = 0; i < 6; i++)
        //     {
        //         ButtonEffect[i].transform.DOScale(Vector3.one * 0.8f, 0.2f).SetDelay(i * 0.05f);
        //     }
        // }

        public static Vector2 GetAspectRatio(int x, int y)
        {
            float f = (float)x / (float)y;
            int i = 0;
            while (true)
            {
                i++;
                if (System.Math.Round(f * i, 2) == Mathf.RoundToInt(f * i))
                    break;
            }

            return new Vector2((float)System.Math.Round(f * i, 2), i);
        }

        public static float DeviceDiagonalSizeInInches()
        {
            float screenWidth = Screen.width / Screen.dpi;
            float screenHeight = Screen.height / Screen.dpi;
            float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

            return diagonalInches;
        }

        public void EventforCompletedCharacter(string charName, string categroyName)
        {
            Dictionary<string, object> property = new Dictionary<string, object>();
        }

        public SpriteSkin bonesData;

        public void PlayAnimation()
        {
            //Instantiate(Final_Character, Handler.MyHandler.AnimationBG.transform.parent);
            Handler.MyHandler.AnimationBG.transform.DOLocalMove(Vector3.zero, 0.5f);
        }

        public bool IsOutline = false;

        void Start()
        {
            Degree = UnityEngine.Random.Range(0, 360);
#if UNITY_ANDROID
            //AdsManager.instance.ShowBanner();
#endif
            sticker = false;


            Degree = UnityEngine.Random.Range(0, 360);
            /*}
            private void Awake()
            {*/

            Debug.Log(("cause Error"));

            MatObj = new GameObject();
            MatObj.transform.position = new Vector3(0, 0, -5f);
            MatObj.AddComponent<MeshRenderer>();
            MatObj.GetComponent<MeshRenderer>().materials = new Material[] { new Material(radialFillShader) };
            radialMat = MatObj.GetComponent<MeshRenderer>().materials[0];
            radialMat.SetTexture("_FillTex", radialFillTexture);
            PlayerPrefs.SetInt("PatternModeTexture", 0);
            PlayerPrefs.SetInt("FloodPatternModeTexture", 0);
            PlayerPrefs.SetString("FloodFillModeColor", "0,255,255,Multi");
            PlayerPrefs.SetString("DefaultModeColor", "Multi");
            PlayerPrefs.SetString("CustomModeColor", "Multi");
            //isTablet = (DeviceDiagonalSizeInInches() > 6.5f && aspect < 2f);
            if (ResCheck.ResolutionType == ResType.tab)
            {
                isTablet = true;
            }
            else
            {
                isTablet = false;
            }

            if (SystemInfo.systemMemorySize > 3000) //when RAM is greater than 2.5 gb
                hiQualityBrush = true;
            else
                hiQualityBrush = false;
            //objToFollow.AddComponent<PolygonCollider2D>();
            character = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.gameObject; // waqas 
            //character.transform.parent.position = new Vector3(0f, -2, 0f);
            character.transform.position = new Vector3(0f, 0f, 0f);
            character.transform.localScale = new Vector3(1f, 1f, 1f);
            cam_pos = ServiceManager.instance.selectedCharacter.CurrentCharacter_new.GetComponent<Camera_Pos>();

            DrawingPath = ServiceManager.instance.selectedCharacter.CharacterLocalPath;
            DrawingPath = Path.Combine(Application.persistentDataPath, DrawingPath);
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                images = new Texture2D[ServiceManager.instance.selectedCharacter.NumberOfImages];
                for (int i = 0; i < ServiceManager.instance.selectedCharacter.NumberOfImages; i++)
                {
                    images[i] = ServiceManager.instance.selectedCharacter.images[i];
                }

                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                {
                    BrushSize_Custom = Handler.MyHandler.PencilSizeForOutline; //extra added by me
                }
                else
                {
                    // Debug.Log("current Index: " + SwitchToolsSubGrid.currentIndex);
                    BrushSize_Custom = Handler.MyHandler.BrushSizeForOutline;
                }
            }
            else
            {
                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                {
                    BrushSize_Custom = Handler.MyHandler.PencilSizeForInner; //extra added by me
                }
                else
                {
                    BrushSize_Custom = Handler.MyHandler.BrushSizeForInner;
                }

                print("resetttt");
                White_image = Handler.MyHandler.WhiteSprite[Self_Index];
            }

            Input.multiTouchEnabled = false;

            IsHitMissingPaintingLayer = false;
            ShapeIndex = 0;
            if (ShapeIndex < 0)
            {
                ShapeIndex = 0;
            }

            cam_pos.maxShapeIndex = maxShapeIndex = ShapeIndex;
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(images[ShapeIndex],
                    new Rect(0, 0, images[ShapeIndex].width, images[ShapeIndex].height), new Vector2(.5f, .5f));
            }
            else
            {
                referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(White_image,
                    new Rect(0, 0, White_image.width, White_image.height), new Vector2(.5f, .5f));
            }

            referenceArea.gameObject.GetComponent<Image>().SetNativeSize();
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                SavePath = Path.Combine(DrawingPath, images[ShapeIndex].name + ".dat");
            }
            else
            {
                SavePath = Path.Combine(DrawingPath, White_image.name + ".dat");
            }

            if (ShapeIndex > 0)
            {
                UndoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                if (File.Exists(SavePath))
                {
                    targetCamPosition = cam_pos.PositionAndScale[ShapeIndex].pos;
                    targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                }
            }
            else
            {
                if (IsOutline)
                {
                    targetCamPosition = cam_pos.PositionAndScale[ShapeIndex].pos;
                }
                else
                {
                    currentAttachmentPos = cam_pos.CameraFinalPosition;
                    Debug.Log(currentAttachmentPos);
                    targetCamPosition = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y, -10f); //checked
                }
            }

            StartupChecking();
            SetNextCanvas = true;
            if (IsOutline)
            {
                cam_pos.Apply(ShapeIndex, true);
            }
            else
            {
                cam_pos.Apply(Self_Index);
            }

            if (ShapeIndex == 0 && IsOutline)
            {
                cam_pos.PlayIntro();
            }
        }

        void Update()
        {
            //if (!Handler.isHome)
            {

                PaintOnMouseTouch();

                if (textureNeedsUpdate /*&& (realTimeTexUpdate || Time.time > nextTextureUpdate)*/)
                {
                    //Debug.Log("Update Texture !!!!!!!!!!");
                    nextTextureUpdate = Time.time + textureUpdateSpeed;
                    UpdateTexture();
                }

                //if (targetCamPosition != CurrentCam.transform.position) //mesh filter 
                //{
                //    if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                //    {
                //        var newpos = Vector3.MoveTowards(CurrentCam.transform.position, targetCamPosition, 0.05f);
                //        CurrentCam.transform.position = new Vector3(newpos.x, newpos.y, CurrentCam.transform.position.z);
                //        RefCam.transform.position = new Vector3(newpos.x, newpos.y, CurrentCam.transform.position.z);
                //        //RefCam.orthographicSize = CurrentCam.orthographicSize*2;
                //    }
                //}

                // For use of hand indication
                {
                    timeWithoutTouch = Time.deltaTime + timeWithoutTouch;
                    RealtimeWithoutTouch = Time.deltaTime + RealtimeWithoutTouch;
                }
                if (Handler.MyHandler.timeWithoutTouch > 5 && Handler.MyHandler.ForIndication)
                {
                    Handler.MyHandler.ForIndication = false;
                    if (!Handler.MyHandler.OutlineCompleted)
                    {
                        Indication();
                    }
                    else
                    {
                        Handler.MyHandler.CallAllIndicationOff();
                    }
                }

                if (RealtimeWithoutTouch > 10 && RealtimeWithoutTouch % 10 < 1)
                {
                    //if (!Camera.main.GetComponent<PlayAudio>().IsPlaying())
                    //    Camera.main.GetComponent<PlayAudio>().PlayCharAudio(Camera.main.GetComponent<PlayAudio>().remindingVoice);
                }

                if (LoadNextImage)
                {
                    if (!isComplete)
                    {
                        nextPicRemainingTime = Time.deltaTime + nextPicRemainingTime;
                        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        colorfill_complete.transform.position = new Vector3(pos.x, pos.y, -1f);
                        //colorfill_complete.Play();
                        if (nextPicRemainingTime > 1.5f)
                        {
                            if (!isBackButtonUsed)
                            {
                                //Handler.MyHandler.eventSystem.SetActive(false);
                                Debug.Log("isBackButtonUsed = False!!!!!!!!!!!!");
                                LoadNextImage = false;
                                NextImage();
                            }
                            else if (Handler.MyHandler.timeWithoutTouch > 4)
                            {
                                Debug.Log("Time without touch > 4!!!!!!!!!!!!");
                                Handler.MyHandler.timeWithoutTouch = 0f;
                                handIndicatorOnNext.GetComponent<Animator>().Play("HandNextAnimation");
                            }
                        }
                    }
                }
                else
                {
                    nextPicRemainingTime = 0;
                }

                if (IsOutline)
                {
                    //float debVal= (float)Math.Round(CurrentCam.orthographicSize, 2);
                    //CurrentCam.orthographicSize = (float)Math.Round(CurrentCam.orthographicSize, 2);
                    //print(debVal+" camera val");
                    //if (CurrentCam.orthographicSize > targetCamSize)
                    //{
                    //    CurrentCam.orthographicSize -= 0.02f;
                    //}
                    //else if (CurrentCam.orthographicSize < targetCamSize)
                    //{
                    //    CurrentCam.orthographicSize += 0.02f;
                    //}
                    if (SetNextCanvas /* && targetCamPosition == CurrentCam.transform.position && targetCamSize == CurrentCam.orthographicSize*/
                       )
                    {
                        if (isGoingBack)
                        {
                            Debug.Log("isGoingBack!!!!!!!!!!!!!!");
                            cam_pos.Apply(ShapeIndex, true);
                            isGoingBack = false;
                        }
                    }
                    else
                    {
                        //Debug.Log("return (Update) ");
                        return;
                    }
                }

                // if (IsOutline)
                {
                    if (SetNextCanvas /*&& targetCamPosition == CurrentCam.transform.position */ && !isLast &&
                        !isComplete)
                    {
                        if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/
                           )
                        {
                            SetNextCanvas = false;
                            Debug.Log("images[ShapeIndex]!!!!!!!!!!!!!!!!!! in (update) " + images[ShapeIndex]);

                            Debug.Log(isLast + " " + isComplete + " Is last ");
                            SetCanvasImage(images[ShapeIndex], ShapeIndex);
                            Color objectColor = GetComponent<Renderer>().material.color;
                            GetComponent<Renderer>().material.color =
                                new Color(objectColor.r, objectColor.g, objectColor.b, 1);
                        }
                        else
                        {
                            SetNextCanvas = false;
                            Debug.Log(isLast + " " + isComplete + " yar ya wala q nai chal raha ");
                            SetCanvasImage(White_image, Self_Index);
                            Color objectColor = this.GetComponent<Renderer>().material.color;
                            this.GetComponent<Renderer>().material.color =
                                new Color(objectColor.r, objectColor.g, objectColor.b, 1);
                        }
                    }
                }
            }
        }



        int CalculateBannerHeight()
        {
            if (Screen.height <= 400 * Mathf.RoundToInt(Screen.dpi / 160))
            {
                return 32 * Mathf.RoundToInt(Screen.dpi / 160);
            }
            else if (Screen.height <= 720 * Mathf.RoundToInt(Screen.dpi / 160))
            {
                return 50 * Mathf.RoundToInt(Screen.dpi / 160);
            }
            else
            {
                return 90 * Mathf.RoundToInt(Screen.dpi / 160);
            }
        }

        private void StartupChecking()
        {
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                if (CurrentCam == null)
                {
                    targetCamPosition = CurrentCam.transform.position;
                    Debug.LogError("Camera Not Found");
                }

                if (RefCam == null)
                {
                    targetCamPosition = RefCam.transform.position;
                    Debug.LogError("RefCamera Not Found");
                }
            }


            if (PaintingLayer.value == 0)
            {
                Debug.LogError("Assign Some Painting Layer to draw on canvas");
            }

            ;
            // Custom brushes validation
            if (useCustomBrushes && (customBrushes == null || customBrushes.Length < 1))
            {
                Debug.LogWarning(
                    "useCustomBrushes is enabled, but no custombrushes assigned to array, disabling customBrushes",
                    gameObject);
                useCustomBrushes = false;
            }

            // Custom patterns validation
            if (useCustomPatterns && (customPatterns == null || customPatterns.Length < 1))
            {
                Debug.LogWarning(
                    "useCustomPatterns is enabled, but no customPatterns assigned to array, disabling useCustomPatterns",
                    gameObject);
                useCustomPatterns = false;
            }

            // MASK validation
            if (useMaskImage)
            {
                if (maskTex == null)
                {
                    Debug.LogWarning("maskImage is not assigned. Setting 'useMaskImage' to false", gameObject);
                    useMaskImage = false;
                    if (overrideResolution)
                        Debug.LogWarning("overrideResolution cannot be used, when useMaskImage is true", gameObject);
                }
            }

            if (getAreaSize)
            {
                if (!useThreshold || !useMaskLayerOnly)
                {
                    Debug.LogWarning(
                        "getAreaSize is enabled, but both useThreshold or useMaskLayerOnly are not enabled, getAreaSize might not work",
                        gameObject);
                }
            }

            // check if target texture exists
            if (!myRenderer.material.HasProperty(targetTexture))
                Debug.LogError("Fatal error: Current shader doesn't have a property: '" + targetTexture + "'",
                    gameObject);


            RotatedTextures = new Texture2D[BrushTextureRotatorLimit];
            for (int i = 0; i < BrushTextureRotatorLimit; i++)
            {
                RotatedTextures[i] = new Texture2D(1, 1);
            }

            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                // EnableToolsGrid();
                // StartCoroutine(StartButtonEffect());
                // StartCoroutine(StartButtonEffectB());
            }
        }

        private void InitializeAllThings()
        {
            ChangeBrushSize();
            // cached calculations
            Debug.Log("Initialize all things start");
            brushSizeX1 = BrushSize_Custom << 1;
            brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
            brushSizeX4 = brushSizeXbrushSize << 2;

            // calculate scaling ratio for different screen resolutions
            float _baseHeightInverted = 1.0f / BASE_HEIGHT;
            float ratio = (Screen.height * _baseHeightInverted) * scaleAdjust;
            canvasSizeAdjust *= ratio;

            // WARNING: fixed maximum amount of touches, is set to 20 here. Not sure if some device supports more?
            pixelUVs = new Vector2[20];
            pixelUVOlds = new Vector2[20];

            if (createCanvasMesh)
            {
                Debug.Log("Create Canvas Mesh");
                CreateFullScreenQuad();
            }
            else
            {
                // using existing mesh
                Debug.Log("create Canvas Mesh is false");
                // if (connectBrushStokes) Debug.LogWarning("Custom mesh used, but connectBrushStokes is enabled, it can cause problems on the mesh borders wrapping");
                if (GetComponent<MeshCollider>() == null)
                    Debug.LogError("MeshCollider is missing, won't be able to raycast to canvas object");
                if (GetComponent<MeshFilter>() == null || GetComponent<MeshFilter>().sharedMesh == null)
                    Debug.LogWarning("Mesh or MeshFilter is missing, won't be able to see the canvas object");
            }

            // create texture
            if (useMaskImage)
            {
                SetMaskImage(maskTex);
            }
            else // no mask texture
            {
                //Debug.Log("Use Mask Image is false");
                if (overrideResolution)
                {
                    Debug.Log("Override Resolution is true");
                    var err = false;
                    if (overrideWidth < 0 || overrideWidth > 4096) err = true;
                    if (overrideHeight < 0 || overrideHeight > 4096) err = true;
                    if (err) Debug.LogError("overrideWidth or overrideWidth is invalid - clamping to 4 or 4096");
                    texWidth = (int)Mathf.Clamp(overrideWidth, 4, 4096);
                    texHeight = (int)Mathf.Clamp(overrideHeight, 4, 4096);
                }
                else
                {
                    // use screen size as texture size
                    Debug.Log("Override Resolution is false");
                    texWidth = (int)(Screen.width * resolutionScaler + canvasSizeAdjust.x);
                    texHeight = (int)(Screen.height * resolutionScaler + canvasSizeAdjust.y);
                }
            }

            // we have no texture set for canvas, FIXME: this returns true if called initialize again, since texture gets created after this
            if (myRenderer.material.GetTexture(targetTexture) == null &&
                !usingClearingImage) // temporary fix by adding && !usingClearingImage
            {
                // create new texture
                Debug.Log("new texture is creating");
                if (drawingTexture != null) Texture2D.DestroyImmediate(drawingTexture, true); // cleanup old texture
                drawingTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
                myRenderer.material.SetTexture(targetTexture, drawingTexture);
                // init pixels array
                pixels = new byte[texWidth * texHeight * 4];
            }
            else
            {
                // we have canvas texture, then use that as clearing texture
                Debug.Log("we have canvas texture, then use that as clearing texture");
                usingClearingImage = true;

                //if (overrideResolution) Debug.LogWarning("overrideResolution is not used, when canvas texture is assiged to material, we need to use the texture size");
                texWidth = myRenderer.material.GetTexture(targetTexture).width;
                texHeight = myRenderer.material.GetTexture(targetTexture).height;

                // init pixels array
                pixels = new byte[texWidth * texHeight * 4];

                if (drawingTexture != null) Texture2D.DestroyImmediate(drawingTexture, true); // cleanup old texture
                drawingTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

                // we keep current maintex and read it as "clear pixels array" (so when "clear image" is clicked, original texture is restored

                ReadClearingImage();
                myRenderer.material.SetTexture(targetTexture, drawingTexture);
            }

            // locking mask enabled
            if (useLockArea)
            {
                lockMaskPixels = new byte[texWidth * texHeight * 4];
            }

            if (customPatterns != null && customPatterns.Length > 0)
            {
                Debug.Log("Custom patterns is not null and the length is grater then zero");
                int divisor;
                int width;
                int height;
                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    divisor = images[ShapeIndex].height > images[ShapeIndex].width
                        ? images[ShapeIndex].height
                        : images[ShapeIndex].width;
                    width = (int)(divisor / 2.5f);
                    height = (int)(divisor / 2.5f);
                }
                else
                {
                    divisor = White_image.height > White_image.width ? White_image.height : White_image.width;
                    width = (int)(divisor / 2.5f);
                    height = (int)(divisor / 2.5f);
                }

                Resize(customPatterns[selectedPattern], width, height);


                if (texWidth > texHeight)
                {
                    //Debug.Log("text width is grater then tex height");
                    LoadFloodFillPattern(RainbowPattern, texWidth, texWidth);
                }
                else
                {
                    //Debug.Log("text width is less then tex height");
                    LoadFloodFillPattern(RainbowPattern, texHeight, texHeight);
                }
            }

            drawingTexture.filterMode = filterMode;
            drawingTexture.wrapMode = TextureWrapMode.Clamp;

            if (useCustomBrushes && drawMode == DrawMode.CustomBrush) ReadCurrentCustomBrush();

            ClearImage(updateUndoBuffer: false);
            LoadImageAsCanvas();

            tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
            Debug.Log("Initialize all things end");

            if (!IsOutline)
                GetComponent<Collider>().enabled = true;
        }

        private CanvasGroup canvasGroup;

        private void UIVisibilityOff()
        {
            if (canvasGroup == null)
            {
                var referenceCanvas = canvas;

                if (referenceCanvas.GetComponent<CanvasGroup>())
                    canvasGroup = referenceCanvas.GetComponent<CanvasGroup>();
                else
                    canvasGroup = referenceCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            if (canvasGroup)
            {
                canvasGroup.DOKill(false);
                //Handler.MyHandler.eventSystem.SetActive(false);
                canvasGroup.DOFade(0, 0.2f); //Canvas Fade
            }
        }

        private void UIVisibilityON()
        {
            if (canvasGroup == null)
            {
                var referenceCanvas = canvas;

                if (referenceCanvas.GetComponent<CanvasGroup>())
                    canvasGroup = referenceCanvas.GetComponent<CanvasGroup>();
                else
                    canvasGroup = referenceCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            if (canvasGroup)
            {
                canvasGroup.DOKill(false);
                canvasGroup.DOFade(1, 0.2f) /*.OnComplete(() =>
                {
                    //Handler.MyHandler.eventSystem.SetActive(true);
                })*/; //Canvas Fade
            }
        }

        private void LateUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                btnDown = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                SoundHandler.instance.stopPaint();
                btnDown = false;
            }
        }

        void LoopColor()
        {
            Degree += 1.25f;
            if (Degree > 358)
            {
                if (btnDown)
                {
                    Degree = 0;
                }
                else
                {
                    Degree = UnityEngine.Random.Range(0, 360);
                }
            }

            paintColor = Color.HSVToRGB(map01(Degree, 0, 360), 1f, 1f);
        }

        public float map01(float value, float min, float max)
        {
            return (value - min) * 1f / (max - min);
        }

        //bool enableEventSystem = false;
        private void PaintOnMouseTouch()
        {
            if (!LoadNextImage)
            {
                if (Input.GetMouseButtonDown(0)) // use for Sticker 
                {
                    timer = Time.time;
                    // LoadNextImage = false;
                    nextPicRemainingTime = 0;
                    // if lock area is used, we need to take full area before painting starts
                    if (useLockArea)
                    {
                        if (!Physics.Raycast(CurrentCam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity,
                                PaintingLayer)) return;
                        CreateAreaLockMask((int)(hit.textureCoord.x * texWidth), (int)(hit.textureCoord.y * texHeight));
                    }
                    //timeWithoutTouch = 0;

                    if (DrawMode.Sticker == drawMode)
                    {
                        if (!Physics.Raycast(CurrentCam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity,
                                PaintingLayer)) return;
                        MergeTextures(Stickers[CurrentSticker], (int)(hit.textureCoord.x * texWidth),
                            (int)(hit.textureCoord.y * texHeight));
                    }
                }

                if (Input.GetMouseButton(0) && Handler.MyHandler.Selected == null && Handler.MyHandler.MouseDown)
                {
                    /*Debug.Log("Layer: " + hit.transform.gameObject.layer);*/
                    if (!Physics.Raycast(CurrentCam.ScreenPointToRay(Input.mousePosition), out hit,
                            Handler.MyHandler.raycastVal, PaintingLayer) && Handler.MyHandler.MouseDown)
                    {
                        IsHitMissingPaintingLayer = true;
                        SoundHandler.instance.stopPaint();
                        return;
                    }
                    else if (!SoundHandler.instance.paintS.isPlaying && DrawMode.Sticker != drawMode)
                    {
                        SoundHandler.instance.playPaint();
                    }
                }

                if (Input.GetMouseButton(0) && Handler.MyHandler.Selected == this ||
                    Input.GetMouseButton(0) && IsOutline && Handler.MyHandler.MouseDown)
                {
                    if (Time.time - timer > 3f && !LoadNextImage && !isComplete)
                    {
                        // SoundManager.instance.PlayInPaintSounds();
                        timer = Time.time;
                    }

                    //if ray does not hit layer then do nothing to draw
                    //if (hit.collider == null)
                    if (!Physics.Raycast(CurrentCam.ScreenPointToRay(Input.mousePosition), out hit,
                            Handler.MyHandler.raycastVal, PaintingLayer))
                    {
                        print("inlayerrrrrr");
                        IsHitMissingPaintingLayer = true;
                        SoundHandler.instance.stopPaint();
                        return;
                    }
                    else if (!SoundHandler.instance.paintS.isPlaying && DrawMode.Sticker != drawMode)
                    {
                        SoundHandler.instance.playPaint();
                    }

                    LoadNextImage = false;
                    Handler.MyHandler.timeWithoutTouch = 0;
                    RealtimeWithoutTouch = 0;
                    Debug.Log("isable to save game " + IsAbleToSaveGame);
                    IsAbleToSaveGame = true;
                    PrevHitPosition = hit.textureCoord;
                    pixelUVOld = pixelUV; // take previous value, so can compare them
                    pixelUV = hit.textureCoord;
                    pixelUV.x *= texWidth;
                    pixelUV.y *= texHeight;
                    int CurrentPixel = (texWidth * ((int)pixelUV.y) + (int)pixelUV.x) << 2;

                    if (MultiColor)
                        LoopColor();
                    if (IsHitMissingPaintingLayer)
                    {
                        pixelUVOld = pixelUV;
                        IsHitMissingPaintingLayer = false;
                    }

                    // lets paint where we hit
                    if (IsOutline || Handler.MyHandler.Selected == this && Handler.MyHandler.paint &&
                        Handler.MyHandler.Selected != null)
                    {
                        if (!IsOutline)
                        {
                            Debug.Log(Self_Index + " Self index is");
                            //SaveGame();
                        }

                        Debug.Log("In Switch Condition");
                        switch (drawMode)
                        {
                            case DrawMode.Default: // brush
                                DrawCircle((int)pixelUV.x, (int)pixelUV.y);
                                //DrawCustomBrush((int)pixelUV.x, (int)pixelUV.y);
                                //DrawPatternCircle((int)pixelUV.x, (int)pixelUV.y);
                                break;

                            case DrawMode.Pattern:
                                DrawPatternCircle((int)pixelUV.x, (int)pixelUV.y);
                                //DrawLineWithPattern((int)pixelUV.x, (int)pixelUV.y);
                                break;

                            case DrawMode.CustomBrush:
                                //DrawCircle((int)pixelUV.x, (int)pixelUV.y);
                                DrawCustomBrush((int)pixelUV.x, (int)pixelUV.y);
                                break;

                            case DrawMode.FloodFill:
                                if (Input.GetMouseButtonDown(0))
                                    CallFloodFill((int)pixelUV.x, (int)pixelUV.y, hit.textureCoord.x,
                                        hit.textureCoord.y);
                                break;

                            case DrawMode.ShapeLines:
                                break;


                            case DrawMode.FloodFillPattern:
                                if (Input.GetMouseButtonDown(0))
                                {
                                    FloodFillPattern((int)pixelUV.x, (int)pixelUV.y, hit.textureCoord.x,
                                        hit.textureCoord.y);
                                }

                                break;
                            case DrawMode.Sticker:
                                break;

                            case DrawMode.Eraser:

                                EraseWithImage((int)pixelUV.x, (int)pixelUV.y);

                                break;


                            default: // unknown DrawMode
                                Debug.LogError("Unknown drawMode");
                                break;
                        }

                        if (DrawMode.FloodFill == drawMode || DrawMode.FloodFillPattern == drawMode)
                            textureNeedsUpdate = false;
                        else
                            textureNeedsUpdate = true;
                    }

                    CurrentPixel = (texWidth * ((int)hit.textureCoord.y) + (int)hit.textureCoord.x) << 2;
                    pixelUVOld = pixelUV;
                    //if (Input.GetMouseButtonDown(0))
                    //{
                    // take this position as start position
                    //int CurrentPixel = (texWidth * ((int)hit.textureCoord.y) + (int)hit.textureCoord.x) << 2;


                    //pixelUVOld = pixelUV;

                    //  timeWithoutTouch = 0;
                    //}
                    if (textureNeedsUpdate)
                    {
                        Debug.Log("Draw Line");
                        switch (drawMode)
                        {
                            //case DrawMode.Default: // drawing
                            //    DrawLine(pixelUVOld, pixelUV);
                            //    break;

                            //case DrawMode.CustomBrush:
                            //    DrawLineWithBrush(pixelUVOld, pixelUV);
                            //    break;

                            case DrawMode.Pattern:
                                DrawLineWithPattern(pixelUVOld, pixelUV);
                                break;

                            case DrawMode.Eraser:

                                EraseWithImageLine(pixelUVOld, pixelUV);
                                break;

                            default: // other modes
                                break;
                        }

                        pixelUVOld = pixelUV;
                        if (DrawMode.FloodFill == drawMode || DrawMode.FloodFillPattern == drawMode)
                        {
                            textureNeedsUpdate = false;
                        }
                        else
                        {
                            textureNeedsUpdate = true;
                        }
                    }
                }

                // left mouse button released
                if (Input.GetMouseButtonUp(0))
                {
                    SoundHandler.instance.stopPaint();
                    if (gameObject != null)
                    {
                        Debug.Log(isComplete + " " + NextBtn);
                        if (isComplete)
                        {
                            print("destroy the paintista here and make inner images textures");
                            NextBtn.SetActive(true);
                            //Handler.MyHandler.NextBtn.SetActive(true);
                        }

                        Debug.Log("On Mouse up " + IsAbleToSaveGame + isComplete);


                        // calculate area size
                        if (getAreaSize && useLockArea && useMaskLayerOnly && drawMode != DrawMode.FloodFill)
                        {
                            LockAreaFillWithThresholdMaskOnlyGetArea(initialX, initialY, true);
                        }

                        /*if(IsOutline)
                        {
                            if (ShapeIndex < cam_pos.PositionAndScale.Length - 1 || !Handler.MyHandler.startWhiteImage && gameObject.name == "PaintistaManager")
                            {
                                Debug.Log("----ShapeIndex outline: " + ShapeIndex + " ||" + cam_pos.PositionAndScale.Length);
                                Handler.MyHandler.eventSystem.SetActive(true);
                                Handler.MyHandler.checkForOutline = true;
                            }
                        }
                        else
                        {
                            if (!Handler.MyHandler.startWhiteImage && !Handler.MyHandler.checkForOutline )
                            {
                                Debug.Log("----ShapeIndex WhiteImage: " + ShapeIndex + " ||" + cam_pos.PositionAndScale.Length);
                                Handler.MyHandler.eventSystem.SetActive(true);
                            }
                        }*/
                        /*if (ShapeIndex < cam_pos.PositionAndScale.Length - 1 || !Handler.MyHandler.startWhiteImage)
                        {
                            Debug.Log("ShapeIndex: " + ShapeIndex + " ||" + cam_pos.PositionAndScale.Length);
                            Handler.MyHandler.eventSystem.SetActive(true);
                        }*/
                        if (CurrentCam.orthographicSize != targetCamSize)
                        {
                            return;
                        }

                        if (targetCamPosition != CurrentCam.transform.position)
                        {
                            return;
                        }

                        if (IsAbleToSaveGame)
                        {
                            if (DrawMode.FloodFill != drawMode && DrawMode.FloodFillPattern != drawMode)
                                StartCoroutine(TakeScreenShot());
                        }
                    }
                }
            }
/*            else
            {
                Debug.Log("White Image Check in Update");
            }*/
        }

        public void Save()
        {
            SaveGame();
        }

        public void DrawCircle(int x, int y)
        {
            if (!IsOutline)
            {
                DrawSmoothCircle(x, y);
            }
            else
            {
                int pixel = 0;

                for (int i = 0; i < brushSizeX4; i++)
                {
                    int tx = (i % brushSizeX1) - BrushSize_Custom;
                    int ty = (i / brushSizeX1) - BrushSize_Custom;

                    if (tx * tx + ty * ty > brushSizeXbrushSize) continue;
                    if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight)
                        continue; // temporary fix for corner painting

                    pixel = (texWidth * (y + ty) + x + tx) << 2;

                    //just paint my color
                    //if ((!useLockArea || (useLockArea && lockMaskPixels[pixel] == 1)) && !(pixels[pixel] == 244 && pixels[pixel + 1] == 244 && pixels[pixel + 2] == 244))
                    if (pixels[pixel + 3] > 0)
                    {
                        pixels[pixel] = paintColor.r;
                        pixels[pixel + 1] = paintColor.g;
                        pixels[pixel + 2] = paintColor.b;
                        pixels[pixel + 3] = paintColor.a;

                        toSavePixels[pixel] = paintColor.r;
                        toSavePixels[pixel + 1] = paintColor.g;
                        toSavePixels[pixel + 2] = paintColor.b;
                        toSavePixels[pixel + 3] = paintColor.a;
                    }
                } // for area
            }
        }

        public void DrawSmoothCircle(int x, int y)
        {
            int pixel = 0;

            for (int i = 0; i < brushSizeX4; i++)
            {
                int tx = (i % brushSizeX1) - BrushSize_Custom;
                int ty = (i / brushSizeX1) - BrushSize_Custom;

                if (tx * tx + ty * ty > brushSizeXbrushSize) continue;
                if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight) continue;

                pixel = (texWidth * (y + ty) + x + tx) << 2;

                // Calculate interpolation factor based on distance from the center of the brush
                float distance = Mathf.Sqrt(tx * tx + ty * ty);
                float normalizedDistance = distance / BrushSize_Custom;

                // Adjusted interpolation factor for smoother blending
                float interpolationFactor = 1.0f - Mathf.Clamp01(normalizedDistance);

                // Blend the colors using interpolation regardless of alpha value
                Color currentColor = new Color(pixels[pixel] / 255f, pixels[pixel + 1] / 255f, pixels[pixel + 2] / 255f,
                    pixels[pixel + 3] / 255f);
                Color blendedColor = Color.Lerp(currentColor, paintColor, interpolationFactor);

                if (pixels[pixel + 3] > 0)
                {
                    pixels[pixel] = (byte)(blendedColor.r * 255f);
                    pixels[pixel + 1] = (byte)(blendedColor.g * 255f);
                    pixels[pixel + 2] = (byte)(blendedColor.b * 255f);
                    pixels[pixel + 3] = (byte)(blendedColor.a * 255f);

                    toSavePixels[pixel] = pixels[pixel];
                    toSavePixels[pixel + 1] = pixels[pixel + 1];
                    toSavePixels[pixel + 2] = pixels[pixel + 2];
                    toSavePixels[pixel + 3] = pixels[pixel + 3];
                }
            }
        }


        void DrawCustomBrush(int px, int py)
        {
            DrawCustomBrushOutline(px, py);
            /*CurrentCounter += 0.2f;
            if (CurrentCounter > currentCounterSpeed)
            {
                CurrentCounter = 0;
                ReadCurrentCustomBrush();
                //random = UnityEngine.Random.Range(0,20);
            }
            // get position where we paint
            int startX = (int)(px - customBrushWidthHalf);
            int startY = (int)(py - customBrushWidthHalf);

            if (startX < 0)
            {
                startX = 0;
            }
            else
            {
                if (startX + customBrushWidth >= texWidth) startX = texWidthMinusCustomBrushWidth;
            }

            if (startY < 1)  // TODO: temporary fix, 1 instead of 0
            {
                startY = 1;
            }
            else
            {
                if (startY + customBrushHeight >= texHeight) startY = texHeightMinusCustomBrushHeight;
            }

            // could use this for speed (but then its box shaped..)
            //System.Array.Copy(splatPixByte,0,data,4*(startY*startX),splatPixByte.Length);

            int pixel = (texWidth * startY + startX) << 2;
            int brushPixel = 0;
            for (int y = 0; y < customBrushHeight; y++)
            {
                for (int x = 0; x < customBrushWidth; x++)
                {
                    brushPixel = (customBrushWidth * (y) + x) << 2;
                    //byte brushAlpha = customBrushBytes[DRotatedTextures[random, brushPixel]];

                    // we have some color at this brush pixel?

                    if (customBrushBytes[brushPixel + 3] > 0 && (pixels[pixel + 3] > 0))
                    {


                        if (useCustomBrushAlpha) // use alpha from brush
                        {
                            if (overrideCustomBrushColor)
                            {
                                pixels[pixel] = (byte)(pixels[pixel] + (paintColor.r - pixels[pixel]) * brushAlphaLerpVal);//ByteLerp(pixels[pixel], paintColor.r, brushAlphaLerpVal);
                                pixels[pixel + 1] = (byte)(pixels[pixel + 1] + (paintColor.g - pixels[pixel + 1]) * brushAlphaLerpVal);// ByteLerp(pixels[pixel + 1], paintColor.g, brushAlphaLerpVal);
                                pixels[pixel + 2] = (byte)(pixels[pixel + 2] + (paintColor.b - pixels[pixel + 2]) * brushAlphaLerpVal);// ByteLerp(pixels[pixel + 2], paintColor.b, brushAlphaLerpVal);

                                if (toSavePixels[pixel + 3] > 0 && toSavePixels[pixel + 3] < 100)
                                {
                                    toSavePixels[pixel] = (byte)(255 + (paintColor.r - 255) * brushAlphaLerpVal);// ByteLerp(255, paintColor.r, brushAlphaLerpVal);
                                    toSavePixels[pixel + 1] = (byte)(255 + (paintColor.g - 255) * brushAlphaLerpVal);// ByteLerp(255, paintColor.g, brushAlphaLerpVal);
                                    toSavePixels[pixel + 2] = (byte)(255 + (paintColor.b - 255) * brushAlphaLerpVal);//ByteLerp(255, paintColor.b, brushAlphaLerpVal);

                                }
                                else
                                {
                                    toSavePixels[pixel] = (byte)(toSavePixels[pixel] + (paintColor.r - toSavePixels[pixel]) * brushAlphaLerpVal);//  ByteLerp(toSavePixels[pixel], paintColor.r, brushAlphaLerpVal);
                                    toSavePixels[pixel + 1] = (byte)(toSavePixels[pixel + 1] + (paintColor.g - toSavePixels[pixel + 1]) * brushAlphaLerpVal);//  ByteLerp(toSavePixels[pixel + 1], paintColor.g, brushAlphaLerpVal);
                                    toSavePixels[pixel + 2] = (byte)(toSavePixels[pixel + 2] + (paintColor.b - toSavePixels[pixel + 2]) * brushAlphaLerpVal);//  ByteLerp(toSavePixels[pixel + 2], paintColor.b, brushAlphaLerpVal);
                                }
                            }
                            else
                            { // use paint color instead of brush texture
                                pixels[pixel] = customBrushBytes[brushPixel];
                                pixels[pixel + 1] = customBrushBytes[brushPixel + 1];
                                pixels[pixel + 2] = customBrushBytes[brushPixel + 2];

                                toSavePixels[pixel] = customBrushBytes[brushPixel];
                                toSavePixels[pixel + 1] = customBrushBytes[brushPixel + 1];
                                toSavePixels[pixel + 2] = customBrushBytes[brushPixel + 2];
                            }

                            if (toSavePixels[pixel + 3] > 0 && toSavePixels[pixel + 3] < 100)
                            {
                                toSavePixels[pixel + 3] = 255;
                                pixels[pixel + 3] = (byte)(pixels[pixel + 3] + (paintColor.a - pixels[pixel + 3]) * brushAlphaLerpVal);//  ByteLerp(pixels[pixel + 3], paintColor.a, brushAlphaLerpVal);
                            }
                            else
                            {
                                toSavePixels[pixel + 3] = 255;
                                pixels[pixel + 3] = (byte)(pixels[pixel + 3] + (paintColor.a - pixels[pixel + 3]) * brushAlphaLerpVal);// ByteLerp(pixels[pixel + 3], paintColor.a, brushAlphaLerpVal);
                            }
                            // pixels[pixel + 3] = 255;

                        }
                        else
                        { // use paint color alpha

                            if (overrideCustomBrushColor)
                            {
                                pixels[pixel] = ByteLerp(pixels[pixel], paintColor.r, brushAlphaLerpVal);
                                pixels[pixel + 1] = ByteLerp(pixels[pixel + 1], paintColor.g, brushAlphaLerpVal);
                                pixels[pixel + 2] = ByteLerp(pixels[pixel + 2], paintColor.b, brushAlphaLerpVal);

                                toSavePixels[pixel] = ByteLerp(toSavePixels[pixel], paintColor.r, brushAlphaLerpVal);
                                toSavePixels[pixel + 1] = ByteLerp(toSavePixels[pixel + 1], paintColor.g, brushAlphaLerpVal);
                                toSavePixels[pixel + 2] = ByteLerp(toSavePixels[pixel + 2], paintColor.b, brushAlphaLerpVal);
                            }
                            else
                            {
                                pixels[pixel] = customBrushBytes[brushPixel];
                                pixels[pixel + 1] = customBrushBytes[brushPixel + 1];
                                pixels[pixel + 2] = customBrushBytes[brushPixel + 2];

                                toSavePixels[pixel] = customBrushBytes[brushPixel];
                                toSavePixels[pixel + 1] = customBrushBytes[brushPixel + 1];
                                toSavePixels[pixel + 2] = customBrushBytes[brushPixel + 2];
                            }

                            pixels[pixel + 3] = ByteLerp(pixels[pixel + 3], paintColor.a, brushAlphaLerpVal);
                            toSavePixels[pixel + 3] = 255;
                        }
                        //  if (actualPixels[pixel + 3] != 0)
                        // toSavePixels[pixel + 3] = pixels[pixel + 3];
                    }

                    pixel += 4;

                } // for x

                pixel = (texWidth * (startY == 0 ? 1 : startY + y) + startX + 1) * 4;
            } // for y*/
        } // DrawCustomBrush

        void DrawCustomBrushOutline(int px, int py)
        {
            CurrentCounter += 0.2f;
            if (CurrentCounter > currentCounterSpeed)
            {
                CurrentCounter = 0;
                ReadCurrentCustomBrush();
            }

            int startX = (int)(px - customBrushWidthHalf);
            int startY = (int)(py - customBrushWidthHalf);

            if (startX < 0)
            {
                startX = 0;
            }
            else if (startX + customBrushWidth >= texWidth)
            {
                startX = texWidth - customBrushWidth;
            }

            if (startY < 1)
            {
                startY = 1;
            }
            else if (startY + customBrushHeight >= texHeight)
            {
                startY = texHeight - customBrushHeight;
            }

            int brushPixel = 0;

            if (customBrushBytes != null &&
                customBrushBytes.Length > 0) // Check if customBrushBytes is initialized and not null
            {
                for (int y = 0; y < customBrushHeight; y++)
                {
                    for (int x = 0; x < customBrushWidth; x++)
                    {
                        brushPixel = (customBrushWidth * y + x) << 2;
                        int pixel = (texWidth * (startY + y) + startX + x) << 2;

                        if (brushPixel >= 0 && brushPixel < customBrushBytes.Length && pixel >= 0 &&
                            pixel < pixels.Length) // Bounds checking
                        {
                            // Check if the current pixel is on the outline
                            bool isOutlinePixel = IsOutlinePixel(brushPixel);

                            if (customBrushBytes[brushPixel + 3] > 0 && (pixels[pixel + 3] > 0))
                            {
                                if (isOutlinePixel && IsOutline)
                                {
                                    // Apply your outline painting logic here
                                    pixels[pixel] = paintColor.r;
                                    pixels[pixel + 1] = paintColor.g;
                                    pixels[pixel + 2] = paintColor.b;
                                    pixels[pixel + 3] = (byte)(paintColor.a * 0.75f);

                                    toSavePixels[pixel] = paintColor.r;
                                    toSavePixels[pixel + 1] = paintColor.g;
                                    toSavePixels[pixel + 2] = paintColor.b;
                                    toSavePixels[pixel + 3] = (byte)(paintColor.a * 0.75f);
                                }
                                else if (isOutlinePixel && !IsOutline)
                                {
                                    pixels[pixel] = paintColor.r;
                                    pixels[pixel + 1] = paintColor.g;
                                    pixels[pixel + 2] = paintColor.b;
                                    pixels[pixel + 3] = (byte)(paintColor.a);

                                    toSavePixels[pixel] = paintColor.r;
                                    toSavePixels[pixel + 1] = paintColor.g;
                                    toSavePixels[pixel + 2] = paintColor.b;
                                    toSavePixels[pixel + 3] = (byte)(paintColor.a);
                                }
                            }
                        }
                    }
                }
            }
        }

        void DrawCustomBrushOutline1(int px, int py)
        {
            CurrentCounter += 0.2f;
            if (CurrentCounter > currentCounterSpeed)
            {
                CurrentCounter = 0;
                ReadCurrentCustomBrush();
            }

            int startX = (int)(px - customBrushWidthHalf);
            int startY = (int)(py - customBrushWidthHalf);

            if (startX < 0)
            {
                startX = 0;
            }
            else if (startX + customBrushWidth >= texWidth)
            {
                startX = texWidth - customBrushWidth;
            }

            if (startY < 1)
            {
                startY = 1;
            }
            else if (startY + customBrushHeight >= texHeight)
            {
                startY = texHeight - customBrushHeight;
            }

            int brushPixel = 0;

            if (customBrushBytes != null &&
                customBrushBytes.Length > 0) // Check if customBrushBytes is initialized and not null
            {
                for (int y = 0; y < customBrushHeight; y++)
                {
                    for (int x = 0; x < customBrushWidth; x++)
                    {
                        brushPixel = (customBrushWidth * y + x) << 2;
                        int pixel = (texWidth * (startY + y) + startX + x) << 2;

                        if (brushPixel >= 0 && brushPixel < customBrushBytes.Length && pixel >= 0 &&
                            pixel < pixels.Length) // Bounds checking
                        {
                            // Check if the current pixel is on the outline
                            bool isOutlinePixel = IsOutlinePixel(brushPixel);

                            if (customBrushBytes[brushPixel + 3] > 0 && (pixels[pixel + 3] > 0))
                            {
                                //Debug.Log("Illyas : R "+ pixels[pixel]);
                                //Debug.Log("Illyas : G "+ pixels[pixel + 1]);
                                //Debug.Log("Illyas : B "+ pixels[pixel + 2]);
                                //Debug.Log("Illyas : A "+ pixels[pixel + 3]);
                                if (isOutlinePixel && IsOutline)
                                {
                                    // Apply your outline painting logic here
                                    pixels[pixel] = paintColor.r;
                                    pixels[pixel + 1] = paintColor.g;
                                    pixels[pixel + 2] = paintColor.b;
                                    pixels[pixel + 3] = (byte)(paintColor.a * 0.75f);

                                    toSavePixels[pixel] = paintColor.r;
                                    toSavePixels[pixel + 1] = paintColor.g;
                                    toSavePixels[pixel + 2] = paintColor.b;
                                    toSavePixels[pixel + 3] = (byte)(paintColor.a * 0.75f);
                                }
                                else if (isOutlinePixel && !IsOutline)
                                {
                                    pixels[pixel] = paintColor.r;
                                    pixels[pixel + 1] = paintColor.g;
                                    pixels[pixel + 2] = paintColor.b;
                                    pixels[pixel + 3] = (byte)(paintColor.a);

                                    toSavePixels[pixel] = paintColor.r;
                                    toSavePixels[pixel + 1] = paintColor.g;
                                    toSavePixels[pixel + 2] = paintColor.b;
                                    toSavePixels[pixel + 3] = (byte)(paintColor.a);
                                }
                            }
                        }
                    }
                }
            }
        }

        bool IsOutlinePixel(int brushPixel)
        {
            // Check if the current pixel in the brush is an outline pixel.
            // You can define your outline detection logic here.
            // For example, check if a neighboring pixel is not fully opaque.

            byte brushAlpha = customBrushBytes[brushPixel + 3];

            // Define your outline detection condition here.
            bool isOutline = (brushAlpha > 0); // Modify this condition as needed.

            return isOutline;
        }


        public void EraseWithImage(int x, int y)
        {
            int pixel = 0;
            for (int i = 0; i < brushSizeX4; i++)
            {
                int tx = (i % brushSizeX1) - BrushSize_Custom;
                int ty = (i / brushSizeX1) - BrushSize_Custom;

                if (tx * tx + ty * ty > brushSizeXbrushSize) continue;
                if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight)
                    continue; // temporary fix for corner painting

                pixel = (texWidth * (y + ty) + x + tx) << 2;

                pixels[pixel] = clearPixels[pixel];
                pixels[pixel + 1] = clearPixels[pixel + 1];
                pixels[pixel + 2] = clearPixels[pixel + 2];
                pixels[pixel + 3] = clearPixels[pixel + 3];

                toSavePixels[pixel] = toSaveClearPixels[pixel];
                toSavePixels[pixel + 1] = toSaveClearPixels[pixel + 1];
                toSavePixels[pixel + 2] = toSaveClearPixels[pixel + 2];
                toSavePixels[pixel + 3] = toSaveClearPixels[pixel + 3];
            }
        }

        int iterator = 0;

        public void Indication()
        {
            if (!Handler.MyHandler.ForIndication)
            {
                myRenderer.material.DOColor(new Color(189f / 255f, 255f / 255f, 255f / 255f, 255f / 255f), .5f)
                    .OnComplete(delegate
                    {
                        myRenderer.material.DOColor(new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f), .5f)
                            .OnComplete(delegate
                            {
                                iterator++;
                                if (iterator >= 3)
                                {
                                    iterator = 0;
                                    Handler.MyHandler.timeWithoutTouch = 0;
                                    Handler.MyHandler.ForIndication = true;
                                    myRenderer.material.DOColor(
                                        new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f), 0f);
                                }

                                Indication();
                            });
                    });
            }
            else
            {
                myRenderer.material.DOColor(new Color(255f / 255f, 255f / 255f, 255f / 255f, 255f / 255f), 0f);
            }
        }

        private void OnMouseDrag()
        {
            Vector3 parPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DragP.transform.position = new Vector3(parPos.x, parPos.y, -1f);
        }

        private void OnMouseDown()
        {
            if (DrawMode.Sticker == drawMode)
            {
                SoundHandler.instance.PlayTap();
            }

            Vector3 parPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DragP.transform.position = new Vector3(parPos.x, parPos.y, -1f);
            DragP.gameObject.SetActive(true);
            UIVisibilityOff();
            Self_Index = GetComponent<PaintistaManager>().Self_Index;
            Handler.MyHandler.Selected = this;
            Handler.MyHandler.paint = true;
            Handler.MyHandler.OffInnerCollider(Self_Index);
            Handler.MyHandler.PointerDownFunc();
            Handler.MyHandler.MouseDown = true;
            Handler.MyHandler.ForIndication = true;
            Handler.MyHandler.CallAllIndicationOff();
        }

        private void OnMouseUp()
        {
            DragP.gameObject.SetActive(false);
            UIVisibilityON();
            Handler.MyHandler.MouseDown = false;
            Handler.MyHandler.Selected = null;
            Handler.MyHandler.paint = false;
            Handler.MyHandler.OnInnerCollider();
        }

        public void DrawPatternCircle(int x, int y)
        {
            Debug.Log(("Draw Circles Fun"));
            int pixel = 0;
            for (int i = 0; i < brushSizeX4; i++)
            {
                int tx = (i % brushSizeX1) - BrushSize_Custom;
                int ty = (i / brushSizeX1) - BrushSize_Custom;

                if (tx * tx + ty * ty > brushSizeXbrushSize) continue;
                if (x + tx < 0 || y + ty < 0 || x + tx >= texWidth || y + ty >= texHeight)
                    continue; // temporary fix for corner painting

                pixel = (texWidth * (y + ty) + x + tx) << 2;
                byte r = temparrayforPattern[pixel];
                byte g = temparrayforPattern[pixel + 1];
                byte b = temparrayforPattern[pixel + 2];
                byte a = temparrayforPattern[pixel + 3];
                if (clearPixels[pixel + 3] > 0)
                {
                    pixels[pixel] = r;
                    pixels[pixel + 1] = g;
                    pixels[pixel + 2] = b;
                    pixels[pixel + 3] = a;

                    toSavePixels[pixel] = r;
                    toSavePixels[pixel + 1] = g;
                    toSavePixels[pixel + 2] = b;
                    toSavePixels[pixel + 3] = a;
                }
            }

            //} // for area
        } // DrawPatternCircle()

        void DrawLineWithPattern(Vector2 start, Vector2 end)
        {
            Debug.Log("Draw Pattern Circles ");
            int x0 = (int)start.x;
            int y0 = (int)start.y;
            int x1 = (int)end.x;
            int y1 = (int)end.y;
            int tempVal = x1 - x0;
            int dx = (tempVal + (tempVal >> 31)) ^
                     (tempVal >> 31); // http://stackoverflow.com/questions/6114099/fast-integer-abs-function
            tempVal = y1 - y0;
            int dy = (tempVal + (tempVal >> 31)) ^ (tempVal >> 31);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int pixelCount = 0;
            int e2;
            for (;;)
            {
                if (hiQualityBrush)
                {
                    DrawPatternCircle(x0, y0);
                }
                else
                {
                    pixelCount += 3;
                    if (pixelCount > brushSizeDiv4)
                    {
                        pixelCount = 0;
                        DrawPatternCircle(x0, y0);
                    }
                }

                if ((x0 == x1) && (y0 == y1)) break;
                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                else if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        public Texture2D patternScaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Bilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }

        void UpdateTexture()
        {
            // Debug.Log("UpdateTexture!!!!!!!!!!!!!!!!!!!!!!!");
            //Debug.Log(gameObject.name);
            textureNeedsUpdate = false;
            drawingTexture.LoadRawTextureData(pixels);
            drawingTexture.Apply(false);
        }

        public void ReadClearingImage()
        {
            Debug.Log("Painting");

            clearPixels = new byte[texWidth * texHeight * 4];
            toSavePixels = new byte[texWidth * texHeight * 4];
            toSaveClearPixels = new byte[texWidth * texHeight * 4];

            drawingTexture.SetPixels32(((Texture2D)myRenderer.material.GetTexture(targetTexture)).GetPixels32());
            drawingTexture.Apply(false);

            int pixel = 0;
            Color32[] tempPixels = drawingTexture.GetPixels32();
            int tempCount = tempPixels.Length;
            totalBlackPixels = 0;
            totalWhitePixels = 0;
            totalcolorPixels = 0;
            totalLightBluePixels = 0;
            for (int i = 0; i < tempCount; i++)
            {
                if ((tempPixels[i].r == 76 && tempPixels[i].g == 76 && tempPixels[i].b == 76 && tempPixels[i].a > 0) ||
                    (tempPixels[i].r == 179 && tempPixels[i].g == 179 && tempPixels[i].b == 179 && tempPixels[i].a > 0))
                {
                    totalBlackPixels++;
                }

                if (tempPixels[i].r == 255 && tempPixels[i].g == 255 && tempPixels[i].b == 255 &&
                    tempPixels[i].a == 255)
                {
                    totalWhitePixels++;
                }

                if (tempPixels[i].a > 2 && tempPixels[i].a < 255 &&
                    (tempPixels[i].r >= 0 && tempPixels[i].g >= 0 && tempPixels[i].b >= 0))
                {
                    totalLightBluePixels++;
                }

                clearPixels[pixel] = tempPixels[i].r;
                clearPixels[pixel + 1] = tempPixels[i].g;
                clearPixels[pixel + 2] = tempPixels[i].b;
                clearPixels[pixel + 3] = tempPixels[i].a;

                if (tempPixels[i].a <= 0)
                {
                }
                else if (tempPixels[i].r == 255 && tempPixels[i].g == 255 && tempPixels[i].b == 255)
                {
                    clearPixels[pixel + 3] = 255;
                    totalcolorPixels++;
                }
                else
                {
                    totalcolorPixels++;
                }

                toSaveClearPixels[pixel + 3] = toSavePixels[pixel + 3] = clearPixels[pixel + 3];

                if (clearPixels[pixel] == 255 && clearPixels[pixel + 1] == 255 && clearPixels[pixel + 2] == 255 &&
                    clearPixels[pixel + 3] >= 200)
                {
                    toSaveClearPixels[pixel] = toSavePixels[pixel] = 255;
                    toSaveClearPixels[pixel + 1] = toSavePixels[pixel + 1] = 255;
                    toSaveClearPixels[pixel + 2] = toSavePixels[pixel + 2] = 255;
                    toSaveClearPixels[pixel + 3] = toSavePixels[pixel + 3] = 255;
                }
                else
                {
                    toSaveClearPixels[pixel] = toSavePixels[pixel] = 225;
                    toSaveClearPixels[pixel + 1] = toSavePixels[pixel + 1] = 255;
                    toSaveClearPixels[pixel + 2] = toSavePixels[pixel + 2] = 255;
                    toSaveClearPixels[pixel + 3] = toSavePixels[pixel + 3] = 1;
                }

                pixel += 4;
            }

            // BlackPixelsText.text = "Black Area 100 %";
            // totalPixelsText.text = "Total Area 100 %";
        }

        public void ClearImage(bool updateUndoBuffer)
        {
            if (usingClearingImage)
            {
                ClearImageWithImage();
                //SaveGame();
            }
            else
            {
                int pixel = 0;
                for (int y = 0; y < texHeight; y++)
                {
                    for (int x = 0; x < texWidth; x++)
                    {
                        pixels[pixel] = clearColor.r;
                        pixels[pixel + 1] = clearColor.g;
                        pixels[pixel + 2] = clearColor.b;
                        pixels[pixel + 3] = clearColor.a;
                        pixel += 4;
                    }
                }

                UpdateTexture();
            }
        } // clear image

        public void ClearImageWithImage()
        {
            // fill pixels array with clearpixels array
            System.Array.Copy(clearPixels, 0, pixels, 0, clearPixels.Length);
            System.Array.Copy(toSaveClearPixels, 0, toSavePixels, 0, toSavePixels.Length);
            //System.Array.Copy(actualClearPixels, 0, actualPixels, 0, actualPixels.Length);

            // just assign our clear image array into tex
            drawingTexture.LoadRawTextureData(clearPixels);
            drawingTexture.Apply(false);
        } // clear image

        // float tabSize = 0;

        void CreateFullScreenQuad()
        {
            Debug.Log("Create Full Screen Quad start");
            // create mesh plane, fits in camera view (with screensize adjust taken into consideration)
            Mesh go_Mesh = GetComponent<MeshFilter>().mesh;
            Debug.Log(go_Mesh.name + "Mesh name");
            go_Mesh.Clear();
            Vector3[] referenceCorners = new Vector3[4];

            if (referenceArea) // use reference object & canvas scaling for size
            {
                print("asdsadasasdasdasdasdasdas");
                if (referenceArea == null)
                    Debug.LogError("RectTransform not assigned in " + transform.name, gameObject);

                // NOTE: this fails, if canvas is not direct parent of the referenceArea object?
                //var temporaryCanvasArray = referenceArea.GetComponentsInParent<Canvas>();
                //var temporaryCanvasArray = referenceArea.GetComponentsInParent<Canvas>();

                //if (temporaryCanvasArray == null || temporaryCanvasArray.Length == 0) Debug.LogError("Canvas not found from ReferenceArea parent", gameObject);
                //if (temporaryCanvasArray.Length > 1) Debug.LogError("More than 1 Canvas was found from ReferenceArea parent, can cause problems", gameObject);

                var referenceCanvas = canvas.GetComponent<Canvas>(); // take first canvas
                if (referenceCanvas == null) Debug.LogError("Canvas not found from ReferenceArea parent", gameObject);

                // get current scale factor
                canvasScaleFactor = referenceCanvas.scaleFactor;
                canvasScaleFactor = 1f;
                // float change = Screen.height > Screen.width? 1008f / Screen.height: 1792f / Screen.width;
                float change = 1008f / Screen.height;
                // get vertex positions for borders
                referenceCorners[0] =
                    new Vector3(referenceArea.offsetMin.x / change, referenceArea.offsetMin.y / change, 0);
                referenceCorners[1] =
                    new Vector3(referenceArea.offsetMin.x / change, referenceArea.offsetMax.y / change, 0);
                referenceCorners[2] =
                    new Vector3(referenceArea.offsetMax.x / change, referenceArea.offsetMax.y / change, 0);
                referenceCorners[3] =
                    new Vector3(referenceArea.offsetMax.x / change, referenceArea.offsetMin.y / change, 0);

                // reset Z position and center/scale to camera view
                for (int i = 0; i < referenceCorners.Length; i++)
                {
                    referenceCorners[i] = referenceCorners[i];
                    referenceCorners[i].z = -RefCam.transform.position.z;
                }

                go_Mesh.vertices = referenceCorners;
            }
            else
            {
                // just use full screen quad for main camera

                referenceCorners[0] = new Vector3(0, canvasSizeAdjust.y, RefCam.nearClipPlane); // bottom left
                referenceCorners[1] =
                    new Vector3(0, RefCam.pixelHeight + canvasSizeAdjust.y, RefCam.nearClipPlane); // top left
                referenceCorners[2] = new Vector3(RefCam.pixelWidth + canvasSizeAdjust.x,
                    RefCam.pixelHeight + canvasSizeAdjust.y, RefCam.nearClipPlane); // top right
                referenceCorners[3] = new Vector3(RefCam.pixelWidth + canvasSizeAdjust.x, canvasSizeAdjust.y,
                    RefCam.nearClipPlane); // bottom right
            }

            // move to screen
            float nearClipOffset = 0.01f; // otherwise raycast wont hit, if exactly at nearclip z


            for (int i = 0; i < referenceCorners.Length; i++)
            {
                referenceCorners[i].z = -RefCam.transform.position.z + nearClipOffset;
                referenceCorners[i] = RefCam.ScreenToWorldPoint(referenceCorners[i]);
            }

            go_Mesh.vertices = referenceCorners;

            go_Mesh.uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            go_Mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };

            go_Mesh.RecalculateNormals();
            go_Mesh.RecalculateBounds();

            go_Mesh.tangents = new[]
            {
                new Vector4(1.0f, 0.0f, 0.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, -1.0f),
                new Vector4(1.0f, 0.0f, 0.0f, -1.0f), new Vector4(1.0f, 0.0f, 0.0f, -1.0f)
            };

            // add mesh collider
            if (gameObject.GetComponent<MeshCollider>() == null)
            {
                //if (IsOutline)
                gameObject.AddComponent<MeshCollider>().enabled = false;
            }
            else
            {
                Destroy(gameObject.GetComponent<MeshCollider>());
                gameObject.AddComponent<MeshCollider>().enabled = false;
            }

            Bounds MeshBounds = go_Mesh.bounds;
            Vector3 offset = this.transform.position - this.transform.TransformPoint(MeshBounds.center);
            Debug.Log(offset);
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                currentAttachmentPos = cam_pos.GetCurrentPosition(ShapeIndex);
            }
            else
            {
                if (Handler.MyHandler.OutlineCompleted)
                {
                    currentAttachmentPos = cam_pos.WhiteImagePos(Self_Index);
                }
                else
                {
                    currentAttachmentPos = cam_pos.GetCurrentPosition(Self_Index);
                    Debug.Log(currentAttachmentPos);
                }
            }

            //right
            // if (ResCheck.instance.resType==ResType.tab)
            // {
            //     if ((go_Mesh.bounds.extents.x + currentAttachmentPos.x + 1f) >
            //         CurrentCam.transform.position.x + CurrentCam.orthographicSize)
            //     {
            //         var sizeToAdd =
            //             (float)Math.Round(
            //                 ((go_Mesh.bounds.extents.x + currentAttachmentPos.x + 1f) -
            //                  (CurrentCam.transform.position.x + (CurrentCam.orthographicSize))), 1);
            //         Debug.Log("size of camera  " + sizeToAdd);
            //         tabSize = sizeToAdd;
            //     }
            //
            //     //top
            //     if ((go_Mesh.bounds.extents.y + currentAttachmentPos.y + 0.4f) >
            //         CurrentCam.transform.position.y + CurrentCam.orthographicSize)
            //     {
            //         var sizeToAdd =
            //             (float)Math.Round(
            //                 ((go_Mesh.bounds.extents.y + currentAttachmentPos.y) -
            //                  (CurrentCam.transform.position.y + CurrentCam.orthographicSize)) / 2, 1);
            //         Debug.Log("size of camera  " + sizeToAdd);
            //         tabSize += sizeToAdd;
            //     }
            //
            //
            //     //left
            //     if ((-go_Mesh.bounds.extents.x + currentAttachmentPos.x - 0.4f) <
            //         CurrentCam.transform.position.x - CurrentCam.orthographicSize * 1.5f)
            //     {
            //         var sizeToAdd =
            //             (float)Math.Round(
            //                 ((CurrentCam.transform.position.x - (CurrentCam.orthographicSize * 1.5f)) -
            //                  (-go_Mesh.bounds.extents.x + currentAttachmentPos.x)) / 2, 1);
            //         Debug.Log("size of camera  " + sizeToAdd);
            //         tabSize += sizeToAdd;
            //     }
            //
            //     //bottom
            //     if ((-go_Mesh.bounds.extents.y + currentAttachmentPos.y - 0.4f) <
            //         CurrentCam.transform.position.y - CurrentCam.orthographicSize)
            //     {
            //         var sizeToAdd =
            //             (float)Math.Round(
            //                 ((CurrentCam.transform.position.y - CurrentCam.orthographicSize) -
            //                  (-go_Mesh.bounds.extents.y + currentAttachmentPos.y)) / 2, 1);
            //         Debug.Log("size of camera  " + sizeToAdd);
            //         tabSize += sizeToAdd;
            //     }
            // }
            // else
            // {
            //     tabSize = 0;
            // }


            if (ShapeIndex > 0)
            {
                Debug.Log("Shape index" + ShapeIndex);
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
                transform.position = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y, -0.2f) + offset;
                //if (isTablet)
                //    targetCamPosition = new Vector3((currentAttachmentPos.x), (currentAttachmentPos.y), -10f);//checked
                //else
                //    targetCamPosition = new Vector3((currentAttachmentPos.x), (currentAttachmentPos.y), -10f);//checked
                if (isTablet)
                    //targetCamPosition = new Vector3((currentAttachmentPos.x), (currentAttachmentPos.y+Math.Abs(currentAttachmentPos.y)), -10f);
                    targetCamPosition = cam_pos.PositionAndScaleTab[ShapeIndex].pos;
                else
                {
                    //targetCamPosition = new Vector3((currentAttachmentPos.x), (currentAttachmentPos.y), -10f);
                    targetCamPosition = cam_pos.PositionAndScale[ShapeIndex].pos;
                }

                if (go_Mesh.bounds.extents.y > go_Mesh.bounds.extents.x)
                {
                    if (isTablet)
                    {
                        //targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                        targetCamSize = cam_pos.PositionAndScaleTab[ShapeIndex].camSize;
                        Debug.Log("Camrea size = " + targetCamSize);
                    }
                    else
                    {
                        //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.y) + 0.5, 1);
                        targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                        Debug.Log("Camrea size = " + targetCamSize);
                    }
                }
                else
                {
                    if (isTablet)
                    {
                        //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.x) + 1, 1);
                        targetCamSize = cam_pos.PositionAndScaleTab[ShapeIndex].camSize;
                        Debug.Log("Camrea size = " + targetCamSize);
                        if (targetCamSize >= 5)
                        {
                            //targetCamSize = 4.2f; //Camerea size resetting 
                            if (!IsOutline)
                            {
                                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                                {
                                    BrushSize_Custom = Handler.MyHandler.BrushSizeForInner; //extra added by me
                                }
                                else
                                {
                                    BrushSize_Custom = Handler.MyHandler.BrushSizeForInner;
                                }
                            }
                            else
                            {
                                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                                {
                                    BrushSize_Custom = Handler.MyHandler.PencilSizeForOutline; //extra added by me
                                }
                                else
                                {
                                    // Debug.Log("current Index: " + SwitchToolsSubGrid.currentIndex);
                                    BrushSize_Custom = Handler.MyHandler.BrushSizeForOutline;
                                }
                            }
                        }
                        else
                        {
                            if (!IsOutline)
                            {
                                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                                {
                                    BrushSize_Custom = Handler.MyHandler.PencilSizeForInner; //extra added by me
                                }
                                else
                                {
                                    BrushSize_Custom = Handler.MyHandler.BrushSizeForInner;
                                }
                            }
                            else
                            {
                                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                                {
                                    BrushSize_Custom = Handler.MyHandler.PencilSizeForOutline; //extra added by me
                                }
                                else
                                {
                                    // Debug.Log("current Index: " + SwitchToolsSubGrid.currentIndex);
                                    BrushSize_Custom = Handler.MyHandler.BrushSizeForOutline;
                                }
                            }
                        }
                    }
                    else
                    {
                        // targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.x) + 0.5, 1);
                        targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                        Debug.Log("Camrea size = " + targetCamSize);
                    }
                }

                if (targetCamSize < 2)
                {
                    Debug.Log("Camrea size is Less then 2");
                    //targetCamSize = 2;
                    targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                }


                if (cam_pos.hideFirstSprite)
                {
                    if (ShapeIndex == 1 && !cam_pos.digitsMoreThanOne)
                    {
                        cam_pos.Camera_Position[ShapeIndex - 1].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() => { cam_pos.Camera_Position[ShapeIndex - 1].gameObject.SetActive(false); });
                        cam_pos.WhiteImagePosition[ShapeIndex - 1].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() =>
                            {
                                cam_pos.WhiteImagePosition[ShapeIndex - 1].gameObject.SetActive(false);
                            });
                    }
                    else if (ShapeIndex == 2 && cam_pos.digitsMoreThanOne)
                    {
                        cam_pos.Camera_Position[ShapeIndex - 2].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() => { cam_pos.Camera_Position[ShapeIndex - 2].gameObject.SetActive(false); });
                        cam_pos.WhiteImagePosition[ShapeIndex - 2].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() =>
                            {
                                cam_pos.WhiteImagePosition[ShapeIndex - 2].gameObject.SetActive(false);
                            });

                        cam_pos.Camera_Position[ShapeIndex - 1].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() => { cam_pos.Camera_Position[ShapeIndex - 1].gameObject.SetActive(false); });
                        cam_pos.WhiteImagePosition[ShapeIndex - 1].GetComponent<SpriteRenderer>().DOFade(0, 0.33f)
                            .OnComplete(() =>
                            {
                                cam_pos.WhiteImagePosition[ShapeIndex - 1].gameObject.SetActive(false);
                            });
                    }
                }

                // tabSize = (tabSize / cam_pos.TabOffSet);
                // targetCamSize += tabSize;
                print(ShapeIndex + "shapeInd");
                CurrentCam.DOOrthoSize(targetCamSize, .75f);
                CurrentCam.transform.DOMove(targetCamPosition, .75f).OnComplete(() =>
                {
                    print("HelloEYS");
                    StartCoroutine(fadeInCurrentSprite());
                });
                RefCam.transform.DOMove(targetCamPosition, .75f).SetEase(Ease.Linear);
            }
            else
            {
                if (isBackButtonUsed)
                {
                    //print("adas");
                    GetComponent<MeshRenderer>().enabled = false;
                }

                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    if (go_Mesh.bounds.extents.y > go_Mesh.bounds.extents.x)
                    {
                        if (isTablet)
                        {
                            //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.y) + 1.2f, 1);
                            targetCamSize = cam_pos.PositionAndScaleTab[ShapeIndex].camSize;
                        }
                        else
                        {
                            //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.y) + 0.5, 1);
                            targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                            Debug.Log("Camrea size = " + targetCamSize + "   " + go_Mesh.bounds.extents.y);
                        }
                    }
                    else
                    {
                        if (isTablet)
                        {
                            //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.x) + 1, 1);
                            targetCamSize = cam_pos.PositionAndScaleTab[ShapeIndex].camSize;
                        }
                        else
                        {
                            //targetCamSize = (float)Math.Round((go_Mesh.bounds.extents.x) + 0.5, 1);
                            targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                            Debug.Log("Camrea size = " + targetCamSize);
                        }
                    }

                    //CurrentCam.orthographicSize = targetCamSize;
                    Debug.Log("Camerea size in create full screen quard" + CurrentCam.orthographicSize);
                    if (isTablet)
                    {
                        //  targetCamPosition = new Vector3(currentAttachmentPos.x + 1.3f, currentAttachmentPos.y+.95f, -10f);//checked-fixed
                        targetCamPosition = cam_pos.PositionAndScaleTab[ShapeIndex].pos;
                        Debug.Log("Camerea size in create full screen quard" + targetCamPosition);
                    }
                    else
                    {
                        //targetCamPosition = new Vector3(currentAttachmentPos.x + 1, currentAttachmentPos.y+.95f, -10f);//checked-fixed
                        targetCamPosition = cam_pos.PositionAndScale[ShapeIndex].pos;
                        Debug.Log("Camerea size in create full screen quard" + targetCamPosition);
                    }

                    // targetCamSize += tabSize;
                    if (ShapeIndex == 0)
                    {
                        CurrentCam.DOOrthoSize(targetCamSize, 0.5f);
                        CurrentCam.transform.DOMove(targetCamPosition, .5f).OnComplete(() =>
                        {
                            print("HelloEYS1");
                            StartCoroutine(fadeInCurrentSprite());
                        });
                        RefCam.transform.DOMove(targetCamPosition, 0.5f).SetEase(Ease.Linear);
                    }
                    else
                    {
                        CurrentCam.DOOrthoSize(targetCamSize, .75f);
                        CurrentCam.transform.DOMove(targetCamPosition, .75f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            print("HelloEYS");
                            if (IsOutline && ShapeIndex == 0)
                            {
                                StartCoroutine(fadeInCurrentSprite());
                            }
                        });
                        RefCam.transform.position = targetCamPosition;
                    }
                }
                else
                {
                    GetComponent<MeshRenderer>().enabled = true;
                    isComplete = true;


                    if (cam_pos.hideFirstSprite)
                    {
                        if (!cam_pos.digitsMoreThanOne)
                        {
                            cam_pos.Camera_Position[0].gameObject.SetActive(true);
                            cam_pos.Camera_Position[0].GetComponent<SpriteRenderer>().DOFade(1, 0f);
                            cam_pos.WhiteImagePosition[0].gameObject.SetActive(true);
                            cam_pos.WhiteImagePosition[0].GetComponent<SpriteRenderer>().DOFade(1, 0f);
                        }
                        else
                        {
                            cam_pos.Camera_Position[0].gameObject.SetActive(true);
                            cam_pos.Camera_Position[0].GetComponent<SpriteRenderer>().DOFade(1, 0f);
                            cam_pos.WhiteImagePosition[0].gameObject.SetActive(true);
                            cam_pos.WhiteImagePosition[0].GetComponent<SpriteRenderer>().DOFade(1, 0f);

                            cam_pos.Camera_Position[1].gameObject.SetActive(true);
                            cam_pos.Camera_Position[1].GetComponent<SpriteRenderer>().DOFade(1, 0f);
                            cam_pos.WhiteImagePosition[1].gameObject.SetActive(true);
                            cam_pos.WhiteImagePosition[1].GetComponent<SpriteRenderer>().DOFade(1, 0f);
                        }
                    }
                    //Handler.MyHandler.CameraFinalPosition();
                    //BrushSize_Custom = 40;
                }
                //CurrentCam.transform.position = targetCamPosition;

                if (Handler.MyHandler.OutlineCompleted)
                {
                    Debug.Log("Z position");
                    transform.position = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y,
                        cam_pos.ZPosition[Self_Index]) + offset;
                    GetComponent<MeshRenderer>().sortingOrder = cam_pos.Inner_part[Self_Index].sortingOrder;
                }
                else
                {
                    Debug.Log("Default position");
                    transform.position = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y, 1f) + offset;
                }

                isGoingForward = false;
                isGoingBack = false;
            }

            //CalculatePathOfShape();
            floodFillMesh = MeshUtil.createPlaneMesh(MeshBounds.size.x, MeshBounds.size.y);
            var meshFilterComp = MatObj.GetComponent<MeshFilter>();
            if (meshFilterComp != null)
            {
                Debug.Log("purany wala mesh");
                meshFilterComp.mesh = floodFillMesh;
            }
            else
            {
                Debug.Log("Mesh component add ho gya ha ");
                MatObj.AddComponent<MeshFilter>().mesh = floodFillMesh;
            }

            MatObj.transform.position = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y, -0.3f);
            //Debug.Log("Create Full Screen Quad end");
        }

        public void DrawLine(Vector2 start, Vector2 end)
        {
            DrawLine((int)start.x, (int)start.y, (int)end.x, (int)end.y);
        }
        // draw line between 2 points (if moved too far/fast)
        // http://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm

        public void DrawLine(int startX, int startY, int endX, int endY)
        {
            int x1 = endX;
            int y1 = endY;
            int tempVal = x1 - startX;
            int dx = (tempVal + (tempVal >> 31)) ^
                     (tempVal >> 31); // http://stackoverflow.com/questions/6114099/fast-integer-abs-function
            tempVal = y1 - startY;
            int dy = (tempVal + (tempVal >> 31)) ^ (tempVal >> 31);


            int sx = startX < x1 ? 1 : -1;
            int sy = startY < y1 ? 1 : -1;
            int err = dx - dy;
            int pixelCount = 0;
            int e2;
            for (;;) // endless loop
            {
                if (hiQualityBrush)
                {
                    DrawCircle(startX, startY);
                }
                else
                {
                    pixelCount += 3;
                    if (pixelCount >
                        brushSizeDiv4) // might have small gaps if this is used, but its alot(tm) faster to skip few pixels
                    {
                        pixelCount = 0;
                        DrawCircle(startX, startY);
                    }
                }

                if (startX == x1 && startY == y1) break;
                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    startX = startX + sx;
                }
                else if (e2 < dx)
                {
                    err = err + dx;
                    startY = startY + sy;
                }
            }
        } // drawline

        void DrawLineWithBrush(Vector2 start, Vector2 end)
        {
            int x0 = (int)start.x;
            int y0 = (int)start.y;
            int x1 = (int)end.x;
            int y1 = (int)end.y;
            int tempVal = x1 - x0;
            int dx = (tempVal + (tempVal >> 31)) ^
                     (tempVal >> 31); // http://stackoverflow.com/questions/6114099/fast-integer-abs-function
            tempVal = y1 - y0;
            int dy = (tempVal + (tempVal >> 31)) ^ (tempVal >> 31);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int pixelCount = 0;
            int e2;
            for (;;)
            {
                if (hiQualityBrush)
                {
                    DrawCustomBrush(x0, y0);
                }
                else
                {
                    pixelCount += 4;
                    if (pixelCount > brushSizeDiv4)
                    {
                        pixelCount = 0;
                        DrawCustomBrush(x0, y0);
                    }
                }

                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                else if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        public void ReadCurrentCustomBrush()
        {
            if (BrushTextureRotator > BrushTextureRotatorLimit - 1)
            {
                BrushTextureRotator = 0;
            }

            // NOTE: this works only for square brushes
            customBrushWidth = RotatedTextures[BrushTextureRotator].width;
            customBrushHeight = RotatedTextures[BrushTextureRotator].height;
            customBrushBytes = new byte[customBrushWidth * customBrushHeight * 4];

            customBrushBytes = RotatedTextures[BrushTextureRotator].GetRawTextureData();
            // precalculate few brush size values
            customBrushWidthHalf = (int)(customBrushWidth * 0.5f);
            texWidthMinusCustomBrushWidth = texWidth - customBrushWidth;
            texHeightMinusCustomBrushHeight = texHeight - customBrushHeight;
            BrushTextureRotator++;
        }

        public void ChangeBrushSize()
        {
            int resolution = 70;
            int circleResolution = 70;
            resolution = 70;
            if (IsOutline)
            {
                print(ToolGrid.SelectedTool);
                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                {
                    if (cam_pos.pencilS.Length > 0)
                    {
                        if (cam_pos.pencilS[ShapeIndex] == 0)
                            BrushSize_Custom = Handler.MyHandler.PencilSizeForOutline; //extra added by me
                        else
                        {
                            BrushSize_Custom = cam_pos.pencilS[ShapeIndex];
                        }
                    }
                    else
                    {
                        BrushSize_Custom = Handler.MyHandler.PencilSizeForOutline;
                    }
                }
                else
                {
                    if (cam_pos.brushS.Length > 0)
                    {
                        if (cam_pos.brushS[ShapeIndex] == 0)
                            BrushSize_Custom = Handler.MyHandler.BrushSizeForOutline;
                        else
                        {
                            BrushSize_Custom = cam_pos.brushS[ShapeIndex];
                        }
                    }
                    else
                    {
                        BrushSize_Custom = Handler.MyHandler.BrushSizeForOutline;
                    }
                }
            }
            else
            {
                if (ToolGrid.SelectedTool == 0 || ToolGrid.SelectedTool == 2)
                {
                    if (cam_pos.InnerPencil.Length > 0)
                    {
                        if (cam_pos.InnerPencil[ShapeIndex] == 0)
                            BrushSize_Custom = Handler.MyHandler.PencilSizeForInner; //extra added by me
                        else
                        {
                            BrushSize_Custom = cam_pos.InnerPencil[ShapeIndex];
                        }
                    }
                    else
                    {
                        BrushSize_Custom = Handler.MyHandler.PencilSizeForInner;
                    }
                }
                else
                {
                    if (cam_pos.InnerBrush.Length > 0)
                    {
                        if (cam_pos.InnerBrush[Self_Index] == 0)
                            BrushSize_Custom = Handler.MyHandler.BrushSizeForInner;

                        else
                        {
                            BrushSize_Custom = cam_pos.InnerBrush[Self_Index];
                        }
                    }
                    else
                    {
                        Debug.Log("brush  index: " + ToolGrid.SelectedTool);
                        BrushSize_Custom = Handler.MyHandler.BrushSizeForInner;
                    }
                }
            }

            circleResolution = BrushSize_Custom;

            Debug.Log("Changee tool");
            resolution = BrushSize_Custom;



            switch (drawMode)
            {
                case DrawMode.CustomBrush:
                    //BrushSize_Custom = 50;
                    brushSizeX1 = BrushSize_Custom << 1;
                    brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
                    brushSizeX4 = brushSizeXbrushSize << 2;
                    brushSizeDiv4 = 6;
                    customBrushes[selectedBrush] = ScaleTexture(customBrushes[selectedBrush], resolution, resolution);
                    for (int i = 0; i < BrushTextureRotatorLimit; i++)
                    {
                        RotateImage(customBrushes[selectedBrush], i * 8, i);
                        // RotatedTextures[i].Apply();
                    }

                    System.Random rand = new System.Random();
                    for (int i = 0; i < RotatedTextures.Length - 1; i++)
                    {
                        int j = rand.Next(i, RotatedTextures.Length);
                        var temp = RotatedTextures[i];
                        RotatedTextures[i] = RotatedTextures[j];
                        RotatedTextures[j] = temp;
                    }

                    break;
                case DrawMode.Pattern:
                    BrushSize_Custom = circleResolution;
                    brushSizeX1 = BrushSize_Custom << 1;
                    brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
                    brushSizeX4 = brushSizeXbrushSize << 2;
                    brushSizeDiv4 = hiQualityBrush ? 0 : BrushSize_Custom >> 2;
                    break;
                case DrawMode.Eraser:
                    //BrushSize_Custom = 50;
                    brushSizeX1 = BrushSize_Custom << 1;
                    brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
                    brushSizeX4 = brushSizeXbrushSize << 2;
                    brushSizeDiv4 = hiQualityBrush ? 0 : BrushSize_Custom >> 2;
                    break;
                case DrawMode.Default:

                    if (IsOutline)
                    {
                        //BrushSize_Custom = 50;
                        brushSizeX1 = BrushSize_Custom << 1;
                        brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
                        brushSizeX4 = brushSizeXbrushSize << 2;
                        brushSizeDiv4 = 6;
                        customBrushes[selectedBrush] =
                            ScaleTexture(customBrushes[selectedBrush], resolution, resolution);
                        for (int i = 0; i < BrushTextureRotatorLimit; i++)
                        {
                            RotateImage(customBrushes[selectedBrush], i * 8, i);
                            // RotatedTextures[i].Apply();
                        }

                        rand = new System.Random();
                        for (int i = 0; i < RotatedTextures.Length - 1; i++)
                        {
                            int j = rand.Next(i, RotatedTextures.Length);
                            var temp = RotatedTextures[i];
                            RotatedTextures[i] = RotatedTextures[j];
                            RotatedTextures[j] = temp;
                        }
                    }
                    else
                    {
                        //brushSize = 100;
                        brushSizeX1 = BrushSize_Custom << 1;
                        brushSizeXbrushSize = BrushSize_Custom * BrushSize_Custom;
                        brushSizeX4 = brushSizeXbrushSize << 2;
                        brushSizeDiv4 = 7;
                        customBrushes[3] = ScaleTexture(customBrushes[3], resolution, resolution);
                        for (int i = 0; i < BrushTextureRotatorLimit; i++)
                        {
                            RotateImage(customBrushes[3], i * 8, i);
                            // RotatedTextures[i].Apply();
                        }

                        rand = new System.Random();
                        for (int i = 0; i < RotatedTextures.Length - 1; i++)
                        {
                            int j = rand.Next(i, RotatedTextures.Length);
                            var temp = RotatedTextures[i];
                            RotatedTextures[i] = RotatedTextures[j];
                            RotatedTextures[j] = temp;
                        }
                    }
                    //brushSize = circleResolution;
                    //brushSizeX1 = brushSize << 1;
                    //brushSizeXbrushSize = brushSize * brushSize;
                    //brushSizeX4 = brushSizeXbrushSize << 2;
                    //brushSizeDiv4 = hiQualityBrush ? 0 : brushSize >> 2;

                    break;
            }
        }


        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            //if(targetWidth > 0 && targetHeight > 0)
            {
                RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32)
                    { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
                RenderTexture.active = rt;
                Graphics.Blit(source, rt);
                currentPattern = new Texture2D(targetWidth, targetHeight);
                currentPattern.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0, true);
                currentPattern.Apply();
            }

            return currentPattern;

            //return source;
        }

        Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        byte ByteLerp(byte value1, byte value2, float amount)
        {
            return (byte)(value1 + (value2 - value1) * amount);
        }

        public void RotateImage(Texture2D originTexture, int angle, int index)
        {
            int W = originTexture.width;
            int H = originTexture.height;
            //  RotateImageResultTex = new Texture2D(W, H);
            RotatedTextures[index].Reinitialize(W, H);
            Color32[] pix2 = originTexture.GetPixels32();
            Color32[] pix1 = new Color32[pix2.Length];
            Color32 BlackTransparentColor = new Color32(0, 0, 0, 0);
            int x = 0;
            int y = 0;
            int xA;
            int yA;
            double sn = Math.Sin((Math.PI / 180 * (double)angle));
            double cs = Math.Cos((Math.PI / 180 * (double)angle));
            Color32[] arr2 = pix2; // originTexture.GetPixels32();
            int WA = W;
            int HA = H;
            int xc = WA / 2;
            int yc = HA / 2;

            for (int j = 0; j < H; j++)
            {
                for (var i = 0; i < W; i++)
                {
                    arr2[j * WA + i] = BlackTransparentColor;
                    xA = (int)(cs * (i - xc) + sn * (j - yc) + xc);
                    yA = (int)(-sn * (i - xc) + cs * (j - yc) + yc);
                    if ((xA > -1) && (xA < WA) && (yA > -1) && (yA < HA))
                    {
                        arr2[j * WA + i] = pix2[yA * WA + xA];
                    }

                    pix1[W / 2 - W / 2 + x + i + W * (H / 2 - H / 2 + j + y)] = arr2[i + j * W];
                }
            }

            //result.SetPixels32(pix1);
            //result.Apply();
            RotatedTextures[index].SetPixels32(pix1);
            RotatedTextures[index].Apply();
            //return pix1;
        }

        public void RotateImageIndex(Texture2D originTexture, int angle, int mArrayIndex)
        {
            Texture2D result;
            result = new Texture2D(originTexture.width, originTexture.height);
            Color32[] pix1 = result.GetPixels32();
            Color32[] pix2 = originTexture.GetPixels32();
            int W = originTexture.width;
            int H = originTexture.height;
            int x = 0;
            int y = 0;
            int xA;
            int yA;
            double sn = Math.Sin((Math.PI / 180 * (double)angle));
            double cs = Math.Cos((Math.PI / 180 * (double)angle));
            Color32[] arr2 = originTexture.GetPixels32();
            int WA = originTexture.width;
            int HA = originTexture.height;
            int xc = WA / 2;
            int yc = HA / 2;
            int index = 0;
            int indexCount = 0;
            for (int j = 0; j < H; j++)
            {
                for (var i = 0; i < W; i++)
                {
                    arr2[j * WA + i] = new Color32(0, 0, 0, 0);
                    xA = (int)(cs * (i - xc) + sn * (j - yc) + xc);
                    yA = (int)(-sn * (i - xc) + cs * (j - yc) + yc);
                    if ((xA > -1) && (xA < WA) && (yA > -1) && (yA < HA))
                    {
                        arr2[j * WA + i] = pix2[yA * WA + xA];
                    }

                    index = result.width / 2 - W / 2 + x + i +
                            result.width *
                            (result.height / 2 - H / 2 + j +
                             y); //pix1[result.width / 2 - W / 2 + x + i + result.width * (result.height / 2 - H / 2 + j + y)] = arr2[i + j * W];
                    DRotatedTextures[mArrayIndex, indexCount] = index;
                    indexCount++;
                }
            }
        }

        public void SelectColor(GameObject selectedButton)
        {
            if (selectedButton.CompareTag("MultiColorBtn"))
            {
                MultiColor = true;
            }
            else
            {
                MultiColor = false;
                if (selectedButton.name == "Yellow")
                {
                    paintColor = new Color32(244, 235, 2, 255);
                }
                else if (selectedButton.name == "Red")
                {
                    paintColor = new Color32(255, 0, 0, 255);
                }
                else if (selectedButton.name == "Purple")
                {
                    paintColor = new Color32(171, 26, 255, 255);
                }
                else if (selectedButton.name == "Orange")
                {
                    paintColor = new Color32(255, 148, 16, 255);
                }
                else if (selectedButton.name == "Green")
                {
                    paintColor = new Color32(61, 214, 13, 255);
                }
                else if (selectedButton.name == "Black")
                {
                    paintColor = new Color32(119, 119, 119, 255);
                }
                else if (selectedButton.name == "Blue")
                {
                    paintColor = new Color32(2, 138, 255, 255);
                }
                else if (selectedButton.name == "Cyan")
                {
                    paintColor = new Color32(0, 255, 255, 255);
                }
                else if (selectedButton.name == "Pink")
                {
                    paintColor = new Color32(254, 0, 136, 255);
                }
                else if (selectedButton.name == "Brown")
                {
                    paintColor = new Color32(190, 157, 52, 255);
                }
                //paintColor = SelectedButton.gameObject.GetComponent<Image>().color;
            }

            switch (drawMode)
            {
                case DrawMode.Default:
                    if (MultiColor)
                        PlayerPrefs.SetString("DefaultModeColor", "Multi");
                    else
                        PlayerPrefs.SetString("DefaultModeColor",
                            paintColor.r + "," + paintColor.g + "," + paintColor.b + "," + selectedButton.name);
                    break;
                case DrawMode.CustomBrush:
                    if (MultiColor)
                        PlayerPrefs.SetString("CustomModeColor", "Multi");
                    else
                        PlayerPrefs.SetString("CustomModeColor",
                            paintColor.r + "," + paintColor.g + "," + paintColor.b + "," + selectedButton.name);
                    break;
                case DrawMode.FloodFill:
                    if (MultiColor)
                        PlayerPrefs.SetString("FloodFillModeColor", "Multi");
                    else
                        PlayerPrefs.SetString("FloodFillModeColor",
                            paintColor.r + "," + paintColor.g + "," + paintColor.b + "," + selectedButton.name);
                    break;
            }
        }

        // assigns new mask layer image
        public void SetMaskImage(Texture2D newTexture)
        {
            // Check if we have correct material to use mask image (layer)
            if (myRenderer.material.name.StartsWith("CanvasWithAlpha") ||
                GetComponent<Renderer>().material.name.StartsWith("CanvasDefault"))
            {
                // FIXME: this is bit annoying to compare material names..
                Debug.LogWarning(
                    "CanvasWithAlpha and CanvasDefault materials do not support using MaskImage (layer). Disabling 'useMaskImage'");
                Debug.LogWarning(
                    "CanvasWithAlpha and CanvasDefault materials do not support using MaskImage (layer). Disabling 'useMaskLayerOnly'");
                useMaskLayerOnly = false;
                useMaskImage = false;
                maskTex = null;
            }
            else
            {
                //material is ok

                // NOTE: if new texture is different size, problems will occur when drawing (mask is not aligned)
                //if (texWidth!=maskTex.width || texHeight != maskTex.height) Debug.LogWarning("SetMaskImage: New mask texture size is different from existing canvas texture, could cause problems. Current resolution:"+texWidth+"x"+texHeight+" | Mask resolution:"+maskTex.width+"x"+maskTex.height);

                maskTex = newTexture;
                texWidth = newTexture.width;
                texHeight = newTexture.height;
                myRenderer.material.SetTexture("_MaskTex", newTexture);
                ReadMaskImage();
                textureNeedsUpdate = true;
            }
        } // SetMaskImage

        public void ReadMaskImage()
        {
            maskPixels = new byte[texWidth * texHeight * 4];

            int smoothenResolution = 5; // currently fixed value
            int smoothArea = smoothenResolution * smoothenResolution;
            int smoothCenter = Mathf.FloorToInt(smoothenResolution / 2);

            int pixel = 0;
            Color c;

            for (int y = 0; y < texHeight; y++)
            {
                for (int x = 0; x < texWidth; x++)
                {
                    if (smoothenMaskEdges)
                    {
                        c = new Color(0, 0, 0, 0);
                        // c = maskTex.GetPixel(x, y); // center

                        if (c.a > 0)
                        {
                            for (int i = 0; i < smoothArea; i++)
                            {
                                int xx = (i / smoothenResolution) | 0; // 0, 0, 0
                                int yy = i % smoothenResolution;
                                if (maskTex.GetPixel(x + xx - smoothCenter, y + yy - smoothCenter).a <
                                    (255 - paintThreshold) / 255f)
                                {
                                    c = new Color(0, 0, 0, 0);
                                }
                            }
                        }
                    }
                    else
                    {
                        // default (works well if texture is "point" filter mode
                        c = maskTex.GetPixel(x, y);
                    }

                    maskPixels[pixel] = (byte)(c.r * 255);
                    maskPixels[pixel + 1] = (byte)(c.g * 255);
                    maskPixels[pixel + 2] = (byte)(c.b * 255);
                    maskPixels[pixel + 3] = (byte)(c.a * 255);
                    pixel += 4;
                }
            }
        }

        void CreateAreaLockMask(int x, int y)
        {
            initialX = x;
            initialY = y;

            if (useThreshold)
            {
                if (useMaskLayerOnly)
                {
                    if (getAreaSize)
                    {
                        LockAreaFillWithThresholdMaskOnlyGetArea(x, y, false);
                    }
                    else
                    {
                        LockAreaFillWithThresholdMaskOnly(x, y);
                    }
                }
                else
                {
                    LockMaskFillWithThreshold(x, y);
                }
            }
            else
            {
                // no threshold
                if (useMaskLayerOnly)
                {
                    LockAreaFillMaskOnly(x, y);
                }
                else
                {
                    LockAreaFill(x, y);
                }
            }
            //lockMaskCreated = true; // not used yet
        }

        void LockAreaFillWithThresholdMaskOnlyGetArea(int x, int y, bool getArea)
        {
            // temporary fix for IOS notification center pulldown crash
            if (x >= texWidth) x = texWidth - 1;
            if (y >= texHeight) y = texHeight - 1;

            int fullArea = 0;
            int alreadyFilled = 0;

            // get canvas color from this point
            byte hitColorR = maskPixels[(texWidth * y + x) * 4 + 0];
            byte hitColorG = maskPixels[(texWidth * y + x) * 4 + 1];
            byte hitColorB = maskPixels[(texWidth * y + x) * 4 + 2];
            byte hitColorA = maskPixels[(texWidth * y + x) * 4 + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];


            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

                    if (lockMaskPixels[pixel] == 0 // this pixel is not used yet
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        lockMaskPixels[pixel] = 1;
                        fullArea++;

                        if (IsSameColor(paintColor, pixels[pixel + 0], pixels[pixel + 1], pixels[pixel + 2]))
                        {
                            alreadyFilled++;
                        }
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                        fullArea++;
                        if (IsSameColor(paintColor, pixels[pixel + 0], pixels[pixel + 1], pixels[pixel + 2]))
                        {
                            alreadyFilled++;
                        }
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                        fullArea++;
                        if (IsSameColor(paintColor, pixels[pixel + 0], pixels[pixel + 1], pixels[pixel + 2]))
                        {
                            alreadyFilled++;
                        }
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        lockMaskPixels[pixel] = 1;
                        fullArea++;
                        if (IsSameColor(paintColor, pixels[pixel + 0], pixels[pixel + 1], pixels[pixel + 2]))
                        {
                            alreadyFilled++;
                        }
                    }
                }
            } // while

            if (getArea)
            {
                if (AreaPaintedEvent != null)
                    AreaPaintedEvent(fullArea, alreadyFilled, alreadyFilled / (float)fullArea * 100f,
                        PixelToWorld(x, y));
            }
        } // void

        // compares if two values are below threshold
        bool CompareThreshold(byte a, byte b)
        {
            if (a < b)
            {
                a ^= b;
                b ^= a;
                a ^= b;
            } // http://lab.polygonal.de/?p=81

            return (a - b) <= paintThreshold;
        }

        bool IsSameColor(Color32 a, byte r, byte g, byte b)
        {
            return (a.r == r && a.g == g && a.b == b);
        }

        public Vector3 PixelToWorld(int x, int y)
        {
            Vector3 pixelPos = new Vector3(x, y, 0); // x,y = texture pixel pos

            float planeWidth = myRenderer.bounds.size.x;
            float planeHeight = myRenderer.bounds.size.y;

            float localX = ((pixelPos.x / texWidth) - 0.5f) * planeWidth;
            float localY = ((pixelPos.y / texHeight) - 0.5f) * planeHeight;

            //			return transform.TransformPoint(new Vector3(localX,localY, 0));
            return new Vector3(localX, localY, 0);
        }

        // create locking mask floodfill, using threshold, checking pixels from mask only
        void LockAreaFillWithThresholdMaskOnly(int x, int y)
        {
            // get canvas color from this point
            byte hitColorR = maskPixels[(texWidth * y + x) * 4 + 0];
            byte hitColorG = maskPixels[(texWidth * y + x) * 4 + 1];
            byte hitColorB = maskPixels[(texWidth * y + x) * 4 + 2];
            byte hitColorA = maskPixels[(texWidth * y + x) * 4 + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

                    if (lockMaskPixels[pixel] == 0 // this pixel is not used yet
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(maskPixels[pixel + 0], hitColorR))
                        && (CompareThreshold(maskPixels[pixel + 1], hitColorG))
                        && (CompareThreshold(maskPixels[pixel + 2], hitColorB))
                        && (CompareThreshold(maskPixels[pixel + 3], hitColorA)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // LockAreaFillWithThresholdMaskOnly

        // create locking mask floodfill, using threshold
        void LockMaskFillWithThreshold(int x, int y)
        {
            // get canvas color from this point
            byte hitColorR = pixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = pixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = pixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = pixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR != 0 && hitColorG != 0 && hitColorB != 0 && hitColorA != 0) return;
            }

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

                    if (lockMaskPixels[pixel] == 0 // this pixel is not used yet
                        && (CompareThreshold(pixels[pixel + 0], hitColorR) ||
                            CompareThreshold(pixels[pixel + 0],
                                paintColor.r)) // if pixel is same as hit color OR same as paint color
                        && (CompareThreshold(pixels[pixel + 1], hitColorG) ||
                            CompareThreshold(pixels[pixel + 1], paintColor.g))
                        && (CompareThreshold(pixels[pixel + 2], hitColorB) ||
                            CompareThreshold(pixels[pixel + 2], paintColor.b))
                        && (CompareThreshold(pixels[pixel + 3], hitColorA) ||
                            CompareThreshold(pixels[pixel + 3], paintColor.a)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(pixels[pixel + 0], hitColorR) ||
                            CompareThreshold(pixels[pixel + 0],
                                paintColor.r)) // if pixel is same as hit color OR same as paint color
                        && (CompareThreshold(pixels[pixel + 1], hitColorG) ||
                            CompareThreshold(pixels[pixel + 1], paintColor.g))
                        && (CompareThreshold(pixels[pixel + 2], hitColorB) ||
                            CompareThreshold(pixels[pixel + 2], paintColor.b))
                        && (CompareThreshold(pixels[pixel + 3], hitColorA) ||
                            CompareThreshold(pixels[pixel + 3], paintColor.a)))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(pixels[pixel + 0], hitColorR) ||
                            CompareThreshold(pixels[pixel + 0],
                                paintColor.r)) // if pixel is same as hit color OR same as paint color
                        && (CompareThreshold(pixels[pixel + 1], hitColorG) ||
                            CompareThreshold(pixels[pixel + 1], paintColor.g))
                        && (CompareThreshold(pixels[pixel + 2], hitColorB) ||
                            CompareThreshold(pixels[pixel + 2], paintColor.b))
                        && (CompareThreshold(pixels[pixel + 3], hitColorA) ||
                            CompareThreshold(pixels[pixel + 3], paintColor.a)))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && (CompareThreshold(pixels[pixel + 0], hitColorR) ||
                            CompareThreshold(pixels[pixel + 0],
                                paintColor.r)) // if pixel is same as hit color OR same as paint color
                        && (CompareThreshold(pixels[pixel + 1], hitColorG) ||
                            CompareThreshold(pixels[pixel + 1], paintColor.g))
                        && (CompareThreshold(pixels[pixel + 2], hitColorB) ||
                            CompareThreshold(pixels[pixel + 2], paintColor.b))
                        && (CompareThreshold(pixels[pixel + 3], hitColorA) ||
                            CompareThreshold(pixels[pixel + 3], paintColor.a)))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // LockMaskFillWithTreshold

        void LockAreaFillMaskOnly(int x, int y)
        {
            byte hitColorR = maskPixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = maskPixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = maskPixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = maskPixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

                    if (lockMaskPixels[pixel] == 0
                        && (maskPixels[pixel + 0] == hitColorR || maskPixels[pixel + 0] == paintColor.r)
                        && (maskPixels[pixel + 1] == hitColorG || maskPixels[pixel + 1] == paintColor.g)
                        && (maskPixels[pixel + 2] == hitColorB || maskPixels[pixel + 2] == paintColor.b))
                        //  && (maskPixels[pixel + 3] == hitColorA || maskPixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && (maskPixels[pixel + 0] == hitColorR || maskPixels[pixel + 0] == paintColor.r)
                        && (maskPixels[pixel + 1] == hitColorG || maskPixels[pixel + 1] == paintColor.g)
                        && (maskPixels[pixel + 2] == hitColorB || maskPixels[pixel + 2] == paintColor.b))
                        //   && (maskPixels[pixel + 3] == hitColorA || maskPixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && (maskPixels[pixel + 0] == hitColorR || maskPixels[pixel + 0] == paintColor.r)
                        && (maskPixels[pixel + 1] == hitColorG || maskPixels[pixel + 1] == paintColor.g)
                        && (maskPixels[pixel + 2] == hitColorB || maskPixels[pixel + 2] == paintColor.b))
                        // && (maskPixels[pixel + 3] == hitColorA || maskPixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && (maskPixels[pixel + 0] == hitColorR || maskPixels[pixel + 0] == paintColor.r)
                        && (maskPixels[pixel + 1] == hitColorG || maskPixels[pixel + 1] == paintColor.g)
                        && (maskPixels[pixel + 2] == hitColorB || maskPixels[pixel + 2] == paintColor.b))
                        //&& (maskPixels[pixel + 3] == hitColorA || maskPixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // LockAreaFillMaskOnly

        void LockAreaFill(int x, int y)
        {
            byte hitColorR = pixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = pixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = pixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = pixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down

                    if (lockMaskPixels[pixel] == 0
                        && (pixels[pixel + 0] == hitColorR || pixels[pixel + 0] == paintColor.r)
                        && (pixels[pixel + 1] == hitColorG || pixels[pixel + 1] == paintColor.g)
                        && (pixels[pixel + 2] == hitColorB || pixels[pixel + 2] == paintColor.b))
                        //&& (pixels[pixel + 3] == hitColorA || pixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && (pixels[pixel + 0] == hitColorR || pixels[pixel + 0] == paintColor.r)
                        && (pixels[pixel + 1] == hitColorG || pixels[pixel + 1] == paintColor.g)
                        && (pixels[pixel + 2] == hitColorB || pixels[pixel + 2] == paintColor.b))
                        // && (pixels[pixel + 3] == hitColorA || pixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && (pixels[pixel + 0] == hitColorR || pixels[pixel + 0] == paintColor.r)
                        && (pixels[pixel + 1] == hitColorG || pixels[pixel + 1] == paintColor.g)
                        && (pixels[pixel + 2] == hitColorB || pixels[pixel + 2] == paintColor.b))
                        //&& (pixels[pixel + 3] == hitColorA || pixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && (pixels[pixel + 0] == hitColorR || pixels[pixel + 0] == paintColor.r)
                        && (pixels[pixel + 1] == hitColorG || pixels[pixel + 1] == paintColor.g)
                        && (pixels[pixel + 2] == hitColorB || pixels[pixel + 2] == paintColor.b))
                        // && (pixels[pixel + 3] == hitColorA || pixels[pixel + 3] == paintColor.a))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // LockAreaFill

        void CallFloodFill(int x, int y, float u = 0f, float v = 0f)
        {
            if (useThreshold)
            {
                print("Using ThresHold");
                if (useMaskLayerOnly)
                {
                    print("Using ThresHold if");
                    FloodFillMaskOnlyWithThreshold(x, y);
                }
                else
                {
                    print("Using ThresHold else");
                    FloodFillWithTreshold(x, y);
                }
            }
            else
            {
                // no threshold
                print("not Using ThresHold");
                if (useMaskLayerOnly)
                {
                    print("not Using ThresHold if");
                    FloodFillMaskOnly(x, y);
                }
                else
                {
                    print("not Using ThresHold else");
                    FloodFill(x, y, u, v);

                }
            }
        }

        void FloodFillWithTreshold(int x, int y)
        {
            // get canvas hit color
            byte hitColorR = pixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = pixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = pixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = pixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorR == 0 && hitColorG == 0 && hitColorB == 0 && hitColorA != 0) return;
            }

            // early exit if outside threshold
            //if (CompareThreshold(paintColor.r,hitColorR) && CompareThreshold(paintColor.g,hitColorG) && CompareThreshold(paintColor.b,hitColorB) && CompareThreshold(paintColor.b,hitColorA)) return;
            if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB) return;
            //  if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA) return;

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(pixels[pixel + 0], hitColorR)
                        && CompareThreshold(pixels[pixel + 1], hitColorG)
                        && CompareThreshold(pixels[pixel + 2], hitColorB)
                        && CompareThreshold(pixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(pixels[pixel + 0], hitColorR)
                        && CompareThreshold(pixels[pixel + 1], hitColorG)
                        && CompareThreshold(pixels[pixel + 2], hitColorB)
                        && CompareThreshold(pixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(pixels[pixel + 0], hitColorR)
                        && CompareThreshold(pixels[pixel + 1], hitColorG)
                        && CompareThreshold(pixels[pixel + 2], hitColorB)
                        && CompareThreshold(pixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(pixels[pixel + 0], hitColorR)
                        && CompareThreshold(pixels[pixel + 1], hitColorG)
                        && CompareThreshold(pixels[pixel + 2], hitColorB)
                        && CompareThreshold(pixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // floodfillWithTreshold

        void FloodFillMaskOnly(int x, int y)
        {
            // get canvas hit color
            byte hitColorR = maskPixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = maskPixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = maskPixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = maskPixels[((texWidth * (y) + x) * 4) + 3];

            // early exit if its same color already
            //if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.b == hitColorA) return;

            if (!canDrawOnBlack)
            {
                if (hitColorA == 0) return;
            }


            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
                    if (lockMaskPixels[pixel] == 0
                        && maskPixels[pixel + 0] == hitColorR
                        && maskPixels[pixel + 1] == hitColorG
                        && maskPixels[pixel + 2] == hitColorB
                        && maskPixels[pixel + 3] == hitColorA)
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && maskPixels[pixel + 0] == hitColorR
                        && maskPixels[pixel + 1] == hitColorG
                        && maskPixels[pixel + 2] == hitColorB
                        && maskPixels[pixel + 3] == hitColorA)
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && maskPixels[pixel + 0] == hitColorR
                        && maskPixels[pixel + 1] == hitColorG
                        && maskPixels[pixel + 2] == hitColorB
                        && maskPixels[pixel + 3] == hitColorA)
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && maskPixels[pixel + 0] == hitColorR
                        && maskPixels[pixel + 1] == hitColorG
                        && maskPixels[pixel + 2] == hitColorB
                        && maskPixels[pixel + 3] == hitColorA)
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // floodfill

        IEnumerator floodFillwait(int pixY = 0, int pixel = 0)
        {
            while (pixY < texHeight)
            {
                for (int pixX = 0; pixX < texWidth; pixX++)
                {
                    if (clearPixels[pixel + 3] < 100)
                    {
                    }
                    else
                    {
                        pixels[pixel] = paintColor.r;
                        pixels[pixel + 1] = paintColor.g;
                        pixels[pixel + 2] = paintColor.b;
                        pixels[pixel + 3] = paintColor.a;

                        toSavePixels[pixel] = paintColor.r;
                        toSavePixels[pixel + 1] = paintColor.g;
                        toSavePixels[pixel + 2] = paintColor.b;
                        toSavePixels[pixel + 3] = paintColor.a;
                    }

                    pixel += 4;
                }

                pixY++;
                UpdateTexture();
                yield return new WaitForSeconds(0.00000001f);
            }
        }

        // basic floodfill
        // void FloodFill(int x, int y, float u, float v)
        // {
        //     //MatObj.transform.position = new Vector2(x,y);
        //     int pixel = (texWidth * y + x) << 2;
        //     if (clearPixels[pixel + 3] < 100) return;
        //     if (fillValue > 0) return;
        //     pixel = 0;
        //     var currentTexSizeRatio = Mathf.Sqrt(pixels.Length / 4) * 2;
        //     int startX = x - ((int)currentTexSizeRatio / 2);
        //     int startY = y - ((int)currentTexSizeRatio / 2);
        //     ReadGradientTexture(radialFillTexture, (int)currentTexSizeRatio, (int)currentTexSizeRatio);
        //     Color32[] tempPixels = gradientTextureTemp.GetPixels32();
        //     int gradientWidth = gradientTextureTemp.width;
        //     int gradientHeight = gradientTextureTemp.height;
        //     for (int pixY = 0; pixY < texHeight; pixY++)
        //     {
        //         for (int pixX = 0; pixX < texWidth; pixX++)
        //         {
        //             pixel = texWidth * pixY + pixX << 2;
        //             if (clearPixels[pixel + 3] < 100)
        //             {
        //             }
        //             else
        //             {
        //                 if (pixX >= startX && pixY >= startY && pixX < gradientWidth + startX &&
        //                     pixY < gradientHeight + startY)
        //                 {
        //                     Color32 wmColor = tempPixels[gradientWidth * (pixY - startY) + (pixX - startX)];
        //                     if (wmColor.a > 1)
        //                     {
        //                         pixels[pixel] = toSavePixels[pixel] = (byte)((wmColor.r * wmColor.a / 255) +
        //                                                                      (paintColor.r * paintColor.a *
        //                                                                       (255 - wmColor.a) /
        //                                                                       (255 *
        //                                                                        255))); // (rA * aA / 255) + (rB * aB * (255 - aA) / (255 * 255))
        //                         pixels[pixel + 1] = toSavePixels[pixel + 1] = (byte)((wmColor.g * wmColor.a / 255) +
        //                             (paintColor.g * paintColor.a * (255 - wmColor.a) /
        //                              (255 * 255))); //(gA * aA / 255) + (gB * aB * (255 - aA) / (255*255))
        //                         pixels[pixel + 2] = toSavePixels[pixel + 2] = (byte)((wmColor.b * wmColor.a / 255) +
        //                             (paintColor.b * paintColor.a * (255 - wmColor.a) /
        //                              (255 * 255))); //(bA * aA / 255) + (bB * aB * (255 - aA) / (255*255))
        //                         pixels[pixel + 3] = toSavePixels[pixel + 3] =
        //                             (byte)(wmColor.a +
        //                                    (paintColor.a * (255 - wmColor.a) / 255)); //aA + (aB * (255 - aA) / 255)
        //                         //pixels[pixel] = colorArrray[pixel] =  (byte)(wmColor.r + (paintColor.r - wmColor.r) * wmColor.a);// (rA * aA / 255) + (rB * aB * (255 - aA) / (255 * 255))
        //                         //pixels[pixel + 1] = colorArrray[pixel + 1] = (byte)(wmColor.g + (paintColor.g - wmColor.g) * wmColor.a);
        //                         //pixels[pixel + 2] = colorArrray[pixel + 2] = (byte)(wmColor.b + (paintColor.b - wmColor.b) * wmColor.a);
        //                     }
        //                     else
        //                     {
        //                         pixels[pixel] = toSavePixels[pixel] = paintColor.r;
        //                         pixels[pixel + 1] = toSavePixels[pixel + 1] = paintColor.g;
        //                         pixels[pixel + 2] = toSavePixels[pixel + 2] = paintColor.b;
        //                         pixels[pixel + 3] = toSavePixels[pixel + 3] = 255;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     toSavePixels[pixel] = pixels[pixel] = paintColor.r;
        //                     toSavePixels[pixel + 1] = pixels[pixel + 1] = paintColor.g;
        //                     toSavePixels[pixel + 2] = pixels[pixel + 2] = paintColor.b;
        //                     toSavePixels[pixel + 3] = pixels[pixel + 3] = paintColor.a;
        //                 }
        //             }
        //             //pixel += 4;
        //         }
        //     }
        //     // UpdateTexture();
        //
        //     tex.LoadRawTextureData(toSavePixels);
        //     tex.Apply();
        //
        //     fillAnimate = true;
        //     radialMat.mainTexture = tex;
        //
        //
        //     radialMat.SetFloat(FILL_VALUE_ID, 0);
        //     radialMat.SetVector(MOUSE_VALUE_ID, new Vector4(u, v, 0, 1f));
        //
        //     StartCoroutine(updateColorsAndAnimate());
        // } // floodfill

        void FloodFill(int x, int y, float u, float v)
        {
            //MatObj.transform.position = new Vector2(x,y);
            int pixel = (texWidth * y + x) << 2;
            if (clearPixels[pixel + 3] < 100) return;
            if (fillValue > 0) return;

            for (int pixY = 0; pixY < texHeight; pixY++)
            {
                for (int pixX = 0; pixX < texWidth; pixX++)
                {
                    pixel = (texWidth * pixY + pixX) << 2;
                    if (clearPixels[pixel + 3] >= 100)
                    {
                        pixels[pixel] = toSavePixels[pixel] = paintColor.r;
                        pixels[pixel + 1] = toSavePixels[pixel + 1] = paintColor.g;
                        pixels[pixel + 2] = toSavePixels[pixel + 2] = paintColor.b;
                        pixels[pixel + 3] = toSavePixels[pixel + 3] = paintColor.a;
                    }
                }
            }

            // Update the texture with the modified pixel data
            tex.LoadRawTextureData(toSavePixels);
            tex.Apply();

            fillAnimate = true;
            radialMat.mainTexture = tex;

            radialMat.SetFloat(FILL_VALUE_ID, 0);
            radialMat.SetVector(MOUSE_VALUE_ID, new Vector4(u, v, 0, 1f));

            StartCoroutine(updateColorsAndAnimate());
        }

        public IEnumerator updateColorsAndAnimate()
        {
            print(fillValue + " fillval");
            //updateAnimationVars();
            while (fillAnimate)
            {
                fillValue +=
                    .1f; //0 + InterpolationUtil.moveInLinear(Time.realtimeSinceStartup - animationStartTime, 0, duration, 1);
                radialMat.SetFloat(FILL_VALUE_ID, fillValue);
                if (fillValue >= 1)
                {
                    print("fillvals   falseeee");
                    fillValue = 0;
                    fillAnimate = false;
                }

                yield return new WaitForSeconds(0.02f);
            }

            radialMat.mainTexture = null;
            UpdateTexture();
            StartCoroutine(TakeScreenShot());
        }

        public Texture2D ReadGradientTexture(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 0, RenderTextureFormat.ARGB32)
                { wrapMode = TextureWrapMode.Mirror, filterMode = FilterMode.Bilinear };
            RenderTexture.active = rt;
            gradientTextureTemp = new Texture2D(targetX, targetY);
            Graphics.Blit(texture2D, rt);
            gradientTextureTemp.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0, true);
            gradientTextureTemp.Apply();

            //gradientPixels = gradientTexture.GetRawTextureData();
            return gradientTextureTemp;
        }

        Vector2 getUvPoint(float x, float y)
        {
            float diag = Mathf.Sqrt(x * x + y * y);
            return new Vector2(0.5f * x / diag, 0.5f * y / diag);
        }

        // floodfillPattern
        void FloodFillPattern(int x, int y, float u = 0, float v = 0)
        {
            print("patternFilling");

            int pixel = (texWidth * y + x) << 2;
            if (clearPixels[pixel + 3] < 100) return;
            int tempcount = 0;
            pixel = 0;
            for (int pixY = 0; pixY < texHeight; pixY++)
            {
                for (int pixX = 0; pixX < texWidth; pixX++)
                {
                    if (clearPixels[pixel + 3] < 100)
                    {
                    }
                    else
                    {
                        float yy = Mathf.Repeat(pixX, currentPattern.width);
                        float xx = Mathf.Repeat(pixY, currentPattern.width);
                        int pixel2 = (int)Mathf.Repeat((currentPattern.width * xx + yy) * 4,
                            floodFillPatternPixels.Length);

                        pixels[pixel] = floodFillPatternPixels[pixel2];
                        pixels[pixel + 1] = floodFillPatternPixels[pixel2 + 1];
                        pixels[pixel + 2] = floodFillPatternPixels[pixel2 + 2];
                        pixels[pixel + 3] = floodFillPatternPixels[pixel2 + 3];

                        toSavePixels[pixel] = floodFillPatternPixels[pixel2];
                        toSavePixels[pixel + 1] = floodFillPatternPixels[pixel2 + 1];
                        toSavePixels[pixel + 2] = floodFillPatternPixels[pixel2 + 2];
                        toSavePixels[pixel + 3] = floodFillPatternPixels[pixel2 + 3];
                    }

                    tempcount++;
                    pixel += 4;
                }
            }

            tex.LoadRawTextureData(toSavePixels);
            tex.Apply();

            fillAnimate = true;
            radialMat.mainTexture = tex;


            radialMat.SetFloat(FILL_VALUE_ID, 0);
            radialMat.SetVector(MOUSE_VALUE_ID, new Vector4(u, v, 0, 1f));

            StartCoroutine(updateColorsAndAnimate());
        } // floodfillPattern

        void FloodFillMaskOnlyWithThreshold(int x, int y)
        {
            // get canvas hit color
            byte hitColorR = maskPixels[((texWidth * (y) + x) * 4) + 0];
            byte hitColorG = maskPixels[((texWidth * (y) + x) * 4) + 1];
            byte hitColorB = maskPixels[((texWidth * (y) + x) * 4) + 2];
            byte hitColorA = maskPixels[((texWidth * (y) + x) * 4) + 3];

            if (!canDrawOnBlack)
            {
                if (hitColorA != 0) return;
            }

            // early exit if outside threshold?
            //if (CompareThreshold(paintColor.r,hitColorR) && CompareThreshold(paintColor.g,hitColorG) && CompareThreshold(paintColor.b,hitColorB) && CompareThreshold(paintColor.b,hitColorA)) return;
            if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB) return;
            //  if (paintColor.r == hitColorR && paintColor.g == hitColorG && paintColor.b == hitColorB && paintColor.a == hitColorA) return;

            Queue<int> fillPointX = new Queue<int>();
            Queue<int> fillPointY = new Queue<int>();
            fillPointX.Enqueue(x);
            fillPointY.Enqueue(y);

            int ptsx, ptsy;
            int pixel = 0;

            lockMaskPixels = new byte[texWidth * texHeight * 4];

            while (fillPointX.Count > 0)
            {
                ptsx = fillPointX.Dequeue();
                ptsy = fillPointY.Dequeue();

                if (ptsy - 1 > -1)
                {
                    pixel = (texWidth * (ptsy - 1) + ptsx) * 4; // down
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                        && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                        && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                        && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy - 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx + 1 < texWidth)
                {
                    pixel = (texWidth * ptsy + ptsx + 1) * 4; // right
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                        && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                        && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                        && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx + 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsx - 1 > -1)
                {
                    pixel = (texWidth * ptsy + ptsx - 1) * 4; // left
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                        && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                        && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                        && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx - 1);
                        fillPointY.Enqueue(ptsy);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }

                if (ptsy + 1 < texHeight)
                {
                    pixel = (texWidth * (ptsy + 1) + ptsx) * 4; // up
                    if (lockMaskPixels[pixel] == 0
                        && CompareThreshold(maskPixels[pixel + 0], hitColorR)
                        && CompareThreshold(maskPixels[pixel + 1], hitColorG)
                        && CompareThreshold(maskPixels[pixel + 2], hitColorB)
                        && CompareThreshold(maskPixels[pixel + 3], hitColorA))
                    {
                        fillPointX.Enqueue(ptsx);
                        fillPointY.Enqueue(ptsy + 1);
                        DrawPoint(pixel);
                        lockMaskPixels[pixel] = 1;
                    }
                }
            }
        } // floodfillWithTreshold

        // draws single point to this pixel array index, with current paint color
        public void DrawPoint(int pixel)
        {
            pixels[pixel] = paintColor.r;
            pixels[pixel + 1] = paintColor.g;
            pixels[pixel + 2] = paintColor.b;
            //   pixels[pixel + 3] = paintColor.a;
        }

        // public void SetDrawMode(int Mode, bool IsCallFromSubGrid = false)
        // {
        //     switch (Mode)
        //     {
        //         case (int)DrawMode.Default:
        //             string temp = PlayerPrefs.GetString("DefaultModeColor");
        //
        //
        //             if (!string.IsNullOrEmpty(temp))
        //             {
        //                 if (temp == "Multi")
        //                 {
        //                     MultiColor = true;
        //                     //  Camera.main.GetComponent<PlayAudio>().SelectColorSound("Multi");
        //                 }
        //                 else
        //                 {
        //                     MultiColor = false;
        //                     var like = temp.Split(',');
        //                     paintColor = new Color32((byte)int.Parse(like[0]), (byte)int.Parse(like[1]),
        //                         (byte)int.Parse(like[2]), 255);
        //                     // Camera.main.GetComponent<PlayAudio>().SelectColorSound(like[3]);
        //                 }
        //             }
        //             else
        //             {
        //                 MultiColor = true;
        //                 //Camera.main.GetComponent<PlayAudio>().SelectColorSound("Multi");
        //             }
        //
        //             drawMode = DrawMode.Default;
        //             //multiColorSpeedConstant = .25f;
        //             ChangeBrushSize();
        //             break;
        //
        //         case (int)DrawMode.CustomBrush:
        //             string tempCustom = PlayerPrefs.GetString("CustomModeColor");
        //             if (!string.IsNullOrEmpty(tempCustom))
        //             {
        //                 if (tempCustom == "Multi")
        //                 {
        //                     MultiColor = true;
        //                     // Camera.main.GetComponent<PlayAudio>().SelectColorSound("Multi");
        //                 }
        //                 else
        //                 {
        //                     MultiColor = false;
        //                     var like = tempCustom.Split(',');
        //                     paintColor = new Color32((byte)int.Parse(like[0]), (byte)int.Parse(like[1]),
        //                         (byte)int.Parse(like[2]), 255);
        //                     // Camera.main.GetComponent<PlayAudio>().SelectColorSound(like[3]);
        //                 }
        //             }
        //             else
        //             {
        //                 MultiColor = true;
        //                 //Camera.main.GetComponent<PlayAudio>().SelectColorSound("Multi");
        //             }
        //
        //             drawMode = DrawMode.CustomBrush;
        //             //multiColorSpeedConstant = .25f;
        //             ChangeBrushSize();
        //
        //             break;
        //
        //         case (int)DrawMode.FloodFill:
        //             string tempFloodFill = PlayerPrefs.GetString("FloodFillModeColor");
        //             MultiColor = false;
        //             if (!string.IsNullOrEmpty(tempFloodFill))
        //             {
        //                 var like = tempFloodFill.Split(',');
        //                 paintColor = new Color32((byte)int.Parse(like[0]), (byte)int.Parse(like[1]),
        //                     (byte)int.Parse(like[2]), 255);
        //                 //   if (!IsCallFromSubGrid)
        //                 // Camera.main.GetComponent<PlayAudio>().SelectColorSound(like[3]);
        //             }
        //             else
        //             {
        //                 paintColor = new Color32(0, 255, 255, 255);
        //             }
        //
        //             if (paintColor.r == 0 && paintColor.g == 255 && paintColor.b == 255 && paintColor.a == 255)
        //             {
        //                 drawMode = DrawMode.FloodFillPattern;
        //                 StartCoroutine(SetCustomPatternCoroutine(0));
        //                 if (!IsCallFromSubGrid)
        //                     // Camera.main.GetComponent<PlayAudio>().SelectColorSound("Multi");
        //                     break;
        //             }
        //
        //             drawMode = DrawMode.FloodFill;
        //             break;
        //
        //         case (int)DrawMode.Pattern:
        //             int tempPattern = PlayerPrefs.GetInt("PatternModeTexture");
        //             drawMode = DrawMode.Pattern;
        //             StartCoroutine(SetCustomPatternCoroutine(tempPattern));
        //             ChangeBrushSize();
        //             break;
        //
        //         case (int)DrawMode.FloodFillPattern:
        //             int tempPatternFlood = PlayerPrefs.GetInt("FloodPatternModeTexture");
        //             drawMode = DrawMode.FloodFillPattern;
        //             StartCoroutine(SetCustomPatternCoroutine(tempPatternFlood));
        //             ChangeBrushSize();
        //             break;
        //
        //         case (int)DrawMode.Eraser:
        //             drawMode = DrawMode.Eraser;
        //             break;
        //
        //         case (int)DrawMode.Sticker:
        //             drawMode = DrawMode.Sticker;
        //             break;
        //
        //         default:
        //             drawMode = DrawMode.Default;
        //             break;
        //     }
        // }
        private bool TryGetColorFromPref(string prefKey, out Color32 color, out bool isMulti)
        {
            color = new Color32(0, 255, 255, 255); // default fallback
            isMulti = false;

            string raw = PlayerPrefs.GetString(prefKey, null);

            if (string.IsNullOrWhiteSpace(raw))
            {
                // no saved value
                return false;
            }

            raw = raw.Trim();

            // handle "Multi" (case-insensitive)
            if (string.Equals(raw, "Multi", StringComparison.OrdinalIgnoreCase))
            {
                isMulti = true;
                return true;
            }

            // expected format "R,G,B" or "R,G,B,optional"
            var parts = raw.Split(',');
            if (parts.Length < 3)
            {
                Debug.LogWarning(
                    $"TryGetColorFromPref: stored value for '{prefKey}' is malformed: '{raw}' (expected at least 3 comma-separated numbers).");
                return false;
            }

            // try parse each component safely
            bool okR = byte.TryParse(parts[0].Trim(), out byte r);
            bool okG = byte.TryParse(parts[1].Trim(), out byte g);
            bool okB = byte.TryParse(parts[2].Trim(), out byte b);

            if (!okR || !okG || !okB)
            {
                Debug.LogWarning($"TryGetColorFromPref: failed to parse RGB for '{prefKey}' from '{raw}'.");
                return false;
            }

            color = new Color32(r, g, b, 255);
            return true;
        }

        public void SetDrawMode(int Mode, bool IsCallFromSubGrid = false)
        {
            switch (Mode)
            {
                case (int)DrawMode.Default:
                {
                    string prefKey = "DefaultModeColor";
                    if (TryGetColorFromPref(prefKey, out Color32 parsedColor, out bool parsedIsMulti))
                    {
                        MultiColor = parsedIsMulti;
                        if (!parsedIsMulti)
                        {
                            paintColor = parsedColor;
                        }
                        // else MultiColor true — keep existing multi behavior
                    }
                    else
                    {
                        // fallback if no valid pref
                        MultiColor = true;
                    }

                    drawMode = DrawMode.Default;
                    ChangeBrushSize();
                    break;
                }

                case (int)DrawMode.CustomBrush:
                {
                    string prefKey = "CustomModeColor";
                    if (TryGetColorFromPref(prefKey, out Color32 parsedColor, out bool parsedIsMulti))
                    {
                        MultiColor = parsedIsMulti;
                        if (!parsedIsMulti)
                        {
                            paintColor = parsedColor;
                        }
                    }
                    else
                    {
                        MultiColor = true;
                    }

                    drawMode = DrawMode.CustomBrush;
                    ChangeBrushSize();
                    break;
                }

                case (int)DrawMode.FloodFill:
                {
                    string prefKey = "FloodFillModeColor";
                    MultiColor = false;
                    if (TryGetColorFromPref(prefKey, out Color32 parsedColor, out bool parsedIsMulti))
                    {
                        // parsedIsMulti shouldn't be true for FloodFill, but guard anyway
                        paintColor = parsedColor;
                    }
                    else
                    {
                        paintColor = new Color32(0, 255, 255, 255); // same default as original
                    }

                    // special-case: if color is default cyan -> FloodFillPattern
                    if (paintColor.r == 0 && paintColor.g == 255 && paintColor.b == 255 && paintColor.a == 255)
                    {
                        drawMode = DrawMode.FloodFillPattern;
                        StartCoroutine(SetCustomPatternCoroutine(0));
                        if (!IsCallFromSubGrid)
                        {
                            // optional audio call if you want
                        }

                        break;
                    }

                    drawMode = DrawMode.FloodFill;
                    break;
                }

                case (int)DrawMode.Pattern:
                {
                    int tempPattern = PlayerPrefs.GetInt("PatternModeTexture", 0);
                    drawMode = DrawMode.Pattern;
                    StartCoroutine(SetCustomPatternCoroutine(tempPattern));
                    ChangeBrushSize();
                    break;
                }

                case (int)DrawMode.FloodFillPattern:
                {
                    int tempPatternFlood = PlayerPrefs.GetInt("FloodPatternModeTexture", 0);
                    drawMode = DrawMode.FloodFillPattern;
                    StartCoroutine(SetCustomPatternCoroutine(tempPatternFlood));
                    ChangeBrushSize();
                    break;
                }

                case (int)DrawMode.Eraser:
                    drawMode = DrawMode.Eraser;
                    break;

                case (int)DrawMode.Sticker:
                    drawMode = DrawMode.Sticker;
                    break;

                default:
                    drawMode = DrawMode.Default;
                    break;
            }
        }

        public void SetCurrentStickerIndex(int index)
        {
            CurrentSticker = index;
        }

        public IEnumerator SetCustomPatternCoroutine(int index)
        {
            selectedPattern = index;
            switch (drawMode)
            {
                case DrawMode.Pattern:
                    PlayerPrefs.SetInt("PatternModeTexture", index);
                    break;
                case DrawMode.FloodFillPattern:
                    PlayerPrefs.SetInt("FloodPatternModeTexture", index);
                    break;
            }

            int divisor = -1;
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                if (ShapeIndex < images.Length)
                {
                    divisor = images[ShapeIndex].height > images[ShapeIndex].width
                        ? images[ShapeIndex].width
                        : images[ShapeIndex].height;
                }
            }
            else
            {
                divisor =
                    Handler.MyHandler.WhiteSprite[Self_Index].height > Handler.MyHandler.WhiteSprite[Self_Index].width
                        ? Handler.MyHandler.WhiteSprite[Self_Index].width
                        : Handler.MyHandler.WhiteSprite[Self_Index].height;
            }

            int width = (int)(divisor / 2.5f);
            int height = (int)(divisor / 2.5f);
            if (width > 250)
            {
                width = 250;
                height = 250;
            }

            switch (drawMode)
            {
                case DrawMode.Pattern:
                    Resize(customPatterns[selectedPattern], width, height);
                    break;
                case DrawMode.FloodFillPattern:
                    PlayerPrefs.SetString("FloodFillModeColor", "0,255,255,Multi");
                    if (texWidth > texHeight)
                    {
                        LoadFloodFillPattern(RainbowPattern, texWidth, texWidth);
                    }
                    else
                    {
                        LoadFloodFillPattern(RainbowPattern, texHeight, texHeight);
                    }

                    break;
            }

            yield return null;
        }

        Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 0, RenderTextureFormat.ARGB32)
                { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            currentPattern = new Texture2D(targetX, targetY);
            currentPattern.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0, true);
            currentPattern.Apply();
            customPatternWidth = currentPattern.width;
            customPatternHeight = currentPattern.height;
            Color32[] tempPixels = currentPattern.GetPixels32();

            temparrayforPattern = new byte[texWidth * texHeight * 4];
            int pixel = 0;
            for (int pixY = 0; pixY < texHeight; pixY++)
            {
                for (int pixX = 0; pixX < texWidth; pixX++)
                {
                    int yy = pixX % targetX;
                    int xx = pixY % targetX;
                    int pixel2 = ((targetX * xx + yy)) % tempPixels.Length;

                    temparrayforPattern[pixel] = tempPixels[pixel2].r;
                    temparrayforPattern[pixel + 1] = tempPixels[pixel2].g;
                    temparrayforPattern[pixel + 2] = tempPixels[pixel2].b;
                    temparrayforPattern[pixel + 3] = tempPixels[pixel2].a;

                    pixel += 4;
                }
            }

            return currentPattern;
        }

        public void LoadFloodFillPattern(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 0, RenderTextureFormat.ARGB32)
                { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            currentPattern = new Texture2D(targetX, targetY);
            currentPattern.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0, true);
            currentPattern.Apply();
            customPatternWidth = currentPattern.width;
            customPatternHeight = currentPattern.height;
            floodFillPatternPixels = new byte[customPatternWidth * customPatternHeight * 4];

            Color32[] tempPixels = currentPattern.GetPixels32();
            int tempCount = tempPixels.Length;
            int pixel = 0;
            for (int i = 0; i < tempCount; i++)
            {
                floodFillPatternPixels[pixel] = tempPixels[i].r;
                floodFillPatternPixels[pixel + 1] = tempPixels[i].g;
                floodFillPatternPixels[pixel + 2] = tempPixels[i].b;
                floodFillPatternPixels[pixel + 3] = tempPixels[i].a;

                pixel += 4;
            }
        }

        IEnumerator TakeScreenShot()
        {
            //SaveGame();
            print("taking ss");
            if (!isComplete)
            {
                Debug.Log(IsAbleToSaveGame);
                IsAbleToSaveGame = false;
                int lightBluePixels = CountPixelsInRange(drawingTexture.GetPixels32(), new Color32(0, 0, 0, 255),
                    new Color32(50, 50, 50, 255));

                Debug.Log("Black Color Set: " + totalLightBluePixels);

                //totalPixelsText.text = "Total Area " + CalculatePercentage(lightBluePixels, totalcolorPixels);
                // Debug.Log(totalPixelsText.text + "Painteed area");
                if (IsOutline && ShapeIndex == 0)
                {
                    var p = Path.Combine(Application.persistentDataPath,
                        ServiceManager.instance.selectedCharacter.CharacterLocalPath);
                }

                //Debug.Log("Selectedddd: " + images[ShapeIndex].name);

                //var z = GetComponent<MeshRenderer>().material;
                Sprite sprite = Sprite.Create(images[ShapeIndex],
                    new Rect(0, 0, images[ShapeIndex].width, images[ShapeIndex].height), new Vector2(0.5f, 0.5f));
                float a = 11.5f;
                float t = (a / Mathf.Sqrt(totalLightBluePixels) * 22.5f);
                //float t = 99.7f;

                Debug.Log($"lightBluePixels: {lightBluePixels}, totalLightBluePixels: {totalLightBluePixels}, t = {t}");

                if (IsBelowThreshold(lightBluePixels, totalLightBluePixels, t))
                {
                    LoadNextImage = true;
                    if (gameObject.GetComponent<MeshCollider>())
                    {
                        gameObject.GetComponent<MeshCollider>().enabled = false;
                    }

                    isBackButtonUsed = false;
                    SaveGame();
                    if (Handler.MyHandler.VoiceOver.Length > 0)
                    {
                        var index = Random.Range(0, Handler.MyHandler.VoiceOver.Length);
                        if (IsOutline && Handler.MyHandler.VoiceOver[index] != null)
                        {
                            SoundHandler.instance.PlaySource(Handler.MyHandler.VoiceOver[index]);
                        }
                    }

                    SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
                    colorfill_complete.Play();
                    Debug.Log("Completed 1st step!");
                    cam_pos.SaveForScreenSHot(ShapeIndex);
                }

                if (DrawMode.FloodFill == drawMode || DrawMode.FloodFillPattern == drawMode)
                {
                    LoadNextImage = true;
                    if (!IsOutline)
                        gameObject.GetComponent<MeshCollider>().enabled = false;
                    else
                        Destroy(gameObject.GetComponent<MeshCollider>());
                    isBackButtonUsed = false;
                    SaveGame();
                    // SoundManager.instance.PlayCharacterVoice();
                    // SoundManager.instance.PlayEffect_Instance(1);
                    SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
                    colorfill_complete.Play();
                    Debug.Log("Completed 1st step!");
                    cam_pos.SaveForScreenSHot(ShapeIndex);
                }

                if (DrawMode.Sticker == drawMode)
                {
                    LoadNextImage = true;
                    gameObject.GetComponent<MeshCollider>().enabled = false;
                    isBackButtonUsed = false;
                    SaveGame();
                    // SoundManager.instance.PlayCharacterVoice();
                    // SoundManager.instance.PlayEffect_Instance(1);
                    SoundHandler.instance.PlaySource(SoundHandler.instance.selectCh);
                    colorfill_complete.Play();
                    Debug.Log("Completed 1st step!");
                    cam_pos.SaveForScreenSHot(ShapeIndex);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        float CalculatePercentage(int count, int total)
        {
            Debug.Log("counttttt: " + count);
            float percentage = (float)count / total * 100f;
            return percentage; // Format the percentage with 2 decimal places
        }

        int CountPixels(Color32[] pixels, params Color32[] targetColors)
        {
            int count = 0;
            foreach (Color32 pixel in pixels)
            {
                if (Array.Exists(targetColors, c => c.Equals(pixel)))
                {
                    count++;
                }
            }

            return count;
        }

        int CountPixelsInRange(Color32[] pixels, Color32 minColor, Color32 maxColor)
        {
            int count = 0;

            foreach (Color32 pixel in pixels)
            {
                if (IsColorInRange(pixel, minColor, maxColor))
                {
                    count++;
                }
            }

            return count;
        }

        bool IsColorInRange(Color32 color, Color32 minColor, Color32 maxColor)
        {
            return color.r >= minColor.r && color.r <= maxColor.r &&
                   color.g >= minColor.g && color.g <= maxColor.g &&
                   color.b >= minColor.b && color.b <= maxColor.b &&
                   color.a >= minColor.a && color.a <= maxColor.a;
        }

        //string CalculatePercentage(int count, int total)
        //{
        //    float percentage = ((float)count / total) * 100;
        //    return percentage.ToString();
        //}

        bool IsBelowThreshold(int count, int total, float threshold)
        {
            Debug.Log("percentage: " + threshold + "Before Count: " + count);


            float percentage = ((float)count / total) * 100;

            Debug.Log("percentage: " + percentage + "   " + threshold);

            /*Debug.Log("Fakhar parcentage: " + int.Parse(totalPixelsText.text));

            if(int.Parse(totalPixelsText.text) >= 95 && SwitchToolsSubGrid.currentIndex == 1)
            {
                return true;
            }
            else*/
            {
                /*int lightBluePixels = CountPixelsInRange(drawingTexture.GetPixels32(), new Color32(0, 0, 0, 255), new Color32(50, 50, 50, 255));
                float x = CalculatePercentage(lightBluePixels, totalcolorPixels);
                Debug.Log("Asad Bhai Percantage: " + percentage);

                if (x > 90f && SwitchToolsSubGrid.currentIndex == 1)
                {
                    return true;
                }
                else if (x <= 5f && SwitchToolsSubGrid.currentIndex == 1&& AnimateToolsBackground.isblack)
                {
                    return true;
                }
                //Checks Black Color
                else if (AnimateToolsBackground.isblack && SwitchToolsSubGrid.currentIndex != 1)
                {
                    return x >= 99.5f;
                }
                else*/
                {
                    /*if (percentage > threshold)
                    {
                        percentage = 0;
                        return true;
                    }
                    else
                    {
                        return false;
                    }*/
                    return percentage < threshold;
                }
            }
        }

        void LoadImageAsCanvas()
        {
            Debug.Log("Load Image Canvas");
            LoadGame();
            UpdateTexture();
        }

        // assigns new canvas image
        public void SetCanvasImage(Texture2D newTexture, int index)
        {
            Debug.Log("SetCanvasImage!!!!!!!!!!");
            // NOTE: if new texture is different size, problems will occur when drawing
            if (!isComplete && !isLast)
            {
                myRenderer.material.SetTexture(targetTexture, newTexture);
                InitializeAllThings();
            }
        }

        public void NextImage()
        {
            if (!isGoingForward)
            {
                isGoingForward = true;
                isGoingBack = false;
                nextPicRemainingTime = 0;

                // handIndicatorOnNext.GetComponent<Animator>().Play("empty anim");
                //handIndicatorOnNext.SetActive(false);

                ShapeIndex = (ShapeIndex + 1); // wrap around
                //EventForStepCompleted(ShapeIndex);
                if (ShapeIndex > ServiceManager.instance.selectedCharacter.NumberOfImages - 1)
                {
                    Debug.Log("Enable all sprite");
                    isComplete = true;
                    isLast = true;
                }
                else if (ShapeIndex > maxShapeIndex)
                {
                    //Debug.Log("shape index is grater then maxshapeindex" + ShapeIndex + "" + maxShapeIndex);
                    cam_pos.maxShapeIndex = maxShapeIndex = ShapeIndex;
                }

                StartCoroutine(fadeSpriteAndSetTargetToNext());
            }
        }

        // Log the events for StepCompleted
        //public void NextImageBtn()
        //{
        //    if (!isGoingForward)
        //    {
        //        handIndicatorOnNext.GetComponent<Animator>().Play("empty anim");
        //        handIndicatorOnNext.SetActive(false);
        //        isGoingForward = true;
        //        if (ShapeIndex + 1 == maxShapeIndex)
        //        {
        //            if (isBackButtonUsed)
        //            {
        //                isGoingBack = false;
        //                nextPicRemainingTime = 0;
        //                LoadNextImage = false;
        //                ShapeIndex = (ShapeIndex + 1);
        //                StartCoroutine(fadeSpriteAndSetTargetToNext());
        //                Debug.Log("Shape index is equal to the max shape index");
        //            }
        //            Debug.Log("IsBack button is false");
        //            isBackButtonUsed = false;
        //            RedoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 125);
        //        }
        //        else if (isBackButtonUsed)
        //        {
        //            isGoingBack = false;
        //            nextPicRemainingTime = 0;
        //            LoadNextImage = false;
        //            ShapeIndex = (ShapeIndex + 1);
        //            StartCoroutine(fadeSpriteAndSetTargetToNext());
        //        }
        //    }
        //}
        public void BackImage()
        {
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                if (cam_pos.objects[ShapeIndex].transform.childCount > 0 && ShapeIndex > 0) //For White Sprite Layer
                {
                    cam_pos.layer = cam_pos.objects[ShapeIndex].transform.GetChild(0).transform
                        .GetComponent<SpriteRenderer>().sortingOrder;
                    cam_pos.objects[ShapeIndex].transform.GetChild(0).transform.GetComponent<SpriteRenderer>()
                        .sortingLayerName = "Painted";
                    cam_pos.objects[ShapeIndex].transform.GetChild(0).transform.GetComponent<SpriteRenderer>()
                        .sortingOrder = cam_pos.layer;
                }
            }

            ShapeIndex = ShapeIndex - 1;
            if (ShapeIndex < 0)
            {
                ShapeIndex = 0;
                return;
            }

            StartCoroutine(fadeSpriteAndSetTargetToBack());
        }

        public IEnumerator fadeSpriteAndSetTargetToNext()
        {
            Debug.Log("fadeSpriteAndSetTargetToNext !!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            isGoingBack = false;
            //changeSpineTex.Apply(ShapeIndex, true); //waqas

            //cam_pos.Apply(ShapeIndex);
            cam_pos.Apply1(ShapeIndex);
            while (this.GetComponent<Renderer>().material.color.a > 0)
            {
                Color objectColor = this.GetComponent<Renderer>().material.color;
                float fadeAmount = objectColor.a - (0.06f);
                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                this.GetComponent<Renderer>().material.color = objectColor;
                yield return new WaitForSeconds(0.01f);
            }


            if (ShapeIndex + 1 > ServiceManager.instance.selectedCharacter.NumberOfImages)
            {
                Debug.Log("Complete!!!!!!!!!!!!!!!!!!!!!!! ");
                //targetCamSize = cam_pos.PositionAndScale[ShapeIndex].camSize;
                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    //currentAttachmentPos = cam_pos.GetCurrentPosition(ShapeIndex - 1);
                    Handler.MyHandler.HandlerCamera = CurrentCam;
                    print("dadasd");
                    Handler.MyHandler.OutlineCompleted = true;
                    Handler.MyHandler.GenerateInner();

                    DOTween.Kill(gameObject);
                    DOTween.KillAll();
                    Destroy(gameObject);
                    Handler.MyHandler.Inner.Remove(this);
                    Handler.MyHandler.CameraFinalPosition();
                }
                else
                {
                    currentAttachmentPos = cam_pos.GetCurrentPosition(Self_Index - 1);
                    Debug.Log(currentAttachmentPos);
                }

                if (!isComplete && !isLast)
                {
                    Debug.Log("Not Last: " + isLast);
                    //targetCamPosition = currentAttachmentPos;
                    //  targetCamPosition = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y, -10);
                    //targetCamPosition = new Vector3(2.5f, 2.5f, -10f);
                    targetCamPosition = cam_pos.PositionAndScale[ShapeIndex].pos;
                    CurrentCam.transform.DOMove(targetCamPosition, 1f).SetEase(Ease.Linear);
                    CurrentCam.DOOrthoSize(targetCamSize, 1f).OnComplete(() =>
                    {
                        Debug.Log("Play animation");
                        LoadNextImage = false;
                    });
                }
            }
            else
            {
                WhenAnimationIsCompleted();
            }
        }

        void WhenAnimationIsCompleted()
        {
            Handler.MyHandler.timeWithoutTouch = 0;
            StartCoroutine(WaitFor2Sec(() =>
            {
                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(images[ShapeIndex],
                        new Rect(0, 0, images[ShapeIndex].width, images[ShapeIndex].height), new Vector2(.5f, .5f));
                }
                else
                {
                    referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(White_image,
                        new Rect(0, 0, White_image.width, White_image.height), new Vector2(.5f, .5f));
                }

                referenceArea.gameObject.GetComponent<Image>().SetNativeSize();
                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    SavePath = Path.Combine(DrawingPath, images[ShapeIndex].name + ".dat");
                }
                else
                {
                    SavePath = Path.Combine(DrawingPath, White_image.name + ".dat");
                }

                if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
                {
                    currentAttachmentPos = cam_pos.GetCurrentPosition(ShapeIndex);
                    Debug.Log(currentAttachmentPos);
                }
                else
                {
                    currentAttachmentPos = cam_pos.GetCurrentPosition(Self_Index);
                    Debug.Log(currentAttachmentPos);
                }

                SetNextCanvas = true;
                Handler.MyHandler.timeWithoutTouch = 0;
                //skeletonAnimation.ClearState();
                //particleEffect.SetActive(false);
                if (ShapeIndex > 0)
                {
                    UndoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                }
                else
                {
                    UndoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 125);
                }
            }));
        }

        IEnumerator fadeSpriteAndSetTargetToBack()
        {
            //Debug.Log("FadeSprite !!!!!!!!!!!!!!!!!" + ShapeIndex);


            var data = Path.Combine(DrawingPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
            data = Path.Combine(DrawingPath, images[ShapeIndex].name + ".dat");
            if (File.Exists(data))
            {
                File.Delete(data);
            }

            var p = Path.Combine(DrawingPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
            p = Path.Combine(DrawingPath, images[ShapeIndex].name + ".png");
            if (File.Exists(p))
            {
                File.Delete(p);
            }

            isGoingBack = true;
            while (this.GetComponent<Renderer>().material.color.a > 0)
            {
                Debug.Log("Renderer in fadesprite !!!!!!!!!!!!!!!!!");
                Color objectColor = this.GetComponent<Renderer>().material.color;
                float fadeAmount = objectColor.a - (0.06f);
                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                this.GetComponent<Renderer>().material.color = objectColor;
                yield return new WaitForSeconds(0.01f);
            }

            if (ShapeIndex - 1 < cam_pos.WhiteImagePosition.Length)
            {
                if (ShapeIndex - 1 > 0 && cam_pos.WhiteImagePosition[ShapeIndex - 1])
                {
                    cam_pos.WhiteImagePosition[ShapeIndex - 1].gameObject.SetActive(false);
                    print(ShapeIndex - 1 + "Channnage");
                }
            }

            //if (cam_pos.WhiteImagePosition[ShapeIndex].transform.GetSiblingIndex() == 1)
            //{
            //    cam_pos.WhiteImagePosition[ShapeIndex].transform.parent.GetChild(0).gameObject.SetActive(false);
            //}
            isBackButtonUsed = true;
            RedoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            nextPicRemainingTime = 0;
            LoadNextImage = false;

            if (ShapeIndex > 0)
            {
                UndoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            else
            {
                UndoBtn.GetComponent<Image>().color = new Color32(255, 255, 255, 125);
            }

            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(images[ShapeIndex],
                    new Rect(0, 0, images[ShapeIndex].width, images[ShapeIndex].height), Vector2.zero);
            }
            else
            {
                referenceArea.gameObject.GetComponent<Image>().sprite = Sprite.Create(White_image,
                    new Rect(0, 0, White_image.width, White_image.height), Vector2.zero);
            }

            referenceArea.gameObject.GetComponent<Image>().SetNativeSize();

            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                SavePath = Path.Combine(DrawingPath, images[ShapeIndex].name + ".dat");
            }
            else
            {
                SavePath = Path.Combine(DrawingPath, White_image.name + ".dat");
            }

            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                currentAttachmentPos = cam_pos.GetCurrentPosition(ShapeIndex);
                Debug.Log(currentAttachmentPos);
            }
            else
            {
                currentAttachmentPos = cam_pos.GetCurrentPosition(Self_Index);
                Debug.Log(currentAttachmentPos);
            }

            SetNextCanvas = true;
        }

        public IEnumerator fadeInCurrentSprite()
        {
            Debug.Log("fadeInCurrentSprite start");

            GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0);
            fadeInAndFadeOutColor = GetComponent<Renderer>().material.color;
            while (GetComponent<Renderer>().material.color.a < 1f)
            {
                fadeInAndFadeOutColor.a = fadeInAndFadeOutColor.a + (0.06f);
                GetComponent<Renderer>().material.color = fadeInAndFadeOutColor;
                yield return new WaitForSeconds(0.005f);
                if (cam_pos.objects[ShapeIndex].transform.childCount >= 2 /*&& ShapeIndex == 0*/) //For Eyes
                {
                    print("MYEYES");

                    if (GetComponent<Renderer>().material.color.a < 1)
                    {
                        cam_pos.objects[ShapeIndex].transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }

            GetComponent<MeshRenderer>().enabled = true;
            print("on");
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                if (cam_pos.objects[ShapeIndex].transform.childCount > 0 && ShapeIndex > 0) //For White Sprite Layer
                {
                    cam_pos.layer = cam_pos.objects[ShapeIndex].transform.GetChild(0).transform
                        .GetComponent<SpriteRenderer>().sortingOrder;
                    cam_pos.objects[ShapeIndex].transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            isGoingForward = false;
            isGoingBack = false;
            Debug.Log("fadeInCurrentSprite end");
            if (!eventS.gameObject.activeInHierarchy && ShapeIndex < cam_pos.PositionAndScale.Length)
                eventS.gameObject.SetActive(true);
            //if (IsOutline)

            {
                GetComponent<MeshCollider>().enabled = true;
            }
        }

        public static void CreateDirectoryIfNoExistsForFileName(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            CreateDirectoryIfNoExists(dir);
        }

        public static void CreateDirectoryIfNoExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public void SaveGame()
        {
            CreateDirectoryIfNoExistsForFileName(SavePath);
            FileStream file = File.Create(SavePath);
            SaveData data = new SaveData();

            // Copy pixel data
            data.savedPixels = new byte[pixels.Length];
            Array.Copy(pixels, data.savedPixels, pixels.Length);

            // Process and copy toSavePixels
            byte[] newPixels = new byte[toSavePixels.Length];
            int pixel = 0;
            for (int i = 0; i < toSavePixels.Length; i += 4)
            {
                bool isSameColor = toSavePixels[i] == toSaveClearPixels[i] &&
                                   toSavePixels[i + 1] == toSaveClearPixels[i + 1] &&
                                   toSavePixels[i + 2] == toSaveClearPixels[i + 2] &&
                                   toSavePixels[i + 3] == toSaveClearPixels[i + 3];

                if (!isSameColor || (isSameColor && toSavePixels[i] != toSavePixels[i + 1] &&
                                     toSavePixels[i] != toSavePixels[i + 2]))
                {
                    newPixels[pixel] = toSavePixels[i];
                    newPixels[pixel + 1] = toSavePixels[i + 1];
                    newPixels[pixel + 2] = toSavePixels[i + 2];
                    newPixels[pixel + 3] = toSavePixels[i + 3];
                }
                else if (isSameColor && (toSaveClearPixels[i] == 255 || toSaveClearPixels[i] == 0))
                {
                    newPixels[pixel] = 255;
                    newPixels[pixel + 1] = 255;
                    newPixels[pixel + 2] = 255;
                    newPixels[pixel + 3] = toSaveClearPixels[i];
                }
                else
                {
                    newPixels[pixel] = 255;
                    newPixels[pixel + 1] = 255;
                    newPixels[pixel + 2] = 255;
                    newPixels[pixel + 3] = 0;
                }

                pixel += 4;
            }

            data.savedPixelsOnlyDrawn = new byte[newPixels.Length];
            Array.Copy(newPixels, data.savedPixelsOnlyDrawn, newPixels.Length);

            // Set other data properties
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                data.path = Path.Combine(DrawingPath, images[ShapeIndex].name + ".png");
            }
            else
            {
                data.path = Path.Combine(DrawingPath, White_image.name + ".png");
            }

            data.targetCamPositionX = targetCamPosition.x;
            data.targetCamPositionY = targetCamPosition.y;
            data.targetCamPositionZ = targetCamPosition.z;
            data.targetCamSize = targetCamSize;

            // Encode texture data
            Texture2D textureToSave =
                new Texture2D(drawingTexture.width, drawingTexture.height, TextureFormat.RGBA32, false);
            textureToSave.LoadRawTextureData(newPixels);
            textureToSave.Apply(false);
            data.texture = textureToSave.EncodeToPNG();

            if (IsOutline && ShapeIndex == images.Length - 1)
            {
                eventS.SetActive(false);
            }

            // Serialize and save data
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, data);
            file.Close();
        }

        void LoadGame()
        {
            if (isBackButtonUsed && File.Exists(SavePath))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(SavePath, FileMode.Open);
                SaveData data = (SaveData)bf.Deserialize(file);
                file.Close();
                System.Array.Copy(data.savedPixels, pixels, data.savedPixels.Length);
                System.Array.Copy(data.savedPixelsOnlyDrawn, toSavePixels, data.savedPixelsOnlyDrawn.Length);

                isGoingForward = false;
            }
            else
            {
                isGoingForward = false;
            }
        }

        void EraseWithImageLine(Vector2 start, Vector2 end)
        {
            int x0 = (int)start.x;
            int y0 = (int)start.y;
            int x1 = (int)end.x;
            int y1 = (int)end.y;
            int tempVal = x1 - x0;
            int dx = (tempVal + (tempVal >> 31)) ^
                     (tempVal >> 31); // http://stackoverflow.com/questions/6114099/fast-integer-abs-function
            tempVal = y1 - y0;
            int dy = (tempVal + (tempVal >> 31)) ^ (tempVal >> 31);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            int pixelCount = 0;
            int e2;
            for (;;)
            {
                if (hiQualityBrush)
                {
                    EraseWithImage(x0, y0);
                }
                else
                {
                    pixelCount++;
                    if (pixelCount > brushSizeDiv4)
                    {
                        pixelCount = 0;
                        EraseWithImage(x0, y0);
                    }
                }

                if ((x0 == x1) && (y0 == y1)) break;
                e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                else if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }
        }

        public void LoadMainScene()
        {
            SceneManager.LoadScene(1);
            //StartCoroutine(ShowLoadingPanel());
            // SendAmplitudeEvent();
        }

        public void SendAmplitudeEvent()
        {
            Dictionary<string, object> property = new Dictionary<string, object>();
        }

        public void CalculatePathOfShape()
        {
            objToFollow.transform.position = RefCam.transform.position;
            objToFollow.GetComponent<SpriteRenderer>().sprite = null;
            var col = objToFollow.GetComponent<PolygonCollider2D>();
            if (col == null)
            {
                Debug.Log("collider does not exist");
            }

            Destroy(col);
            Texture2D texturea;

            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                texturea = new Texture2D(images[ShapeIndex].width, images[ShapeIndex].height, TextureFormat.RGBA32,
                    false);
            }
            else
            {
                texturea = new Texture2D(White_image.width, White_image.height, TextureFormat.RGBA32, false);
            }

            //texturea.SetPixels32(images[ShapeIndex].GetPixels32());
            //texturea.Apply();
            Color32[] tempPixels;
            Color32[] tempPixelsOrg;
            if (IsOutline /*&& PlayerPrefs.GetInt(ServiceManager.instance.selectedCharacter.PrefName) == 0*/)
            {
                tempPixels = images[ShapeIndex].GetPixels32();
                tempPixelsOrg = images[ShapeIndex].GetPixels32();
            }
            else
            {
                tempPixels = White_image.GetPixels32();
                tempPixelsOrg = White_image.GetPixels32();
            }

            int tempCount = tempPixels.Length;
            int whitePixels = 0;
            for (int j = 0; j < tempCount; j++)
            {
                if (tempPixelsOrg[j].r == 255 && tempPixelsOrg[j].g == 255 && tempPixelsOrg[j].b == 255)
                {
                    tempPixels[j].r = tempPixelsOrg[j].r;
                    tempPixels[j].g = tempPixelsOrg[j].g;
                    tempPixels[j].b = tempPixelsOrg[j].b;
                    tempPixels[j].a = tempPixelsOrg[j].a;
                    //tempPixels[j] = new Color32(tempPixelsOrg[j].r, tempPixelsOrg[j].g, tempPixelsOrg[j].b, tempPixelsOrg[j].a);
                    whitePixels++;
                }
                else
                {
                    tempPixels[j].r = tempPixelsOrg[j].r;
                    tempPixels[j].g = tempPixelsOrg[j].g;
                    tempPixels[j].b = tempPixelsOrg[j].b;
                    tempPixels[j].a = 0;
                    //tempPixels[j] = new Color32(tempPixelsOrg[j].r, tempPixelsOrg[j].g, tempPixelsOrg[j].b, 0);
                }
            }

            if (whitePixels < 100)
            {
                for (int j = 0; j < tempCount; j++)
                {
                    if (tempPixelsOrg[j].a < 100)
                    {
                        //tempPixels[j] = new Color32(tempPixelsOrg[j].r, tempPixelsOrg[j].g, tempPixelsOrg[j].b, 0);
                        tempPixels[j].r = tempPixelsOrg[j].r;
                        tempPixels[j].g = tempPixelsOrg[j].g;
                        tempPixels[j].b = tempPixelsOrg[j].b;
                        tempPixels[j].a = 0;
                    }
                    else
                    {
                        tempPixels[j].r = tempPixelsOrg[j].r;
                        tempPixels[j].g = tempPixelsOrg[j].g;
                        tempPixels[j].b = tempPixelsOrg[j].b;
                        tempPixels[j].a = tempPixelsOrg[j].a;
                        //tempPixels[j] = new Color32(tempPixelsOrg[j].r, tempPixelsOrg[j].g, tempPixelsOrg[j].b, tempPixelsOrg[j].a);
                    }
                }
            }

            texturea.SetPixels32(tempPixels);
            texturea.Apply();

            objToFollow.transform.position = new Vector3(currentAttachmentPos.x, currentAttachmentPos.y,
                objToFollow.transform.position.z);
            objToFollow.GetComponent<SpriteRenderer>().sprite = Sprite.Create(texturea,
                new Rect(0, 0, texturea.width, texturea.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight);

            PolygonCollider2D comp = null;

            //StartCoroutine(WaitWhile(() =>
            //{
            objToFollow.AddComponent<PolygonCollider2D>();
            comp = objToFollow.GetComponent<PolygonCollider2D>();
            //}));

            StartCoroutine(WaitWhile(() =>
            {
                if (comp == null)
                {
                    comp = objToFollow.GetComponent<PolygonCollider2D>();
                }

                Vector2[] pathlist = comp.GetPath(0);
                Mesh mesh = comp.CreateMesh(true, true);
                Vector3[] vertices = new Vector3[pathlist.Length];

                for (int i = 0; i < pathlist.Length; i++)
                {
                    vertices[i] = pathlist[i];
                }

                Triangulator triangulator = new Triangulator(pathlist);
                int[] tris = triangulator.Triangulate();
                var boundaryPath = EdgeHelpers.GetEdges(tris).FindBoundary().SortEdges();

                path = new Vector3[boundaryPath.Count + 1];
                for (int i = 0; i < boundaryPath.Count; i++)
                {
                    path[i] = vertices[boundaryPath[i].v1];
                    Vector3 OT = objToFollow.transform.position;
                    path[i] = new Vector3(vertices[boundaryPath[i].v1].x + OT.x, vertices[boundaryPath[i].v1].y + OT.y,
                        -1);
                }

                path[boundaryPath.Count] = path[0];
                Handler.MyHandler.timeWithoutTouch = -2;
                RealtimeWithoutTouch = -2;
            }));
        }

        // public void EnableToolsGrid()
        // {
        //     if (ShapeIndex == 0 && !toolsGridHolder.activeSelf)
        //     {
        //         toolsGridHolder.SetActive(true);
        //         topBtnsHolder.SetActive(true);
        //     }
        // }

        IEnumerator WaitWhile(Action CallBack)
        {
            yield return new WaitForSeconds(0.1f);
            CallBack?.Invoke();
        }

        IEnumerator WaitFor2Sec(Action CallBack)
        {
            yield return null;
            CallBack?.Invoke();
        }

        private Mesh SpriteToMesh(Sprite sprite)
        {
            Mesh mesh = new Mesh();
            mesh.SetVertices(Array.ConvertAll(sprite.vertices, i => (Vector3)i).ToList());
            mesh.SetUVs(0, sprite.uv.ToList());
            mesh.SetTriangles(Array.ConvertAll(sprite.triangles, i => (int)i), 0);

            return mesh;
        }

        public bool IsVisibleFrom(Renderer renderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            var IsVisible = false;
            TestPlanesAABB(planes, planes.Length, renderer.bounds, out IsVisible);
            return IsVisible;
        }

        public bool TestPlanesAABB(Plane[] planes, int planeMask, Bounds bounds, out bool entirelyInside)
        {
            int planeIndex = 0;
            int entirelyInsideCount = 0;
            Vector3 boundsCenter = bounds.center; // center of bounds
            Vector3 boundsExtent = bounds.extents; // half diagonal
            // do intersection test for each active frame
            int mask = 1;
            entirelyInside = false;
            // while active frames
            while (mask <= planeMask)
            {
                // if active
                if ((uint)(planeMask & mask) != 0)
                {
                    Plane p = planes[planeIndex];
                    Vector3 n = p.normal;
                    n.x = Mathf.Abs(n.x);
                    n.y = Mathf.Abs(n.y);
                    n.z = Mathf.Abs(n.z);

                    float distance = p.distance; // p.GetDistanceToPoint(boundsCenter);
                    float radius = Vector3.Dot(boundsExtent, n);

                    if (distance + radius < 0)
                    {
                        // behind clip plane
                        entirelyInside = false;
                        return false;
                    }

                    if (distance > radius)
                    {
                        //entirelyInside = true;
                        entirelyInsideCount++;
                    }
                }

                mask += mask;
                planeIndex++;
            }

            entirelyInside = entirelyInsideCount >= 2;
            return true;
        }

        public void MergeTextures(Texture2D layer1, int startX, int startY)
        {
            Vector2 textureSize = new Vector2(layer1.width, layer1.height);
            startX = startX - (int)(textureSize.x / 2);
            startY = startY - (int)(textureSize.y / 2);

            for (int x = 0; x < drawingTexture.width; x++)
            {
                for (int y = 0; y < drawingTexture.height; y++)
                {
                    if (x >= startX && y >= startY && x < textureSize.x + startX && y < textureSize.y + startY
                        && pixels[((texWidth * y + x) << 2) + 3] > 50
                       )
                    {
                        int pixel = (texWidth * y + x) << 2;

                        Color32 bgColor = new Color32(pixels[pixel], pixels[pixel + 1], pixels[pixel + 2],
                            pixels[pixel + 3]);
                        Color32 wmColor = layer1.GetPixel(x - startX, y - startY);

                        Color32 finalColor = Color32.Lerp(bgColor, wmColor, wmColor.a / 255f);

                        pixels[pixel] = finalColor.r;
                        pixels[pixel + 1] = finalColor.g;
                        pixels[pixel + 2] = finalColor.b;
                        pixels[pixel + 3] = finalColor.a;
                        toSavePixels[pixel] = finalColor.r;
                        toSavePixels[pixel + 1] = finalColor.g;
                        toSavePixels[pixel + 2] = finalColor.b;
                        toSavePixels[pixel + 3] = finalColor.a;
                    }
                }
            }

            //StartCoroutine(TimeToApplyStickers());
            drawingTexture.LoadRawTextureData(pixels);
            drawingTexture.Apply();
        }

        IEnumerator TimeToApplyStickers()
        {
            yield return new WaitForSeconds(1);
            sticker = true;
            IsBelowThreshold(0, 0, 0);
            //TakeScreenShot();
            //isComplete = true;
        }

        public IEnumerator FloodFillFourWay(int aX, int aY, Color32 replacementColor, Queue<Point> nodes,
            int loopCount = 0)
        {
            while (nodes.Count > 0)
            {
                Point a = nodes.Dequeue();
                if (a.x < drawingTexture.width && a.x > 0 &&
                    a.y < drawingTexture.height && a.y > 0) //make sure we stay within bounds
                {
                    // Color C = colors[i + current.y * drawingTexture.width];
                    int pixel = (drawingTexture.width * a.y + a.x) << 2;
                    if (pixels[pixel + 3] >= 100)
                    {
                        pixels[pixel] = replacementColor.r;
                        pixels[pixel + 1] = replacementColor.g;
                        pixels[pixel + 2] = replacementColor.b;
                        pixels[pixel + 3] = replacementColor.a;
                    }

                    // drawingTexture.SetPixel(aX, aY, replacementColor);
                    nodes.Enqueue(new Point(a.x - 1, a.y));
                    nodes.Enqueue(new Point(a.x + 1, a.y));
                    nodes.Enqueue(new Point(a.x, a.y - 1));
                    nodes.Enqueue(new Point(a.x, a.y + 1));
                }
            }

            return null;
        }

        public IEnumerator FloodFillArea(int aX, int aY, Color32 aFillColor, int w, int h, Color[] colors,
            Queue<Point> nodes, int loopCOunt = 0)
        {
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color32 C = colors[i + current.y * w];
                    if (C.a < 80 || C.Equals(aFillColor))
                        break;
                    colors[i + current.y * w] = aFillColor;
                    pixels[(i + current.y * w) * 4] = aFillColor.r;
                    pixels[(i + current.y * w) * 4 + 1] = aFillColor.g;
                    pixels[(i + current.y * w) * 4 + 2] = aFillColor.b;
                    pixels[(i + current.y * w) * 4 + 3] = aFillColor.a;
                    toSavePixels[(i + current.y * w) * 4] = aFillColor.r;
                    toSavePixels[(i + current.y * w) * 4 + 1] = aFillColor.g;
                    toSavePixels[(i + current.y * w) * 4 + 2] = aFillColor.b;
                    toSavePixels[(i + current.y * w) * 4 + 3] = aFillColor.a;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C.a >= 80 && !C.Equals(aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }

                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C.a >= 80 && !C.Equals(aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }

                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color32 C = colors[i + current.y * w];
                    if (C.a < 80 || C.Equals(aFillColor))
                        break;
                    colors[i + current.y * w] = aFillColor;
                    pixels[(i + current.y * w) * 4] = aFillColor.r;
                    pixels[(i + current.y * w) * 4 + 1] = aFillColor.g;
                    pixels[(i + current.y * w) * 4 + 2] = aFillColor.b;
                    pixels[(i + current.y * w) * 4 + 3] = aFillColor.a;
                    toSavePixels[(i + current.y * w) * 4] = aFillColor.r;
                    toSavePixels[(i + current.y * w) * 4 + 1] = aFillColor.g;
                    toSavePixels[(i + current.y * w) * 4 + 2] = aFillColor.b;
                    toSavePixels[(i + current.y * w) * 4 + 3] = aFillColor.a;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C.a >= 80 && !C.Equals(aFillColor))
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }

                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C.a >= 80 && !C.Equals(aFillColor))
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }

                loopCOunt++;
                if (loopCOunt > 10000)
                {
                    loopCOunt = 0;
                    drawingTexture.SetPixels(colors);
                    drawingTexture.Apply();
                    yield return new WaitForSeconds(0.00005f);
                }
            }

            drawingTexture.SetPixels(colors);
            drawingTexture.Apply();
            UpdateTexture();
            BlackPixelsText.text = "Black Area  100%";
            totalPixelsText.text = "Total Area  100%";
            StartCoroutine(TakeScreenShot());
            LoadNextImage = true;
        }

        public void MyFill(Color32[] colors, int x, int y, int w, int h)
        {
            _MyFill(colors, x, y, w, h);
        }

        void _MyFill(Color32[] colors, int x, int y, int width, int height)
        {
            //at this point, we know array[y, x] is clear, and we want to move as far as possible to the upper - left.moving
            // up is much more important than moving left, so we could try to make this smarter by sometimes moving to
            // the right if doing so would allow us to move further up, but it doesn't seem worth the complexity
            while (true)
            {
                int ox = x, oy = y;
                while (y != 0 && !colors[x + (y - 1) * width].Equals(paintColor)) y--;
                while (x != 0 && !colors[(x - 1) + y * width].Equals(paintColor)) x--;
                if (x == ox && y == oy) break;
            }

            StartCoroutine(MyFillCore(colors, x, y, width, height));
        }

        IEnumerator MyFillCore(Color32[] colors, int x, int y, int width, int height, int loopCount = 0)
        {
            // at this point, we know that array[y,x] is clear, and array[y-1,x] and array[y,x-1] are set.
            // we'll begin scanning down and to the right, attempting to fill an entire rectangular block
            int lastRowLength = 0; // the number of cells that were clear in the last row we scanned
            do
            {
                int rowLength = 0,
                    sx = x; // keep track of how long this row is. sx is the starting x for the main scan below
                // now we want to handle a case like |***|, where we fill 3 cells in the first row and then after we move to
                // the second row we find the first  | **| cell is filled, ending our rectangular scan. rather than handling
                // this via the recursion below, we'll increase the starting value of 'x' and reduce the last row length to
                // match. then we'll continue trying to set the narrower rectangular block
                if (lastRowLength != 0 &&
                    colors[x + y * width]
                        .Equals(paintColor)) // if this is not the first row and the leftmost cell is filled...
                {
                    do
                    {
                        if (--lastRowLength == 0) yield return null; // shorten the row. if it's full, we're done
                    } while
                        (colors[++x + y * width]
                         .Equals(paintColor)); // otherwise, update the starting point of the main scan to match

                    sx = x;
                }
                // we also want to handle the opposite case, | **|, where we begin scanning a 2-wide rectangular block and
                // then find on the next row that it has     |***| gotten wider on the left. again, we could handle this
                // with recursion but we'd prefer to adjust x and lastRowLength instead
                else
                {
                    for (; x != 0 && !colors[(x - 1) + y * width].Equals(paintColor); rowLength++, lastRowLength++)
                    {
                        if (colors[--x + y * width].a > 100)
                            colors[--x + y * width] =
                                paintColor; // to avoid scanning the cells twice, we'll fill them and update rowLength here
                        // if there's something above the new starting point, handle that recursively. this deals with cases
                        // like |* **| when we begin filling from (2,0), move down to (2,1), and then move left to (0,1).
                        // the  |****| main scan assumes the portion of the previous row from x to x+lastRowLength has already
                        // been filled. adjusting x and lastRowLength breaks that assumption in this case, so we must fix it
                        if (y != 0 && !colors[x + (y - 1) * width].Equals(paintColor))
                            _MyFill(colors, x, y - 1, width, height); // use _Fill since there may be more up and left
                    }
                }

                // now at this point we can begin to scan the current row in the rectangular block. the span of the previous
                // row from x (inclusive) to x+lastRowLength (exclusive) has already been filled, so we don't need to
                // check it. so scan across to the right in the current row
                for (; sx < width && !colors[sx + y * width].Equals(paintColor); rowLength++, sx++)
                    if (colors[sx + y * width].a > 100)
                        colors[sx + y * width] = paintColor;
                // now we've scanned this row. if the block is rectangular, then the previous row has already been scanned,
                // so we don't need to look upwards and we're going to scan the next row in the next iteration so we don't
                // need to look downwards. however, if the block is not rectangular, we may need to look upwards or rightwards
                // for some portion of the row. if this row was shorter than the last row, we may need to look rightwards near
                // the end, as in the case of |*****|, where the first row is 5 cells long and the second row is 3 cells long.
                // we must look to the right  |*** *| of the single cell at the end of the second row, i.e. at (4,1)
                if (rowLength < lastRowLength)
                {
                    for (int end = x + lastRowLength;
                         ++sx < end;) // 'end' is the end of the previous row, so scan the current row to
                    {
                        // there. any clear cells would have been connected to the previous
                        if (!colors[sx + y * width].Equals(paintColor))
                            MyFillCore(colors, sx, y, width,
                                height); // row. the cells up and left must be set so use FillCore
                    }
                }
                // alternately, if this row is longer than the previous row, as in the case |*** *| then we must look above
                // the end of the row, i.e at (4,0)                                         |*****|
                else if
                    (rowLength > lastRowLength && y != 0) // if this row is longer and we're not already at the top...
                {
                    for (int ux = x + lastRowLength; ++ux < sx;) // sx is the end of the current row
                    {
                        if (!colors[ux + (y - 1) * width].Equals(paintColor))
                            _MyFill(colors, ux, y - 1, width,
                                height); // since there may be clear cells up and left, use _Fill
                    }
                }

                lastRowLength = rowLength; // record the new row length
                loopCount++;
                if (loopCount > 100)
                {
                    loopCount = 0;
                    drawingTexture.SetPixels32(colors);
                    drawingTexture.Apply();
                    yield return new WaitForSeconds(0.01f);
                }
            } while (lastRowLength != 0 && ++y < height); // if we get to a full row or to the bottom, we're done

            drawingTexture.SetPixels32(colors);
            drawingTexture.Apply();
        }
    }

    class TextureExtension
    {
        public IEnumerator FloodFillArea(Texture2D aTex, int aX, int aY, Color aFillColor, int w, int h, Color[] colors,
            Queue<Point> nodes, int loopCOunt = 0)
        {
            while (nodes.Count > 0)
            {
                Point current = nodes.Dequeue();
                for (int i = current.x; i < w; i++)
                {
                    Color C = colors[i + current.y * w];
                    if (C.a < 1 || C == aFillColor)
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C.a > 0 && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }

                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C.a > 0 && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }

                for (int i = current.x - 1; i >= 0; i--)
                {
                    Color C = colors[i + current.y * w];
                    if (C.a < 1 || C == aFillColor)
                        break;
                    colors[i + current.y * w] = aFillColor;
                    if (current.y + 1 < h)
                    {
                        C = colors[i + current.y * w + w];
                        if (C.a > 0 && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y + 1));
                    }

                    if (current.y - 1 >= 0)
                    {
                        C = colors[i + current.y * w - w];
                        if (C.a > 0 && C != aFillColor)
                            nodes.Enqueue(new Point(i, current.y - 1));
                    }
                }

                loopCOunt++;
                if (loopCOunt > 10000)
                {
                    loopCOunt = 0;
                    aTex.SetPixels(colors);
                    aTex.Apply();
                    yield return new WaitForSeconds(0.01f);
                }
            }

            aTex.SetPixels(colors);
            aTex.Apply();
        }
    }
}