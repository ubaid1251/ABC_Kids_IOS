using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using KGF.Coloring;
public class MergeImagesInOne : MonoBehaviour
{
    private Camera _camera;
    GameObject targetObject; // The GameObject you want to focus on
    public float zoomFactor = 1.0f;

    private void Start()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Assets/Outputs"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Assets/Outputs");
        }
        _camera = GetComponent<Camera>();
        targetObject = ServiceManager.instance.selectedCharacter.gameObject;
        if (targetObject != null)
        {
            // Calculate the bounds of the target GameObject
            Bounds bounds = CalculateBounds(targetObject);

            // Calculate the center point of the bounds
            Vector3 center = bounds.center;

            // Move the camera to the center point
            transform.position = new Vector3(center.x, center.y, Camera.main.transform.position.z);
            float targetOrthographicSize = Mathf.Max(bounds.size.x, bounds.size.y) * 0.5f * zoomFactor;

            // Set the camera's orthographic size
            _camera.orthographicSize = targetOrthographicSize;
        }
        else
        {
            Debug.LogWarning("Target GameObject not assigned.");
        }
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found in the target GameObject and its children.");
            return new Bounds();
        }

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }
    //private void OnApplicationQuit()
    //{
    //    SetPaintedAndCapture();
    //}

    public void Capture()
    {

        RenderTexture activeRenderTexture = RenderTexture.active;
        Debug.Log(_camera);
        RenderTexture.active = _camera.targetTexture;

        _camera.Render();

        Texture2D image = new Texture2D(_camera.targetTexture.width, _camera.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        Destroy(image);

        Debug.Log(bytes);
        if (Directory.Exists(Application.persistentDataPath + "/" + ServiceManager.instance.selectedCharacter.CharacterLocalPath))
        {
            File.WriteAllBytes(Application.persistentDataPath + "/Assets/Outputs/" + ServiceManager.instance.selectedCharacter.PrefName + ".png", bytes);
            PlayerPrefs.SetInt(ServiceManager.instance.selectedCharacter.PrefName, 1);
        }
    }

    public void SetPaintedAndCapture()
    {
        if (Handler.MyHandler.Inner.Count > 0)
        {
            if (!Handler.MyHandler.Inner[0].IsOutline)
            {
                foreach (var img in Handler.MyHandler.Inner)
                {
                    img.gameObject.layer = LayerMask.NameToLayer("Painted");
                }
            }
        }
        Capture();
    }

    public void SetPaintedImagesToDefault()
    {
        if (Handler.MyHandler.Inner.Count > 0)
        {
            if (!Handler.MyHandler.Inner[0].IsOutline)
            {
                foreach (var img in Handler.MyHandler.Inner)
                {
                    img.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }

}
