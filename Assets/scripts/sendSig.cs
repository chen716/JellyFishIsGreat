using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Net.NetworkInformation;

public class sendSig : MonoBehaviour
{
    public int participantNumber;
     // Interval in minutes to send data
    private float nextSendTime;
    [System.Serializable]
    public class EventData
    {
        public string event_name;
        public string condition;
    }
    public string currentConditionToUse;
    public float intervalMinutes = 1;
    // Hardcoded Latin Square for n=4
    private string[,] latinSquare = new string[,]
    {
        { "A" , "Q" },
        { "B" , "Q"},
        { "C" , "Q"},
        { "D" , "Q"}
    };
    // public string currentCondition = "";

    private int currentConditionIndex = 0;
    public static string currentCondition;
    
    // Network endpoints and clients
    string url = "http://10.x.y.z/event";
    private IPEndPoint apiEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
    private IPEndPoint raspberryPiEndPoint = new IPEndPoint(IPAddress.Parse("192.168.86.239"), 8003);

    private IPEndPoint raspberryPiEndPoint2 = new IPEndPoint(IPAddress.Parse("192.168.86.239"), 8004);
    private UdpClient udpClient;
    private UdpClient udpClient2;
    private bool start = false;
    IEnumerator sendAPI(int coditions)
    {
        //string url = "http://10.x.y.z/event"; // Replace with the actual IP address
        EventData data = new EventData { event_name = "abx_play_request", condition = coditions.ToString() };
        string jsonData = JsonUtility.ToJson(data);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                //Debug.Log("Error: " + request.error);
            }
            else
            {
//Debug.Log("Response Code: " + request.responseCode);
             //   Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }
    public void Request(string condition)
    {
        try
        {
            string url = "localhost:5000/event";

            var request = UnityWebRequest.PostWwwForm(url, "");
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "text/csv");
            request.SetRequestHeader("event_name", "abx_play_request");
            string jsonData = "{\"event\":\"abx_play_request\", \"condition\":\""+condition.ToString()+"\"}";
            // Creating a UnityWebRequest and setting the method, header, and body
           
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);

            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
                StartCoroutine(onResponse(request));
        }
        catch (Exception e) { Debug.Log("ERROR : " + e.Message); }
    }
    private IEnumerator onResponse(UnityWebRequest req)
    {

        yield return req.SendWebRequest();
        if (req.isNetworkError)
        {
            print(req.error);
          //  Debug.Log("Network error has occured: " + req.GetResponseHeader(""));
        }
        else
           Debug.Log("Success " + req.downloadHandler.text);
        byte[] results = req.downloadHandler.data;
       // Debug.Log("Second Success");
        // Some code after success

    }
    void Start()
    {
        udpClient = new UdpClient();
        udpClient2 = new UdpClient();
        //print("starting the senting");
        //SetNextCondition();
        //print("right now: "+currentCondition);
        //Debug.Log(currentCondition.ToString());
        //Debug.Log("??" + currentConditionToUse);

        if (currentConditionToUse != "A" && currentConditionToUse != "B" && currentConditionToUse != "C" && currentConditionToUse != "D" && currentConditionToUse != "E")
        {
            currentConditionToUse = "A";
            Debug.Log("WTF are you using???????? RTFM");
        }
        currentCondition = currentConditionToUse;
        Request(currentCondition);
        //Debug.Log(currentCondition.ToString());
        SendConditionToRaspberryPi(currentCondition);
        SendConditionToRaspberryPi2(currentCondition);
        nextSendTime = Time.time + intervalMinutes * 60;
        //Debug.Log(Time.time + "," + nextSendTime + "," + currentCondition);

        start = true;
    }

    void Update()
    {
        //Debug.Log(Time.time +","+ nextSendTime+","+currentCondition);
        if (Time.time >= nextSendTime && start)
        {
           
            nextSendTime += intervalMinutes * 60;

            if (currentConditionIndex >= 5 )
            {
                // End the Unity application
                Application.Quit();
            }
            else
            {
                //Debug.Log("WTF");
                currentCondition = "Q";
                //SetNextCondition();
            }
           // print("sending sigal out");
            //print("right now: " + currentCondition);
            Request(currentCondition);
            
        }
        SendConditionToRaspberryPi(currentCondition);
        SendConditionToRaspberryPi2(currentCondition);
    }


    void SetNextCondition()
    {
        // Select the right condition from the Latin Square based on the participant number
        int participantIndex = participantNumber % 4; // Assumes participant numbers start at 0
        currentCondition = latinSquare[participantIndex, currentConditionIndex%4];
        currentConditionIndex++;
    }

    void SendConditionToAPI(int condition)
    {
        string message = condition.ToString();
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(bytes, bytes.Length, apiEndPoint);
    }

    void SendConditionToRaspberryPi(string condition)
    {
        string message = participantNumber.ToString() +","+ condition.ToString();
        print(message);
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(bytes, bytes.Length, raspberryPiEndPoint);
    }
    void SendConditionToRaspberryPi2(string condition)
    {
        string message = participantNumber.ToString() + "," + condition.ToString();
        print(message);
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        udpClient2.Send(bytes, bytes.Length, raspberryPiEndPoint2);
    }
}
