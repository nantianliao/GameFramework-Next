# GameFramework-Next


[![UnityVersion](https://img.shields.io/badge/Unity%20Ver-2019.4.12++-blue.svg?style=flat-square)](https://github.com/nantianliao/GameFramework-Next)
[![License](https://img.shields.io/github/license/nantianliao/GameFramework-Next)](https://github.com/nantianliao/GameFramework-Next)
[![License](https://img.shields.io/github/last-commit/nantianliao/GameFramework-Next)](https://github.com/nantianliao/GameFramework-Next)
[![License](https://img.shields.io/github/issues/nantianliao/GameFramework-Next)](https://github.com/nantianliao/GameFramework-Next)

GameFramework

YooAsset

UniTask

luban

hybridclr

实现初衷：
作为一个商业级成熟的资源框架 YooAsset对资源包的设计和划分会稍微更成熟一些。包括对DLC的支持 以及webgl的支持，资源定位地址的支持等。不管是上steam还是小游戏都更自洽。且GameFramework的资源模块存在一定的设计过度问题。故在此首次把YooAsset接入GF并实现热更新。（HybridCLR热更新流程已经实现）

PS：本Repository改自Alex_Rachel的<a href="https://github.com/Alex-Rachel/GameFramework-Next"><strong>GameFramework-Next</strong></a>，能力有限，可能存在Bug，仅作学习交流使用。


``` 
//程序集划分设计
Assets/GameScripts
├── Editor              // 编辑器程序集
├── HotFix              // 游戏热更程序集目录 [Folder]
|   ├── GameBase        // 游戏基础框架程序集 [Dll]
|   ├── GameProto       // 游戏配置协议程序集 [Dll]  
|   ├── BattleCore      // 游戏核心战斗程序集 [Dll] 
|   └── GameLogic       // 游戏业务逻辑程序集 [Dll]
|           ├── GameApp.cs                  // 热更主入口
|           └── GameApp_RegisterSystem.cs   // 热更主入口注册系统
└── Runtime             // Runtime程序集
```

## <strong>特别鸣谢
#### <a href="https://github.com/tuyoogame/YooAsset"><strong>YooAsset</strong></a> - YooAsset是一套商业级经历百万DAU游戏验证的资源管理系统。

#### <a href="https://github.com/EllanJiang/GameFramework"><strong>GameFramework</strong></a> - Game Framework 是一个基于 Unity 引擎的游戏框架。

#### <a href="https://github.com/Alex-Rachel/TEngine"><strong>TEngine</strong></a> - Unity框架解决方案-支持HybridCLR(最好的次时代热更)、Obfuz混淆代码加固与YooAssets(优秀商业级资源框架)。
