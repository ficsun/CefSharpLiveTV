# CefSharpLiveTV
基于CefSharp的网络电视直播软件

项目基于Visual Studio 2015

键盘操作说明：

Keys.Left：频道-1

Keys.Right：频道+1

Keys.Enter：全屏切换

Keys.F4：关闭软件

使用HEX Editer修改pepflashplayer64_32_0_0_101.dll中的字符串“COMSPEC”为"COM8PEC"，“cmd.exe”为“c8d.exe”，即可解决CefSharp加载Flash插件时弹出命令行窗口现象。
