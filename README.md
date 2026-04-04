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

<img width="2560" height="1391" alt="img1" src="https://github.com/user-attachments/assets/7963dd84-fb73-46d5-baa7-bf507a76f472" />

<img width="2560" height="1392" alt="img2" src="https://github.com/user-attachments/assets/8f3c3cda-852e-4016-a1c2-9a3f7d91615f" />

<img width="2560" height="1392" alt="img3" src="https://github.com/user-attachments/assets/5b44ef8b-985b-4629-8c45-295dcb90b881" />

<img width="2560" height="1392" alt="img4" src="https://github.com/user-attachments/assets/4ec0de69-eb69-44ca-8f63-b6e2d8c42d38" />

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
