# HardwareMonitor - Windows 硬件监控工具

一款轻量级的 Windows 10/11 硬件监控工具，常驻系统托盘，实时显示硬件状态。

## 功能

### 实时监控
- CPU 占用率 / 温度
- GPU 占用率 / 温度 / 显存
- 内存占用
- 上传 / 下载网速
- 硬盘占用 / 温度
- 风扇转速
- 电池信息（电量、健康度、充电状态）

### 界面特性
- 仪表盘卡片式显示，支持拖动排序
- 悬浮窗 / 紧凑模式
- 托盘图标动态显示温度或电量
- 任务栏标题实时显示硬件数据

### 系统功能
- 开机自启动
- 最小化到托盘
- 温度过高报警
- 风扇控制（自动 / 手动 / 静音 / 平衡 / 性能 / 自定义曲线）

## 快速开始

### 方式一：下载 Release
从 [Releases](../../releases) 页面下载 `HardwareMonitor.exe`，双击运行。

### 方式二：自行编译

**环境要求：**
- .NET 6.0 SDK
- Windows 10/11

```bash
# 克隆仓库
git clone https://github.com/fordtddf/xianshi.git
cd xianshi/HardwareMonitor

# 编译运行
dotnet run -p src/HardwareMonitor

# 或发布为独立 exe
dotnet publish src/HardwareMonitor -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 权限说明

| 操作 | 权限 | 说明 |
|------|------|------|
| CPU/GPU/内存监控 | 普通用户 | LibreHardwareMonitorLib 驱动读取 |
| 温度/风扇读取 | 管理员 | 需要管理员权限访问传感器 |
| 风扇控制 | 管理员 | EC/ACPI 写入需要 Ring 0 权限 |
| 网速监控 | 普通用户 | Performance Counter |
| 电池信息 | 普通用户 | WMI 查询 |

**建议以管理员身份运行以获取完整的硬件数据。**

## 技术栈

- C# + .NET 6.0 + WPF
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - 硬件数据采集
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/MVVM) - MVVM 框架
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) - 系统托盘

## 项目结构

```
HardwareMonitor/
├── src/HardwareMonitor/
│   ├── Core/              # 核心层（模型、常量、转换器）
│   ├── Services/          # 服务层（硬件监控、风扇控制、托盘等）
│   ├── ViewModels/        # MVVM 视图模型
│   └── Views/             # WPF 界面
```

## 许可证

MIT License
