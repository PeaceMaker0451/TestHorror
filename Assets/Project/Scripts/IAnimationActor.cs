using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAnimationActor
{
    public bool IsAnimationCurrentlyScripted { get; }
    public bool IsPlayingAnimation { get; }

    public void SetAnimationControl(bool enabled);
    public void Teleport(Transform position);
    public UniTask MoveToAsync(Transform position, float duration);
    public void PlayAnimation(AnimationClip clip);
    public UniTask PlayAnimationAsync(AnimationClip clip);
    public void StopAnimation();
}

