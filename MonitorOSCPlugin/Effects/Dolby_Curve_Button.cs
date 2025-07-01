namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class Dolby_Curve_Button : Effect_Button_Base
    {
        public Dolby_Curve_Button() : base(
            channelName: "Dolby Curve",
            displayName: "Dolby Curve",
            description: "Dolby Curve",
            oscAddress: "Master Effect/Dolby Curve",
            activeColor: new BitmapColor(136, 226, 255)) // 黄色背景
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {
            // 根据激活状态切换文字颜色
            var textColor_1 = _isActive
                ? BitmapColor.White  // 激活时白色文字
                : new BitmapColor(136, 226, 255);
            // 根据激活状态切换文字颜色
            var textColor_2 = _isActive
                ? BitmapColor.White  // 激活时白色文字
                : BitmapColor.White;


            bitmap.DrawText(
                text: "Dolby",
                x: 25,
                y: 25,
                width: 28,
                height: 28,
                fontSize: 28,
                color: textColor_1
            );
            bitmap.DrawText(
                text: "Curve",
                x: 55,
                y: 50,
                width: 14,
                height: 14,
                fontSize: 14,
                color: textColor_2
            );

        }
    }
}