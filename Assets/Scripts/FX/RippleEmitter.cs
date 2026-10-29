using System.Collections.Generic;
using UnityEngine;

public enum RippleStyle
{
    Ink = 0,
    CrystalPlayer = 1,
    CrystalNpc = 2,
}

[DisallowMultipleComponent]
public class RippleEmitter : MonoBehaviour
{
    [SerializeField] private int poolSize = 4;
    [SerializeField] private RippleStyle style = RippleStyle.Ink;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float endScale = 3.2f;
    [SerializeField] private float duration = 1.6f;
    [SerializeField] private float startAlpha = 0.5f;
    [SerializeField] private float heightOffset = 0.03f;
    [SerializeField] private bool loop;
    [SerializeField] private float loopInterval = 0.9f;
    [SerializeField] private bool randomRotation = true;

    private static readonly Dictionary<RippleStyle, Material> Materials = new Dictionary<RippleStyle, Material>();

    private readonly List<Ripple> _pool = new List<Ripple>();
    private float _nextLoopTime;

    private struct Ripple
    {
        public Transform Transform;
        public MaterialPropertyBlock Property;
        public float StartTime;
        public bool Active;
    }

    public void Configure(bool loopRings, float interval, float ringDuration, float scaleStart, float scaleEnd, float alpha, RippleStyle rippleStyle)
    {
        loop = loopRings;
        loopInterval = Mathf.Max(0.1f, interval);
        duration = Mathf.Max(0.1f, ringDuration);
        startScale = scaleStart;
        endScale = scaleEnd;
        startAlpha = alpha;
        style = rippleStyle;
    }

    public void Configure(bool loopRings, float interval, float ringDuration, float scaleStart, float scaleEnd, float alpha)
    {
        Configure(loopRings, interval, ringDuration, scaleStart, scaleEnd, alpha, style);
    }

    private void Awake()
    {
        Material material = GetMaterial(style);

        for (int i = 0; i < Mathf.Max(1, poolSize); i++)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ring.name = "RippleRing";
            ring.transform.SetParent(transform, false);
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ring.transform.localScale = Vector3.zero;

            Collider collider = ring.GetComponent<Collider>();

            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = ring.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            _pool.Add(new Ripple
            {
                Transform = ring.transform,
                Property = new MaterialPropertyBlock(),
                Active = false,
            });
        }

        if (loop)
        {
            _nextLoopTime = Time.time;
        }
    }

    private void Update()
    {
        if (loop && Time.time >= _nextLoopTime)
        {
            _nextLoopTime = Time.time + loopInterval;
            PlayOnce();
        }

        for (int i = 0; i < _pool.Count; i++)
        {
            Ripple ripple = _pool[i];

            if (!ripple.Active)
            {
                continue;
            }

            float progress = (Time.time - ripple.StartTime) / duration;

            if (progress >= 1f)
            {
                ripple.Active = false;
                ripple.Transform.localScale = Vector3.zero;
                _pool[i] = ripple;
                continue;
            }

            float eased = 1f - (1f - progress) * (1f - progress);
            float scale = Mathf.Lerp(startScale, endScale, eased);
            float alpha = Mathf.Lerp(startAlpha, 0f, progress);

            ripple.Transform.localScale = new Vector3(scale, scale, 1f);
            ripple.Transform.localPosition = new Vector3(0f, heightOffset, 0f);

            Renderer renderer = ripple.Transform.GetComponent<Renderer>();

            if (renderer != null)
            {
                ripple.Property.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
                renderer.SetPropertyBlock(ripple.Property);
            }

            _pool[i] = ripple;
        }
    }

    public void PlayOnce()
    {
        float stagger = 0.18f;

        for (int i = 0; i < _pool.Count; i++)
        {
            Ripple ripple = _pool[i];
            ripple.Active = true;
            ripple.StartTime = Time.time + i * stagger;
            ripple.Transform.localScale = Vector3.zero;

            if (randomRotation)
            {
                ripple.Transform.localRotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            }

            _pool[i] = ripple;
        }
    }

    private static Material GetMaterial(RippleStyle rippleStyle)
    {
        if (Materials.TryGetValue(rippleStyle, out Material cached) && cached != null)
        {
            return cached;
        }

        string path;

        switch (rippleStyle)
        {
            case RippleStyle.CrystalPlayer:
                path = "FX/M_RipplePlayer";
                break;
            case RippleStyle.CrystalNpc:
                path = "FX/M_RippleNpc";
                break;
            default:
                path = "FX/M_Ripple";
                break;
        }

        Material material = Resources.Load<Material>(path);

        if (material == null)
        {
            Debug.LogWarning($"[Ripple] 找不到 Resources/{path}，回退到 M_Ripple");
            material = Resources.Load<Material>("FX/M_Ripple");
        }

        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            material = new Material(shader);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.SetColor("_BaseColor", Color.white);
        }

        Materials[rippleStyle] = material;
        return material;
    }
}
