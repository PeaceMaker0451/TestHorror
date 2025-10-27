using System;
using UnityEngine;

public class InteractionFactory : MonoBehaviour
{
    private InteractionConfig _config;
    private bool _isSetup;

    public static InteractionFactory Instance;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Setup(InteractionConfig config)
    {
        _config = config;
        _isSetup = true;
    }

    public Interaction CreateInteraction(Transform transform, string name)
    {
        if (_isSetup == false)
            throw new InvalidOperationException();

        Interaction interaction = Instantiate(_config.Prefab, transform).GetComponent<Interaction>();
        interaction.Init(name);
        return interaction;
    }
}
