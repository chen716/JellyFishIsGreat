using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class apiCaller : MonoBehaviour
{
    sendSig s;
    private string last = "0";
    private void Start()
    {
// sendSig s = FindObjectOfType<sendSig>();
        StartCoroutine(PostRequest("http://127.0.0.1:5000/event"));
        
    }
    IEnumerator PostRequest(string url)
    {
        print("sending***************** " + sendSig.currentCondition);
        // Creating the JSON data
        string jsonData = "{\"event\":\"abx_play_request\", \"condition\":\""+ sendSig.currentCondition+"\"}";
        // Creating a UnityWebRequest and setting the method, header, and body
        if (sendSig.currentCondition != last) { 
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
                webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            
                webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                // Sending the request and waiting for a response
                yield return webRequest.SendWebRequest();
                // Handling the response
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("Response: " + webRequest.downloadHandler.text);
                }
            }
            print("just sent " + sendSig.currentCondition);
            last = sendSig.currentCondition;
        }
    }
}