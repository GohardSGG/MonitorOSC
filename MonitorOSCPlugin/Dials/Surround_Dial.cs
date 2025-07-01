namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Surround_Dial : Group_Dial_Base
    {
        public Surround_Dial()
            : base(
                groupName: "Surround",
                soloAddresses: new[] { "LSS", "RSS", "LRS", "RRS" }, // 只需要传入相对路径
                muteAddresses: new[] { "LSS", "RSS", "LRS", "RRS" }, // 只需要传入相对路径
                displayName: "Surround Dial",
                description: "控制Surround组的Solo和Mute状态",
                fontSize: 12)
        { }
    }
}
