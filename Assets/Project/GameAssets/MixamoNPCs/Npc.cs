using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using UnityEngine.Playables;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Npc : MonoBehaviour, IAnimationActor, ITextActor
{
    const string SpeedParameterName = "Speed";
    const string ArmedParameterName = "Armed";
    const string DieTriggerName = "Die";
    const string SitTriggerName = "Sit";
    const string StandTriggerName = "Stand";
    const string IdleTriggerName = "Idle";

    [SerializeField] private Character _character;

    [SerializeField] private float _followTriggerDistance;

    private Animator _animator;
    private NavMeshAgent _navAgent;
    private Transform _followTarget;
    private PlayableGraph _graph;
    private AnimationClipPlayable _clipPlayable;
    private bool movingAnimationPlaing = false;

    private Coroutine _followCoroutine;
    private Coroutine _reachCoroutine;

    public float FollowStopDistance { get; private set; } = 0.2f;
    public float Speed => _navAgent.speed;
    public float AngularSpeed => _navAgent.angularSpeed;
    public bool IsAnimationCurrentlyScripted { get; private set; } = false;
    public bool IsPlayingAnimation { get; private set; } = false;
    public bool IsMoving { get; private set; } = false;
    

    public event Action TargetReached;
    public event Action FollowTargetReached;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _navAgent = GetComponent<NavMeshAgent>();

        _navAgent.updatePosition = false;
        _navAgent.updateRotation = true;
    }

    private void Update()
    {
        if (IsAnimationCurrentlyScripted == false)
        {
            if (_navAgent.hasPath)
            {
                _animator.SetFloat("Speed", _navAgent.velocity.magnitude);
            }
            else
            {
                _animator.SetFloat("Speed", 0f);
            }
        }
    }

    private void OnAnimatorMove()
    {
        var pos = _animator.rootPosition;
        pos.y = _navAgent.nextPosition.y;
        transform.position = pos;

        if (_navAgent.hasPath && _navAgent.desiredVelocity.magnitude > 0)
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(_navAgent.desiredVelocity),
                Time.deltaTime * 10f
            );

        _navAgent.nextPosition = transform.position;
    }

    public void SetArmed(bool isArmed)
    {
        _animator.SetBool(ArmedParameterName, isArmed);
    }

    public void Sit()
    {
        SetNavMeshAgentActive(false);
        _animator.SetTrigger(SitTriggerName);
    }

    public void Stand()
    {
        SetNavMeshAgentActive(true);
        _animator.SetTrigger(StandTriggerName);
    }

    public void Die()
    {
        SetNavMeshAgentActive(false);
        _animator.SetTrigger(DieTriggerName);
    }

    public void SetNavMeshAgentActive(bool active)
    {
        _navAgent.enabled = active;
    }

    public void SetFollowStopDistance(float distance)
    {
        FollowStopDistance = distance;
    }

    public void SetSpeed(float speed)
    {
        _navAgent.speed = speed;
    }

    public void SetAngularSpeed(float speed)
    {
        _navAgent.angularSpeed = speed;
    }

    public void Reach(Vector3 position, float stopDistance = 0.2f)
    {
        StopFollowing();
        StopReaching();

        if (IsAnimationCurrentlyScripted) return;

        _reachCoroutine = StartCoroutine(ReachAsync(position, stopDistance));
    }

    public void StopReaching()
    {
        if (_reachCoroutine == null)
            return;

        StopCoroutine(_reachCoroutine);
        _reachCoroutine = null;
        IsMoving = false;

        if (_navAgent.enabled)
            _navAgent.ResetPath();
    }

    public void Follow(Transform target)
    {
        StopFollowing();
        StopReaching();

        if (target == null)
            return;

        _followTarget = target;

        _followCoroutine = StartCoroutine(FollowAsync());
    }

    public void StopFollowing()
    {
        if (_followCoroutine == null)
            return;

        StopCoroutine(_followCoroutine);
        _followTarget = null;
        _followCoroutine = null;
        IsMoving = false;

        if (_navAgent.enabled)
            _navAgent.ResetPath();
    }

    public void SetAnimationControl(bool enabled)
    {
        IsAnimationCurrentlyScripted = enabled;

        _navAgent.enabled = !enabled;
        //_animator.enabled = !enabled;
    }

    public void Teleport(Transform target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        transform.SetPositionAndRotation(target.position, target.rotation);
    }

    public async UniTask MoveToAsync(Transform target, float duration)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        EnsureScriptControl(true);

        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        var moveTween = transform.DOMove(endPos, duration).SetEase(Ease.Linear);
        var rotTween = transform.DORotateQuaternion(endRot, duration).SetEase(Ease.Linear);

        await UniTask.WhenAll(
            moveTween.AsyncWaitForCompletion().AsUniTask(),
            rotTween.AsyncWaitForCompletion().AsUniTask()
        );

        transform.SetPositionAndRotation(endPos, endRot);
    }

    public void PlayAnimation(AnimationClip clip)
    {
        EnsureScriptControl(true);

        StopAnimation();

        _graph = PlayableGraph.Create($"NpcCutsceneGraph_{name}");
        var output = AnimationPlayableOutput.Create(_graph, "Animation", _animator);
        _clipPlayable = AnimationClipPlayable.Create(_graph, clip);

        output.SetSourcePlayable(_clipPlayable);
        _graph.Play();

        IsPlayingAnimation = true;
        IsAnimationCurrentlyScripted = true;
    }

    public async UniTask PlayAnimationAsync(AnimationClip clip)
    {
        PlayAnimation(clip);

        float time = 0f;
        while (time < clip.length && IsPlayingAnimation)
        {
            time += Time.deltaTime;
            await UniTask.NextFrame();
        }

        StopAnimation();
    }

    public void StopAnimation()
    {
        if (IsPlayingAnimation == false)
            return;

        _graph.Stop();
        _graph.Destroy();
        IsPlayingAnimation = false;
        IsAnimationCurrentlyScripted = false;
    }

    public TextBoxLine PersonalizeLine(string text)
    {
        return _character.PersonalizeLine(text);
    }

    public List<TextBoxLine> PersonalizeLines(IEnumerable<string> text)
    {
        return _character.PersonalizeLines(text);
    }

    private IEnumerator FollowAsync()
    {
        EnsureScriptControl(false);

        var wait = new WaitForSeconds(0.1f);

        IsMoving = true;

        bool isCloseEnough = false;

        while (_followTarget != null)
        {
            if (_navAgent.enabled)
            {
                float distance = Vector3.Distance(transform.position, _followTarget.position);

                if (distance <= _followTriggerDistance)
                {
                    if (!isCloseEnough)
                    {
                        isCloseEnough = true;
                        FollowTargetReached?.Invoke();
                        Debug.Log("ÄÎÃÍÀË!!!!");
                    }
                }
                else
                {
                    isCloseEnough = false;
                }

                _navAgent.SetDestination(_followTarget.position);
            }

            yield return wait;
        }

        StopFollowing();
    }

    private IEnumerator ReachAsync(Vector3 target, float stopDistance)
    {
        EnsureScriptControl(false);

        float reachOffset = 0.2f;

        IsMoving = true;
        _navAgent.SetDestination(target);

        while (Vector3.Distance(transform.position, target) > (stopDistance + reachOffset) && IsMoving == true)
        {
            yield return null;
        }

        StopReaching();
        TargetReached?.Invoke();
    }

    private void EnsureScriptControl(bool active)
    {
        if (IsAnimationCurrentlyScripted != active)
            throw new InvalidOperationException($"{name} is not in script-controlled animation mode. Call SetAnimationControl(true) first.");
    }
}