using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class TextBox : MonoBehaviour, IControllable
{
    [SerializeField] private GameObject _canvasRoot;
    [SerializeField] private Button _nextButton;
    [SerializeField] private GameObject _readyIcon;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private AudioSource _audioPlayer;
    [SerializeField] private AudioClip _defaultTextSound;

    private BaseInput _input;
    private int _defaultTypingSpeed;
    private int _newLineTypingSpeed;
    private int _periodTypingSpeed;
    private int _commaTypingSpeed;
    private int _dashTypingSpeed;
    private bool _autoSkip;
    private int _autoSkipTimer;

    private Queue<TextBoxLine> _lines;

    private bool _isTyping = false;
    private bool _awaitingInput = false;

    private CancellationTokenSource _typingCts;
    private CancellationTokenSource _skipCts;

    public event Action OnLinesEnd;
    public event Action OnLastPhraseTypingEnd;

    private void Awake()
    {
        _canvasRoot.SetActive(false);
        _readyIcon.SetActive(false);

        _nextButton.onClick.AddListener(() => ClickTextBox());
        
        GameManager.Instance.InjectControls(this);
    }
    void OnEnable()
    {
        _input.Player.Interact.started += OnInteract;
    }

    void OnDisable()
    {
        _input.Player.Interact.started -= OnInteract;
    }

    public void ClickTextBox()
    {
        if (_isTyping)
        {
            SkipTyping();
            return;
        }

        if (_awaitingInput)
        {
            _awaitingInput = false;
            _readyIcon.SetActive(false);
            DisplayNextPhraseAsync(this.GetCancellationTokenOnDestroy());
        }
    }

    public UniTask WriteAsync(List<TextBoxLine> lineList, int autoSkipDuration)
    {
        if (lineList == null || lineList.Count == 0) return UniTask.CompletedTask;

        var tcs = new UniTaskCompletionSource();

        Action endAwaiter = null;

        endAwaiter += () =>
        {
            OnLinesEnd -= endAwaiter;
            tcs.TrySetResult();
        };

        OnLinesEnd += endAwaiter;
        
        _lines = new Queue<TextBoxLine>(lineList);
        _canvasRoot.SetActive(true);
        _readyIcon.SetActive(false);

        if(autoSkipDuration > 0)
        {
            _autoSkip = true;
            _autoSkipTimer = autoSkipDuration;
        }
        else
        {
            _autoSkip = false;
        }

        _ = DisplayNextPhraseAsync(this.GetCancellationTokenOnDestroy());
        return tcs.Task;
    }

    public UniTask WriteAsync(TextBoxLine line, int autoSkipDuration)
    {
        return WriteAsync(new List<TextBoxLine> { line }, autoSkipDuration);
    }

    private void SkipTyping()
    {
        _typingCts?.Cancel();
    }

    private void CancelAllProcesses()
    {
        _typingCts?.Cancel();
        _skipCts?.Cancel();
    }

    private async UniTask DisplayNextPhraseAsync(CancellationToken parentToken)
    {
        const char NEW_LINE_CHAR = '\n';
        const char COMMA_CHAR = ',';
        const char PERIOD_CHAR = '.';
        const char DASH_CHAR = '-';

        CancelAllProcesses();
        _typingCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
        _skipCts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);

        if (_lines == null || _lines.Count == 0)
        {
            EndMonologue();
            return;
        }

        _isTyping = true;

        TextBoxLine line = _lines.Dequeue();
        _defaultTypingSpeed = line.TypingSpeed;
        _newLineTypingSpeed = line.NewLineTypingSpeed;
        _commaTypingSpeed = line.CommaTypingSpeed;
        _periodTypingSpeed = line.PeriodTypingSpeed;
        _dashTypingSpeed = line.DashTypingSpeed;

        _audioPlayer.clip = line.TextSound ?? _defaultTextSound;

        string phrase = line.Text;
        _titleText.text = line.Head;
        _contentText.text = string.Empty;

        _titleText.color = line.TextColor;
        _contentText.color = line.TextColor;

        int index = 0;

        async UniTask ApplyDelay(int delayMs, CancellationToken token)
        {
            if (delayMs <= 0) return;
            await UniTask.Delay(delayMs, cancellationToken: token, cancelImmediately: true);
        }

        while (index < phrase.Length && !_typingCts.IsCancellationRequested)
        {
            char c = phrase[index];

            switch (phrase[index])
            {
                case NEW_LINE_CHAR:
                    await ApplyDelay(_newLineTypingSpeed, _skipCts.Token);
                    break;

                case COMMA_CHAR:
                    await ApplyDelay(_commaTypingSpeed, _skipCts.Token);
                    break;

                case PERIOD_CHAR:
                    await ApplyDelay(_periodTypingSpeed, _skipCts.Token);
                    break;

                case DASH_CHAR:
                    await ApplyDelay(_dashTypingSpeed, _skipCts.Token);
                    break;

                default:
                    await ApplyDelay(_defaultTypingSpeed, _skipCts.Token);
                    break;
            }

            if (_typingCts.IsCancellationRequested)
            {
                _contentText.text = phrase;
                break;
            }

            _contentText.text += c;

            if (c != '\n' && _audioPlayer.clip != null)
            {
                _audioPlayer.Play();
            }

            index++;
        }

        _isTyping = false;

        if (_lines.Count == 0)
            OnLastPhraseTypingEnd?.Invoke();

        if (_autoSkip && !_skipCts.IsCancellationRequested)
        {
            if (_autoSkipTimer == 0)
                await DisplayNextPhraseAsync(parentToken);
            else
            {
                try
                {
                    await UniTask.Delay(_autoSkipTimer, cancellationToken: _skipCts.Token);
                    if (!_skipCts.IsCancellationRequested)
                        await DisplayNextPhraseAsync(parentToken);
                }
                catch (OperationCanceledException) { }
            }
        }
        else 
        {
            _readyIcon.SetActive(true);
            _awaitingInput = true;
        }
    }

    private void EndMonologue()
    {
        _canvasRoot.SetActive(false);
        _awaitingInput = false;
        _contentText.text = "";
        OnLinesEnd?.Invoke();
    }

    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        ClickTextBox();
    }

    public void InjectController(BaseInput input)
    {
        _input = input;
    }
}
