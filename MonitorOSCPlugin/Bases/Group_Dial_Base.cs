namespace Loupedeck.MonitorOSCPlugin.Dials
{
    using Loupedeck;

    using System;

    public abstract class Group_Dial_Base : PluginDynamicAdjustment, IDisposable
    {
        protected readonly string GroupName;
        protected readonly string[] ChannelNames;     // 通道名称列表
        protected readonly string[] ChannelAddresses; // 新地址格式: /Monitor/Channel/{name}
        protected readonly int FontSize; // 自定义字体大小

        private readonly BitmapColor _soloColor = new BitmapColor(0, 255, 0); // 绿色
        private readonly BitmapColor _muteColor = new BitmapColor(255, 0, 0); // 红色
        private readonly BitmapColor _defaultColor = new BitmapColor(0, 0, 0); // 黑色

        private bool _isSoloActive = false;
        private bool _isMuteActive = false;

        // 追踪此旋钮是否激活了当前模式（用于增量 vs 完全退出逻辑）
        private bool _isModeActivator = false;

        protected Group_Dial_Base(string groupName, string[] channelNames, string[] muteAddresses, string displayName, string description, int fontSize = 19)
            : base(displayName, description, "Dials", hasReset: true)
        {
            this.GroupName = groupName;
            this.ChannelNames = channelNames;
            this.FontSize = fontSize; // 设置自定义字体大小

            // 拼接完整的 OSC 地址（新格式：/Monitor/Channel/{name}）
            this.ChannelAddresses = new string[channelNames.Length];
            for (int i = 0; i < channelNames.Length; i++)
            {
                this.ChannelAddresses[i] = $"/Monitor/Channel/{channelNames[i]}";
            }

            OSCStateManager.Instance.StateChanged += OnOSCStateChanged;

            PluginLog.Info($"[{this.GroupName}] 组旋钮初始化");
        }

        protected override bool OnLoad()
        {
            // 初始化时检查Solo和Mute状态（使用新的三态逻辑）
            _isSoloActive = CheckAllChannelsState(2f); // 2 = Solo
            _isMuteActive = CheckAllChannelsState(1f); // 1 = Mute
            return true;
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= OnOSCStateChanged;

        // 检查所有通道是否都处于指定状态 (1=Mute, 2=Solo)
        private bool CheckAllChannelsState(float targetState)
        {
            foreach (var address in ChannelAddresses)
            {
                if (Math.Abs(OSCStateManager.Instance.GetState(address) - targetState) > 0.1f)
                    return false;
            }
            return true;
        }

        // 处理旋钮旋转
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            var isSoloMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Solo") > 0.5f;
            var isMuteMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Mute") > 0.5f;
            var isIdle = !isSoloMode && !isMuteMode;

            if (ticks > 0)  // 右转 = 有声音
            {
                if (isIdle)
                {
                    // 从 Idle 激活 Solo 模式，标记此旋钮为激活者
                    MonitorOSCPlugin.SendOSCMessage("/Monitor/Mode/Solo", 2f);
                    _isModeActivator = true;
                }
                // 否则模式已存在，此旋钮不是激活者

                // 发送"有声音"语义 (value=10)
                foreach (var addr in ChannelAddresses)
                    MonitorOSCPlugin.SendOSCMessage(addr, 10f);
            }
            else if (ticks < 0)  // 左转 = 没声音
            {
                if (isIdle)
                {
                    // 从 Idle 激活 Mute 模式，标记此旋钮为激活者
                    MonitorOSCPlugin.SendOSCMessage("/Monitor/Mode/Mute", 2f);
                    _isModeActivator = true;
                }
                // 否则模式已存在，此旋钮不是激活者

                // 根据是否为激活者选择语义：
                // - 激活者：value=12（可退出空模式）
                // - 非激活者：value=11（仅增量移除）
                float removeValue = _isModeActivator ? 12f : 11f;
                foreach (var addr in ChannelAddresses)
                    MonitorOSCPlugin.SendOSCMessage(addr, removeValue);
            }

            this.AdjustmentValueChanged(actionParameter);
        }

        protected override void RunCommand(string actionParameter)
        {
            if (_isMuteActive)
            {
                _isMuteActive = false;
                // 使用新地址: /Monitor/Channel/{name} = 0 (Off)
                foreach (var address in ChannelAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 0f);
                }
            }
            else
            {
                _isMuteActive = true;
                // 使用新地址: /Monitor/Channel/{name} = 1 (Mute)
                foreach (var address in ChannelAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, 1f);
                }
            }
            // **优化**：Mute 状态变化时刷新一次
            this.AdjustmentValueChanged(actionParameter);
        }

        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            // 检查模式状态变化，用于重置 _isModeActivator
            if (e.Address == "/Monitor/Mode/Solo" || e.Address == "/Monitor/Mode/Mute")
            {
                var isSoloMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Solo") > 0.5f;
                var isMuteMode = OSCStateManager.Instance.GetState("/Monitor/Mode/Mute") > 0.5f;
                var isIdle = !isSoloMode && !isMuteMode;

                // 当模式退出回到 Idle 时，重置激活者标记
                if (isIdle)
                {
                    _isModeActivator = false;
                }
            }

            bool newSolo = CheckAllChannelsState(2f); // 2 = Solo
            bool newMute = CheckAllChannelsState(1f); // 1 = Mute
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