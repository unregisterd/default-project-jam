using Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraTargetAutoBinder : MonoBehaviour
{
    private void Awake()
    {
        Bind();
    }

    public void Bind()
    {
        Transform target = ResolveTarget();

        if (target == null)
        {
            return;
        }

        CinemachineVirtualCamera virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera != null)
        {
            if (virtualCamera.Follow == null) virtualCamera.Follow = target;
            if (virtualCamera.LookAt == null) virtualCamera.LookAt = target;
            return;
        }

        CinemachineFreeLook freeLook = GetComponent<CinemachineFreeLook>();

        if (freeLook != null)
        {
            if (freeLook.Follow == null) freeLook.Follow = target;
            if (freeLook.LookAt == null) freeLook.LookAt = target;
        }
    }

    private static Transform ResolveTarget()
    {
        GameObject player = GameObject.Find("PlayerArmature");

        if (player == null)
        {
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>();

            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];

                if (behaviour != null && behaviour.GetType().Name == "ThirdPersonController")
                {
                    player = behaviour.gameObject;
                    break;
                }
            }
        }

        return player != null ? FindDeepChild(player.transform, "PlayerCameraRoot") : null;
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), name);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
