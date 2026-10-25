using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

public class RaisePillarVisual : MonoBehaviour
{
    [Tooltip("柱段高度，与关卡高度单位一致")]
    [SerializeField] private float segmentHeight = 0.5f;
    [Tooltip("柱段预制体（美术版，无碰撞、轴心居中）；留空则用方块的占地自动生成方块段")]
    [SerializeField] private GameObject segmentPrefab;
    [Tooltip("每段之间弹出的错峰时间")]
    [SerializeField] private float stagger = 0.04f;
    [Tooltip("单段弹出时长")]
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private bool castShadows;

    private Transform _container;
    private Material _material;

    public void Play(Block block, float delta)
    {
        if (block == null || delta <= 0f || segmentHeight <= 0f)
        {
            return;
        }

        Clear();

        BlockColorDefinition definition = block.CurrentDefinition;
        _material = definition != null ? definition.material : null;

        Collider collider = block.GetComponentInChildren<Collider>();
        Vector3 footprint = collider != null ? collider.bounds.size : new Vector3(2f, 0.5f, 2f);
        float bottom = block.GetBottomHeight();
        Vector3 center = block.transform.position;

        GameObject container = new GameObject($"Pillar_{block.name}");
        Transform parent = block.transform.parent;
        container.transform.SetParent(parent != null ? parent : null, false);
        _container = container.transform;

        int count = Mathf.Max(1, Mathf.RoundToInt(delta / segmentHeight));
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < count; i++)
        {
            GameObject segment = CreateSegment(footprint);

            if (segment == null)
            {
                continue;
            }

            segment.transform.SetParent(_container, false);
            segment.transform.position = new Vector3(center.x, bottom + segmentHeight * (i + 0.5f), center.z);
            segment.transform.rotation = Quaternion.identity;

            Vector3 baseScale = segment.transform.localScale;
            segment.transform.localScale = new Vector3(baseScale.x, baseScale.y * 0.01f, baseScale.z);

            Tween pop = segment.transform
                .DOScaleY(baseScale.y, popDuration)
                .SetEase(Ease.OutBack);

            sequence.Insert(i * stagger, pop);
        }

        sequence.SetLink(gameObject);
    }

    public void Clear()
    {
        if (_container != null)
        {
            Destroy(_container.gameObject);
            _container = null;
        }
    }

    private GameObject CreateSegment(Vector3 footprint)
    {
        if (segmentPrefab != null)
        {
            return Instantiate(segmentPrefab, _container);
        }

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "PillarSegment";

        Collider collider = cube.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        cube.transform.localScale = new Vector3(footprint.x, segmentHeight, footprint.z);

        Renderer renderer = cube.GetComponent<Renderer>();

        if (renderer != null)
        {
            if (_material != null)
            {
                renderer.sharedMaterial = _material;
            }

            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        return cube;
    }
}
