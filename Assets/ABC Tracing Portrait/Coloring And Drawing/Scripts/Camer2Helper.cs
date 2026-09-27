using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using KGF.Coloring; // assuming you still need this
using static NativeGallery;

public class Camer2Helper : MonoBehaviour
{
    //public CaptureAndSave capture;
    public GameObject PermissionPanel;
    public Camera _camera;
    public GameObject BorderImage;
    public RefreshGalleryWrapper galleryWrapper;
    public Image BGImage, BorderParent;
    public GameObject AnimationPanel;
    public RectTransform homeBar;

    void Start()
    {
        //if (ResCheck.ResolutionType == ResType.tab)
            //homeBar.DOAnchorPosY(424, 0);
    }

    public void SaveToGallery()
    {
        int apiLevel = GetAndroidAPILevel();

        // Only Android 9 and below need WRITE_EXTERNAL_STORAGE for saving
        if (apiLevel > -1 && apiLevel <= 28)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!AndroidRuntimePermissions.CheckPermission("android.permission.WRITE_EXTERNAL_STORAGE"))
            {
                var result = AndroidRuntimePermissions.RequestPermission("android.permission.WRITE_EXTERNAL_STORAGE");
                if (result != AndroidRuntimePermissions.Permission.Granted)
                {
                    ToastMessage.Instance.ShowToastMessage("Storage permission is required to save images on this device.");
                    return;
                }
            }
#endif
        }

        // UI pre-save animation
        BGImage.gameObject.SetActive(true);
        AnimationPanel.SetActive(false);
        BorderParent.gameObject.SetActive(true);

        BGImage.DOFade(1, 0.25f).OnComplete(() =>
        {
            SoundHandler.instance.PlaySource(SoundHandler.instance.camera);
            //capture.FILENAME_PREFIX = ServiceManager.instance.selectedCharacter.PrefName;

            BGImage.DOFade(0, 0.25f).OnComplete(() =>
            {
                BGImage.gameObject.SetActive(false);
                AnimationPanel.SetActive(true);

                // Take screenshot
                //Texture2D screenshot = capture.GetScreenShot(Screen.width, Screen.height, _camera, ImageType.PNG);
                //string fileName = ServiceManager.instance.selectedCharacter.PrefName + ".png";

                //// Let NativeGallery handle MediaStore insert / scoped storage
                //var permission =SaveImageToGallery(screenshot, "DrawingBook", fileName);

                //if (permission == Permission.Granted)
                //{
                //    ToastMessage.Instance.ShowToastMessage("Drawing saved to Gallery.");
                //}
                //else if (permission == Permission.Denied)
                //{
                //    // On API >=29, denial here is rarely about WRITE permission; user may have blocked gallery access flows.
                //    ToastMessage.Instance.ShowToastMessage("Couldn’t save to Gallery.");
                //}
                //else
                //{
                //    ToastMessage.Instance.ShowToastMessage("Save canceled.");
                //}

                //if (screenshot != null)
                //    Destroy(screenshot);
            });
        });
    }

    private int GetAndroidAPILevel()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            return version.GetStatic<int>("SDK_INT");
#else
        return -1;
#endif
    }

    public void PressOK()
    {
        PermissionPanel.SetActive(false);
        SoundHandler.instance.PlayTap();

        // Don’t request legacy permissions on modern Android here.
        // Just proceed; SaveToGallery will handle API <=28 permission ask.
#if !UNITY_EDITOR && UNITY_ANDROID
        PlayerPrefs.SetInt("PermissionToGallery", 1);
        SaveToGallery();
#else
        // If you use any editor/test path, keep wrapper as needed
        //galleryWrapper.StoragePermissionRequest();
        PlayerPrefs.SetInt("PermissionToGallery", 1);
        SaveToGallery();
#endif
    }
}
