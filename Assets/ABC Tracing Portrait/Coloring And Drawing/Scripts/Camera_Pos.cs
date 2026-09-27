using KGF.Coloring;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEditor;
using System;
using System.Collections;

[System.Serializable]
public class CameraPosScale
{
    public Vector3 pos = new Vector3(0, 0, -10);
    public float camSize = 6.0f;
}

public class Camera_Pos : MonoBehaviour
{
    public int maxShapeIndex;
    public int layer;
    public Transform[] Camera_Position;
    [HideInInspector]
    public Sprite[] Character_Sprite;
    [HideInInspector]
    public SpriteRenderer[] objects;
    [Header("--------------------White Sprite--------------------")]
    [HideInInspector]
    public Sprite[] WhiteSprite;
    [HideInInspector]
    public SpriteRenderer[] Inner_part;
    public Transform[] WhiteImagePosition;
    public float[] ZPosition;
    public static bool EligibleForBanner = true;
    public CameraPosScale[] PositionAndScale,PositionAndScaleTab;
    public int[] pencilS, brushS,InnerPencil ,InnerBrush;

    public Vector3 CameraFinalPosition;
    public float CamSize = 6f;
    public Vector3 CameraFinalPositionTab;
    public float CamSizeTab = 6f;
   // public float TabOffSet = 1.75f;
    public bool digitsMoreThanOne = false;

    public AudioClip startClip;
    public AudioClip CompleteClip;

    public bool hideFirstSprite;

    public bool OverWriteTab = false;
    public string DrawingPath;
    public string PrefabObjPath;
    public void PlayIntro()
    {
        if (startClip)
        {
            StartCoroutine(playIntro());
        }
    }

    IEnumerator playIntro()
    {
        print("sounndsdss");
        yield return new WaitForSeconds(0.2f);
        // SoundManager.instance.InPaintSource.clip = startClip;
        // SoundManager.instance.InPaintSource.Play();
    }

    private void Awake()
    {
        foreach (var obj in WhiteImagePosition)
        {
            obj.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Painted");
        }

        foreach (var obj in Camera_Position)
        {
            obj.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Painted");
        }
        if(PlayerPrefs.GetInt("RemoveAds") == 1)
        {
            for (int i = 0; i < PositionAndScale.Length; i++)
            {
               //// PositionAndScale[i].pos = PositionAndScale[i].pos - new Vector3(0, 0.62f, 0);
            }
            /*CameraFinalPosition = CameraFinalPosition - new Vector3(0, 1.33f, 0);
            CameraFinalPositionTab = CameraFinalPositionTab - new Vector3(0, 1.5f, 0);*/
        }


        //assigning supportive parts for outline
        Character_Sprite = new Sprite[Camera_Position.Length];
        objects = new SpriteRenderer[Camera_Position.Length];
        for (int i = 0; i < Camera_Position.Length; i++)
        {
            Character_Sprite[i] = Camera_Position[i].GetComponent<SpriteRenderer>().sprite;
            objects[i] = Camera_Position[i].GetComponent<SpriteRenderer>();
        }
        //assigning supportive parts for inner parts
        WhiteSprite = new Sprite[WhiteImagePosition.Length];
        Inner_part = new SpriteRenderer[WhiteImagePosition.Length];
        for (int i = 0; i < WhiteImagePosition.Length; i++)
        {
            WhiteSprite[i] = WhiteImagePosition[i].GetComponent<SpriteRenderer>().sprite;
            Inner_part[i] = WhiteImagePosition[i].GetComponent<SpriteRenderer>();
        }

        

        



    }
    public Vector3 GetCurrentPosition(int index)
    {
        return Camera_Position[index].position;
    }
    public Vector3 WhiteImagePos(int index)
    {
        return WhiteImagePosition[index].position;
    }
    public void Apply(int Limit, bool IsDrawingScene)
    {
        Color32 whiteTransparentColor = new Color32(255, 255, 255, 0);

        for (int i = 0; i < ServiceManager.instance.selectedCharacter.images.Length; i++)
        {
            var localPath = Path.Combine(Application.persistentDataPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
            localPath = Path.Combine(localPath, Character_Sprite[i].name + ".dat");
            if (i >= Limit)
            {
                Texture2D originaltexture = ServiceManager.instance.selectedCharacter.images.FirstOrDefault(x => x.name.Contains(Character_Sprite[i].name));
                if (originaltexture != null)
                {

                    Texture2D texturea = new Texture2D(originaltexture.width, originaltexture.height, TextureFormat.RGBA32, false);
                    texturea.SetPixels32(originaltexture.GetPixels32());
                    texturea.Apply();
                    Color32[] tempPixels = texturea.GetPixels32();
                    int tempCount = tempPixels.Length;
                    for (int j = 0; j < tempCount; j++)
                    {
                        tempPixels[j] = whiteTransparentColor;
                    }
                    texturea.SetPixels32(tempPixels);
                    texturea.Apply();
                    objects[i].sprite = Sprite.Create(texturea, new Rect(0, 0, texturea.width, texturea.height), new Vector2(0.5f, 0.5f));

                    Debug.Log("Orignal Texture is Null");
                }
                else
                {
                    Debug.Log("Orignal Texture is Not Null");
                }

            }
        }
        Resources.UnloadUnusedAssets();
    }

    public void Apply(int index)
    {
        Color32 whiteTransparentColor = new Color32(255, 255, 255, 0);

        var localPath = Path.Combine(Application.persistentDataPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
        localPath = Path.Combine(localPath, Inner_part[index].name + ".dat");
        if (File.Exists(localPath)/* && index > 0 && index < Character_Sprite.Length*/)
        {
            //if (File.Exists(localPath))
            Texture2D originaltexture = ServiceManager.instance.selectedCharacter.White_Image.FirstOrDefault(x => x.name.Contains(Inner_part[index].name));
            if (originaltexture != null)
            {

                Texture2D texturea = new Texture2D(originaltexture.width, originaltexture.height, TextureFormat.RGBA32, false);
                texturea.SetPixels32(originaltexture.GetPixels32());
                texturea.Apply();
                Color32[] tempPixels = texturea.GetPixels32();
                int tempCount = tempPixels.Length;
                for (int j = 0; j < tempCount; j++)
                {
                    tempPixels[j] = whiteTransparentColor;
                }
                texturea.SetPixels32(tempPixels);
                texturea.Apply();

                Inner_part[index].sprite = Sprite.Create(texturea, new Rect(0, 0, texturea.width, texturea.height), new Vector2(0.5f, 0.5f));
            }
            Resources.UnloadUnusedAssets();
        }
    }
    public void SaveForScreenSHot(int i)
    {
        Transform[] a = objects[i].gameObject.GetComponentsInChildren<Transform>(true);

        foreach (var item in a)
        {
            item.gameObject.layer = LayerMask.NameToLayer("Painted");
        }
    }
    public void Apply1(int index)
    {

        var localPath = Path.Combine(Application.persistentDataPath, ServiceManager.instance.selectedCharacter.CharacterLocalPath);
        localPath = Path.Combine(localPath, Character_Sprite[index - 1].name + ".dat");
        if (File.Exists(localPath)/* && index > 0 && index <= Character_Sprite.Length*/)
        {

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(localPath, FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
            file.Close();

            Texture2D texture = new Texture2D(10, 10);
            texture.LoadImage(data.texture);

            objects[index - 1].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 1, SpriteMeshType.FullRect);
            objects[index - 1].sortingLayerName = "Painted";
            if (objects[index - 1].transform.childCount > 0 && index - 1 > 0)
            {
                objects[index - 1].transform.GetChild(0).transform.GetComponent<SpriteRenderer>().sortingLayerName = "Painted";
                objects[index - 1].transform.GetChild(0).transform.GetComponent<SpriteRenderer>().sortingOrder = layer;
            }
            if (objects[index - 1].transform.childCount == 2)
            {
                objects[index - 1].transform.GetChild(1).gameObject.SetActive(true);
                //SaveForScreenSHot(index - 1);
            }
            else if (objects[index - 1].transform.childCount > 0)
            {
                objects[index - 1].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
}


