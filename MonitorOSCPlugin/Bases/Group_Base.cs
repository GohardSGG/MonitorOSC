namespace Loupedeck.MonitorOSCPlugin.Dials
{
    using Loupedeck;

    using System;

    public abstract class Group_Base : PluginDynamicAdjustment, IDisposable
    {
        protected readonly string GroupName;
        protected readonly string[] SoloAddresses;
        protected readonly string[] MuteAddresses;
        protected readonly int FontSize; // 自定义字体大小

        private readonly BitmapColor _soloColor = new BitmapColor(0, 255, 0); // 绿色
        private readonly BitmapColor _muteColor = new BitmapColor(255, 0, 0); // 红色
        private readonly BitmapColor _defaultColor = new BitmapColor(0, 0, 0); // 黑色

        private bool _isSoloActive = false;
        private bool _isMuteActive = false;

        protected Group_Base(string groupName, string[] soloAddresses, string[] muteAddresses, string displayName, string description, int fontSize = 19)
            : base(displayName, description, "Dials", hasReset: true)
        {
            this.GroupName = groupName;
            this.FontSize = fontSize; // 设置自定义字体大小

            // 拼接完整的 OSC 地址
            this.SoloAddresses = new string[soloAddresses.Length];
            for (int i = 0; i < soloAddresses.Length; i++)
            {
                this.SoloAddresses[i] = $"/Monitor/Solo/{soloAddresses[i]}";
            }

            this.MuteAddresses = new string[muteAddresses.Length];
            for (int i = 0; i < muteAddresses.Length; i++)
            {
                this.MuteAddresses[i] = $"/Monitor/Mute/{muteAddresses[i]}";
            }

            OSCStateManager.Instance.StateChanged += OnOSCStateChanged;

            PluginLog.Info($"[{this.GroupName}] 组旋钮初始化");
        }

        protected override bool OnLoad()
        {
            // 初始化时检查Solo和Mute状态
            _isSoloActive = CheckAllAddressesActive(SoloAddresses);
            _isMuteActive = CheckAllAddressesActive(MuteAddresses);
            return true;
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= OnOSCStateChanged;

        // 检查所有地址是否都处于激活状态
        private bool CheckAllAddressesActive(string[] addresses)
        {
            foreach (var address in addresses)
            {
                if (OSCStateManager.Instance.GetState(address) <= 0.5f)
                    return false;
            }
            return true;
        }

        // 处理旋钮旋转
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (_isMuteActive)
                return; // Mute 状态下旋转无效

            if (ticks > 0 && !_isSoloActive)
            {
                _isSoloActive = true;
                foreach (var address in SoloAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 1f);
                }
            }
            else if (ticks < 0 && _isSoloActive)
            {
                _isSoloActive = false;
                foreach (var address in SoloAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 0f);
                }
            }

                this.AdjustmentValueChanged(actionParameter);
        }

        protected override void RunCommand(string actionParameter)
        {
            if (_isMuteActive)
            {
                _isMuteActive = false;
                foreach (var address in MuteAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 0f);
                }
            }
            else
            {
                _isMuteActive = true;
                foreach (var address in MuteAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 1f);
                }
            }
            // **优化**：Mute 状态变化时刷新一次
            this.AdjustmentValueChanged(actionParameter);
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            bool newSolo = CheckAllAddressesActive(SoloAddresses);
            bool newMute = CheckAllAddressesActive(MuteAddresses);
            // 仅在任一状态实际变化时刷新，避免重复刷新
            if (newSolo != _isSoloActive || newMute != _isMuteActive)
            {
                _isSoloActive = newSolo;
                _isMuteActive = newMute;
                this.AdjustmentValueChanged();
            }
        }



        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                if (_isSoloActive)
                {
                    bitmap.Clear(_soloColor);
                }
                else if (_isMuteActive)
                {
                    bitmap.Clear(_muteColor);
                }
                else
                {
                    bitmap.Clear(_defaultColor);
                }

                // 使用自定义的 FontSize 绘制文本
                bitmap.DrawText(GroupName, fontSize: FontSize, color: BitmapColor.White);
                return bitmap.ToImage();
            }
        }
    }
}