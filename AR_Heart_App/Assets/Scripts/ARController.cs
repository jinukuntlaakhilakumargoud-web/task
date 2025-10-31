
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

/// <summary>
/// Manages AR plane detection, object placement, and UI interaction.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class ARController : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField]
    private ARRaycastManager arRaycastManager;

    [Header("Prefabs and Scene Objects")]
    [SerializeField]
    private GameObject heartPrefab;

    [Header("UI")]
    [SerializeField]
    private Button diagnosisButton;
    [SerializeField]
    private Button chatButton;
    [SerializeField]
    private GameObject chatPanel;

    [Header("Managers")]
    [SerializeField]
    private APIManager apiManager;
    [SerializeField]
    private HeartAnimator heartAnimator;

    private GameObject placedHeartInstance;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        if (arRaycastManager == null)
            arRaycastManager = GetComponent<ARRaycastManager>();

        if (apiManager == null)
            Debug.LogError("APIManager is not assigned.");

        if (heartAnimator == null)
            Debug.LogError("HeartAnimator is not assigned. It should be on the Heart Prefab.");

        if (diagnosisButton != null)
        {
            diagnosisButton.onClick.AddListener(OnDiagnoseButtonPressed);
            diagnosisButton.gameObject.SetActive(false); // Initially hidden
        }

        if (chatButton != null)
        {
            chatButton.onClick.AddListener(OnChatButtonPressed);
            chatButton.gameObject.SetActive(false); // Initially hidden
        }

        if (chatPanel != null)
        {
            chatPanel.SetActive(false); // Ensure chat is hidden on start
        }
    }

    void Update()
    {
        // Handle touch input only if the heart has not been placed yet.
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && placedHeartInstance == null)
        {
            PlaceObjectOnPlane(Input.GetTouch(0).position);
        }
    }

    /// <summary>
    /// Performs a raycast from the touch position to detect AR planes.
    /// </summary>
    private void PlaceObjectOnPlane(Vector2 touchPosition)
    {
        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hit a plane, get the pose
            var hitPose = hits[0].pose;

            // Instantiate the heart prefab at the hit position
            placedHeartInstance = Instantiate(heartPrefab, hitPose.position, hitPose.rotation);

            // Assign the HeartAnimator from the newly instantiated prefab
            heartAnimator = placedHeartInstance.GetComponent<HeartAnimator>();
            if (heartAnimator == null) {
                Debug.LogError("The instantiated Heart Prefab is missing a HeartAnimator component.");
                return;
            }

            // Activate the UI buttons
            if (diagnosisButton != null)
            {
                diagnosisButton.gameObject.SetActive(true);
            }
            if (chatButton != null)
            {
                chatButton.gameObject.SetActive(true);
            }
            Debug.Log("Heart placed. UI buttons activated.");
        }
    }

    /// <summary>
    /// Toggles the visibility of the chatbot panel.
    /// </summary>
    public void OnChatButtonPressed()
    {
        if (chatPanel != null)
        {
            bool isActive = chatPanel.activeSelf;
            chatPanel.SetActive(!isActive);
            Debug.Log($"Chat panel toggled to: {!isActive}");
        }
    }

    /// <summary>
    /// This method is called when the Diagnosis UI button is pressed.
    /// </summary>
    public void OnDiagnoseButtonPressed()
    {
        Debug.Log("Diagnosis button pressed.");

        // 1. Create a mock ECG signal (187 samples)
        List<float> mockSignal = new List<float>();
        for (int i = 0; i < 187; i++)
        {
            // Generate some simple sine wave data for testing
            mockSignal.Add(Mathf.Sin(i * 0.1f) * 0.5f + Mathf.Cos(i * 0.25f) * 0.2f);
        }

        // 2. Call the APIManager to get the diagnosis
        StartCoroutine(apiManager.GetDiagnosis(mockSignal, (response) => {
            if (response != null)
            {
                Debug.Log($"Diagnosis Received: {response.arrhythmia_type} with {response.confidence:P2} confidence.");

                // 3. Trigger the animation based on the response
                if (heartAnimator != null)
                {
                    heartAnimator.TriggerAnimation(response.arrhythmia_type);
                }
                else
                {
                    Debug.LogError("HeartAnimator is not assigned. Cannot trigger animation.");
                }
            }
            else
            {
                Debug.LogError("Failed to get a diagnosis from the API.");
            }
        }));
    }
}
