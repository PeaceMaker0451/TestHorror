using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class CoffeeSet : MonoBehaviour
{
    [SerializeField] private GameObject _coffeeCupPrefab;

    [SerializeField] private Transform _takeCupPlace;
    [SerializeField] private Transform _fillCupPlace;
    [SerializeField] private Transform _giveFilledCupPlace;

    [SerializeField] private Transform _fillCupPoint;
    [SerializeField] private Transform _placeCupPoint;

    [SerializeField] private float _fillCupTime;

    private CoffeeCup _currentCup;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public async UniTask RunNewCofeeSequence()
    {
        await ForInteraction(_takeCupPlace, "Взять стакан");

        var cupGO = Instantiate(_coffeeCupPrefab);
        var cupComp = cupGO.GetComponent<CoffeeCup>();
        if (cupComp == null)
            throw new Exception("Prefab не содержит CoffeeCup.");

        _currentCup = cupComp;
        Player.Instance.HandleItem(_currentCup);

        await ForInteraction(_fillCupPlace, "Приготовить кофе");

        var taken = Player.Instance.GiveHandledItem();
        if (taken != null)
        {
            taken.transform.SetParent(_fillCupPoint, worldPositionStays: true);
            taken.transform.position = _fillCupPoint.position;
            taken.transform.rotation = _fillCupPoint.rotation;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_fillCupTime));

        _currentCup?.Fill();

        await ForInteraction(_fillCupPlace, "Взять наполенный стакан");

        if (_currentCup != null)
            Player.Instance.HandleItem(_currentCup);

        _currentCup = null;
    }

    protected async UniTask<Interaction> ForInteraction(Transform transform, string interactionName, bool destroy = true)
    {
        bool interacted = false;
        Interaction interaction = InteractionFactory.Instance.CreateInteraction(transform, interactionName);

        Action onInteract = null;
        onInteract = () =>
        {
            interaction.OnInteractAction -= onInteract;
            interacted = true;

            if (destroy)
                Destroy(interaction.gameObject);
        };

        interaction.OnInteractAction += onInteract;
        interaction.SetActive(true);

        while (interacted == false && interaction != null)
        {
            await UniTask.NextFrame();
        }

        return interaction;
    }
}
