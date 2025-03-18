namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class SUB_L_Button : Channel_Base
    {
        public SUB_L_Button() : base(
            channelName: "SUB L",
            displayName: "SUB L Button",
            description: "SUB Left 通道按钮")
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {


            bitmap.DrawText(
                text: "L",
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

