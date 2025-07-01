using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocketSharp;
using Loupedeck;

namespace Loupedeck.MonitorOSCPlugin
{
    public class MonitorOSCPlugin : Plugin
    {
        // === Loupedeck必需配置 ===
        // ... existing code ...

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
            // ... existing code ...
        }

        // === WebSocket连接管理 ===
        private void InitializeWebSocket()
        {
            PluginLog.Info("WebSocket: Attempting to initialize and connect...");
            this._isManuallyClosed = false;

            if (this._wsClient != null)
            {
                PluginLog.Info("WebSocket: Cleaning up existing WebSocket client before reinitialization.");
                this._wsClient.OnOpen -= this.OnWebSocketOpen;
                this._wsClient.OnMessage -= this.OnWebSocketMessage;
                this._wsClient.OnClose -= this.OnWebSocketClose;
                this._wsClient.OnError -= this.OnWebSocketError;

                if (this._wsClient.IsAlive)
                {
                    try
                    {
                        this._wsClient.Close(CloseStatusCode.Normal, "Reinitializing WebSocket");
                        PluginLog.Info("WebSocket: Old client closed during reinitialization.");
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error(ex, "WebSocket: Exception during old client Close() on reinitialization.");
                    }
                }
                this._wsClient = null;
                PluginLog.Info("WebSocket: Old client instance nullified.");
            }

            try
            {
                this._wsClient = new WebSocket(WS_SERVER);
                PluginLog.Info($"WebSocket: New client created for {WS_SERVER}.");

                this._wsClient.OnOpen += this.OnWebSocketOpen;
                this._wsClient.OnMessage += this.OnWebSocketMessage;
                this._wsClient.OnClose += this.OnWebSocketClose;
                this._wsClient.OnError += this.OnWebSocketError;
                PluginLog.Info("WebSocket: Event handlers subscribed.");

                this._wsClient.Connect();
                PluginLog.Info("WebSocket: Connection attempt initiated (Connect() called).");
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "WebSocket: Exception during new WebSocket client creation or Connect() call in InitializeWebSocket.");
                this.ScheduleDelayedReconnect();
            }
        }

        private void OnWebSocketOpen(Object sender, EventArgs e)
        {
            PluginLog.Info("WebSocket: Connection opened successfully.");
            this._isReconnecting = false;
            if (this._reconnectTimer != null)
            {
                this._reconnectTimer.Stop();
                PluginLog.Info("WebSocket: Reconnect timer stopped due to successful connection.");
            }
        }

        private void OnWebSocketMessage(object sender, MessageEventArgs e)
        {
            // ... existing code ...
        }

        private void OnWebSocketClose(Object sender, CloseEventArgs e)
        {
            PluginLog.Warning($"WebSocket: Connection closed. WasClean: {e.WasClean}, Code: {e.Code} ({((CloseStatusCode)e.Code).ToString()}), Reason: '{e.Reason}'");
            if (!this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: Connection closed unexpectedly. Scheduling reconnect...");
                this.ScheduleDelayedReconnect();
            }
            else
            {
                PluginLog.Info("WebSocket: Connection closed manually (or during reinitialization). No reconnect scheduled by this event.");
            }
        }

        private void OnWebSocketError(Object sender, WebSocketSharp.ErrorEventArgs e)
        {
            PluginLog.Error(e.Exception, $"WebSocket: Error occurred: {e.Message}");
            if (!this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: Error indicates potential connection issue. Scheduling reconnect.");
                this.ScheduleDelayedReconnect();
            }
            else
            {
                PluginLog.Info("WebSocket: Error occurred but manual close is flagged. No reconnect scheduled by this event.");
            }
        }

        private void ScheduleDelayedReconnect()
        {
            if (this._isManuallyClosed)
            {
                PluginLog.Info("WebSocket: Plugin is shutting down or connection was manually closed, skipping reconnect schedule.");
                return;
            }

            if (this._isReconnecting)
            {
                PluginLog.Info("WebSocket: Reconnect already in progress or scheduled by another event. Skipping duplicate schedule.");
                return;
            }

            this._isReconnecting = true;
            PluginLog.Info($"WebSocket: Scheduling reconnect attempt in {RECONNECT_DELAY_MS / 1000} seconds...");

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

        private void OnReconnectTimerElapsed(Object sender, System.Timers.ElapsedEventArgs e)
        {
            PluginLog.Info("WebSocket: Reconnect timer elapsed. Attempting to re-initialize WebSocket.");
            this._isReconnecting = false;
            this.InitializeWebSocket();
        }

        // === OSC二进制消息解析 ===
        private (string address, float value) ParseOSCMessage(byte[] data)
        {
            // ... existing code ...
        }

        // === 对外发送OSC消息（静态方法）===
        public static void SendOSCMessage(string address, float value)
        {
            // ... existing code ...
        }

        // 将 (地址 + float数值) 封装成简单的OSC二进制格式
        private static byte[] EncodeOSCMessage(string address, float value)
        {
            // ... existing code ...
        }

        // === 插件卸载 ===
        public override void Unload()
        {
            PluginLog.Info("WebSocket: Plugin Unload called. Cleaning up WebSocket resources.");
            this._isManuallyClosed = true;

            if (this._reconnectTimer != null)
            {
                this._reconnectTimer.Stop();
                this._reconnectTimer.Elapsed -= this.OnReconnectTimerElapsed;
                this._reconnectTimer.Dispose();
                this._reconnectTimer = null;
                PluginLog.Info("WebSocket: Reconnect timer stopped and disposed.");
            }

            if (this._wsClient != null)
            {
                this._wsClient.OnOpen -= this.OnWebSocketOpen;
                this._wsClient.OnMessage -= this.OnWebSocketMessage;
                this._wsClient.OnClose -= this.OnWebSocketClose;
                this._wsClient.OnError -= this.OnWebSocketError;
                PluginLog.Info("WebSocket: Event handlers unsubscribed for Unload.");

                if (this._wsClient.IsAlive)
                {
                    try
                    {
                        this._wsClient.Close(CloseStatusCode.Normal, "Plugin unloading");
                        PluginLog.Info("WebSocket: Connection closed on Unload.");
                    }
                    catch (Exception ex)
                    {
                        PluginLog.Error(ex, "WebSocket: Exception during Close() on Unload.");
                    }
                }
                else
                {
                    PluginLog.Info("WebSocket: Connection was not alive during Unload.");
                }
                this._wsClient = null;
                PluginLog.Info("WebSocket: Client instance nullified.");
            }
            else
            {
                PluginLog.Info("WebSocket: Client instance was already null on Unload.");
            }

            this._isReconnecting = false;

            base.Unload();
            PluginLog.Info("插件已成功卸载。");
        }
    }

    // === OSC状态管理器 ===
    // ... existing code ...
}