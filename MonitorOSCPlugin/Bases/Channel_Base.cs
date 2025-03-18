namespace Loupedeck.MonitorOSCPlugin
{
    using Loupedeck;
    using Loupedeck.MonitorOSCPlugin.Buttons;

    using System;

    public abstract class Channel_Base : PluginDynamicCommand, IDisposable
    {
        protected readonly string ChannelName;
        protected readonly string SoloAddress;
        protected readonly string MuteAddress;

        private readonly BitmapColor _soloColor = new BitmapColor(0, 255, 0);
        private readonly BitmapColor _muteColor = new BitmapColor(255, 0, 0);
        private readonly BitmapColor _defaultColor = new BitmapColor(0, 0, 0);

        protected Channel_Base(string channelName, string displayName, string description)
            : base(displayName, description, "Buttons")
        {
            this.ChannelName = channelName;
            this.SoloAddress = $"/Monitor/Solo/{channelName}";
            this.MuteAddress = $"/Monitor/Mute/{channelName}";

            this.AddParameter(channelName, displayName, "Channels");
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;

            //PluginLog.Info($"[{this.ChannelName}] 通道按钮初始化（简化版）");
        }

        protected override void RunCommand(string actionParameter)
        {
            // 根据当前激活模式（Solo 或 Mute），切换相应状态
            if (Solo_Button.IsActive)
            {
                var currentVal = OSCStateManager.Instance.GetState(this.SoloAddress);
                var newVal = currentVal > 0.5f ? 0f : 1f;
                MonitorOSCPlugin.SendOSCMessage(this.SoloAddress, newVal);
            }
            else if (Mute_Button.IsActive)
            {
                var currentVal = OSCStateManager.Instance.GetState(this.MuteAddress);
                var newVal = currentVal > 0.5f ? 0f : 1f;
                MonitorOSCPlugin.SendOSCMessage(this.MuteAddress, newVal);
            }
            // 非 Solo/Mute 模式下不执行任何操作

            // **优化**：延迟调用图像刷新，让状态先更新
            // 使用 Task.Run 异步等待 50ms 后刷新，避免阻塞 UI 线程
            _ = Task.Run(async () =>
            {
                //await Task.Delay(50);
                this.ActionImageChanged(actionParameter); // 仅刷新当前按钮图像
            });
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address == this.SoloAddress || e.Address == this.MuteAddress)
            {
                //PluginLog.Info($"[{this.ChannelName}] 收到状态更新: {e.Address} = {e.Value}");
                this.ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            var isSoloActive = OSCStateManager.Instance.GetState(this.SoloAddress) > 0.5f;
            var isMuteActive = OSCStateManager.Instance.GetState(this.MuteAddress) > 0.5f;

            using (var bitmap = new BitmapBuilder(imageSize))
            {
                if (isSoloActive)
                {
                    bitmap.Clear(this._soloColor);
                }
                else if (isMuteActive)
                {
                    bitmap.Clear(this._muteColor);
                }
                else
                {
                    bitmap.Clear(this._defaultColor);
                }

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