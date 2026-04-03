# 多人五子棋游戏♟️

开发工具💻：ASP.NET.CORE8 C# Quasar yarn Vue3 Server Management Studio 20

## 概述📋

一个在线可聊天多人版五子棋游戏

## 操作指南🕹️

- 创建新房间
> 创建一个新的游戏房间，其他人可根据房号加入游戏。
- 加入房间
> 加入一个游戏房间，可以和其他人一起游玩

玩法和五子棋一样，只不过多了几个人而已，可以聊天。

## 成果展示🖼️

<img width="2560" height="1392" alt="img1" src="https://github.com/user-attachments/assets/9c1ea057-856a-40ea-9fc0-ea8506b79e8f" />

<img width="2560" height="1392" alt="img2" src="https://github.com/user-attachments/assets/2379806c-0f1b-46cd-8579-22acd59b130c" />

<img width="2560" height="1392" alt="img3" src="https://github.com/user-attachments/assets/0cadb2d1-4121-4a0a-9d7b-c43c76f93ca3" />

<img width="2560" height="1392" alt="img4" src="https://github.com/user-attachments/assets/c656e74a-9776-4143-b3a0-76ef2cffe1ca" />


## 环境搭建🏗️

> 注意⚠️ 需要安装SQL Server Management Studio 20 或其他SQL Server

在`appsettings.json`文件中需要在`conn`中填入你的SQL Server的用户数据

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "conn": ""
  }
}
```

quasar启动方式
`yarn quasar dev`
