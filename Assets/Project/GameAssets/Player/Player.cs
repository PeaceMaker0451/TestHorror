using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(InteractionCatcher))]
public class Player : MonoBehaviour, IAnimationActor, ITextActor, IControllable
{
    public static Player Instance { get; private set; }

    [Header("Компоненты")]
    [SerializeField] private Camera _camera;
    [SerializeField] private PlayerUIManager _ui;
    [SerializeField] private Transform _handledItemRoot;

    [Header("Настройки движения")]
    [SerializeField] private float _walkSpeed = 2.5f;
    [SerializeField] private float _runSpeed = 4.5f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _cameraRoot;

    [Header("Анимации")]
    //[SerializeField] private string _moveXParameterName = "MoveX";
    //[SerializeField] private string _moveYParameterName = "MoveY";
    [SerializeField] private string _speedParameterName = "Speed";
    [SerializeField] private string _damagedParameterName = "Speed";
    [SerializeField] private float _yRigOffset = 0.86f;

    [Header("Обзор")]
    [SerializeField] private float _lookSensitivity = 2f;
    [SerializeField] private float _maxPitch = 80f;
    [SerializeField] private float _minPitch = -80f;

    [Header("Персонаж")]
    [SerializeField] private Character _character;

    private InteractionCatcher _interactionCatcher;
    private Animator _animator;
    private CharacterController _controller;
    private BaseInput _input;
    private Interaction lastFrameInteraction;

    private float _walkSpeedModifier = 1;
    private float _runSpeedModifier = 1;
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _pitch = 0f;

    private PlayableGraph _graph;
    private AnimationClipPlayable _clipPlayable;

    public IHandableItem HandledItem { get; private set; }
    public PlayerUIManager UI => _ui;
    public bool IsAnimationCurrentlyScripted { get; private set; } = false;
    public bool IsPlayingAnimation { get; private set; } = false;
    public float TotalWalkSpeed => _walkSpeed * _walkSpeedModifier;
    public float TotalRunSpeed => _runSpeed * _runSpeedModifier;
    public float WalkSpeedModifier => _walkSpeedModifier;
    public float RunSpeedModifier => _runSpeedModifier;
    public bool IsHandleItem => HandledItem != null;
    public bool CanMove { get; private set; } = true;
    public bool CanLook { get; private set; } = true;
    public bool CanInteract { get; private set; } = true;

    public Transform CameraTransform => _camera.transform;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _interactionCatcher = GetComponent<InteractionCatcher>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        GameManager.Instance.InjectControls(this);
    }

    void OnEnable()
    {
        _input.Player.Move.performed += OnMove;
        _input.Player.Move.canceled += OnMove;
        GameManager.Instance?.SetCursorLocked(true);
    }

    void OnDisable()
    {
        _input.Player.Move.performed -= OnMove;
        _input.Player.Move.canceled -= OnMove;
        GameManager.Instance?.SetCursorLocked(false);
    }

    void Update()
    {
        if (IsAnimationCurrentlyScripted)
            return;

        if (CanMove)
        {
            MovePlayer();
            ApplyGravity();
            UpdateAnimator();
        }

        if (CanLook)
            HandleLook();

        if (CanInteract)
            HandleInteract();

        if (_interactionCatcher.TryCatchInteraction(out var interaction))
        {
            if (lastFrameInteraction != interaction && lastFrameInteraction != null)
                lastFrameInteraction.HideMessage();

            lastFrameInteraction = interaction;
            interaction.ShowMessage();
        }
        else
        {
            if (lastFrameInteraction != null)
                lastFrameInteraction.HideMessage();

            lastFrameInteraction = null;
        }
    }

    public void HandleItem(IHandableItem item)
    {
        item.transform.SetParent(_handledItemRoot);
        item.transform.localPosition = item.PositionOffset;
        item.transform.localEulerAngles = item.RotationOffset;
        HandledItem = item;
    }

    public IHandableItem GiveHandledItem()
    {
        var item = HandledItem;
        HandledItem = null;
        return item;
    }

    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
    }

    public void SetCanLook(bool canLook)
    {
        CanLook = canLook;
    }

    public void SetCanInteract(bool canInteract)
    {
        CanInteract = canInteract;
    }

    public void SetWalkSpeedModidfier(float mod)
    {
        _walkSpeedModifier = mod;
    }

    public void SetSprintSpeedModifier(float mod)
    {
        _runSpeedModifier = mod;
    }

    public void InjectController(BaseInput input)
    {
        _input = input;
    }

    public void SetDamaged(bool value)
    {
        _animator.SetBool(_speedParameterName, value);
    }

    public void SetAnimationControl(bool enabled)
    {
        IsAnimationCurrentlyScripted = enabled;

        _controller.enabled = !enabled;
    }

    public void Teleport(Transform target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));

        Vector3 newPosition = new Vector3(target.position.x, target.position.y + _yRigOffset, target.position.z);
        
        _controller.enabled = false;
        transform.SetPositionAndRotation(newPosition, target.rotation);
        _controller.enabled = true;
    }

    public async UniTask MoveToAsync(Transform target, float duration)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        EnsureScriptControl(true);

        Vector3 endPos = new Vector3(target.position.x, target.position.y + _yRigOffset, target.position.z);
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

        _graph = PlayableGraph.Create($"PlayerCutsceneGraph_{name}");
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
        if (!IsPlayingAnimation) return;

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

    private void EnsureScriptControl(bool active)
    {
        if (IsAnimationCurrentlyScripted != active)
            throw new InvalidOperationException($"{name} is not in script-controlled animation mode. Call SetAnimationControl(true) first.");
    }

    private void MovePlayer()
    {
        bool isSprintPressed = _input.Player.Sprint.ReadValue<float>() > 0;
        _isGrounded = _controller.isGrounded;

        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;

        float targetSpeed = isSprintPressed ? (_runSpeed * _runSpeedModifier) : (_walkSpeed * _walkSpeedModifier);

        _controller.Move(move * targetSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        //_animator.SetFloat(_moveXParameterName, _moveInput.x);
        //_animator.SetFloat(_moveYParameterName, _moveInput.y);
        _animator.SetFloat(_speedParameterName, _moveInput.magnitude);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void HandleInteract()
    {
        bool isInteractPressed = _input.Player.Interact.ReadValue<float>() > 0;

        if (isInteractPressed && lastFrameInteraction != null)
        {
            lastFrameInteraction.Interact();
        }
    }

    private void HandleLook()
    {
        Vector2 lookInput = _input.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * _lookSensitivity;
        float mouseY = lookInput.y * _lookSensitivity;

        transform.Rotate(Vector3.up, mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        if (_cameraRoot != null)
            _cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
