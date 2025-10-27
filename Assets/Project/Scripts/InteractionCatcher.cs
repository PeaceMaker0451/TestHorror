using UnityEngine;

[ExecuteAlways]
public class InteractionCatcher : MonoBehaviour
{
    [SerializeField] private Transform _catchPoint;
    [SerializeField] private float _catchRadius = 1f;
    [SerializeField] private int _interactionPhysicsLayer = 5;

    private Collider[] _buffer = new Collider[16];

    public bool TryCatchInteraction(out Interaction interaction)
    {
        interaction = null;
        if (_catchPoint == null) return false;

        int count = Physics.OverlapSphereNonAlloc(_catchPoint.position, _catchRadius, _buffer);

        float nearestDistance = float.MaxValue;
        Interaction nearest = null;

        for (int i = 0; i < count; i++)
        {
            var findedInteraction = _buffer[i].GetComponent<Interaction>();
            if (findedInteraction == null) continue;

            float distance = Vector3.Distance(_catchPoint.position, findedInteraction.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = findedInteraction;
            }
        }

        if (nearest != null)
        {
            interaction = nearest;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_catchPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_catchPoint.position, _catchRadius);
    }
#endif
}