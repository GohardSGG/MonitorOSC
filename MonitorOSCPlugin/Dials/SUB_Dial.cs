namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class SUB_Dial : Group_Dial_Base
    {
        public SUB_Dial()
            : base(
                groupName: "SUB",
                soloAddresses: new[] { "SUB F", "SUB B", "SUB L", "SUB R" }, // 只需要传入相对路径
                muteAddresses: new[] { "SUB F", "SUB B", "SUB L", "SUB R" }, // 只需要传入相对路径
                displayName: "SUB Dial",
                description: "控制SUB组的Solo和Mute状态")
        { }
    }
}
