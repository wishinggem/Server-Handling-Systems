using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.Serialization;

class MainControl
{
    public static string fromEmail;
    public static string password;
    public static string toEmail;
    public static string serverName;
    public static string folderPath;
    public static string fileName;
    public static List<string> driveNameList;

    private static string filePath;
    private static string connected = null;

    private static float storageWarningLimit; //in Gigabytes

    private static int reportMaxCount;

    private static int logAmount;

    private static string readFilePath;
    private static string settingsFileName = "ReportConfig.txt";

    [Obsolete]
    static void Main()
    {
        string sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
        readFilePath = string.Concat("/home/", sudoUser, "/", settingsFileName);
        SettingObject settings = ReadFromBinaryFile<SettingObject>(readFilePath);
        fromEmail = settings.email;
        password = settings.password;
        toEmail = settings.email;
        serverName = settings.serverName;
        folderPath = settings.folderPath;
        fileName = settings.fileName;
        driveNameList = settings.driveList;
        storageWarningLimit = settings.storageWarningLimit;
        reportMaxCount = settings.reportMaxCount;
        logAmount = settings.logAmount;
        filePath = folderPath + fileName;
        CreateReport(filePath);
    }

    public static void CreateReport(string filePath)
    {
        CreateFile(filePath);
    }

    public static void CreateFile(string fileLocationPath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
        FileInfo[] files = dirInfo.GetFiles().OrderBy(f => f.CreationTime).ToArray();

        int num = 1;

        if (files.Length > 0)
        {
            string[] fileNumbers = files.Select(file =>
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
                string[] parts = fileNameWithoutExtension.Split('-');
                if (parts.Length > 1)
                {
                    return parts[parts.Length - 1];
                }
                return "0"; // Return 0 if there's no valid number in the filename
            }).ToArray();

            int[] fileNumArray = fileNumbers.Select(int.Parse).ToArray();
            num = fileNumArray.Max() + 1;
        }

        if (dirInfo.GetFiles().Length >= reportMaxCount)
        {
            File.Delete(files.First().FullName);
        }

        FileWriting(Path.Combine(folderPath, $"{fileLocationPath}-{num}.txt"));
    }

    private static void FileWriting(string filePath) //gets info from all info functions then writes to the file in an easy to read format formate data line data line and then x amount of lines of the log at the end of the file
    {
        List<Tuple<string, float>> driveStorageList = new List<Tuple<string, float>>();
        foreach (Tuple<string, float> driveData in GetTotalFreeSpace(driveNameList))
        {
            driveStorageList.Add(driveData);
        }
        if (CheckForInternetConnection())
            connected = "Connected";
        else
            connected = "Not Connected";
        StreamWriter file = File.CreateText(filePath);
        file.WriteLine(string.Concat("Server: ", serverName));
        file.WriteLine();
        file.WriteLine(string.Concat("Connection: ", connected));
        file.WriteLine();
        foreach (Tuple<string, float> driveData in driveStorageList)
        {
            file.WriteLine(string.Concat("Remaining Storage For Drive; ", driveData.Item1, " ---- " , driveData.Item2, " GB"));
            if (NearFull(driveData.Item2))
            {
                file.WriteLine($"Warning Low Storage for; {driveData.Item1}");
            }
        }
        file.WriteLine();
        file.WriteLine(string.Concat("IP Address: ", GetIP()));
        DateTime dateTime = DateTime.Now;
        int day = dateTime.Day;
        int month = dateTime.Month;
        int year = dateTime.Year;
        int hour = dateTime.Hour;
        int minute = dateTime.Minute;
        int second = dateTime.Second;
        string hour1;
        string hourString = hour.ToString();
        if (hourString.Length < 2)
        {
            hour1 = string.Concat("0", hourString);
        }
        else
        {
            hour1 = hourString;
        }
        string minute1;
        string minuteString = minute.ToString();
        if (minuteString.Length < 2)
        {
            minute1 = string.Concat("0", minuteString);
        }
        else
        {
            minute1 = minuteString;
        }
        string second1;
        string secondString = second.ToString();
        if (secondString.Length < 2)
        {
            second1 = string.Concat("0", secondString);
        }
        else
        {
            second1 = secondString;
        }
        file.WriteLine();
        file.WriteLine(string.Concat("Time Of Report: ", hour1, ":", minute1, ":", second1, "  ", day, "/", month, "/", year));
        for (int i = 0; i < 10; i++)
        {
            file.WriteLine();
        }
        file.WriteLine(string.Concat("System Logs: "));
        file.WriteLine();
        file.WriteLine(GetLogsNew(logAmount));
        file.Close();
    }

    private static bool NearFull(float storageRemaining) // checks if the storage passes in is bellow the threshold
    {
        if (storageRemaining <= storageWarningLimit)
            return true;
        else
            return false;
    }

    private static List<Tuple<string, float>> GetTotalFreeSpace(List<string> names)
    {
        List<Tuple<string, float>> driveStorage = new List<Tuple<string, float>>();
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady && names.Contains(drive.Name))
            {
                driveStorage.Add(new Tuple<string, float>(drive.Name, MathF.Floor(Convert.ToSingle(Convert.ToDouble((drive.AvailableFreeSpace / 1000 / 1000 / 1000)) * 0.931323)))); 
                //converts from binary prefix to metric prefix and converts to GB then floors the outcome so the display size is they same as if you looked at the drive on file explorer
            }
        }
        return driveStorage;
    }

    public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null) //checks if a ping sent is recived back
    {
        try
        {
            url = "http://www.gstatic.com/generate_204";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.Timeout = timeoutMs;
            using (var response = (HttpWebResponse)request.GetResponse())
                return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetIP() //gets current ip of device
    {
        string hostName = Dns.GetHostName();
        return
        Dns.GetHostByName(hostName).AddressList[0].ToString();
    }

    private static string GetLogsNew(int numLogsToRetrieve)
    {
        string LogFilePath = "/var/log/syslog"; // Replace with the actual log file path

        if (File.Exists(LogFilePath))
        {
            // Read all lines from the log file
            string[] allLines = File.ReadAllLines(LogFilePath);

            // Calculate the start index to retrieve the last 'numLogsToRetrieve' lines
            int startIndex = Math.Max(0, allLines.Length - numLogsToRetrieve);

            // Select the last 'numLogsToRetrieve' lines
            string[] selectedLines = allLines.Skip(startIndex).ToArray();

            // Join the selected lines into a single string
            string output = string.Empty;
            selectedLines.Reverse();
            for (int i = 0; i < selectedLines.Count(); i++)
            {
                output += Environment.NewLine;
                output += selectedLines[i];
            }

            return output;
        }
        else
        {
            return "Log file not found";
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

sealed class CustomizedBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type returntype = null;
        string sharedAssemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        assemblyName = Assembly.GetExecutingAssembly().FullName;
        typeName = typeName.Replace(sharedAssemblyName, assemblyName);
        returntype =
                Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

        return returntype;
    }

    public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        base.BindToName(serializedType, out assemblyName, out typeName);
        assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    }
}
