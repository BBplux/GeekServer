namespace Geek.Server.Gateway.Common
{
    public class GateSettings : BaseSetting
    {
        public int InnerUdpPort { get; set; }
        public int OuterPort { get; set; }  //tcp udp���԰�ͬһ���˿�  �����ⲿ�˿ڿ���ͬһ��
        public int MaxClientCount { get; set; }
    }
}