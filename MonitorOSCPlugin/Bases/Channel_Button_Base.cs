namespace Loupedeck.MonitorOSCPlugin
{
    using Loupedeck;
    using Loupedeck.MonitorOSCPlugin.Buttons;

    using System;

    public abstract class Channel_Button_Base : PluginDynamicCommand, IDisposable
    {
        protected readonly string ChannelName;
        protected readonly string ChannelAddress;  // 新：单一地址 /Monitor/Channel/{name}

        private readonly BitmapColor _soloColor = new BitmapColor(0, 255, 0);
        private readonly BitmapColor _muteColor = new BitmapColor(255, 0, 0);
        private readonly BitmapColor _defaultColor = new BitmapColor(0, 0, 0);

        private int _ledState = 0;  // 0=off, 1=mute, 2=solo

        protected Channel_Button_Base(string channelName, string displayName, string description)
            : base(displayName, description, "Buttons")
        {
            this.ChannelName = channelName;
            this.ChannelAddress = $"/Monitor/Channel/{channelName}";

            this.AddParameter(channelName, displayName, "Channels");
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;

            //PluginLog.Info($"[{this.ChannelName}] 通道按钮初始化（简化版）");
        }

        protected override void RunCommand(string actionParameter)
        {
            // 从 OSCStateManager 读取当前模式（而非本地 Solo_Button.IsActive）
            var isSoloMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Solo") > 0.5f;
            var isMuteMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Mute") > 0.5f;

            if (isSoloMode || isMuteMode)
            {
                // 发送点击事件（1.0），让 VST 执行 toggle
                MonitorOSCPlugin.SendOSCMessage(this.ChannelAddress, 1f);
            }
            // 非 Solo/Mute 模式下不执行任何操作
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address == this.ChannelAddress)
            {
                // 0 = off, 1 = mute (red), 2 = solo (green)
                this._ledState = (int)e.Value;
                //PluginLog.Info($"[{this.ChannelName}] 收到状态更新: {e.Address} = {e.Value}");
                this.ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            var color = this._ledState switch
            {
                1 => this._muteColor,    // 红色
                2 => this._soloColor,    // 绿色
                _ => this._defaultColor  // 黑色/不亮
            };

            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(color);

                // 调用新的可重写绘制方法
                this.DrawButtonContent(bitmap);

                return bitmap.ToImage();
            }
        }

        /// <summary>
        /// 可重写的按钮内容绘制方法，默认绘制通道名称
        /// </summary>
        protected virtual void DrawButtonContent(BitmapBuilder bitmap)
        {
            bitmap.DrawText(this.ChannelName, fontSize: 36, color: BitmapColor.White);
        }

        public void Dispose()
        {
            OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
            //PluginLog.Info($"[{this.ChannelName}] 通道按钮资源释放");
        }
    }
}