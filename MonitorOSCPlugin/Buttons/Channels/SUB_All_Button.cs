namespace Loupedeck.MonitorOSCPlugin.Buttons
{
    using Loupedeck;

    /// <summary>
    /// ���� SUB ������ͨ�� ("SUB F", "SUB B", "SUB L", "SUB R") �İ�ť��
    /// </summary>
    public class SUB_All_Button : Group_Button_Base
    {
        public SUB_All_Button()
            : base(
                groupName: "SUB",
                // �ο� SUB_Dial.cs �����ͨ��
                channelNames: new[] { "SUB F", "SUB B", "SUB L", "SUB R" },
                displayName: "SUB All Button",
                description: "����SUB������ͨ����Solo��Mute״̬")
        { }
   }
}