namespace Loupedeck.MonitorOSCPlugin.Dials
{
    using Loupedeck;
    using Loupedeck.MonitorOSCPlugin;

    using System;

    public class MasterVolumeDial : PluginDynamicAdjustment, IDisposable
    {
        // ========== 核心配置 ==========
        private const string VOLUME_ADDRESS = "/Monitor/Master/Volume";
        private const string CUT_ADDRESS = "/Monitor/Master/Cut";

        // ========== 状态管理 ==========
        private float _currentVolume = 0f;
        private bool _isCut = false;

        public MasterVolumeDial() : base(
            displayName: "Master Volume Dial",
            description: "主音量旋钮",
            groupName: "Dials",
            hasReset: true)
        {
            OSCStateManager.Instance.StateChanged += OnOSCStateChanged;
        }

        protected override bool OnLoad()
        {
            _currentVolume = OSCStateManager.Instance.GetState(VOLUME_ADDRESS);
            _isCut = OSCStateManager.Instance.GetState(CUT_ADDRESS) == 1f;
            return true;
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= OnOSCStateChanged;

        // ========== 旋钮控制逻辑 ==========
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (_isCut)
                return;

            var newVolume = Math.Clamp(_currentVolume + (ticks * 0.01f), 0f, 1f);
            if (newVolume == _currentVolume)
                return;

            _currentVolume = newVolume;
            MonitorOSCPlugin.SendOSCMessage(VOLUME_ADDRESS, newVolume);
            AdjustmentValueChanged(actionParameter);
        }

        // 按下旋钮：发送 toggle 请求，等待 Rust 回复后才更新显示
        protected override void RunCommand(string actionParameter)
        {
            MonitorOSCPlugin.SendOSCMessage(CUT_ADDRESS, 1f);
        }

        // ========== OSC 状态变化处理 ==========
        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (e.Address == VOLUME_ADDRESS)
            {
                // Volume 只在初始化广播时接收（Rust 不回显用户操作）
                _currentVolume = e.Value;
                AdjustmentValueChanged();
            }
            else if (e.Address == CUT_ADDRESS)
            {
                _isCut = e.Value == 1f;
                AdjustmentValueChanged();
            }
        }

        protected override string GetAdjustmentValue(string actionParameter) =>
            _isCut ? "X" : $"{Math.Round(_currentVolume * 100)}%";
    }
}