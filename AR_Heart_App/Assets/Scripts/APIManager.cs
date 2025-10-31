
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Serializable class for the JSON request body.
/// </summary>
[System.Serializable]
public class EcgRequest
{
    public List<float> signal;
}

/// <summary>
/// Serializable class to parse the JSON response from the server.
/// </summary>
[System.Serializable]
public class DiagnosisResponse
{
    public string arrhythmia_type;
    public float confidence;
    public int class_id;
}

/// <summary>
/// Manages API calls to the ECG diagnosis backend.
/// </summary>
public class APIManager : MonoBehaviour
{
    // The URL of the FastAPI backend.
    // Use http://10.0.2.2:8000 for Android Emulator to connect to localhost.
    // Use http://localhost:8000 or http://127.0.0.1:8000 for Unity Editor.
    private const string apiURL = "http://127.0.0.1:8000/predict";

    /// <summary>
    /// Sends ECG data to the backend and retrieves the diagnosis.
    /// </summary>
    /// <param name="signalData">A list of floats representing the ECG signal.</param>
    /// <param name="callback">Action to be executed upon receiving a response.</param>
    public IEnumerator GetDiagnosis(List<float> signalData, System.Action<DiagnosisResponse> callback)
    {
        // 1. Create the request object
        EcgRequest requestData = new EcgRequest { signal = signalData };
        string jsonData = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        // 2. Create the UnityWebRequest
        UnityWebRequest request = new UnityWebRequest(apiURL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending request to " + apiURL);

        // 3. Send the request and wait for a response
        yield return request.SendWebRequest();

        // 4. Handle the response
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Response: " + request.downloadHandler.text);
            callback?.Invoke(null); // Notify callback of failure
        }
        else
        {
            Debug.Log("Received response: " + request.downloadHandler.text);
            try
            {
                DiagnosisResponse response = JsonUtility.FromJson<DiagnosisResponse>(request.downloadHandler.text);
                callback?.Invoke(response); // Trigger the callback with the parsed data
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JSON Deserialization Error: " + ex.Message);
                callback?.Invoke(null);
            }
        }
    }
}
