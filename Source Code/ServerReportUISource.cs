using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainControl : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailIn;
    [SerializeField] private TMP_InputField passwordIn;
    [SerializeField] private TMP_InputField sNameIn;
    [SerializeField] private TMP_InputField folderPathIn;
    [SerializeField] private TMP_InputField fileNameIn;
    [SerializeField] private TMP_InputField driveIn;
    [SerializeField] private TMP_InputField storageWarningIn;
    [SerializeField] private TMP_InputField reportMaxIn;
    [SerializeField] private TMP_InputField logAmountIn;
    [SerializeField] private TMP_InputField hourlyRateIn;
    [SerializeField] private TMP_InputField reportCMDIn;
    [SerializeField] private TMP_InputField messageCMDIn;

    [Header("Buttons")]
    [SerializeField] private Button driveAdd;
    [SerializeField] private Button driveClear;
    [SerializeField] private Button allApply;
    [SerializeField] private Button close;

    [Header("Info")]
    [SerializeField] private TextMeshProUGUI drives;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime;
    [SerializeField] private float errorDisplayTime;

    private string fileName = "ReportConfig.txt";
    private string filePath;

    [Obsolete]
    // Start is called before the first frame update
    void Start()
    {
        SettingObject settings = null;
        string sudoUser = Environment.GetEnvironmentVariable("USER");
        filePath = string.Concat("/home/", sudoUser, "/", fileName);
        if (File.Exists(filePath))
        {
            settings = FileHandler.ReadFromBinaryFile<SettingObject>(filePath);
        }
        else
        {
            settings = new SettingObject();
        }
        PopInputText(settings);
        driveAdd.onClick.AddListener(delegate { AddDrive(settings); });
        driveClear.onClick.AddListener(delegate { ClearDrives(settings); });
        allApply.onClick.AddListener(delegate { ApplySettings(settings); });
        close.onClick.AddListener(delegate { Close(); });
    }

    private void Close()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void PopInputText(SettingObject settings)
    {
        drives.text = string.Empty;
        /*emailIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.email;
        passwordIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.password;
        sNameIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.serverName;
        folderPathIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.folderPath;
        fileNameIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.fileName;
        storageWarningIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.storageWarningLimit.ToString();
        reportMaxIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.reportMaxCount.ToString();
        logAmountIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.logAmount.ToString();
        hourlyRateIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.reportHourly.ToString();
        reportCMDIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.reportCMD.ToString();
        messageCMDIn.placeholder.GetComponent<TextMeshProUGUI>().text = settings.messageCMD.ToString();*/
        string drivesL = string.Empty;
        foreach (string drive in settings.driveList)
        {
            drivesL += string.Concat(drive, ", ");
        }
        drives.text += drivesL;
        emailIn.text = settings.email;
        passwordIn.text = settings.password;
        sNameIn.text = settings.serverName;
        folderPathIn.text = settings.folderPath;
        fileNameIn.text = settings.fileName;
        storageWarningIn.text = settings.storageWarningLimit.ToString();
        reportMaxIn.text = settings.reportMaxCount.ToString();
        logAmountIn.text = settings.logAmount.ToString();
        hourlyRateIn.text = settings.reportHourly.ToString();
        reportCMDIn.text = settings.reportCMD;
        messageCMDIn.text = settings.messageCMD;
    }

    private void AddDrive(SettingObject settings)
    {
        settings.driveList.Add(driveIn.text);
        driveIn.text = null;
        string drivesL = string.Empty;
        foreach (string drive in settings.driveList)
        {
            drivesL += string.Concat(drive, ", ");
        }
        drives.text = drivesL;
    }

    private void ClearDrives(SettingObject settings)
    {
        settings.driveList.Clear();
        driveIn.text = null;
        drives.text = string.Empty;
    }

    [Obsolete]
    private void ApplySettings(SettingObject settings)
    {
        bool isError = false;
        string email = emailIn.text;
        string password = passwordIn.text;
        string sName = sNameIn.text;
        string folderPath = folderPathIn.text;
        string reportFileName = fileNameIn.text;
        float storageWarningLimit = float.Parse(storageWarningIn.text);
        int reportMax = int.Parse(reportMaxIn.text);
        int logAmount = int.Parse(logAmountIn.text);
        int hourlyRate = int.Parse(hourlyRateIn.text);
        string repCMD = reportCMDIn.text;
        string mesCMD = messageCMDIn.text;
        try
        {
            bool isEmpty = (email == string.Empty) || (password == string.Empty) || (sName == string.Empty) ||
                (folderPath == string.Empty) || (reportFileName == string.Empty) || (storageWarningLimit < 0) ||
                (reportMax <= 0) || (logAmount < 0) || (settings.driveList.Count == 0) || (hourlyRate <= 0) || (repCMD == string.Empty) || (mesCMD == string.Empty);
            if (isEmpty)
            {
                throw new SaveError("Data Field Missing Please Enter All Data");
            }
            else
            {
                settings.email = email;
                settings.password = password;
                settings.serverName = sName;
                settings.folderPath = folderPath;
                settings.fileName = reportFileName;
                settings.storageWarningLimit = storageWarningLimit;
                settings.reportMaxCount = reportMax;
                settings.logAmount = logAmount;
                settings.reportHourly = hourlyRate;
                settings.reportCMD = repCMD;
                settings.messageCMD = mesCMD;
                FileHandler.WriteToBinaryFile<SettingObject>(filePath, settings);
                StartCoroutine(PopWait(filePath));
                isError = false;
            }
        }
        catch (Exception error)
        {
            StartCoroutine(DisplayMessage(string.Concat("Failed To Save, Error Code: ", " ", error), true));
            isError = true;
        }
        if (!isError)
        {
            StartCoroutine(DisplayMessage(string.Concat("Saved Data")));
        }
    }

    [Obsolete]
    private IEnumerator PopWait(string filePath)
    {
        yield return new WaitForSeconds(0.02f);
        SettingObject newSet = FileHandler.ReadFromBinaryFile<SettingObject>(filePath);
        PopInputText(newSet);
    }

    private IEnumerator DisplayMessage(string messageString, bool error = false)
    {
        messageText.text = messageString;
        float displayTime = messageDisplayTime;
        if (error)
        {
            displayTime = errorDisplayTime;
        }
        yield return new WaitForSeconds(displayTime);
        messageText.text = string.Empty;
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
            return (T)binaryFormatter.Deserialize(stream);
        }
    }
}

public class SaveError : Exception
{
    public SaveError() { }

    public SaveError(string message) : base(message) { }
}
