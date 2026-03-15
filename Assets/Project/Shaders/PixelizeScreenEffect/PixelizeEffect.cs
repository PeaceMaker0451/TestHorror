using UnityEngine;
using UnityEngine.Rendering;

public class PixelizeEffect : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter PixelSize = new ClampedFloatParameter(1f, 0f, 10f);

    public bool IsActive() => PixelSize.value > 0f;
}
