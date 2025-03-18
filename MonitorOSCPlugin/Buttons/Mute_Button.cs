namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;

    using System;
    using System.Linq;

    public class Mute_Button : PluginDynamicCommand, IDisposable
    {
        // 内部状态缓存
        private static bool _isActive;

        public static bool IsActive => _isActive;

        public Mute_Button() : base(
            displayName: "Mute Button",
            description: "Mute按钮",
            groupName: "Buttons")
        {
            this.AddParameter("mute_mode", "Mute Master", "Mute");
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;
        }

        protected override void RunCommand(string actionParameter)
        {
            _isActive = !_isActive;
            if (!_isActive)
            {
                // 取消 Mute 模式：清除所有 Mute 状态
                var addresses = OSCStateManager.Instance.GetAllStates()
                    .Where(kvp => kvp.Key.StartsWith("/Monitor/Mute/"))
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (var addr in addresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(addr, 0f);
                }
            }
            // **优化**：刷新 Mute 按钮自身状态
            this.ActionImageChanged(actionParameter);
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address?.StartsWith("/Monitor/Mute/") == true)
            {
                bool anyMute = OSCStateManager.Instance.GetAllStates()
                    .Any(kvp => kvp.Key.StartsWith("/Monitor/Mute/") && kvp.Value > 0.5f);
                // 只有在全局 Mute 激活状态变化时才刷新
                if (anyMute != _isActive)
                {
                    _isActive = anyMute;
                    this.ActionImageChanged();
                }
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(_isActive ? new BitmapColor(255, 0, 0) : new BitmapColor(0, 0, 0));
                bitmap.DrawText("M", fontSize: 39, color: BitmapColor.White);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
    }
}