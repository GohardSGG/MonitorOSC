namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class SUB_F_Button : Channel_Base
    {
        public SUB_F_Button() : base(
            channelName: "SUB F",
            displayName: "SUB F Button",
            description: "SUB Front 通道按钮")
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {


            bitmap.DrawText(
                text: "F",
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

