using UnityEngine;

/// <summary>
/// 特殊段预制体的入口/出口标记。生成器按 Entry / Exit 的位置拼接段落。
/// </summary>
public class TowerSection : MonoBehaviour
{
    [SerializeField] private Transform entry;
    [SerializeField] private Transform exit;
    [Tooltip("这一段需要的能力，仅用于生成器插补给和校验")]
    [SerializeField] private string requiredAbility;
    [Tooltip("这一段需要的星光点数")]
    [SerializeField] private int requiredPoints = 1;

    public Transform Entry => entry;
    public Transform Exit => exit;
    public string RequiredAbility => requiredAbility;
    public int RequiredPoints => requiredPoints;

    private void OnValidate()
    {
        if (entry == null)
        {
            entry = transform.Find("Entry");
        }

        if (exit == null)
        {
            exit = transform.Find("Exit");
        }
    }

    [ContextMenu("Create Entry/Exit If Missing")]
    public void EnsureMarkers()
    {
        if (entry == null)
        {
            entry = CreateMarker("Entry", Vector3.zero);
        }

        if (exit == null)
        {
            exit = CreateMarker("Exit", Vector3.up * 4f);
        }
    }

    private Transform CreateMarker(string markerName, Vector3 localPosition)
    {
        GameObject marker = new GameObject(markerName);
        marker.transform.SetParent(transform, false);
        marker.transform.localPosition = localPosition;
        return marker.transform;
    }
}
