using PolymorphicMessagePack;
namespace Geek.Server.Proto
{
	public partial class PolymorphicRegister
	{
	    static PolymorphicRegister()
        {
            System.Console.WriteLine("***PolymorphicRegister Init***"); 
            Register();
        }

		public static void Register()
        {
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqBagInfo>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResBagInfo>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqComposePet>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResComposePet>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqUseItem>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqSellItem>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResItemChange>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqConnectGate>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResConnectGate>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.NodeNotFound>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqInnerConnectGate>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResInnerConnectGate>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.PlayerDisconnected>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.A>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.B>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ReqLogin>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResLogin>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResLevelUp>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.HearBeat>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResErrorCode>();
			PolymorphicTypeMapper.Register<Geek.Server.Proto.ResPrompt>();
        }
	}
}
