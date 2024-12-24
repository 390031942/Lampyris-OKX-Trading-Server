/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-12
 */
namespace HongJinInvestment.OKX.Server;

public static class BehaviourSingletonRegistry
{
    private static readonly List<BehaviourSingletonBase> m_BehaviourSingletonList = 
        new List<BehaviourSingletonBase>();

    public static List<BehaviourSingletonBase> BehaviourSingletonList => m_BehaviourSingletonList;
    
    public static void Register(BehaviourSingletonBase behaviourSingleton)
    {
        m_BehaviourSingletonList.Add(behaviourSingleton);
    }
}

public abstract class BehaviourSingletonBase
{
    public virtual void OnStart() { }

    public virtual void OnUpdate(float deltaTime) { }

    public virtual void OnDestroy() { }
}

public abstract class BehaviourSingleton<T>:BehaviourSingletonBase where T : BehaviourSingletonBase, new()
{
    private static T? m_instance;

    public static T Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new T();
            }

            return m_instance;
        }
    }
}