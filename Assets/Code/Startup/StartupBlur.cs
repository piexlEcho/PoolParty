using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartupBlur : MonoBehaviour
{
    public Material blurMaterial;
    public float blurSize = 3f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        blurMaterial.SetFloat("_BlurSize", blurSize);
        Graphics.Blit(src, dest, blurMaterial);
    }
}
