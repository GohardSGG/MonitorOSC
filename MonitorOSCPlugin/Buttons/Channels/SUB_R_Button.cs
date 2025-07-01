namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class SUB_R_Button : Channel_Button_Base
    {
        public SUB_R_Button() : base(
            channelName: "SUB R",
            displayName: "SUB R Button",
            description: "SUB Right 通道按钮")
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {


            bitmap.DrawText(
                text: "R",
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

