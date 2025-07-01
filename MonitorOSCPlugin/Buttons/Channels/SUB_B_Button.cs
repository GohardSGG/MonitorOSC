namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class SUB_B_Button : Channel_Button_Base
    {
        public SUB_B_Button() : base(
            channelName: "SUB B",
            displayName: "SUB B Button",
            description: "SUB Back 通道按钮")
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {


            bitmap.DrawText(
                text: "B",
                fontSize: 38,
                color: new BitmapColor(136, 226, 255)
            );
            bitmap.DrawText(
                text: "SUB",
                x: 55,
                y: 58,
                width: 14,
                height: 14,
                fontSize: 14,
                color: BitmapColor.White
            );

        }
    }
}

