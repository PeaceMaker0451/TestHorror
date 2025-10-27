using Cysharp.Threading.Tasks;
using UnityEngine;

public class Maniac : Npc
{
    [SerializeField] private Transform _otherActorAttackPosition;

    [SerializeField] private AnimationClip _firstAttack;
    [SerializeField] private AnimationClip _secondAttack;
    [SerializeField] private AnimationClip _firstDefend;
    [SerializeField] private AnimationClip _secondDefend;

    public bool IsAlreadyAttacked { get; private set; }

    public async UniTask Attack(IAnimationActor _actor)
    {
        SetAnimationControl(true);
        _actor.SetAnimationControl(true);

        _ = _actor.MoveToAsync(_otherActorAttackPosition, 0.4f);
        
        if(IsAlreadyAttacked == false)
        {
            await UniTask.WhenAll(
                PlayAnimationAsync(_firstAttack),
                _actor.PlayAnimationAsync(_firstDefend));
            
        }
        else
        {
            await UniTask.WhenAll(
                PlayAnimationAsync(_secondAttack),
                _actor.PlayAnimationAsync(_secondDefend));
        }

        _actor.SetAnimationControl(false);

        await UniTask.WaitForSeconds(3);
        SetAnimationControl(false);

        IsAlreadyAttacked = true;
    }
}
