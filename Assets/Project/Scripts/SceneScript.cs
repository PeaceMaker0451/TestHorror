using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public abstract class SceneScript : MonoBehaviour
{
    public abstract UniTask RunScene();

    public Player Player => Player.Instance;
    public PlayerUIManager UI => Player ? Player.UI : null;
    
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

    protected async UniTask Say(ITextActor actor, string phrase)
    {
        Player player = GetPlayer();
        TextBoxLine line = actor.PersonalizeLine(phrase);

        bool oldCanMove = player.CanMove;
        bool oldCanInteract = player.CanInteract;

        player.SetCanMove(false);
        player.SetCanInteract(false);

        await player.UI.DialogueBox.WriteAsync(line, 0);

        player.SetCanInteract(oldCanInteract);
        player.SetCanMove(oldCanMove);
    }

    protected async UniTask SayDynamically(ITextActor actor, string phrase, int onScreenDuration)
    {
        Player player = GetPlayer();
        TextBoxLine line = actor.PersonalizeLine(phrase);
        await player.UI.DialogueBox.WriteAsync(line, onScreenDuration);
    }

    private Player GetPlayer()
    {
        Player player = Player.Instance;
        if (player == null)
            throw new Exception("Player not found");
        return player;
    }
}
