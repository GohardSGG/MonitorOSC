namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Top_Front_Dial : Group_Base
    {
        public Top_Front_Dial()
            : base(
                groupName: "Top Front",
                soloAddresses: new[] { "LTF", "RTF" }, // 只需要传入相对路径
                muteAddresses: new[] { "LTF", "RTF" }, // 只需要传入相对路径
                displayName: "Top Front Dial",
                description: "控制Top Front组的Solo和Mute状态",
                fontSize: 17)
        { }
    }
}
