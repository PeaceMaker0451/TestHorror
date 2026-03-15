using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizeRenderFeature : FullScreenPassRendererFeature
{
    [SerializeField] private string pixelSizePropertyName = "_PixelSize";
    //private int pixelSizeId;
    //private MaterialPropertyBlock mpb;

    public override void Create()
    {
        base.Create();
        //pixelSizeId = Shader.PropertyToID(pixelSizePropertyName);
       // mpb = new MaterialPropertyBlock();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Читаем значение Volume
        var volume = VolumeManager.instance.stack.GetComponent<PixelizeEffect>();
        float pixelSize = (volume != null && volume.IsActive()) ? volume.PixelSize.value : 10f;

        // Подставляем в MPB
        //mpb.SetFloat(pixelSizeId, pixelSize);

        passMaterial.SetFloat(pixelSizePropertyName, pixelSize);

        base.AddRenderPasses(renderer, ref renderingData);
    }
}