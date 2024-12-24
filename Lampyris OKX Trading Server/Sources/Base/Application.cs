/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-12
 */
namespace HongJinInvestment.OKX.Server;

public class Application:Singleton<Application>
{
    // 运行标志
    private bool m_AppRunning = false;

    public bool AppRunning => m_AppRunning;

    public Application() 
    {
        AppDomain.CurrentDomain.ProcessExit += new EventHandler((object? sender, EventArgs e) => {
            Quit();
        });
    }
    private readonly List<BehaviourSingletonBase> m_InstanceList = new List<BehaviourSingletonBase>()
    {
        CallTimer.Instance,
        QuoteManager.Instance,
        OkxServer.Instance,
        CoroutineManager.Instance,
    };

    public void Quit()
    {
        if (m_AppRunning)
        {
            m_AppRunning = false;
            for (int i = m_InstanceList.Count - 1; i >= 0; i--)
            {
                var behaviourSingletonBase = m_InstanceList[i];
                behaviourSingletonBase.OnDestroy();
            }
            SerializationManager.Instance.OnDestroy();
        }
    }

    public int Run()
    {
        try
        {
            foreach (var behaviourSingletonBase in m_InstanceList)
            {
                behaviourSingletonBase.OnStart();
            }
            SerializationManager.Instance.OnStart();

            m_AppRunning = true;
             
            long timestamp = 0;
            while (m_AppRunning)
            {
                long timestamp2 = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                long deltaTime = timestamp2 - timestamp;
                foreach (var behaviourSingletonBase in m_InstanceList)
                {
                    behaviourSingletonBase.OnUpdate(deltaTime);
                }
                timestamp = timestamp2;
            }
        }
        catch (Exception ex)
        {
            LogManager.Instance.LogError($"Uncaught exception:{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            return 1;
        }

        return 0;
    }
}