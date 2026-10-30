using UnityEngine;

public class HeroTurntable : MonoBehaviour
{
    [SerializeField] private float degreesPerSecond = 18f;
    [SerializeField] private bool disableInPlayMode;

    private void Update()
    {
        if (disableInPlayMode && Application.isPlaying)
        {
            return;
        }

        transform.Rotate(0f, degreesPerSecond * Time.unscaledDeltaTime, 0f, Space.World);
    }
}
