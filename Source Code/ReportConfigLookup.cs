using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System;

public class TerminalChecker
{
    public static string recorderLocation = "/home/chinatsu/Reporting/";
    public static string recorderName = "MasterRecorder.txt";
    public static string configLocation = "/home/chinatsu/Reporting/";
    public static string configName = "ReportConfig.txt";

    public static string configPath;
    public static string recorderPath;

    [Obsolete]
    static void Main()
    {
    	string sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
configPath = string.Concat("/home/", sudoUser, "/", configName);
recorderPath = string.Concat("/home/", sudoUser, "/", recorderName);

        if (File.Exists(configPath) && File.Exists(recorderPath))
        {
            SettingObject settings = FileHandler.ReadFromBinaryFile<SettingObject>(configPath);
            MasterRecorder recorder = FileHandler.ReadFromBinaryFile<MasterRecorder>(recorderPath);

            Console.WriteLine();
            Console.WriteLine($"{configName} elements-");
            Console.WriteLine(string.Concat("Email: ", settings.email));
            Console.WriteLine(string.Concat("Server Name: ", settings.serverName));
            Console.WriteLine(string.Concat("Status Folder Path: ", settings.folderPath));
            Console.WriteLine(string.Concat("Status File Name: ", settings.fileName));
            string drives = string.Empty;
            foreach (string drive in settings.driveList)
            {
                drives += string.Concat(drive, ", ");
            }
            Console.WriteLine(string.Concat("Lsited Drives: ", drives));
            Console.WriteLine(string.Concat("Storage Low Warning: ", settings.storageWarningLimit, " GB"));
            Console.WriteLine(string.Concat("Log Line Count: ", settings.logAmount));
            Console.WriteLine(string.Concat("Report Message Rate: ", settings.reportHourly));
            Console.WriteLine(string.Concat("Report Creation Command: ", settings.reportCMD));
            Console.WriteLine(string.Concat("Message Send Command: ", settings.messageCMD));

            Console.WriteLine();

            Console.WriteLine($"{recorderName} elements-");
            Console.WriteLine(string.Concat("Report Call Number: ", recorder.callNumber));
            Console.WriteLine();
        }
        else if (File.Exists(recorderPath))
        {
            MasterRecorder recorder = FileHandler.ReadFromBinaryFile<MasterRecorder>(recorderPath);
            Console.WriteLine();
            Console.WriteLine($"{recorderName} elements-");
            Console.WriteLine(string.Concat("Report Call Number: ", recorder.callNumber));

            Console.WriteLine();
            Console.WriteLine("ERROR: cannot locate config file");
            Console.WriteLine("ReportConfig.txt file does not exist please create file using command or application");
            Console.WriteLine();
        }
        else if (File.Exists(configPath))
        {
            SettingObject settings = FileHandler.ReadFromBinaryFile<SettingObject>(configPath);
            Console.WriteLine();
            Console.WriteLine($"{configName} elements-");
            Console.WriteLine(string.Concat("Email: ", settings.email));
            Console.WriteLine(string.Concat("Server Name: ", settings.serverName));
            Console.WriteLine(string.Concat("Status Folder Path: ", settings.folderPath));
            Console.WriteLine(string.Concat("Status File Name: ", settings.fileName));
            string drives = string.Empty;
            foreach (string drive in settings.driveList)
            {
                drives += string.Concat(drive, ", ");
            }
            Console.WriteLine(string.Concat("Lsited Drives: ", drives));
            Console.WriteLine(string.Concat("Storage Low Warning: ", settings.storageWarningLimit, " GB"));
            Console.WriteLine(string.Concat("Log Line Count: ", settings.logAmount));
            Console.WriteLine(string.Concat("Report Message Rate: ", settings.reportHourly));
            Console.WriteLine(string.Concat("Report Creation Command: ", settings.reportCMD));
            Console.WriteLine(string.Concat("Message Send Command: ", settings.messageCMD));

            Console.WriteLine();
            Console.WriteLine("ERROR: cannot locate recorder file");
            Console.WriteLine("MasterRecorder.txt file does not exist please create file using command or wait for file to be created from cronjob");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("ERROR: cannot locate any config file");
        }
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
