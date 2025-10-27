using Cysharp.Threading.Tasks;
using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class LightGroup : MonoBehaviour
{
    [SerializeField] private Light[] _lights;
    private float[] _intencities;
    void Start()
    {
        _intencities = new float[_lights.Length];

        for(int i = 0;  i < _lights.Length; i++)
        {
            _intencities[i] = _lights[i].intensity;
            _lights[i].intensity = 0;
            _lights[i].gameObject.SetActive(true);
        }
    }

    public async UniTask TurnOn(float duration)
    {
        UniTask[] tweenUniTasks = new UniTask[_lights.Length];

        for (int i = 0; i < _lights.Length; i++)
        {
            tweenUniTasks[i] = _lights[i].DOIntensity(_intencities[i], duration).AsyncWaitForCompletion().AsUniTask();
        }

        await UniTask.WhenAll(tweenUniTasks);
    }

    public async UniTask TurnOff(float duration)
    {
        UniTask[] tweenUniTasks = new UniTask[_lights.Length];

        for (int i = 0; i < _lights.Length; i++)
        {
            tweenUniTasks[i] = _lights[i].DOIntensity(0, duration).AsyncWaitForCompletion().AsUniTask();
        }

        await UniTask.WhenAll(tweenUniTasks);
    }
}
