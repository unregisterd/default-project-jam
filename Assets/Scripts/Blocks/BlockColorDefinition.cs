using System;
using UnityEngine;

[Serializable]
public class BlockColorDefinition
{
    [Tooltip("程序内使用的唯一 ID，区分大小写不敏感")]
    public string id = "Red";
    [Tooltip("显示名（HUD / 编辑器提示）")]
    public string displayName = "Red";
    [Tooltip("HUD 与编辑器里显示的颜色")]
    public Color uiColor = Color.red;
    [Tooltip("方块使用该颜色时替换的材质")]
    public Material material;
}
