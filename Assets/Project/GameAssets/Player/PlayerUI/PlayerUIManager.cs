using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlayerUIManager : MonoBehaviour
{
    private const string BloodyDistortionParameterName = "_ScreenDistortionAmount";
    private const float AciveDistortionValue = 0.3f;
    private const string OpacityParameterName = "_ColorOpacity";
    private const float AciveOpacityValue = 0.8f;

    [SerializeField] private TextBox _dialogueBox;
    [SerializeField] private Image _screenFill;
    [SerializeField] private Material _screenBloodEffect;
    private Canvas _canvas;

    public TextBox DialogueBox => _dialogueBox;

    void Start()
    {
        _screenFill.gameObject.SetActive(true);
        _screenFill.color = new Color(0, 0, 0, 0);

        _screenBloodEffect.SetFloat(BloodyDistortionParameterName, 0);
        _screenBloodEffect.SetFloat(OpacityParameterName, 0);
    }

    public void SetCamera(Camera camera)
    {
        _canvas.worldCamera = camera;
    }

    public async UniTask FillFadeIn(float duration)
    {
        var tween = _screenFill.DOFade(1, duration);
        await tween.AsyncWaitForCompletion().AsUniTask();
    }

    public async UniTask FillFadeOut(float duration)
    {
        var tween = _screenFill.DOFade(0, duration);
        await tween.AsyncWaitForCompletion().AsUniTask();
    }

    public async UniTask BloodyOverlayFadeIn(float duration)
    {
        var opacityTween = _screenBloodEffect.DOFloat(AciveOpacityValue, OpacityParameterName, duration);
        var distortionTween = _screenBloodEffect.DOFloat(AciveDistortionValue, BloodyDistortionParameterName, duration);

        await UniTask.WhenAll(
            opacityTween.AsyncWaitForCompletion().AsUniTask(),
            distortionTween.AsyncWaitForCompletion().AsUniTask()
            );
    }

    public async UniTask BloodyOverlayFadeOut(float duration)
    {
        var opacityTween = _screenBloodEffect.DOFloat(0, OpacityParameterName, duration);
        var distortionTween = _screenBloodEffect.DOFloat(0, BloodyDistortionParameterName, duration);

        await UniTask.WhenAll(
            opacityTween.AsyncWaitForCompletion().AsUniTask(),
            distortionTween.AsyncWaitForCompletion().AsUniTask()
            );
    }

    public void SetFillColor(Color color)
    {
        float alpha = _screenFill.color.a;

        _screenFill.color = new Color(color.r, color.g, color.b, alpha);
    }
}
