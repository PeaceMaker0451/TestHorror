using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private InteractionConfig _interactionConfig;
    [SerializeField] private PlayerConfig _playerConfig;
    private BaseInput _input;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Physics.queriesHitTriggers = true;

        _input = new BaseInput();
        _input.Enable();
    }

    public void InjectControls(IControllable controllable)
    {
        controllable.InjectController(_input);
    }

    private void Start()
    {
        InteractionFactory.Instance.Setup(_interactionConfig);
        PlayerSpawner.Instance.Setup(_playerConfig);
    }

    public void SetCursorLocked(bool lockCursor)
    {
        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}