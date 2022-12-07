﻿using Consul;
using Geek.Server.App.Net;
using Geek.Server.Core.Actors;
using Geek.Server.Core.Center;
using Geek.Server.Core.Comps;
using Geek.Server.Core.Hotfix;
using PolymorphicMessagePack;
using Geek.Server.Core.Storage;
using Geek.Server.Proto;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Geek.Server.App.Common
{
    internal class AppStartUp
    {
        static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static async Task Enter()
        {
            try
            {
                var flag = Start();
                if (!flag) return; //启动服务器失败

                await Task.Run(async () =>
                {
                    //连接中心rpc
                    if (await AppNetMgr.ConnectCenter())
                    {
                        var getNode = () =>
                        {
                            return new NetNode
                            {
                                NodeId = Settings.ServerId,
                                Ip = Settings.LocalIp,
                                TcpPort = Settings.TcpPort,
                                HttpPort = Settings.HttpPort,
                                Type = NodeType.Game
                            };
                        };

                        if (!await AppNetMgr.CenterRpcClient.Register(getNode))
                            throw new Exception($"中心服注册失败... {JsonConvert.SerializeObject(getNode())}");

                        //到中心服拉取通用配置
                        await AppNetMgr.GetGlobalConfig();

                        Log.Info($"launch embedded db...");
                        GameDB.Init();
                        GameDB.Open();

                        Log.Info($"regist comps...");
                        await CompRegister.Init();
                        Log.Info($"load hotfix module");
                        await HotfixMgr.LoadHotfixModule();

                        Settings.InsAs<AppSetting>().ServerReady = true;
                        _ = AppNetMgr.ConnectGateway();
                    }
                });

                Log.Info("进入游戏主循环...");
                Console.WriteLine("***进入游戏主循环***");
                Settings.LauchTime = DateTime.Now;
                Settings.AppRunning = true;
                TimeSpan delay = TimeSpan.FromSeconds(1);
                while (Settings.AppRunning)
                {
                    await Task.Delay(delay);
                }
            }
            catch (Exception e)
            {
                var re = e.InnerException != null ? e.InnerException : e;
                Console.WriteLine($"服务器执行异常，e:{re}");
                Log.Fatal(re);
            }

            Console.WriteLine($"退出服务器开始");
            await HotfixMgr.Stop();
            Console.WriteLine($"退出服务器成功");
        }

        private static bool Start()
        {
            try
            {
                Settings.Load<AppSetting>("Configs/app_config.json", ServerType.Game);
                Console.WriteLine("init NLog config...");
                LayoutRenderer.Register<NLogConfigurationLayoutRender>("logConfiguration");
                LogManager.Configuration = new XmlLoggingConfiguration("Configs/app_log.config");
                LogManager.AutoShutdown = false;

                PolymorphicTypeMapper.Register(typeof(AppStartUp).Assembly); //app
                PolymorphicRegister.Load();
                PolymorphicResolver.Init();
                //GeekServerAppPolymorphicDBStateRegister.Load();

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"启动服务器失败,异常:{e}");
                return false;
            }
        }
    }
}
