using UnityEngine;

public class CoffeeCup : MonoBehaviour, IHandableItem
{
    [SerializeField] private GameObject _coffee;
    [SerializeField] private Vector3 _positionOffset;
    [SerializeField] private Vector3 _rotationOffset;

    public Vector3 PositionOffset => _positionOffset;
    public Vector3 RotationOffset => _rotationOffset;

    void Start()
    {
        _coffee.SetActive(false);
    }
    
    public void Fill()
    {
        _coffee.SetActive(true);
    }

    public void Empty()
    {
        _coffee.SetActive(false);
    }
}
