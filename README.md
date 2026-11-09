# magic&miracle 鼠标特效

给 Windows 鼠标加上主题光标和交互动画。可以单独调整指针造型、配色、拖尾和点击反馈。

**[下载 Windows 安装程序](https://github.com/3088302316/magic-miracle/releases/latest)** · Windows x64

## 安装与使用

1. 在发布页下载 `magic-miracle-setup.exe`，双击并选择安装位置。
2. 从开始菜单打开软件。桌面快捷方式和开机自启可在安装时选择。
3. 在「外观装扮」里换造型或配色，在「性能调节」里调整拖尾和动画刷新。
4. 按 **Ctrl + Alt + M** 暂停或恢复特效。关闭设置窗口后仍会在托盘运行，右击托盘图标可退出。

包含蝶翼、焰羽、青叶、星糖、闪电、云旋六种造型，以及拖尾、点击、滚轮和拖动反馈。应用隐藏系统光标时，特效会随之隐藏。

## 从源码构建

项目使用 C# 和 Windows Forms。使用 Windows 自带的 .NET Framework x64 C# 编译器，在项目目录运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

主程序在 `dist/`，单文件安装器在 `发布/`。光标素材和设计参考在 [assets](assets/)，设置与操作细节见 [使用说明](magic%26miracle-说明.txt)。
