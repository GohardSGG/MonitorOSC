namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Rear_Dial : Group_Dial_Base
    {
        public Rear_Dial()
            : base(
                groupName: "Rear",
                soloAddresses: new[] { "LRS", "RRS" }, // 只需要传入相对路径
                muteAddresses: new[] { "LRS", "RRS" }, // 只需要传入相对路径
                displayName: "Rear Dial",
                description: "控制Rear组的Solo和Mute状态")
        { }
    }
}
