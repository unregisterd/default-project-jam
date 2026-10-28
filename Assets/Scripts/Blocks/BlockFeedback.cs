using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BlockFeedback : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Tooltip("压扁幅度（0.12 = 20%）")]
    [SerializeField] private float squashAmount = 0.12f;
    [SerializeField] private float squashDuration = 0.18f;
    [Tooltip("自发光闪一下的强度")]
    [SerializeField] private float flashIntensity = 1.4f;
    [SerializeField] private float flashDuration = 0.25f;

    private Block _block;
    private Transform _visual;
    private Renderer[] _renderers;
    private Vector3 _baseScale = Vector3.one;
    private MaterialPropertyBlock _propertyBlock;
    private Coroutine _routine;

    private void Awake()
    {
        _block = GetComponent<Block>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _propertyBlock = new MaterialPropertyBlock();

        Transform model = transform.Find("Visual");

        if (model == null)
        {
            model = transform.Find("Model");
        }

        if (model == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.GetComponent<Collider>() == null && child.GetComponentInChildren<Renderer>(true) != null)
                {
                    model = child;
                    break;
                }
            }
        }

        if (model != null)
        {
            _visual = model;
        }
        else if (GetComponent<Collider>() == null)
        {
            _visual = transform;
        }

        if (_visual != null)
        {
            _baseScale = _visual.localScale;
        }
    }

    public void Play()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        Color flashColor = Color.white;

        if (_block != null && _block.CurrentDefinition != null)
        {
            flashColor = _block.CurrentDefinition.uiColor;
        }

        float elapsed = 0f;
        bool hasVisual = _visual != null;

        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / squashDuration);
            float wave = Mathf.Sin(progress * Mathf.PI);

            if (hasVisual)
            {
                float y = 1f - squashAmount * wave;
                float xz = 1f + squashAmount * 0.5f * wave;
                _visual.localScale = new Vector3(_baseScale.x * xz, _baseScale.y * y, _baseScale.z * xz);
            }

            ApplyEmission(flashColor * (flashIntensity * (1f - progress)));
            yield return null;
        }

        if (hasVisual)
        {
            _visual.localScale = _baseScale;
        }

        elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            ApplyEmission(flashColor * (flashIntensity * 0.5f * (1f - elapsed / flashDuration)));
            yield return null;
        }

        ApplyEmission(Color.black);
        _routine = null;
    }

    private void ApplyEmission(Color color)
    {
        if (_renderers == null)
        {
            return;
        }

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];

            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(EmissionColorId, color);
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
