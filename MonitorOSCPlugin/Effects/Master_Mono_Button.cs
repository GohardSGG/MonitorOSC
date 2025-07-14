namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class Master_Mono_Button : Effect_Button_Base
    {
        public Master_Mono_Button() : base(
            channelName: "Master Mono",
            displayName: "Master Mono Button",
            description: "主通道Mono控制",
            oscAddress: "Master/Effect/Mono",
            activeColor: new BitmapColor(255, 0, 0)) // 
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {
            // 根据激活状态切换文字颜色
            var textColor = _isActive
                ? BitmapColor.White  // 激活时白色文字
                : new BitmapColor(255, 0, 0); // 

            bitmap.DrawText(
                text: "Mono",
                fontSize: 32,
                color: textColor);

        }
    }
}