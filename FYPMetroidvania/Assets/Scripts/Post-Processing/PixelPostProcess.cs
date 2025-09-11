using NUnit.Framework.Internal;
using UnityEngine;

[ExecuteAlways]
public class PixelPostProcess : MonoBehaviour
{
    public Material pixelMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (pixelMaterial != null)
            Graphics.Blit(source, destination, pixelMaterial);
        else
            Graphics.Blit(source, destination);
    }
}
