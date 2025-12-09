namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class SUB_Dial : Group_Dial_Base
    {
        public SUB_Dial()
            : base(
                groupName: "SUB",
                channelNames: new[] { "SUB_F", "SUB_B", "SUB_L", "SUB_R" }, // 只需要传入相对路径
                muteAddresses: new[] { "SUB_F", "SUB_B", "SUB_L", "SUB_R" }, // 只需要传入相对路径
                displayName: "SUB Dial",
                description: "控制SUB组的Solo和Mute状态")
        { }
    }
}
