# Default Project — Game Jam 协作仓库

Unity **2022.3.62f3c1**（URP 14.0.12）+ 内置 ProjectBase 框架（DOTween、UIManager、EventManager、ObjectPool 等）。
版本控制使用 **Unity Version Control（UVCS / 原 Plastic SCM）**，已集成在编辑器内。

仓库：`Default Project GameJam@tj_lyker05@cloud.cn`（组织 `tj_lyker05`，分支 `main`）。

## 1. 首次准备（每人一次）

1. 安装 **Unity 2022.3.62f3c1**（版本必须一致，用 Unity Hub 安装）。
2. 用**被邀请的 Unity ID** 登录 Unity Hub（不是 GitHub 账号）。
3. 美术同学如果不想开 Unity，可另装 **Unity Version Control（Gluon）** 客户端。

## 2. 拉工程

1. 打开 Unity（任意工程或本项目）→ `Window → Version Control`
2. `Sign in`（被邀请的 Unity ID）
3. 工作区下拉框 → **创建新的工作区 / Create workspace**
4. 弹窗填写：
   - **存储库**：`Default Project GameJam`
   - **工作区名称**：自己取，建议 `名字-main`（不需要和主程同名）
   - 模式选 `I need branching, merging, and the ability to push/pull (Unity VCS workspace)`
   - **路径**：一个空的本地目录（建议 `E:\UNITY\<项目名>`，不要放 OneDrive）
5. `Create workspace` 等待下载 → Unity Hub `Add project from disk` 选该目录打开

> 拉下来的是**项目根目录**（和 `Assets/` 平级），不要只取 `Assets`。
> 工作区是本机的，每人各建一个；同名的是“存储库”，不是工作区。

## 3. 每天流程

1. `Window → Version Control` 点 **更新 / Update**（先拉最新）
2. 改自己的文件
3. `Changes` 里只勾自己改的东西，填 Changeset 说明，点 **签入 / Check in**
4. 需要独占的文件先 **Checkout** 加锁，签入后自动解锁

**说明写法建议**：`feat: 玩家移动与相机跟随`、`fix: 跳跃穿地`、`art: 主角贴图`、`ui: 主菜单`。

## 4. 目录约定

| 目录 | 用途 | 说明 |
| --- | --- | --- |
| `Assets/Scenes/Main.unity` | 主场景 | **仅主程**修改，唯一的整合场景 |
| `Assets/Scenes/Sandbox/` | 沙盒 | 每人自己的测试场景 |
| `Assets/Scripts/` | 程序 | 按模块建子目录，如 `Player/`、`UI/`、`System/` |
| `Assets/Prefabs/` | 交付 | 做完的 Prefab 放这里，主程拖进 Main |
| `Assets/Art/` | 美术 | 贴图、模型、材质 |
| `Assets/Audio/` | 音效 | BGM、SFX |
| `Assets/UI/` | UI | 界面图、Sprite、字体 |
| `Assets/Text/` | 策划 | 文本表（json/csv/ScriptableObject） |
| `Assets/AssetsTemplate/` | 框架 | ProjectBase 框架与 DOTween，**不要改** |

## 5. 测试场景规范（重要）

每个人在自己的沙盒场景里干活，成果通过 Prefab 交付，`Main.unity` 只由主程合并。

- **美术**：在 `Assets/Scenes/Sandbox/` 建 `Sandbox_Art_名字`，随便调模型/材质/灯光；满意后做成 Prefab 放 `Assets/Prefabs/`，签入。
- **程序**：同样建 `Dev_模块名`，在里面跑通逻辑，交付 Prefab 或独立脚本。
- 需要在主场景环境下测试：右键 `Main.unity` → `Open Scene Additive`，**不要保存对 Main 的修改**（保存前确认 Active Scene 不是 Main）。
- 所有 `Sandbox_*` / `Dev_*` 场景**不要加入 Build Settings**（当前只有 `Main.unity` 启用；别按 `Add Open Scenes`，它会把当前打开的沙盒场景加进去）。
- 交付 Prefab 时连带依赖一起签入：右键 Prefab → `Select Dependencies`，确认材质/贴图/Shader 都在，否则队友拉下来是粉色。
- 沙盒场景可以入库（防本地丢失），但只改自己名字的那个。

## 6. 冲突处理

UVCS 对 Unity YAML 有内置合并，但**同一场景/同一 Prefab 的并发修改仍会产生冲突**：

1. 冲突时先 **撤销更改 / Undo changes** 撤回自己未签入的改动，别硬合。
2. 在群里说明改了哪个文件，由该文件当前负责人处理。
3. 场景/Prefab 无法自动合并时，保留原始文件，重做改动。

预防永远比合并便宜：改主场景前先 Checkout，改完立刻签入。

## 7. 硬性规则

- **`.meta` 文件必须提交**，Unity 靠它维持引用；不要手动删/改。
- 不提交 `Library/`、`Temp/`、`Logs/`、`obj/`、`UserSettings/`、`*.csproj`、`*.sln`（已在 `ignore.conf`）。
- 不提交 `Build/` 打包产物。
- 大文件（fbx/png/wav/psd/视频/字体）直接加，UVCS 原生支持，不需要 LFS。
- 项目设置（URP、Input、Tag/Layer、Package）改动单独签入并在群里说一声。
- `.git` / `.gitignore` / `.gitattributes` 仅作 GitHub 备份保留，日常不要再用 Git 提交。
