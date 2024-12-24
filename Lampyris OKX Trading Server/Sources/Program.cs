/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-05
 */
namespace HongJinInvestment.OKX.Server;

public static class Program
{
    private static int Main(string[] args)
    {
        LogManager.Instance.AddLogger(new ConsoleLogger());
        LogManager.Instance.AddLogger(new FileLogger("D:\\hji_okx_server.log"));
        
        LogManager.Instance.LogInfo("Starting server...");

        return Application.Instance.Run();
    }
}