using UnityEngine;

public class ParticleHeightTint : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float maxHeight = 120f;
    [SerializeField] private Color lowColor = new Color(1f, 0.95f, 0.85f);
    [SerializeField] private Color highColor = new Color(0.55f, 0.7f, 1f);

    private ParticleSystem _particles;
    private ParticleSystem.MainModule _main;
    private Color _lastColor;
    private bool _hasColor;

    private void Awake()
    {
        _particles = GetComponent<ParticleSystem>();

        if (_particles != null)
        {
            _main = _particles.main;
        }

        if (player == null)
        {
            PlayerBlockAgent agent = FindObjectOfType<PlayerBlockAgent>();

            if (agent != null)
            {
                player = agent.transform;
            }
        }
    }

    private void Update()
    {
        if (_particles == null || player == null)
        {
            return;
        }

        float ratio = Mathf.Clamp01(player.position.y / Mathf.Max(1f, maxHeight));
        Color target = Color.Lerp(lowColor, highColor, ratio);

        if (_hasColor
            && Mathf.Abs(target.r - _lastColor.r) < 0.01f
            && Mathf.Abs(target.g - _lastColor.g) < 0.01f
            && Mathf.Abs(target.b - _lastColor.b) < 0.01f)
        {
            return;
        }

        _main.startColor = target;
        _lastColor = target;
        _hasColor = true;
    }
}
