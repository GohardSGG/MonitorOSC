namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;
    using System;
    using System.Linq;

    public class Solo_Button : PluginDynamicCommand, IDisposable
    {
        // 从 OSCStateManager 读取状态（VST 广播的状态）
        public static bool IsActive =>
            OSCStateManager.Instance.GetState("/Monitor/Mode/Solo") > 0.5f;

        public Solo_Button() : base(
            displayName: "Solo Button",
            description: "Solo按钮",
            groupName: "Buttons")
        {
            this.AddParameter("solo_mode", "Solo Master", "Solo");
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;
        }

        protected override void RunCommand(string actionParameter)
        {
            // 只发送 toggle 消息，不改变本地状态
            // VST 会处理状态切换并广播结果
            MonitorOSCPlugin.SendOSCMessage("/Monitor/Mode/Solo", 1f);
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            // 监听模式状态变化
            if (e.Address == "/Monitor/Mode/Solo")
            {
                this.ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            // 从 OSCStateManager 读取状态决定显示
            var isActive = OSCStateManager.Instance.GetState("/Monitor/Mode/Solo") > 0.5f;

            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(isActive ? new BitmapColor(0, 255, 0) : new BitmapColor(0, 0, 0));
                bitmap.DrawText("S", fontSize: 39, color: BitmapColor.White);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
    }
}