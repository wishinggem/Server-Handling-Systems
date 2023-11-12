using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

class SendLatestReports
{
    public static string fromEmail;
    public static string password;
    public static string toEmail;
    public static string serverName;
    public static string folderPath;
    public static string fileName;
    public static List<string> driveNameList;

    private static float storageWarningLimit; //in Gigabytes

    private static int reportHourlyRate;

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
        reportHourlyRate = settings.reportHourly;
        SendReports();
    }

    public static void SendReports()
    {
        SendEmail();
    }

    public static void SendEmail()
    {
        var fromAddress = new MailAddress(fromEmail);   //covnerts strings to email address for sending
        var toAddress = new MailAddress(toEmail);
        string fromPassword = password;
        string subject = $"Server Report {serverName}";
        bool storageLow = false;
        List<Tuple<string, float>> driveStorageList = new List<Tuple<string, float>>();
        List<string> lowSpaceNames = new List<string>();
        foreach (Tuple<string, float> driveData in GetTotalFreeSpace(driveNameList))
        {
            if (NearFull(driveData.Item2))
            {
                lowSpaceNames.Add(driveData.Item1);
                storageLow = true;
            }
        }
        if (storageLow)
        {
            subject = $"Server Report {serverName}: REMAINING STORAGE BELLOW {storageWarningLimit} GB; For Drives: ";
            foreach (string name in lowSpaceNames)
            {
                subject += string.Concat(name, " ");
            }
        }
        else
        {
            subject = $"Server Report {serverName}";
        }

        var smtp = new SmtpClient
        {
            Host = "smtp.office365.com",  //using microsoft smtp server
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, fromPassword)

        };
        using (var message = new MailMessage(fromAddress, toAddress) //sets up message
        {
            Subject = subject,
        })
        {
            string[] fileNames = Directory.GetFiles(folderPath);

            // Sort files by creation date in ascending order
            Array.Sort(fileNames, (a, b) => File.GetCreationTime(b).CompareTo(File.GetCreationTime(a)));

            for (int i = 0; i < reportHourlyRate; i++)
            {
                string fileSource = fileNames[i];  // Change the index to access files in ascending order
                FileStream file = File.Open(fileSource, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(file, Encoding.UTF8);
                ContentType ct = new ContentType("text/plain");
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(reader.ReadToEnd()));

                var attach = new System.Net.Mail.Attachment(memoryStream, ct);
                string[] fileSplit = fileSource.Split("/");
                attach.Name = fileSplit[fileSplit.Length - 1];

                message.Attachments.Add(attach);
            }
            message.Body = string.Concat("Server Reports From The Last: ", reportHourlyRate, " Hour(s) For: ", serverName);
            smtp.Send(message);
        }
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
