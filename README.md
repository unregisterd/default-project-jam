# Default Project — Game Jam 协作仓库

Unity **2022.3.62f3c1**（URP 14.0.12）+ 内置 ProjectBase 框架（DOTween、UIManager、EventManager、ObjectPool 等）。
版本控制使用 **Unity Version Control（UVCS / 原 Plastic SCM）**，已在编辑器内集成。

## 1. 首次准备（每人一次）

1. 安装 **Unity 2022.3.62f3c1**（版本必须一致，用 Unity Hub 安装）。
2. 用**被邀请的 Unity ID** 登录 Unity Hub（不是 GitHub 账号）。
3. 美术/程序想用独立客户端时，可另装 **Unity Version Control（Gluon）**，但编辑器内已够用。

## 2. 拉工程

1. Unity Hub → 项目列表右上 `⋯` → **Connect to source control**（或打开工程后 `Window → Version Control`）。
2. 登录 Unity ID → 选择本队组织 → 选择仓库 → 选本地路径（建议 `E:\UNITY\<项目名>`，不要放 OneDrive）。
3. 等待下载完成后，Unity Hub `Add project from disk` 选这个根目录打开。

> 拉下来的是**项目根目录**（和 `Assets/` 平级），不要只取 `Assets`。

## 3. 每天流程

1. `Window → Version Control` 点 **Update**（拉取最新）。
2. 改自己的文件。
3. `Pending Changes` 里只勾自己改的东西，填 Changeset 说明，点 **Check in**。
4. 点 **Push**（或按设置自动推送）。

**说明写法建议**：`feat: 玩家移动与相机跟随`、`fix: 跳跃穿地`、`art: 主角贴图`、`ui: 主菜单`。

**需要独占修改的文件**（主场景、共享 Prefab、ProjectSettings）先 **Checkout** 加锁，改完 Check in 自动解锁，避免和别人同时改。

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

**主场景规则**：美术/第二程序只提交 Prefab、脚本、文本表；主程 Update 后自己拖进 `Main.unity` 再 Check in。任何人不要同时编辑主场景。

## 5. 冲突处理

UVCS 对 Unity YAML 有内置合并，但**同一场景/同一 Prefab 的并发修改仍会产生冲突**。流程：

1. 冲突时先 **Undo changes** 把自己未提交的改动撤回，别硬合。
2. 在群里说明改了哪个文件，由该文件当前负责人处理。
3. 场景/Prefab 冲突无法自动合并时，保留一份原始文件，重做改动。

预防永远比合并便宜：改主场景前先 Checkout；改完立刻 Check in。

## 6. 硬性规则

- **`.meta` 文件必须提交**，Unity 靠它维持引用；不要手动删/改。
- 不提交 `Library/`、`Temp/`、`Logs/`、`obj/`、`UserSettings/`、`*.csproj`、`*.sln`（已在 `ignore.conf`）。
- 不要提交 `Build/` 打包产物。
- 大文件（fbx/png/wav/psd/视频/字体）直接加，UVCS 原生支持，不需要 LFS。
- 项目设置改了（URP、Input、Tag/Layer、Package）要单独 Check in 并在群里说一声。
- `.git` / `.gitignore` / `.gitattributes` 保留仅作 GitHub 备份，日常不要再用 Git 提交。
