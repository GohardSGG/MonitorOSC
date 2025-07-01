namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Bottom_Dial : Group_Dial_Base
    {
        public Bottom_Dial()
            : base(
                groupName: "Bottom",
                soloAddresses: new[] { "LBF", "RBF", "LBB", "RBB" }, // 只需要传入相对路径
                muteAddresses: new[] { "LBF", "RBF", "LBB", "RBB" }, // 只需要传入相对路径
                displayName: "Bottom Dial",
                description: "控制Bottom组的Solo和Mute状态",
                fontSize: 14)
        { }
    }
}
