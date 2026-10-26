using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopeAbility : MonoBehaviour, IAbility
{
    [Tooltip("临时平台预制体（需带 Block + Collider）；留空则用基础方块生成")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private BlockType platformType;
    [SerializeField] private BlockPalette palette;
    [Tooltip("生成位置：前方距离（米）")]
    [SerializeField] private float forwardDistance = 2.5f;
    [Tooltip("生成位置：向上偏移（米）")]
    [SerializeField] private float upOffset = 0.8f;
    [Tooltip("平台存在时间（秒）")]
    [SerializeField] private float lifetime = 10f;
    [Tooltip("消失前开始闪烁的时间（秒）")]
    [SerializeField] private float blinkTime = 2f;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    public AbilityType Type => AbilityType.Hope;
    public int Cost => 2;

    public bool CanUse(out string reason)
    {
        reason = null;
        return true;
    }

    public void Execute()
    {
        Vector3 spawnPosition = transform.position
                                + transform.forward * forwardDistance
                                + Vector3.up * upOffset;

        GameObject platform = platformPrefab != null
            ? Instantiate(platformPrefab, spawnPosition, Quaternion.identity)
            : CreateFallbackPlatform(spawnPosition);

        _spawned.Add(platform);
        StartCoroutine(LifeCycle(platform));
    }

    private GameObject CreateFallbackPlatform(Vector3 position)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "HopePlatform";
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one;

        Block block = cube.AddComponent<Block>();
        BlockView_Whitebox view = cube.AddComponent<BlockView_Whitebox>();
        view.CollectRenderers();
        block.SetView(view);

        if (platformType != null)
        {
            block.SetType(platformType);
            block.ResetState();
        }

        if (palette != null)
        {
            block.SetPalette(palette);
        }

        return cube;
    }

    private IEnumerator LifeCycle(GameObject platform)
    {
        Renderer[] renderers = platform.GetComponentsInChildren<Renderer>(true);

        float wait = Mathf.Max(0f, lifetime - blinkTime);

        if (wait > 0f)
        {
            yield return new WaitForSeconds(wait);
        }

        float endTime = Time.time + blinkTime;
        bool visible = true;

        while (Time.time < endTime)
        {
            visible = !visible;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = visible;
                }
            }

            yield return new WaitForSeconds(0.15f);
        }

        _spawned.Remove(platform);

        if (platform != null)
        {
            Destroy(platform);
        }
    }
}
