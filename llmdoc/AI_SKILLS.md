# AI 如何选择 Unity 技能

AI 会根据需求中描述的目标、Unity 功能和问题现象，自动匹配对应技能。每个技能的 `SKILL.md` 都包含适用场景和操作流程；当任务符合这些场景时，AI 会先读取并按其中的流程执行。

也可以在需求中直接指定技能，例如：`用 ui-ugui skill 做主菜单`。

## 常见任务与技能对应

| 需求 | 使用的技能 |
| --- | --- |
| 创建新 Unity 项目或原型 | `new-unity-project` |
| 控制 Unity Editor、运行测试、管理编辑器版本 | `unity-cli` |
| 制作菜单、HUD、弹窗或通用 UI | `ui`，再根据项目选择 `ui-ugui`、`ui-uitk` 或 `ui-imgui` |
| 编辑 Canvas、RectTransform、Layout Group 或 UI Prefab | `ui-ugui` |
| 制作 UI Toolkit 的 UXML、USS 或运行时 UI | `ui-uitk` |
| 制作编辑器窗口、自定义 Inspector 或 OnGUI 工具 | `ui-imgui` |
| 像素画面模糊、抖动或像素对不齐 | `2d-pixel-perfect` |
| 创建 Tile Palette 或 RuleTile 自动拼接地形 | `tilemap-palette-create`、`tilemap-ruletile-createempty`、`tilemap-ruletile-createfromsegment` |
| 切分 Sprite Sheet、修改 Sprite 边框或 Pivot | `sprite-editor` |
| 创建或优化 SpriteAtlas | `manage-sprite-at las` |
| 配置 NavMesh、寻路、巡逻和避障 | `initialize-ai-navigation` |
| 排查碰撞、Trigger、Raycast 或 MeshCollider 问题 | `physics-3d-collision` |
| 配置 Bloom、色调映射、景深等 URP 后处理 | `urp-postprocessing` |
| 从 Built-in Render Pipeline 迁移至 URP | `migrate-birp-to-urp` |
| 审核 URP Render Graph Renderer Feature | `validate-urp-render-graph-renderer-feature` |
| 优化 WebGL/WebGPU 包大小或加载、卡顿问题 | `optimize-web` |
| 优化音频内存、CPU 或 Mixer 路由 | `optimize-audio`、`audio-setup-mixers` |
| 配置多语言、翻译 UI 或中文字体 | `localization`、`optimize-text-mesh-pro` |
| 配置联机房间、匹配、会话和网络拓扑 | `setup-multiplayer-services` |
| 接入语音/文字聊天 | `setup-vivox-voice-chat` |
| 接入成就、排行榜、存档、Battle Pass 等在线服务 | `build-live-game` |
| 接入内购、订阅或收据校验 | `implement-in-app-purchases` |
| 接入激励视频、插屏或横幅广告 | `levelplay-unity-integration` |
| 管理或安装 Unity Package Manager 包 | `unity-package-management` |
| 使用 Project Auditor 查找并修复项目问题 | `project-auditor-fixes` |

## 推荐的需求写法

需求里尽量包含：

- 想实现的玩法或玩家可见效果
- 目标平台，例如 Windows、iOS、Android、WebGL
- 当前 Unity 版本和渲染管线（Built-in、URP、HDRP）
- 现有报错、表现异常或复现步骤
- 是否要修改现有资源、场景或 Prefab

示例：

> 在 URP 项目中给主菜单制作一个自适应分辨率的开始、设置、退出界面，使用现有的 uGUI Canvas。

这会匹配 `ui-ugui`。
