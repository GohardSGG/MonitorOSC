namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Bottom_Dial : Group_Dial_Base
    {
        public Bottom_Dial()
            : base(
                groupName: "Bottom",
                channelNames: new[] { "LBF", "RBF", "LBB", "RBB" },
                muteAddresses: new[] { "LBF", "RBF", "LBB", "RBB" }, // 保留参数以兼容
                displayName: "Bottom Dial",
                description: "控制Bottom组的Solo和Mute状态",
                fontSize: 14)
        { }
    }
}
