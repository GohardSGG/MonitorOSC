namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;

    using System;
    using System.Linq;

    public class Mute_Button : PluginDynamicCommand, IDisposable
    {
        // 从 OSCStateManager 读取状态（VST 广播的状态）
        public static bool IsActive =>
            OSCStateManager.Instance.GetState("/Monitor/Mode/Mute") > 0.5f;

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
            // 只发送 toggle 消息，不改变本地状态
            // VST 会处理状态切换并广播结果
            MonitorOSCPlugin.SendOSCMessage("/Monitor/Mode/Mute", 1f);
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            // 监听模式状态变化
            if (e.Address == "/Monitor/Mode/Mute")
            {
                this.ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            // 从 OSCStateManager 读取状态决定显示
            var isActive = OSCStateManager.Instance.GetState("/Monitor/Mode/Mute") > 0.5f;

            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(isActive ? new BitmapColor(255, 0, 0) : new BitmapColor(0, 0, 0));
                bitmap.DrawText("M", fontSize: 39, color: BitmapColor.White);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
    }
}