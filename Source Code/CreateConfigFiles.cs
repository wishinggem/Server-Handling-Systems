using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class ConfigFileCreationg
{
    public static string recorderName = "MasterRecorder.txt";
    public static string configName = "ReportConfig.txt";

    public static string configPath;
    public static string recorderPath;

    [Obsolete]
    static void Main()
    {
        string sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
        configPath = string.Concat("/home/", sudoUser, "/", configName);
        recorderPath = string.Concat("/home/", sudoUser, "/", recorderName);
        SettingObject settings = new SettingObject();
        MasterRecorder recorder = new MasterRecorder();
        if (!File.Exists(configPath))
        {
            FileHandler.WriteToBinaryFile<SettingObject>(configPath, settings);
        }
        if (!File.Exists(recorderPath))
        {
            FileHandler.WriteToBinaryFile<MasterRecorder>(recorderPath, recorder);
        }
        Console.WriteLine("Config and Data Files Created");
    }
}

[Serializable]
public class SettingObject
{
    public string email;
    public string password;
    public string serverName;
    public string folderPath;
    public string fileName;
    public List<string> driveList;
    public float storageWarningLimit;
    public int reportMaxCount;
    public int logAmount;
    public int reportHourly;
    public string reportCMD;
    public string messageCMD;

    public SettingObject()
    {
        email = string.Empty;
        password = string.Empty;
        serverName = string.Empty;
        folderPath = string.Empty;
        fileName = "StatusFile";
        driveList = new List<string>();
        storageWarningLimit = 20;
        reportMaxCount = 36;
        logAmount = 20;
        reportHourly = 6;
        reportCMD = string.Empty;
        messageCMD = string.Empty;
    }
}

[Serializable]
public class MasterRecorder
{
    public int callNumber;

    public MasterRecorder()
    {
        callNumber = 0;
    }
}

public static class FileHandler
{
    [Obsolete]
    public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
    {
        using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(stream, objectToWrite);
        }
    }

    [Obsolete]
    public static T ReadFromBinaryFile<T>(string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Open))
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Binder = new CustomizedBinder();
            return (T)binaryFormatter.Deserialize(stream);
        }
    }
}

sealed class CustomizedBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type returntype = null;
        string sharedAssemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        assemblyName = Assembly.GetExecutingAssembly().FullName;
        typeName = typeName.Replace(sharedAssemblyName, assemblyName);
        returntype = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

        return returntype;
    }

    public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        base.BindToName(serializedType, out assemblyName, out typeName);
        assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    }
}
