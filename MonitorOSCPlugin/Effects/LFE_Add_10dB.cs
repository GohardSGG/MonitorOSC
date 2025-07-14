namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class LFE_Add_10dB : Effect_Button_Base
    {
        public LFE_Add_10dB() : base(
            channelName: "LFE Add 10dB",
            displayName: "LFE +10dB",
            description: "LFE通道 +10dB",
            oscAddress: "LFE/Add_10dB",
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
                text: "+10dB",
                x: 25,
                y: 25,
                width: 28,
                height: 28,
                fontSize: 28,
                color: textColor_1
            );
            bitmap.DrawText(
                text: "LFE",
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