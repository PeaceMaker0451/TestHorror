using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class HorrorTestScene : SceneScript
{
    [SerializeField] private Transform _playerSpawnPoint;

    [SerializeField] private CoffeeSet _coffeSet;

    [Header("Интеракции дверей")]
    [SerializeField] private Transform _frontDoorInteractionPoint;
    [SerializeField] private Transform _backDoorInteractionPoint;

    [Header("Уборка стола")]
    [SerializeField] private Transform _trashedTableInteractionPoint;
    [SerializeField] private GameObject _trashOnTheTable;

    [Header("Джонни")]
    [SerializeField] private Transform _johnnyInteractionPoint;

    [Header("Проводы механика")]
    [SerializeField] private Transform _mechInteractionPoint;
    [SerializeField] private Transform _mechSittingPoint;

    [Header("NPC")]
    [SerializeField] private Npc _johnny;
    [SerializeField] private Npc _mech;
    [SerializeField] private Maniac _maniac;

    [Header("Атака")]
    [SerializeField] private GameObject _maniacVolume;
    [SerializeField] private AnimationClip _maniacAttack;
    [SerializeField] private AnimationClip _playerAttack;
    [SerializeField] private AudioSource _playerAttackSound;
    [SerializeField] private Transform _maniacPlace;
    [SerializeField] private Transform _playerPlace;
    [SerializeField] private Transform _giveCoffeeInteraction;
    [SerializeField] private Transform _keyInteractionPlace;
    [SerializeField] private LightGroup _thrillerLights;
    [SerializeField] private AudioSource _thrillerSoundtrack;
    [SerializeField] private Volume _thrillerVolume;

    [Header("Цели Навмешей")]
    [SerializeField] private Transform _leaveCafeFrontTarget;
    [SerializeField] private Transform _leaveCafeBackTarget;
    [SerializeField] private Transform _getCoffeeTarget;

    private bool _mechLeaved;
    private bool _tableCleaned;

    async void Start()
    {
        _mech.SetAnimationControl(true);
        _mech.Teleport(_mechSittingPoint);
        _mech.Sit();
        _thrillerVolume.weight = 0;
        _maniacVolume.SetActive(false);
        PlayerSpawner.Instance.Spawn(_playerSpawnPoint);
        Player.SetWalkSpeedModidfier(0.3f);
        Player.SetSprintSpeedModifier(0.3f);
        await UniTask.WaitForSeconds(3);
        await RunScene();
    }

    override public async UniTask RunScene()
    {
        await SayDynamically(Player, "Гребанный вечер...", 800);
        await SayDynamically(Player, "Он специально таскает меня на ночь глядя по всему городу, сука.", 800);
        await SayDynamically(Player, "Зная, что я по уши в расследовании из-за того сталкинга.." +
            "\nЭто типа смешно.", 800);

        Player.SetWalkSpeedModidfier(0.8f);
        Player.SetSprintSpeedModifier(0.8f);

        //Жонне
        await ForInteraction(_johnnyInteractionPoint, "Джонни", true);
        await Say(_johnny, "Наконец-то! " +
            "\nДождались!");
        await Say(_johnny, "Я чуть смену за тебя не закрыл.");
        await Say(Player, "Так и что не закрыл?");
        await Say(_johnny, "Ха-ха-ха~" +
            "\nСоблюдай субординацию - я -СТАРШИЙ- менеджер");
        await Say(_johnny, "Ты должен обращаться ко мне на \"Вы\"" +
            "\nИначе я доложу твоему супервайзеру.");
        await Say(_johnny, "Короче говоря." +
            "\nУберешь мусор с столов, дождешься пока гости уйдут..");
        await Say(_johnny, "Закрываешь кассу и идешь на все четыре стороны.");
        await Say(Player, "Да-да, сам знаю.");
        await Say(_johnny, "Не слышу.");
        await Say(Player, "Сэр - есть - сэр!");
        await Say(_johnny, "Пфф");

        _johnny.Reach(_leaveCafeBackTarget.position);

        _ = SayDynamically(_johnny, "Завтра в 8 утра чтобы здесь был." +
    "Ни минуты опаздания.", 800);

        Action _johnnyLeavedAction = null;
        _johnnyLeavedAction = () =>
        {
            _johnny.TargetReached -= _johnnyLeavedAction;
            _johnny.gameObject.SetActive(false);
        };

        _johnny.TargetReached += _johnnyLeavedAction;

        //Стол и мех
        var mechInteraction = InteractionFactory.Instance.CreateInteraction(_mechInteractionPoint, "Попросить уйти");
        mechInteraction.OnInteractAction += () =>
        {
            mechInteraction.SetActive(false);
            Destroy(mechInteraction.gameObject);

            _ = MechSequence();
        };
        mechInteraction.SetActive(true);

        var tableInteraction = InteractionFactory.Instance.CreateInteraction(_trashedTableInteractionPoint, "Убрать стол");
        tableInteraction.OnInteractAction += () =>
        {
            _ = HandleTableInteractionAsync(tableInteraction);
        };
        tableInteraction.SetActive(true);

        await UniTask.WhenAll(
                UniTask.WaitUntil(() => _tableCleaned),
                UniTask.WaitUntil(() => _mechLeaved)
            );

        await UniTask.WaitForSeconds(5);

        _maniac.Reach(_getCoffeeTarget.position);

        await UniTask.WaitForSeconds(5);

        await SayDynamically(Player, "Извините но мы уже закрыты.", 800);
        await SayDynamically(Player, "?..", 600);

        await ForInteraction(_giveCoffeeInteraction, "Сказать, что кафе закрыто");
        await Say(_maniac, "сделай мне кофе.");
        await Say(Player, "Мхххх--" +
            "\nОкей..");

        await _coffeSet.RunNewCofeeSequence();

        await ForInteraction(_giveCoffeeInteraction, "Дать кофе.");

        var item = Player.GiveHandledItem();
        Destroy((item as MonoBehaviour).gameObject);

        Player.SetAnimationControl(true);
        _maniac.SetAnimationControl(true);

        await UniTask.WhenAll(
                Player.MoveToAsync(_playerPlace, 0.6f),
                _maniac.MoveToAsync(_maniacPlace, 0.6f)
            );

        _ = ThrillerDialogueSequence();
        _ = AttackEffectsSequence();
        _playerAttackSound.Play();

        await UniTask.WhenAll(
            Player.PlayAnimationAsync(_playerAttack),
            _maniac.PlayAnimationAsync(_maniacAttack)
            );

        Player.SetAnimationControl(false);
        _maniac.SetAnimationControl(false);

        Player.SetWalkSpeedModidfier(0.4f);
        Player.SetSprintSpeedModifier(0.6f);

        _maniac.Follow(Player.transform);

        _maniac.FollowTargetReached += () => _ = FollowPlayerSequence();
    }

    private async UniTask FollowPlayerSequence()
    {
        bool firstAttack = _maniac.IsAlreadyAttacked == false;

        if (firstAttack == false)
            _ = DieSequence();

        await _maniac.Attack(Player);

        if(firstAttack)
            await SayDynamically(_maniac, "я иду за тобой~~", 600);
    }

    private async UniTask DieSequence()
    {
        await UniTask.WaitForSeconds(1f);
        await UI.FillFadeIn(2);
        await UniTask.WaitForSeconds(1);
        Application.Quit();
    }

    private async UniTask ThrillerDialogueSequence()
    {
        await SayDynamically(Player, "Вот ваш кофе. " +
            "С вас 2 доллара.", 400);
        await SayDynamically(Player, "Чт-..", 800);
        await SayDynamically(Player, "угххх--", 600);
        await UniTask.WaitForSeconds(2);
        await SayDynamically(_maniac, "хехехехе~~~", 1500);
        await SayDynamically(_maniac, "раз - два - три~~", 2000);
        await SayDynamically(_maniac, "теленочка беги-", 2000);
        await SayDynamically(_maniac, "четыре - пять - шесть", 2000);
        await SayDynamically(_maniac, "принес тебе последнюю весть~~", 2000);
        await SayDynamically(Player, "Двери.. Нужно бежать...", 600);

        _ = FrontDoorSequence();

        _ = BackDorrSequence();
    }

    private async UniTask BackDorrSequence()
    {
        await ForInteraction(_backDoorInteractionPoint, "Сбежать");
        await SayDynamically(Player, "ЗАКРЫТО." +
            "\nКЛЮЧ! КЛЮЧ ПОД КАССОЙ!", 600);
        await SayDynamically(_maniac, "мне почти тебя жаль~" +
            "\nвообще нет." +
            "\nты сам во всем виноват", 1000);

        await ForInteraction(_keyInteractionPlace, "КЛЮЧА НЕТ");
        await SayDynamically(Player, "АААААААААААААА" +
            "\nКЛЮЧА НЕТ", 600);
        await SayDynamically(_maniac, "никуда ты от меня не убежишь, теленочек~", 1000);
        _maniac.SetSpeed(_maniac.Speed * 2);
    }

    private async UniTask FrontDoorSequence()
    {
        await ForInteraction(_frontDoorInteractionPoint, "Сбежать");
        await SayDynamically(Player, "Закрыто!?" +
            "\nЗАКРЫТО, СУКА" +
            "\nСУКА, СУКА!", 600);
        await SayDynamically(_maniac, "как же ты жалок-", 1000);
    }

    private async UniTask AttackEffectsSequence()
    {
        await UniTask.WaitForSeconds(5);
        _ = _thrillerLights.TurnOn(4);
        _thrillerSoundtrack.Play();
        _thrillerVolume.weight = 1;
        Player.SetDamaged(true);
        _maniacVolume.SetActive(true);
        await UI.BloodyOverlayFadeIn(1);
    }

    private async UniTask MechSequence()
    {
        await Say(Player, "Аэээм.. " +
                "Здравствуйте?");
        await Say(_mech, "..." +
            "Здравствуйте.");
        await Say(Player, "Вы не могли бы покинуть это заведение?" +
            "\nМы закрываемся.");
        await Say(_mech, "Хм.. А как же \"работать до последнего клиента\"?");
        await Say(Player, "...");
        await Say(_mech, "А если мне некуда идти?");
        await Say(_mech, "Забудь, я понимаю." +
            "\nСделай мне кофе, покрепче" +
            "\nИ я пойду.");
        await Say(Player, "Сделаю. " +
            "\nЗа счет заведения.");
        await Say(_mech, "А?.. " +
            "\nОх, спасибо..");

        await _coffeSet.RunNewCofeeSequence();

        await ForInteraction(_mechInteractionPoint, "Дать кофе", true);

        var item = Player.GiveHandledItem();
        Destroy((item as MonoBehaviour).gameObject);

        await Say(_mech, "Спасибо..");
        await Say(_mech, "..");
        await Say(_mech, "Я.. Пойду..");

        _mechLeaved = true;
        _mech.SetAnimationControl(false);
        _mech.Stand();
        await UniTask.WaitForSeconds(3);
        _mech.Reach(_leaveCafeFrontTarget.position);

        await SayDynamically(Player, "Приходите еще.", 800);
    }

    private async UniTask HandleTableInteractionAsync(Interaction interaction)
    {
        Destroy(interaction.gameObject);
        _tableCleaned = true;
        Player.SetCanInteract(false);
        Player.SetCanMove(false);

        await UI.FillFadeIn(0.8f);

        _trashOnTheTable.SetActive(false);
        Player.SetCanInteract(true);
        Player.SetCanMove(true);

        await UI.FillFadeOut(0.8f);
    }
}
