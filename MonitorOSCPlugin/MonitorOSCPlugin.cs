namespace Loupedeck.MonitorOSCPlugin
{
    using System;
    using System.Text;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using WebSocketSharp;
    using Loupedeck;
    using System.Timers;

    public class MonitorOSCPlugin : Plugin
    {
        // === Loupedeck必需配置 ===
        public override bool UsesApplicationApiOnly => true;
        public override bool HasNoApplication => true;

        // === 单例实例 ===
        public static MonitorOSCPlugin Instance { get; private set; }

        // === WebSocket配置 ===
        private const string WS_SERVER = "ws://localhost:9122";
        private WebSocket _wsClient;
        private bool _isReconnecting = false;
        private System.Timers.Timer _reconnectTimer;
        private bool _isManuallyClosed = false;
        private const int RECONNECT_DELAY_MS = 5000;

        // === 插件初始化 ===
        public MonitorOSCPlugin()
        {
            Instance = this;
            PluginLog.Init(this.Log);
            PluginResources.Init(this.Assembly);
            this.InitializeWebSocket();
        }

        // === WebSocket连接管理 ===
        private void InitializeWebSocket()
        {
            PluginLog.Info("WebSocket: 尝试初始化并连接...");
            this._isManuallyClosed = false;

            if (this._wsClient != null)
            {
                PluginLog.Info("WebSocket: 清理现有WebSocket客户端以便重新初始化。");
                this._wsClient.OnOpen -= this.OnWebSocketOpen;
                this._wsClient.OnMessage -= this.OnWebSocketMessage;
                this._wsClient.OnClose -= this.OnWebSocketClose;
                this._wsClient.OnError -= this.OnWebSocketError;

                if (this._wsClient.IsAlive)
                {
                    try
                    {
                        this._wsClient.Close(CloseStatusCode.Normal, "重新初始化WebSocket");
                        PluginLog.Info("WebSocket: 旧客户端在重新初始化过程中已关闭。");
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"WebSocket: 重新初始化时关闭旧客户端异常: {ex.Message}");
                    }
                }
                this._wsClient = null;
                PluginLog.Info("WebSocket: 旧客户端实例已置空。");
            }

            try
            {
                this._wsClient = new WebSocket(WS_SERVER);
                PluginLog.Info($"WebSocket: 为 {WS_SERVER} 创建新客户端。");

                this._wsClient.OnOpen += this.OnWebSocketOpen;
                this._wsClient.OnMessage += this.OnWebSocketMessage;
                this._wsClient.OnClose += this.OnWebSocketClose;
                this._wsClient.OnError += this.OnWebSocketError;
                PluginLog.Info("WebSocket: 事件处理器已订阅。");

                this._wsClient.Connect();
                PluginLog.Info("WebSocket: 连接尝试已启动 (Connect() 已调用)。");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"WebSocket: 在InitializeWebSocket中创建新WebSocket客户端或调用Connect()时异常: {ex.Message}");
                this.ScheduleDelayedReconnect();
            }
        }

        private void OnWebSocketOpen(object sender, EventArgs e)
        {
            PluginLog.Info("WebSocket: 连接成功打开。");
            this._isReconnecting = false;
            if (this._reconnectTimer != null)
            {
                this._reconnectTimer.Stop();
                PluginLog.Info("WebSocket: 由于连接成功，重连定时器已停止。");
            }
        }

        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            // 收到服务端推送的 OSC 二进制消息
            if (e.IsBinary)
            {
                var (address, value) = this.ParseOSCMessage(e.RawData);
                if (!string.IsNullOrEmpty(address))
                {
                    // 更新缓存并通知所有订阅者
                    OSCStateManager.Instance.UpdateState(address, value);
                }
            }
        }

        private void OnWebSocketClose(object sender, CloseEventArgs e)
        {
            PluginLog.Warning($"WebSocket: 连接已关闭。WasClean: {e.WasClean}, Code: {e.Code} ({((CloseStatusCode)e.Code).ToString()}), Reason: '{e.Reason}'");
            if (!this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: 连接意外关闭。正在安排重连...");
                this.ScheduleDelayedReconnect();
            }
            else
            {
                PluginLog.Info("WebSocket: 连接手动关闭 (或在重新初始化期间)。此事件不安排重连。");
            }
        }

        private void OnWebSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            PluginLog.Error($"WebSocket: 发生错误: {e.Message}");
            if (!this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: 错误表明潜在的连接问题。正在安排重连。");
                this.ScheduleDelayedReconnect();
            }
            else
            {
                PluginLog.Info("WebSocket: 发生错误但已标记手动关闭。此事件不安排重连。");
            }
        }

        private void ScheduleDelayedReconnect()
        {
            if (this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: 插件正在关闭或连接被手动关闭，跳过重连安排。");
                return;
            }

            if (this._isReconnecting)
            {
                PluginLog.Info("WebSocket: 重连已在进行中或已被其他事件安排。跳过重复安排。");
                return;
            }

            this._isReconnecting = true;
            PluginLog.Info($"WebSocket: 安排在 {RECONNECT_DELAY_MS / 1000} 秒后尝试重连...");

            if (this._reconnectTimer == null)
            {
                this._reconnectTimer = new System.Timers.Timer(RECONNECT_DELAY_MS);
                this._reconnectTimer.Elapsed += this.OnReconnectTimerElapsed;
                this._reconnectTimer.AutoReset = false;
            }
            else
            {
                this._reconnectTimer.Interval = RECONNECT_DELAY_MS;
                this._reconnectTimer.Stop();
            }
            this._reconnectTimer.Start();
        }

        private void OnReconnectTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            PluginLog.Info("WebSocket: 重连定时器已触发。尝试重新初始化WebSocket。");
            this._isReconnecting = false;
            this.InitializeWebSocket();
        }

        // === OSC二进制消息解析 ===
        private (string address, float value) ParseOSCMessage(byte[] data)
        {
            try
            {
                int index = 0;

                // 解析地址部分（以null结尾的字符串）
                int addrEnd = Array.IndexOf(data, (byte)0, index);
                if (addrEnd < 0)
                    return (null, 0f);

                string address = Encoding.ASCII.GetString(data, 0, addrEnd);

                // 地址填充对齐到4字节
                index = (addrEnd + 4) & ~3;
                if (index + 4 > data.Length)
                    return (null, 0f); // 至少需要 ",f" + float

                // 检查类型标签是否为 ",f"
                string typeTag = Encoding.ASCII.GetString(data, index, 2);
                if (typeTag != ",f")
                    return (null, 0f);

                // 浮点数值偏移（类型标签后的填充）
                index += 4; // ",f" + 2字节填充
                if (index + 4 > data.Length)
                    return (null, 0f);

                // 读取大端序float
                byte[] floatBytes = new byte[4];
                Buffer.BlockCopy(data, index, floatBytes, 0, 4);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(floatBytes);
                }

                float value = BitConverter.ToSingle(floatBytes, 0);
                return (address, value);
            }
            catch (Exception ex)
            {
                //PluginLog.Error($"解析异常：{ex.Message}");
                return (null, 0f);
            }
        }

        // === 对外发送OSC消息（静态方法）===
        public static void SendOSCMessage(string address, float value)
        {
            if (Instance?._wsClient?.IsAlive != true)
            {
                PluginLog.Warning($"WebSocket: 连接不活跃。无法发送OSC: {address} -> {value}");
                return;
            }

            try
            {
                var oscData = CreateOSCMessage(address, value);
                Instance._wsClient.Send(oscData);

                //PluginLog.Info($"发送OSC消息成功: {address} -> {value}");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"发送OSC消息失败: {address} -> {value}, 异常: {ex.Message}");
                Instance.ScheduleDelayedReconnect();
            }
        }

        // 将 (地址 + float数值) 封装成简单的OSC二进制格式
        private static byte[] CreateOSCMessage(string address, float value)
        {
            // 第0步：若传入地址是null或空字符串，给个默认，以免后续出现异常
            if (string.IsNullOrEmpty(address))
            {
                address = "/EmptyAddress";
            }

            // 第1步：强制去掉所有非可见 ASCII 字符，防止潜在的 \0、零宽字符、回车等
            //  可见ASCII范围：0x20(空格) ~ 0x7E(~)
            address = System.Text.RegularExpressions.Regex.Replace(address, @"[^\x20-\x7E]", "");

            // 第2步：统一在末尾加上 '\0'，无论它原本是否带有
            //   先 TrimEnd('\0') 清除尾部已有的 \0，再追加一次
            address = address.TrimEnd('\0') + "\0";

            // 第3步：用 ASCII 编码再做 4 字节对齐
            var addressBytes = Encoding.ASCII.GetBytes(address);
            int pad = (4 - (addressBytes.Length % 4)) % 4;
            var addressBuf = new byte[addressBytes.Length + pad];
            Buffer.BlockCopy(addressBytes, 0, addressBuf, 0, addressBytes.Length);
            // 不用手动补零，new 出来的 byte[] 默认都是 0，正好满足对齐需求

            // 第4步：类型标签 ",f\0\0"
            var typeTagBytes = new byte[] { 0x2C, 0x66, 0x00, 0x00 };

            // 第5步：处理 float 为大端序
            var valueBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(valueBytes);
            }

            // 第6步：拼接成最终的 OSC 消息
            var oscMessage = new byte[addressBuf.Length + typeTagBytes.Length + valueBytes.Length];
            Buffer.BlockCopy(addressBuf, 0, oscMessage, 0, addressBuf.Length);
            Buffer.BlockCopy(typeTagBytes, 0, oscMessage, addressBuf.Length, typeTagBytes.Length);
            Buffer.BlockCopy(valueBytes, 0, oscMessage, addressBuf.Length + typeTagBytes.Length, valueBytes.Length);

            // 你可以保留日志打印，便于检查最终字节长度
            //DebugLogHex(oscMessage, "一劳永逸的OSC数据:");
            return oscMessage;
        }


        // === 插件卸载 ===
        public override void Unload()
        {
            PluginLog.Info("WebSocket: 插件卸载被调用。正在清理WebSocket资源。");
            this._isManuallyClosed = true;

            if (this._reconnectTimer != null)
            {
                this._reconnectTimer.Stop();
                this._reconnectTimer.Elapsed -= this.OnReconnectTimerElapsed;
                this._reconnectTimer.Dispose();
                this._reconnectTimer = null;
                PluginLog.Info("WebSocket: 重连定时器已停止并释放。");
            }

            if (this._wsClient != null)
            {
                this._wsClient.OnOpen -= this.OnWebSocketOpen;
                this._wsClient.OnMessage -= this.OnWebSocketMessage;
                this._wsClient.OnClose -= this.OnWebSocketClose;
                this._wsClient.OnError -= this.OnWebSocketError;
                PluginLog.Info("WebSocket: 卸载时事件处理器已取消订阅。");

                if (this._wsClient.IsAlive)
                {
                    try
                    {
                        this._wsClient.Close(CloseStatusCode.Normal, "插件卸载中");
                        PluginLog.Info("WebSocket: 卸载时连接已关闭。");
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error($"WebSocket: 卸载时Close()异常: {ex.Message}");
                    }
                }
                else
                {
                    PluginLog.Info("WebSocket: 卸载时连接未活跃。");
                }
                this._wsClient = null;
                PluginLog.Info("WebSocket: 客户端实例已置空。");
            }
            else
            {
                PluginLog.Info("WebSocket: 卸载时客户端实例已经为空。");
            }
            
            this._isReconnecting = false;

            base.Unload();
            PluginLog.Info("MonitorOSC插件已成功卸载。");
        }
    }

    // === OSC状态管理器 ===
    public sealed class OSCStateManager
    {
        private static readonly Lazy<OSCStateManager> _instance =
            new Lazy<OSCStateManager>(() => new OSCStateManager());
        public static OSCStateManager Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, float> _stateCache =
            new ConcurrentDictionary<string, float>();

        public class StateChangedEventArgs : EventArgs
        {
            public string Address { get; set; }
            public float Value { get; set; }
        }

        // 当任意地址的状态更新时，会触发此事件
        public event EventHandler<StateChangedEventArgs> StateChanged;

        // 更新某地址的float数值，同时触发事件
        public void UpdateState(string address, float value)
        {
            this._stateCache.AddOrUpdate(address, value, (k, v) => value);

            PluginLog.Info($"[OSCStateManager] Update: {address} = {value}");
            this.StateChanged?.Invoke(this, new StateChangedEventArgs
            {
                Address = address,
                Value = value
            });
        }

        // 获取某地址当前值，若不存在则返回0
        public float GetState(string address) =>
            this._stateCache.TryGetValue(address, out var value) ? value : 0f;

        // 新增：获取所有状态的快照，用于遍历
        public IDictionary<string, float> GetAllStates()
        {
            // 返回一个拷贝，以免外部直接改动内部字典
            return new Dictionary<string, float>(this._stateCache);
        }
    }
}
