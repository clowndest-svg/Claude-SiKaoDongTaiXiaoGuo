# v1.3.0 实际客户端验证

日期：2026-10-31（UTC+8）。环境：Windows x64，Microsoft Store Claude Desktop 4.4212.0.0，Claude-3p。使用 computer-use 操作 Claude 原生控件，并对照已安装小克娘的 `runtime/display-state.json`。本文件不包含真实聊天 ID、正文或本机路径。

| 检查 | 实际结果 |
| --- | --- |
| 已发送 Cowork 会话，原生 Low | `mode=cowork`，`effort=low`，`form=chibi` |
| 同一会话切换原生 Max | `effort=max`，`form=max` |
| 打开未发送的新 Cowork 聊天，Max | `sessionId=null`，仍连接，`effort=max` |
| 新聊天切换原生 Low | `sessionId=null`，`effort=low`，`form=chibi` |
| 从新聊天返回已有 Cowork 会话 | 重新识别真实会话 ID，保持跟随原生 Low |
| Cowork 切换已有 Code 聊天 | `mode=code`，`effort=xhigh`，`form=xhigh`，来源为明确聚焦会话元数据 |
| Code 切回 Cowork | `mode=cowork`，`effort=low`，`form=chibi`，来源为原生界面 |
| 桌面视觉检查 | 小克娘在 Low 与 Max 对应不同形态，底部设置默认隐藏 |

本次使用已有真实 Cowork 聊天及未发送新聊天验证跟随，不额外发送模型请求。自动验证另覆盖 21 项聚焦/只读/路径与档位解析检查、六张素材哈希、六阶三时刻动画渲染。未知版本、其他语言的选择器标签、多窗口和多账户场景不在本次验证范围；UIA 数据不可读或过期时暂停跟随。
