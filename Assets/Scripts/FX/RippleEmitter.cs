using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RippleEmitter : MonoBehaviour
{
    [SerializeField] private int poolSize = 4;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float endScale = 3.2f;
    [SerializeField] private float duration = 1.6f;
    [SerializeField] private float startAlpha = 0.5f;
    [SerializeField] private float heightOffset = 0.03f;
    [SerializeField] private bool loop;
    [SerializeField] private float loopInterval = 0.9f;
    [SerializeField] private bool randomRotation = true;

    private static Material _sharedMaterial;

    private readonly List<Ripple> _pool = new List<Ripple>();
    private float _nextLoopTime;

    private struct Ripple
    {
        public Transform Transform;
        public MaterialPropertyBlock Property;
        public float StartTime;
        public bool Active;
    }

    public void Configure(bool loopRings, float interval, float ringDuration, float scaleStart, float scaleEnd, float alpha)
    {
        loop = loopRings;
        loopInterval = Mathf.Max(0.1f, interval);
        duration = Mathf.Max(0.1f, ringDuration);
        startScale = scaleStart;
        endScale = scaleEnd;
        startAlpha = alpha;
    }

    private void Awake()
    {
        Material material = GetMaterial();

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

    private static Material GetMaterial()
    {
        if (_sharedMaterial != null)
        {
            return _sharedMaterial;
        }

        _sharedMaterial = Resources.Load<Material>("FX/M_Ripple");

        if (_sharedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            _sharedMaterial = new Material(shader);
            _sharedMaterial.SetFloat("_Surface", 1f);
            _sharedMaterial.SetFloat("_Blend", 0f);
            _sharedMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _sharedMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            _sharedMaterial.SetInt("_ZWrite", 0);
            _sharedMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _sharedMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            _sharedMaterial.SetColor("_BaseColor", Color.white);
        }

        return _sharedMaterial;
    }
}
