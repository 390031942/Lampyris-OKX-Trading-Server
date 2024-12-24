/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-05
 */
namespace HongJinInvestment.OKX.Server;

using System.Text.Json;

/*
 *  序列化工具类，负责将App设置，股票分析数据等保存到本地磁盘
*/
public class SerializationManager: BehaviourSingleton<SerializationManager>
{
    private class SerializationInfo
    {
        public object serializableObject;
        public string name;
    }

    private readonly List<SerializationInfo> m_serializableInfo = new List<SerializationInfo>();

    public override void OnStart()
    {

    }
    
    // Unused
    public override void OnUpdate(float deltaTime)
    {

    }

    public void Register(object serializableObject, string specificName = "")
    {
        if (serializableObject != null)
        {
            m_serializableInfo.Add(new SerializationInfo()
            {
                serializableObject = serializableObject,
                name = string.IsNullOrEmpty(specificName) ? serializableObject.GetType().Name : specificName
            });
        }
    }

    public T TryDeserializeObjectFromFile<T>(string specificName = "")
    {
        string filePath = Path.Combine(PathUtil.SerializedDataSavePath, (string.IsNullOrEmpty(specificName) ? typeof(T).Name : specificName) + ".bin");

        if(File.Exists(filePath))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                return JsonSerializer.Deserialize<T>(stream);
            }
        }

        return default(T);
    }
    
    public override void OnDestroy()
    {
        foreach (SerializationInfo serializationInfo in m_serializableInfo)
        {
            string filePath = Path.Combine(PathUtil.SerializedDataSavePath, serializationInfo.name + ".bin");
            using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
            {
                JsonSerializer.Serialize(stream, serializationInfo.serializableObject);
            }
        }
    }
}
