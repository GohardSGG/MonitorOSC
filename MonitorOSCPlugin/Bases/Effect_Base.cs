namespace Loupedeck.MonitorOSCPlugin
{
    using Loupedeck;

    using System;

    public abstract class Effect_Base : PluginDynamicCommand, IDisposable
    {
        protected bool _isActive;
        protected readonly string OscAddress;
        protected readonly BitmapColor ActiveColor;
        protected readonly BitmapColor DefaultColor = BitmapColor.Black;
        private readonly BitmapColor _defaultActiveColor = new BitmapColor(173, 216, 230);

        // 基础构造函数
        protected Effect_Base(string channelName, string displayName, string description)
            : this(
                channelName: channelName,
                displayName: displayName,
                description: description,
                oscAddress: $"/Monitor/{channelName}",
                activeColor: new BitmapColor(173, 216, 230))
        { }

        // 全参数构造函数
        protected Effect_Base(
            string channelName,
            string displayName,
            string description,
            string oscAddress,
            BitmapColor activeColor)
            : base(displayName, description, "Effects")
        {
            this.OscAddress = $"/Monitor/{oscAddress}";
            this.ActiveColor = activeColor;

            // 初始化状态监听
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;
            this._isActive = OSCStateManager.Instance.GetState(oscAddress) > 0.5f;

            this.AddParameter(channelName, displayName, "Effects");
            //PluginLog.Info($"[{channelName}] 效果按钮初始化 (双向通信)");
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address == this.OscAddress)
            {
                this._isActive = e.Value > 0.5f;
                //PluginLog.Info($"[{this.OscAddress}] 接收状态更新: {e.Value}");
                this.ActionImageChanged();
            }
        }

        protected override void RunCommand(string actionParameter)
        {
            var newValue = !this._isActive;
            //PluginLog.Info($"[{this.OscAddress}] 发送状态: {newValue}");
            MonitorOSCPlugin.SendOSCMessage(this.OscAddress, newValue ? 1f : 0f);

            // 等待服务器确认更新，本地状态由OnOSCStateChanged更新
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(_isActive ? this.ActiveColor : this.DefaultColor);
                this.DrawButtonContent(bitmap);
                return bitmap.ToImage();
            }
        }

        protected virtual void DrawButtonContent(BitmapBuilder bitmap)
        {
            bitmap.DrawText(
                text: this.Name,
                fontSize: 28,
                color: BitmapColor.White);
        }

        public void Dispose()
        {
            OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
            //PluginLog.Info($"[{this.OscAddress}] 按钮资源释放");
        }
    }
}