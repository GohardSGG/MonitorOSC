namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Center_Dial : Group_Dial_Base
    {
        public Center_Dial()
            : base(
                groupName: "Center",
                channelNames: new[] { "C" }, // 只需要传入相对路径
                muteAddresses: new[] { "C" }, // 只需要传入相对路径
                displayName: "Center Dial",
                description: "控制Center组的Solo和Mute状态",
                fontSize: 15)
        { }
    }
}
