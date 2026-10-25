using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class BlockLabel : MonoBehaviour
{
    [SerializeField] private float heightAboveTop = 0.9f;
    [SerializeField] private float fontSize = 2.2f;
    [SerializeField] private float maxVisibleDistance = 30f;
    [SerializeField] private bool tintByCurrentColor;

    private Block _block;
    private TextMeshPro _text;

    private void Awake()
    {
        _block = GetComponent<Block>();
        BuildText();
        RefreshLabel();

        if (_block != null)
        {
            _block.ColorChanged += HandleColorChanged;
        }
    }

    private void OnDestroy()
    {
        if (_block != null)
        {
            _block.ColorChanged -= HandleColorChanged;
        }
    }

    private void LateUpdate()
    {
        if (_text == null)
        {
            return;
        }

        Camera camera = Camera.main;

        if (camera == null)
        {
            return;
        }

        float distance = Vector3.Distance(camera.transform.position, _text.transform.position);
        _text.enabled = distance <= maxVisibleDistance;
        _text.transform.rotation = camera.transform.rotation;
    }

    public void RefreshLabel()
    {
        if (_text == null || _block == null)
        {
            return;
        }

        _text.text = Describe(_block.Type);

        if (tintByCurrentColor)
        {
            BlockColorDefinition definition = _block.CurrentDefinition;
            _text.color = definition != null ? definition.uiColor : Color.white;
        }
    }

    public static string Describe(BlockType type)
    {
        if (type == null || !type.IsValid)
        {
            return "?";
        }

        string[] colors = type.Colors;

        if (colors.Length == 1)
        {
            return colors[0];
        }

        string separator = type.Loop ? "<-->" : "->";
        return string.Join(separator, colors);
    }

    private void BuildText()
    {
        if (_text != null)
        {
            return;
        }

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(transform, false);

        _text = labelObject.AddComponent<TextMeshPro>();
        _text.alignment = TextAlignmentOptions.Center;
        _text.fontSize = fontSize;
        _text.enableWordWrapping = false;
        _text.raycastTarget = false;
        _text.outlineWidth = 0.18f;
        _text.outlineColor = Color.black;

        float halfHeight = 1f;
        Collider collider = GetComponentInChildren<Collider>();

        if (collider != null)
        {
            halfHeight = collider.bounds.size.y * 0.5f;
        }

        labelObject.transform.localPosition = Vector3.up * (halfHeight + heightAboveTop);
    }

    private void HandleColorChanged(Block block)
    {
        RefreshLabel();
    }
}
