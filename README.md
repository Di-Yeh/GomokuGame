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

> 后端项目使用命令行发布 `dotnet publish -c Release -o ./publish`

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

**局域网联机配置**🛜

第一步：获取主机的局域网 IP

> 在自己的主机（运行代码的那台）上：
> 按下 `Win + R`，输入 `cmd` 回车。
> 
> 输入 `ipconfig`。
> 
> 找到 IPv4 地址，通常是 192.168.1.XX 或 192.168.0.XX。
> 
> 假设主机的 IP 是：192.168.1.5 (以下步骤请替换为主机实际查到的 IP)。

第二步：后端配置 (ASP.NET Core)

> 修改 `launchSettings.json`
>
> 确保监听地址包含 0.0.0.0，这样它才会接收来自外部 IP 的请求。
>
> ```json
> "applicationUrl": "https://0.0.0.0:7239;http://0.0.0.0:5203",
> ```
>
> 修改 Program.cs (CORS 跨域配置)
>
> ```csharp
> builder.Services.AddCors(options =>
> {
>     options.AddPolicy("AllowAll", policy =>
>     {
>         policy.SetIsOriginAllowed(_ => true) // 允许局域网内任何设备连接
>               .AllowAnyHeader()
>               .AllowAnyMethod()
>               .AllowCredentials(); // SignalR 必须
>     });
> });
> 
> var app = builder.Build();
> app.UseCors("AllowAll"); // 确保在 UseRouting 之后，UseEndpoints 之前
> ```

第三步：前端配置 (Vue 3 + SignalR)

> 修改 SignalR 连接地址
>
> 在 `initSignalR` 函数里，把 `localhost` 换成你刚才查到的主机 IP。
>
> ```javascript
> connection = new signalR.HubConnectionBuilder()
>    // 必须用 IP 地址，不能用 localhost
>    .withUrl('https://192.168.1.5:5203/gomokuHub') 
>    .withAutomaticReconnect()
>    .build();
> ```

第四步：Windows 防火墙问题（可选）

> 如果前面三步完成后就可以访问网站的话就不需要第四步了
>
> 1. 打开控制面板 -> 系统和安全 -> Windows Defender 防火墙。
> 2. 点击 "高级设置"。
> 3. 点击 "入站规则" -> "新建规则"。
> 4. 选择 "端口" -> TCP。
> 5. 在特定本地端口输入：7239, 5203, 5173。
> 6. 一路点击 "下一步"，确保选中了 "允许连接"。
> 7. 给规则起名：`GomokuGame-LAN`。

第五步：如何访问

> 启动后端：在 Visual Studio 里运行项目。
>
> 启动前端：在终端运行 `yarn quasar dev`。
>
> 确保要访问的电脑/手机是在同一个网域，在浏览器输入`http://192.168.1.5:9000/`就可以访问网站了。
