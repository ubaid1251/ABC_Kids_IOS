using UnityEngine;

public class TextureSwitcher : MonoBehaviour
{
    public Material targetMaterial;
    public Texture[] textures; // Array of textures to switch between
    public float switchInterval = 1f; // Time interval between texture switches

    private bool isSwitching = false;
    private int currentTextureIndex = 0;

    void Start()
    {
        if (targetMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                targetMaterial = renderer.material;
            }
            else
            {
                Debug.LogError("No target material assigned and no Renderer component found.");
                return;
            }
        }

        if (textures.Length == 0)
        {
            Debug.LogError("No textures assigned.");
            return;
        }

       Invoke(nameof(SwitchTexture),switchInterval);
    }

    void SwitchTexture()
    {
        if (!isSwitching)
        {
            isSwitching = true;

            // Switch the texture
            targetMaterial.mainTexture = textures[currentTextureIndex];

            // Increment or reset the texture index
            currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
            isSwitching = false;
            Invoke(nameof(SwitchTexture), switchInterval);
        }
    }
}
