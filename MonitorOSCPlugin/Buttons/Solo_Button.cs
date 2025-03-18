namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;
    using System;
    using System.Linq;

    public class Solo_Button : PluginDynamicCommand, IDisposable
    {
        private static bool _userActivated;
        private static bool _autoActivated;
        public static bool IsActive => _userActivated || _autoActivated;

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
            if (IsActive)
            {
                // Clear all solo states when deactivating
                var addresses = OSCStateManager.Instance.GetAllStates()
                    .Where(kvp => kvp.Key.StartsWith("/Monitor/Solo/"))
                    .Select(kvp => kvp.Key);
                
                foreach (var addr in addresses)
                    MonitorOSCPlugin.SendOSCMessage(addr, 0f);

                _userActivated = _autoActivated = false;
            }
            else
            {
                _userActivated = true;
            }
            this.ActionImageChanged();
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address?.StartsWith("/Monitor/Solo/") == true)
            {
                _autoActivated = OSCStateManager.Instance.GetAllStates()
                    .Any(kvp => kvp.Key.StartsWith("/Monitor/Solo/") && kvp.Value > 0.5f);
                
                this.ActionImageChanged();
            }
        }

        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                bitmap.Clear(IsActive ? new BitmapColor(0, 255, 0) : new BitmapColor(0, 0, 0));
                bitmap.DrawText("S", fontSize: 39, color: BitmapColor.White);
                return bitmap.ToImage();
            }
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
    }
}