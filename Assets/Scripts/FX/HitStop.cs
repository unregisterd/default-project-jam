using UnityEngine;

public static class HitStop
{
    public static void Play(float duration = 0.05f, float scale = 0.05f)
    {
        TimeController.HitStop(duration, scale);
    }
}
