namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;

    /// <summary>
    /// 控制 SUB 组所有通道 ("SUB F", "SUB B", "SUB L", "SUB R") 的按钮。
    /// </summary>
    public class SUB_All_Button : Group_Button_Base
    {
        public SUB_All_Button()
            : base(
                groupName: "SUB",
                // 参考 SUB_Dial.cs 定义的通道
                channelNames: new[] { "SUB F", "SUB B", "SUB L", "SUB R" },
                displayName: "SUB All Button",
                description: "控制SUB组所有通道的Solo和Mute状态")
        { }
   }
}