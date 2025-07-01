namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Top_Back_Dial : Group_Dial_Base
    {
        public Top_Back_Dial()
            : base(
                groupName: "Top Back",
                soloAddresses: new[] { "LTB", "RTB" }, // 只需要传入相对路径
                muteAddresses: new[] { "LTB", "RTB" }, // 只需要传入相对路径
                displayName: "Top Back Dial",
                description: "控制Top Back组的Solo和Mute状态",
                fontSize: 17)
        { }
    }
}
