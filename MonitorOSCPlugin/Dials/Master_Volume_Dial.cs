namespace Loupedeck.MonitorOSCPlugin.Dials
{
    using Loupedeck;
    using Loupedeck.MonitorOSCPlugin;

    using System;
    using System.Threading.Tasks;

    public class MasterVolumeDial : PluginDynamicAdjustment, IDisposable
    {
        // ========== 核心配置 ==========
        private const string VOLUME_ADDRESS = "/Monitor/Master/Volume";
        private const string DIM_ADDRESS = "/Monitor/Master/Mute";

        // ========== 状态管理 ==========
        private float _currentVolume = 0f;
        private bool _isDimmed = false;
        private float _lastVolumeBeforeDim = 0f;
        private bool _isProcessingUserAction = false;

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
            _isDimmed = OSCStateManager.Instance.GetState(DIM_ADDRESS) == 1f;
            return true;
        }

        public void Dispose() => OSCStateManager.Instance.StateChanged -= OnOSCStateChanged;

        // ========== 旋钮控制逻辑 ==========
        protected override void ApplyAdjustment(string actionParameter, int ticks)
        {
            if (_isDimmed)
                return;

            var newVolume = Math.Clamp(_currentVolume + (ticks * 0.01f), 0f, 1f);
            if (newVolume == _currentVolume)
                return;

            _currentVolume = newVolume;
            MonitorOSCPlugin.SendOSCMessage(VOLUME_ADDRESS, newVolume);
            AdjustmentValueChanged(actionParameter);
        }

        protected override async void RunCommand(string actionParameter)
        {
            _isProcessingUserAction = true;

            _isDimmed = !_isDimmed;
            AdjustmentValueChanged(actionParameter);

            if (_isDimmed)
            {
                _lastVolumeBeforeDim = OSCStateManager.Instance.GetState(VOLUME_ADDRESS); // 始终同步最新音量
                MonitorOSCPlugin.SendOSCMessage(DIM_ADDRESS, 1f);
            }
            else
            {
                var actualVolume = OSCStateManager.Instance.GetState(VOLUME_ADDRESS); // 获取解除静音时的实际值
                _currentVolume = actualVolume;
                MonitorOSCPlugin.SendOSCMessage(DIM_ADDRESS, 0f);
                MonitorOSCPlugin.SendOSCMessage(VOLUME_ADDRESS, actualVolume);
            }

            await Task.Delay(200);
            _isProcessingUserAction = false;
        }

        // ========== OSC 状态变化处理 ==========
        private void OnOSCStateChanged(object sender, OSCStateManager.StateChangedEventArgs e)
        {
            if (_isProcessingUserAction)
                return;

            if (e.Address == VOLUME_ADDRESS)
            {
                _currentVolume = e.Value;
                AdjustmentValueChanged();
            }
            else if (e.Address == DIM_ADDRESS)
            {
                _isDimmed = e.Value == 1f;
                AdjustmentValueChanged();
            }
        }

        protected override string GetAdjustmentValue(string actionParameter) =>
            _isDimmed ? "X" : $"{Math.Round(_currentVolume * 100)}%";
    }
}