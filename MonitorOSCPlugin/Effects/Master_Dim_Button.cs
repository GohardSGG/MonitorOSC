namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    public class Master_Dim_Button : Effect_Base
    {
        public Master_Dim_Button() : base(
            channelName: "Master Dim",
            displayName: "Master Dim Button",
            description: "主通道衰减控制",
            oscAddress: "Master Volume/Dim",
            activeColor: new BitmapColor(255, 255, 0)) // 黄色背景
        { }

        protected override void DrawButtonContent(BitmapBuilder bitmap)
        {
            // 根据激活状态切换文字颜色
            var textColor = _isActive
                ? BitmapColor.White  // 激活时白色文字
                : new BitmapColor(255, 255, 0); // 默认黄色文字

            bitmap.DrawText(
                text: "DIM",
                fontSize: 32,
                color: textColor);

        }
    }
}