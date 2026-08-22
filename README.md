# ChineseGold

骑砍 2（Mount & Blade II: Bannerlord）金币中文数字显示 Mod。

## 目标

只改变游戏 UI 中金币数字的显示形式，不修改实际金币数据、交易结算或经济系统。

当前格式规则：

```text
0 ～ 9,999
    9,999

10,000 ～ 99,999,999
    1万
    1.2万
    12.35万
    1,000万

100,000,000+
    1亿
    12.35亿
```

最多保留两位小数，末尾零自动省略。

## 当前已处理的原版入口

基于 `Qika2-Source-Decompiled` 反编译源码确认：

- `MapInfoVM.UpdatePlayerInfo`
  - 地图顶部玩家金币。
- `MissionConversationVM.Refresh`
  - 对话界面玩家金币。
- `BarterItemVM.CurrentOfferedAmountText`
  - 只处理 `GoldBarterGroup` 的金币出价。
- `BarterItemVM.TotalItemCountText`
  - 只处理 `GoldBarterGroup` 的金币数量。

没有全局 Patch `CampaignUIHelper.GetAbbreviatedValueTextFromValue(int)`，避免把部队人数、物品数量等非金币数值一起改掉。

## 尚未处理

以下 ViewModel 属性本身是 `int`，同时承担数据与 UI 绑定职责，因此不能简单替换为字符串：

- `SPInventoryVM.RightInventoryOwnerGold`
- `SPInventoryVM.LeftInventoryOwnerGold`
- `BarterVM.RightMaxGold`
- `BarterVM.LeftMaxGold`

这些位置下一阶段应针对对应 Gauntlet 控件的最终文本绑定继续处理。

## 工程结构

工程参考 [BUTR/Bannerlord.Module.Template](https://github.com/BUTR/Bannerlord.Module.Template) 的 SDK 模式：

```text
ChineseGold/
├── .editorconfig
├── .gitignore
├── LICENSE
├── README.md
└── src/
    ├── ChineseGold.csproj
    ├── ChineseGoldFormatter.cs
    ├── GoldDisplayPatches.cs
    ├── SubModule.cs
    └── _Module/
        └── SubModule.xml
```

项目使用 `Bannerlord.BUTRModule.Sdk`，`_Module` 是模块输出根目录；`SubModule.xml` 使用 SDK 的 `$moduleid$`、`$modulename$`、`$version$`、`$gameversion$` 等构建变量。该模式来自 BUTR 模板。citeturn60file0turn63file0

## 构建

推荐设置环境变量：

```text
BANNERLORD_GAME_DIR=C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord
```

然后：

```powershell
dotnet restore
dotnet build src\ChineseGold.csproj -c Release
```

模板本身也推荐使用 `BANNERLORD_GAME_DIR` 管理游戏路径，而不是把路径硬编码进项目。citeturn64file0

构建输出由 `Bannerlord.BUTRModule.Sdk` 负责按照模块结构处理；不需要手工维护一套自定义 DLL 拷贝脚本。

## 依赖

- Native
- SandBoxCore
- Sandbox
- StoryMode
- CustomBattle
- Bannerlord.Harmony 2.2.2

Harmony 是硬依赖，避免 Mod 自带一份独立 Harmony DLL。
