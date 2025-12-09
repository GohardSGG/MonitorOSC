namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Front_Dial : Group_Dial_Base
    {
        public Front_Dial()
            : base(
                groupName: "Front",
                channelNames: new[] { "L", "R" }, // 只需要传入相对路径
                muteAddresses: new[] { "L", "R" }, // 只需要传入相对路径
                displayName: "Front Dial",
                description: "控制Front组的Solo和Mute状态")
        { }
    }
}