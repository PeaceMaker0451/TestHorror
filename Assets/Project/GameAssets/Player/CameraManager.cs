using UnityEngine;

[RequireComponent(typeof(AudioListener))]
[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    private Camera _camera;
    private AudioListener _audioListener;
}
