namespace Loupedeck.MonitorOSCPlugin.Dials
{
    public class Side_Dial : Group_Base
    {
        public Side_Dial()
            : base(
                groupName: "Side",
                soloAddresses: new[] { "LSS", "RSS" }, // 只需要传入相对路径
                muteAddresses: new[] { "LSS", "RSS" }, // 只需要传入相对路径
                displayName: "Side Dial",
                description: "控制Side组的Solo和Mute状态")
        { }
    }
}
