# 崩坏3扫码器[BOT] / bh3_login_simulation
代码借鉴自@Haocen2004的[安卓项目](https://github.com/Haocen2004/bh3_login_simulation)以及@cc004的[Bot项目](https://github.com/cc004/pcrjjc2), 感谢两位大佬的付出

## 说明
- 渠道服限制: B服
- Bug齁多, 遇到奇怪的问题请向我反馈(;´Д`)

## 进度
[2021.9.23] 崩坏三5.2.0版本恢复了手机扫码，插件测试功能通过

## 配置
在数据目录下新建一个名为`Config.ini`的文件, 并复制下述文本, `Version`字段内写的是当前崩坏三的版本号, 请以游戏为准
```ini
[Config]
Version=5.2.0
```

## 指令说明
> 所有的指令均可自定义, 下述说明的指令均为默认指令

```csharp
#扫码帮助: 获取扫码机的帮助文档
#扫码登记: 进行哔哩哔哩账号与QQ的绑定
#扫码删除 已绑定账号: 取消哔哩哔哩账号与QQ的绑定
#扫码登录 [可选参数: 绑定序号/账号]: 进行扫码过程, 单一绑定无需附加参数, 若有多个账号, 可在指令后指定序号或者账号
#扫码验证: 登录过程中需要验证码但未获取成功或验证失败, 输入此指令重新获取
```

## 修改指令
在配置文件`Config.ini`内, 修改这些对应的字段即可
```csharp
#扫码帮助: Help
#扫码登记: SetAccount
#扫码删除: RemoveAccount
#扫码登录: QRCodeScan
#扫码验证: CaptchaVerify
```
示例: 更改帮助文本的指令
1. 打开配置文件`Config.ini`
2. 寻找`Help`字段, 若没有手动加上
3. 更改后的文件如下所示
```ini
[Config]
Version=5.2.0
Help=#扫码机帮助
```
## 效果展示
![image](https://user-images.githubusercontent.com/50934714/134467814-62e4a775-4833-4bfc-96e5-aeecf8ef6d4e.png)
