using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BlockIconVisual : MonoBehaviour
{
    private static readonly Dictionary<int, Material> MaterialCache = new Dictionary<int, Material>();

    private Block _block;
    private MeshRenderer _renderer;
    private Transform _quad;
    private float _sizeScale = 0.62f;
    private float _heightOffset = 0.012f;

    private void Awake()
    {
        _block = GetComponent<Block>();
    }

    public void Apply(Texture2D atlas, int columns, int rows, int iconIndex, float sizeScale, float heightOffset)
    {
        _sizeScale = sizeScale;
        _heightOffset = heightOffset;

        BuildQuad();
        UpdateQuadTransform();
        _renderer.sharedMaterial = GetMaterial(atlas, columns, rows, iconIndex);
        _renderer.enabled = atlas != null;
    }

    private void BuildQuad()
    {
        if (_quad != null)
        {
            return;
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Icon";
        quad.transform.SetParent(transform, false);
        quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Collider collider = quad.GetComponent<Collider>();

        if (collider != null)
        {
            Destroy(collider);
        }

        _renderer = quad.GetComponent<MeshRenderer>();

        if (_renderer != null)
        {
            _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
        }

        _quad = quad.transform;
    }

    private void UpdateQuadTransform()
    {
        if (_quad == null)
        {
            return;
        }

        Vector3 footprint = GetFootprint();
        _quad.localScale = new Vector3(footprint.x * _sizeScale, footprint.z * _sizeScale, 1f);
        _quad.localPosition = new Vector3(0f, GetHalfHeight() + _heightOffset, 0f);
    }

    private Vector3 GetFootprint()
    {
        Collider collider = GetComponentInChildren<Collider>();
        return collider != null ? collider.bounds.size : new Vector3(2f, 2f, 2f);
    }

    private float GetHalfHeight()
    {
        Collider collider = GetComponentInChildren<Collider>();
        return collider != null ? collider.bounds.size.y * 0.5f : 1f;
    }

    private static Material GetMaterial(Texture2D atlas, int columns, int rows, int iconIndex)
    {
        int safeColumns = Mathf.Max(1, columns);
        int safeRows = Mathf.Max(1, rows);
        int total = safeColumns * safeRows;
        int index = Mathf.Clamp(iconIndex, 0, total - 1);

        int key = atlas.GetInstanceID() * 100 + index;
        key = key * 31 + safeColumns;
        key = key * 31 + safeRows;

        if (MaterialCache.TryGetValue(key, out Material cached) && cached != null)
        {
            return cached;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            Debug.LogError("[BlockIcon] 找不到 URP Unlit shader。");
            return null;
        }

        Material material = new Material(shader);
        material.name = $"M_BlockIcon_{index}";
        material.SetTexture("_BaseMap", atlas);
        material.SetColor("_BaseColor", Color.white);
        material.SetFloat("_AlphaClip", 1f);
        material.SetFloat("_Cutoff", 0.35f);
        material.EnableKeyword("_ALPHATEST_ON");
        material.SetOverrideTag("RenderType", "TransparentCutout");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

        int column = index % safeColumns;
        int row = index / safeColumns;
        float scaleX = 1f / safeColumns;
        float scaleY = 1f / safeRows;

        material.mainTextureScale = new Vector2(scaleX, scaleY);
        material.mainTextureOffset = new Vector2(column * scaleX, 1f - (row + 1) * scaleY);

        MaterialCache[key] = material;
        return material;
    }
}
