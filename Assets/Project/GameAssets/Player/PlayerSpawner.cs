using System;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private PlayerConfig _config;
    private bool _isSetup;

    public static PlayerSpawner Instance;

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

    public void Setup(PlayerConfig config)
    {
        _config = config;
        _isSetup = true;
    }

    public Player Spawn(Transform transform)
    {
        if (_isSetup == false)
            throw new InvalidOperationException();
        
        Player player = Instantiate(_config.Prefab).GetComponent<Player>();
        player.Teleport(transform);
        return player;
    }
}
