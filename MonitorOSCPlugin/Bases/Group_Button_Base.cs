namespace Loupedeck.MonitorOSCPlugin
{
    using Loupedeck;
    using Loupedeck.MonitorOSCPlugin.Buttons;

    using System;
    using System.Linq;

    /// <summary>
    /// 代表一组通道按钮的基类，处理组的 Solo/Mute 逻辑。
    /// </summary>
    public abstract class Group_Button_Base : PluginDynamicCommand, IDisposable
    {
        protected readonly string GroupName;
        protected readonly string[] ChannelNames;
        protected readonly string[] SoloAddresses;
        protected readonly string[] MuteAddresses;

        private readonly BitmapColor _soloColor = new BitmapColor(0, 255, 0); // 绿色
        private readonly BitmapColor _muteColor = new BitmapColor(255, 0, 0); // 红色
        private readonly BitmapColor _defaultColor = new BitmapColor(0, 0, 0); // 黑色

        protected bool _isSoloActive = false;
        protected bool _isMuteActive = false;

        protected Group_Button_Base(string groupName, string[] channelNames, string displayName, string description)
            : base(displayName, description, "Buttons") // 将其放入 "Buttons" 组
        {
            this.GroupName = groupName;
            this.ChannelNames = channelNames ?? new string[0]; // 确保不为 null

            // 构建完整的 OSC 地址
            this.SoloAddresses = this.ChannelNames.Select(cn => $"/Monitor/Solo/{cn}").ToArray();
            this.MuteAddresses = this.ChannelNames.Select(cn => $"/Monitor/Mute/{cn}").ToArray();

            // 为 Loupedeck UI 添加参数
            this.AddParameter(groupName, displayName, "Group Buttons");

            // 订阅 OSC 状态变化事件
            OSCStateManager.Instance.StateChanged += this.OnOSCStateChanged;

            // 初始化时检查一次当前状态
            this.UpdateGroupState();

            //PluginLog.Info($"[{this.GroupName}] 组按钮初始化");
        }

        /// <summary>
        /// 检查提供的所有地址是否都处于激活状态 (值 > 0.5f)。
        /// </summary>
        private bool CheckAllAddressesActive(string[] addresses)
        {
            if (addresses == null || addresses.Length == 0)
                return false;
            // 确保至少有一个地址，并且所有地址的值都大于 0.5f
            return addresses.Length > 0 && addresses.All(address => OSCStateManager.Instance.GetState(address) > 0.5f);
        }

        /// <summary>
        /// 根据当前的 OSC 状态更新组的 Solo 和 Mute 激活状态。
        /// </summary>
        private void UpdateGroupState()
        {
            this._isSoloActive = this.CheckAllAddressesActive(this.SoloAddresses);
            this._isMuteActive = this.CheckAllAddressesActive(this.MuteAddresses);
        }

        /// <summary>
        /// 处理按钮按下事件。
        /// </summary>
        protected override void RunCommand(string actionParameter)
        {
            // 根据当前激活的模式（Solo 或 Mute）切换组的状态
            if (Solo_Button.IsActive)
            {
                // 如果当前组已 Solo，则取消所有 Solo；否则，激活所有 Solo
                var newSoloValue = this._isSoloActive ? 0f : 1f;
                foreach (var address in this.SoloAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, newSoloValue);
                }
            }
            else if (Mute_Button.IsActive)
            {
                // 如果当前组已 Mute，则取消所有 Mute；否则，激活所有 Mute
                var newMuteValue = this._isMuteActive ? 0f : 1f;
                foreach (var address in this.MuteAddresses)
                {
                    MonitorOSCPlugin.SendOSCMessage(address, newMuteValue);
                }
            }
            // 如果 Solo 和 Mute 按钮都未激活，则不执行任何操作。

            // 注：OSC 消息发送后会触发状态更新，OnOSCStateChanged 会负责刷新图像。
            // 这样可以避免在状态未实际更新前刷新图像。
        }

        /// <summary>
        /// 处理 OSC 状态变化事件。
        /// </summary>
        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            // 检查变化的地址是否属于本组
            if (this.SoloAddresses.Contains(e.Address) || this.MuteAddresses.Contains(e.Address))
            {
                var oldSolo = this._isSoloActive;
                var oldMute = this._isMuteActive;

                this.UpdateGroupState(); // 重新计算组状态

                // 仅当组的整体状态（影响显示的）发生变化时才刷新图像
                if (oldSolo != this._isSoloActive || oldMute != this._isMuteActive)
                {
                    this.ActionImageChanged();
                }
            }
        }

        /// <summary>
        /// 生成按钮图像。
        /// </summary>
        protected override BitmapImage GetCommandImage(string actionParameter, PluginImageSize imageSize)
        {
            using (var bitmap = new BitmapBuilder(imageSize))
            {
                // 根据组状态设置背景色
                if (this._isSoloActive)
                {
                    bitmap.Clear(this._soloColor);
                }
                else if (this._isMuteActive)
                {
                    bitmap.Clear(this._muteColor);
                }
                else
                {
                    bitmap.Clear(this._defaultColor);
                }

                // 调用可重写的绘制方法来绘制按钮内容（如文本）
                this.DrawButtonContent(bitmap);

                return bitmap.ToImage();
            }
        }

        /// <summary>
        /// 可重写的按钮内容绘制方法，默认绘制组名称。
        /// </summary>
        protected virtual void DrawButtonContent(BitmapBuilder bitmap)
        {
            bitmap.DrawText(this.GroupName, fontSize: 36, color: BitmapColor.White);
        }

        /// <summary>
        /// 释放资源，取消事件订阅。
        /// </summary>
        public void Dispose()
        {
            OSCStateManager.Instance.StateChanged -= this.OnOSCStateChanged;
            //PluginLog.Info($"[{this.GroupName}] 组按钮资源释放");
        }
    }
}