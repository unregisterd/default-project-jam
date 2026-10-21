# Default Project — Game Jam 协作仓库

Unity **2022.3.62f3c1**（URP 14.0.12）+ 内置 ProjectBase 框架（DOTween、UIManager、EventManager、ObjectPool 等）。

## 1. 首次准备（每人一次）

1. 安装 **Unity 2022.3.62f3c1**（版本必须一致，用 Unity Hub 安装）。
2. 安装 **Git** 和 **Git LFS**：<https://git-lfs.com>
3. 打开终端执行一次：

   ```bash
   git lfs install
   ```

   不装 LFS 会出现 `smudge filter lfs failed` 或图片/模型变成几十字节的指针文件。

## 2. Clone

- **GitHub Desktop**：`File → Clone repository` → 选本仓库 → 本地路径建议 `E:\UNITY\Default Project`。
- **命令行**：

  ```bash
  git clone <仓库地址> "E:\UNITY\Default Project"
  ```

> Clone 的是**项目根目录**（和 `Assets/` 平级），不要只 clone `Assets`。
> 打开工程时用 Unity Hub `Add project from disk` 选这个根目录。

## 3. 每天流程

```bash
git pull            # 1. 先拉最新
# 2. 改自己的文件
git add .           # 3. 暂存
git commit -m "feat: 玩家移动与相机跟随"   # 4. 写清楚改了什么
git push            # 5. 推上去
```

GitHub Desktop 对应：`Pull origin` → 改文件 → 左下角填 Summary → `Commit to main` → `Push origin`。

**提交信息建议**：`feat:` 新功能 / `fix:` 修 bug / `art:` 美术资源 / `ui:` 界面 / `chore:` 杂项。

## 4. 目录约定（避免互相覆盖）

| 目录 | 负责人 | 说明 |
| --- | --- | --- |
| `Assets/Scenes/Main.unity` | **仅主程** | 唯一的整合场景，别人不要改 |
| `Assets/Scripts/` | 程序 | 按模块建子目录，如 `Player/`、`UI/`、`System/` |
| `Assets/Prefabs/` | 各自 | 做完的 Prefab 放这里，主程负责拖进 Main |
| `Assets/Art/` | 美术 | 贴图、模型、材质 |
| `Assets/Audio/` | 美术 / 音效 | BGM、SFX |
| `Assets/UI/` | UI / 美术 | 界面图、Sprite |
| `Assets/Text/` | 策划 | 文本表（json/csv/ScriptableObject） |
| `Assets/AssetsTemplate/` | 框架 | ProjectBase 框架与 DOTween，**不要改** |

**主场景规则**：美术/第二程序只提交 Prefab、脚本、文本表；主程 Pull 后自己拖进 `Main.unity` 再提交。任何人不要同时编辑主场景，否则 `.unity` 冲突极难合。

## 5. 合并冲突处理（可选，推荐）

`.gitattributes` 已给 Unity YAML 资产配置了 SmartMerge。每人执行一次（路径按自己的 Unity 安装位置改）：

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'E:/UNITY/2022.3.62f3c1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p --force --fallback none %O %B %A %A"
```

仍然是冲突高发区：`Main.unity`、`ProjectSettings/*`、同一个 Prefab。出现冲突先找主程，不要瞎点“Use mine”。

## 6. 硬性规则

- **`.meta` 文件必须提交**，Unity 靠它维持引用；不要手动删/改。
- 不提交 `Library/`、`Temp/`、`Logs/`、`obj/`、`UserSettings/`、`*.csproj`、`*.sln`（已在 `.gitignore`）。
- 不要提交 `Build/` 打包产物。
- 大文件（fbx/png/wav/psd/视频/字体）由 LFS 自动接管，正常 `git add` 即可。
- 项目设置改了（URP、Input、Tag/Layer、Package）要单独 commit 并在群里说一声。
