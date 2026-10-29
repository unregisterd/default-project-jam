using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RippleEmitter : MonoBehaviour
{
    [SerializeField] private int poolSize = 4;
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float endScale = 3.2f;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float startAlpha = 0.75f;
    [SerializeField] private float heightOffset = 0.03f;
    [SerializeField] private bool loop;
    [SerializeField] private float loopInterval = 1.6f;
    [SerializeField] private bool randomRotation = true;

    private static Material _sharedMaterial;
    private static Mesh _ringMesh;

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
        duration = Mathf.Max(0.05f, ringDuration);
        startScale = scaleStart;
        endScale = scaleEnd;
        startAlpha = alpha;
    }

    private void Awake()
    {
        EnsureResources();

        for (int i = 0; i < Mathf.Max(1, poolSize); i++)
        {
            GameObject ring = new GameObject("RippleRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = new Vector3(0f, heightOffset, 0f);
            ring.transform.localScale = Vector3.zero;

            MeshFilter filter = ring.AddComponent<MeshFilter>();
            filter.sharedMesh = _ringMesh;

            MeshRenderer renderer = ring.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _sharedMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

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

        Color moodColor = SkyMoodController.Instance != null
            ? SkyMoodController.Instance.RippleColor
            : new Color(1f, 0.86f, 0.8f);

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
            float alpha = Mathf.Lerp(startAlpha, 0f, progress * progress);

            ripple.Transform.localScale = new Vector3(scale, scale, scale);

            Renderer renderer = ripple.Transform.GetComponent<Renderer>();

            if (renderer != null)
            {
                Color color = moodColor;
                color.a = alpha;
                ripple.Property.SetColor("_BaseColor", color);
                renderer.SetPropertyBlock(ripple.Property);
            }

            _pool[i] = ripple;
        }
    }

    public void PlayOnce()
    {
        float stagger = 0.16f;

        for (int i = 0; i < _pool.Count; i++)
        {
            Ripple ripple = _pool[i];
            ripple.Active = true;
            ripple.StartTime = Time.time + i * stagger;
            ripple.Transform.localScale = Vector3.zero;

            if (randomRotation)
            {
                ripple.Transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            _pool[i] = ripple;
        }
    }

    private static void EnsureResources()
    {
        if (_ringMesh == null)
        {
            _ringMesh = BuildHexagonRing();
        }

        if (_sharedMaterial != null)
        {
            return;
        }

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

    private static Mesh BuildHexagonRing()
    {
        const int sides = 6;
        float inner = 1f - 0.12f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < sides; i++)
        {
            float a0 = (i / (float)sides) * Mathf.PI * 2f;
            float a1 = ((i + 1) / (float)sides) * Mathf.PI * 2f;

            Vector3 inner0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * inner;
            Vector3 outer0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0));
            Vector3 inner1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * inner;
            Vector3 outer1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));

            int start = vertices.Count;
            vertices.Add(inner0);
            vertices.Add(outer0);
            vertices.Add(inner1);
            vertices.Add(outer1);

            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 1);

            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        Mesh mesh = new Mesh();
        mesh.name = "HexagonRing";
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }
}
