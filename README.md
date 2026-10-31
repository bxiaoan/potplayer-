<div align="center">

# PotPlayer 面板风皮肤

**Panel Skin for PotPlayer** — 圆角卡片 · 柔和投影 · macOS 浅色/深色 · Win11 DWM 圆角窗

[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6)](#)
[![PotPlayer](https://img.shields.io/badge/PotPlayer-1.7%2B-orange)](#)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

<img src="预览/preview-mac-light.png" width="880">

</div>

---

## 这是什么

一套 PotPlayer 皮肤，不是套图，而是**把官方默认皮肤的 21 张精灵图集逐单元格重绘**：

- 每个按钮从"直角色块"重新画成**圆角卡片**，图标原样分离后叠回原位
- 卡片带**真实柔和投影**（对 alpha 通道做三次盒式模糊）
- 强调色按 **HSL 等比换算**，抗锯齿边缘不会出灰边
- 布局与图标几何 **100% 沿用官方**，所以不会错位、不会变形、不会点不到

## 版本一览

| 皮肤 | 风格 | 窗口外框 |
|---|---|---|
| **面板风-1-macOS浅色-圆角窗** | macOS 浅色 | DWM 圆角窗 + 系统投影 |
| **面板风-2-macOS深色-圆角窗** | macOS 深色 | DWM 圆角窗 + 系统投影 |
| 面板风-3-蓝灰-圆角窗 | 蓝灰面板 | DWM 圆角窗 |
| 面板风-4-macOS浅色 | macOS 浅色 | 自绘直角边框 |
| 面板风-5-macOS深色 | macOS 深色 | 自绘直角边框 |
| 面板风-6-蓝灰 | 蓝灰面板 | 自绘直角边框 |
| 面板风-7-浅灰 | 浅灰面板 | 自绘直角边框 |
| 面板风-8-初版 | 最早直角面板 | 自绘直角边框 |

<img src="预览/preview-mac-light-round.png" width="880">

## 快速开始

**安装**：完全退出 PotPlayer（含托盘图标）→ 双击 `install.bat` → UAC 点「是」→ 重开播放器

**切换**：右键播放器 → 皮肤 → 选 `%面板风-`% 开头的任意一条

**卸载**：双击 `uninstall.bat`，只删本包的皮肤文件，不动官方皮肤和设置

<details>
<summary>手动安装</summary>

把 `皮肤/` 目录里的 `.dsf` 复制到：

    C:\Program Files\DAUM\PotPlayer\Skins\

重启 PotPlayer 即可。
</details>

## 细节

| 元素 | 处理 |
|---|---|
| 底部播放按钮 | 41×40 单元格内缩 4px → 33×32 圆角方块，圆角 12px，带投影 |
| 进度行 / 控制条 | 整条重绘为浮动大卡片，圆角 10px，带投影 |
| 进度条 / 音量条 | 胶囊形 |
| 播放列表 / 控制面板按钮 | 全部圆角卡片 |
| 滚动条 | 轨道 = 列表底色（隐形），只留滑块 |

<img src="预览/scrollbar.png" width="420">

## 自定义配色

`recolor.ps1` 可以从源码重新生成任意配色：

    # macOS 浅色 + 玫红强调
    .\recolor.ps1 -Theme mac-light -Hue 350 -Name 面板风-玫红

    # macOS 深色 + 紫色，圆角窗
    .\recolor.ps1 -Theme mac-dark -Hue 272 -Frame round -Name 面板风-紫

| 参数 | 作用 | 默认 |
|---|---|---|
| `-Theme` | `@mac-light` / `@mac-dark` / `@light` / `@dark` | `@mac-light` |
| `-Hue` | 强调色色相 | `211` |
| `-Frame` | `@square` / `@round` | `@square` |
| `-Sat` / `-Lum` | 强调色饱和度 / 明度系数 | `1.00` / `1.02` |
| `-TintR` `-TintG` `-TintB` | 面板整体冷暖微调 | `0` |

色相参考：`150` 绿 / `172` 青绿 / `188` 青 / `200` 天蓝 / `211` 系统蓝 / `230` 靛蓝 / `272` 紫 / `300` 品红 / `330` 粉 / `355` 红 / `30` 橙

---

## 技术实现

### 皮肤格式

`.dsf` 是标准 ZIP，内部结构：

    VideoSkin.xml         入口，按 SkinType/SkinName 条件包含
    Common.txt            滚动条、菜单、弹窗定义
    Top.txt / BottomVideo.txt / SubCommon.txt …   各区域布局
    Main.png / Control.png / PlayList.png …       精灵图集

布局通过三个机制引用图集：

- `ImageLeft/ImageTop/ImageWidth/ImageHeight` — 从图集**抠单元格**（可横向拉伸）
- `LeftMargin/RightMargin/TopMargin/BottomMargin` — 拉伸时的边缘保留区
- `ColorX/ColorY` — 直接**从图集像素取色**当文字颜色

### 圆角卡片化

`MakeCard()` 的四步：

1. 统计单元格**底色**（出现最多的颜色）
2. 把偏离底色的像素判定为**图标**并抽成独立图层，顺带剔除边缘 1px 分隔线
3. 用 GDI+ 抗锯齿画**圆角卡片**（填充 + 1px 柔和描边 + 投影）
4. 把图标图层**按原位叠回**

这样按钮的"正常/悬停/按下/禁用"四态各自保留原有明暗关系，只是全部变成圆角。

### 柔和投影

GDI+ 没有发光效果，所以自己实现：

1. 按卡片形状画实心圆角矩形
2. 只对 **alpha 通道**做 **3 次盒式模糊**（`BoxH` / `BoxV`）
3. 按主题透明度着色，叠在卡片下方

前提是卡片在单元格里**内缩绘制**（`CardInset = 4`），四周留出投影空间。

### 圆角窗口

官方 `WindowFrame.dsf` 和 `Default.dsf` 的 21 张图片**逐字节相同**，只有 8 个布局文件不同，关键就一处：

    Default.dsf      <MainSkinArea … UseWindowFrame='0'>   皮肤自绘边框（只能直角）
    WindowFrame.dsf  <MainSkinArea … UseWindowFrame='3'>   交给 DWM（Win11 给 8px 圆角 + 投影）

### 踩过的坑

| 现象 | 根因 |
|---|---|
| 滚动条整条纯黑 | GDI+ 存 BMP 时 `biSizeImage` 留 0，PotPlayer 解析器不认 → 补成非零 |
| 播放列表标签页黑底 | 圆角把角落变透明，而该区域背后无底色 → 先铺底色再画卡片 |
| 浅色主题下页面图集整片变黑 | 白底图集被明暗反转 → 单独加白名单，只换强调色不反转 |
| 滚动条滑块看不见 | 源图里滑块和按钮格同色且只差 8 级灰 → 改为按单元格**区域**绘制 |

---

## 项目结构

    PotPlayer面板风皮肤/
    ├── install.bat / install.ps1        一键安装
    ├── uninstall.bat / uninstall.ps1    一键卸载
    ├── recolor.ps1                      自定义配色
    ├── 说明.md                          详细使用说明
    ├── 皮肤/                            ★ 成品 .dsf
    ├── 预览/                            效果截图
    └── 源码/
        ├── build/
        │   ├── PanelSkin.cs             配色 + 圆角卡片 + 投影 + 打包
        │   ├── Preview.cs               预览图生成器
        │   ├── rebuild-all.ps1          一键重建全部皮肤
        │   └── extract-sources.ps1      从本机 PotPlayer 提取官方素材
        ├── src/                         官方 Default.dsf 解包素材（构建输入）
        └── src_wf/                      同上，改用 WindowFrame 布局

## 从源码构建

仓库**不包含**官方原始素材（版权原因），首次构建前先提取：

    .\源码\build\extract-sources.ps1      # 从本机已安装的 PotPlayer 提取
    .\源码\build\rebuild-all.ps1          # 重建全部 8 个皮肤

依赖：Windows PowerShell 5.1+ / .NET Framework（`System.Drawing`），无需额外安装。

## 已知限制

- **窗口圆角需要 Windows 11**：`UseWindowFrame='3'` 依赖 DWM，Win10 上会退化为直角
- **圆角半径不可调**：由系统统一控制
- **窗口外框无法用皮肤做圆角**：官方格式里外框只有 4 条可拉伸细条，没有"角"的单元格
- 圆角与投影只做在 **1:1 绘制**的单元格上，横向拉伸的面板条只能做沿拉伸方向恒定的改动

## 素材来源与许可

**代码**（`PanelSkin.cs` / `Preview.cs` / 各 `*.ps1`）采用 [MIT License](LICENSE)。

**皮肤素材**基于 PotPlayer 官方默认皮肤重绘，原始素材版权归 **DAUM / Kakao** 及原作者所有。本仓库**不再分发官方原始素材**（`src` / `src_wf` / 参考`/` 已加入 `.gitignore`），构建时由 `extract-sources.ps1` 从用户本机的 PotPlayer 提取。

成品 `.dsf` 仅供**个人学习与使用**，请勿用于商业用途。如需商业使用，请自行确认授权。
