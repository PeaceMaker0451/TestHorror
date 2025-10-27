using UnityEngine;

public interface IHandableItem
{
    public Transform transform { get; }
    public Vector3 PositionOffset { get; }
    public Vector3 RotationOffset { get; }
}
