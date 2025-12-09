namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Top_Dial : Group_Dial_Base
    {
        public Top_Dial()
            : base(
                groupName: "Top",
                channelNames: new[] { "LTF", "RTF", "LTB", "RTB" }, // 只需要传入相对路径
                muteAddresses: new[] { "LTF", "RTF", "LTB", "RTB" }, // 只需要传入相对路径
                displayName: "Top Dial",
                description: "控制Top组的Solo和Mute状态")
        { }
    }
}
