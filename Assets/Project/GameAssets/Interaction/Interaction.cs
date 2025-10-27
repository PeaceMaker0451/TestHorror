using System;
using UnityEngine;
using TMPro;

public class Interaction : MonoBehaviour
{
    [SerializeField] private SpriteRenderer interactionIcon;
    [SerializeField] private TextMeshPro messageText;
    [SerializeField] private Vector3 iconOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector3 textOffset = new Vector3(0, 0.5f, 0);

    private Player _player;
   
    public Interaction()
    {
        _player = Player.Instance;
    }

    public bool IsActive { get; private set; }
    public bool IsInitialized { get; private set; }
    public string Message {  get; private set; }

    public event Action OnInteractAction;

    void Awake()
    {
        if (interactionIcon != null)
            interactionIcon.gameObject.SetActive(false);

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (IsActive && IsInitialized)
        {
            if (interactionIcon != null && interactionIcon.isVisible)
            {
                interactionIcon.transform.position = transform.position + iconOffset;
                interactionIcon.transform.LookAt(_player.CameraTransform);
                interactionIcon.transform.Rotate(0, 180f, 0);
            }

            if (messageText != null && messageText.IsActive())
            {
                messageText.transform.position = transform.position + iconOffset + textOffset;
                messageText.transform.LookAt(_player.CameraTransform);
                messageText.transform.Rotate(0, 180f, 0);
            }
        }
    }

    public void Init(string message)
    {
        if (IsInitialized)
            throw new InvalidOperationException("Интеракция уже инициализирована.");

        Message = message;
        IsInitialized = true;
    }

    public void Interact()
    {
        if (IsInitialized)
            OnInteractAction?.Invoke();
        else
            throw new InvalidOperationException("Интеракция не инициализирована.");
    }

    public void SetActive(bool active)
    {
        IsActive = active && IsInitialized;
        interactionIcon.gameObject.SetActive(IsActive);
    }

    public void SetMessage(string message)
    {
        Message = message;
    }
    
    public void ShowMessage()
    {
        if (messageText != null)
        {
            messageText.text = Message;
            messageText.gameObject.SetActive(true);
        }
    }

    public void HideMessage()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }
}