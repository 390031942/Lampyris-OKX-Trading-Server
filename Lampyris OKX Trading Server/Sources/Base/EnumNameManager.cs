/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-05
 */
namespace HongJinInvestment.OKX.Server;

using System.Reflection;


[AttributeUsage(AttributeTargets.Field)]
public class NamedValueAttribute : Attribute
{
    public string Name { get; private set; }

    public NamedValueAttribute(string name)
    {
        this.Name = name;
    }
}

public class EnumNameManager
{
    // ���ڴ洢ö��ֵ�����Ƶ�ӳ��
    private static Dictionary<Enum, string>                   ms_NameMap      = new ();
    private static Dictionary<Type, Dictionary<string, Enum>> ms_NameValueMap = new ();
    private static Dictionary<Type, List<string>>             ms_NameListMap  = new ();

    static EnumNameManager()
    {
        // �ھ�̬���캯����ɨ�貢��¼����ö��ֵ������
        ScanAndRecordEnumNames();
    }

    // ɨ������ö�����ͣ���¼���ǵ�NamedValueAttribute
    private static void ScanAndRecordEnumNames()
    {
        var enumTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsEnum);

        foreach (var type in enumTypes)
        {
            var nameList = ms_NameListMap[type] = new List<string>();
            ms_NameValueMap[type] = new Dictionary<string, Enum>();
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<NamedValueAttribute>();
                if (attribute != null)
                {
                    var enumValue = field.GetValue(null) as Enum;
                    ms_NameMap[enumValue] = attribute.Name;
                    ms_NameValueMap[type][attribute.Name] = enumValue; 
                    nameList.Add(attribute.Name);
                }
            }
        }
    }

    // ��ȡö��ֵ�����ƣ����û���ҵ��򷵻�null
    public static string GetName(Enum enumValue)
    {
        if (ms_NameMap.TryGetValue(enumValue, out string name))
        {
            return name;
        }

        return null;
    }

    // ��ȡö��ֵ�������б�
    public static List<string> GetNames(Type enumType)
    {
        if (ms_NameListMap.TryGetValue(enumType, out List<string> namelist))
        {
            return namelist;
        }

        return null;
    }

    // ����
    public static Enum GetEnum(Type enumType, string name)
    {
        if (ms_NameValueMap.TryGetValue(enumType, out var map))
        {
            if(map.TryGetValue(name, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
