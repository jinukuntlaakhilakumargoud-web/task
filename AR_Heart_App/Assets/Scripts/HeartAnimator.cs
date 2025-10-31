
using UnityEngine;

/// <summary>
/// Controls the animations of the 3D heart model based on diagnosis results.
/// This script should be attached to the heart prefab which has an Animator component.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HeartAnimator : MonoBehaviour
{
    private Animator animator;

    // Animation trigger names (must match the parameters in the Animator Controller)
    private const string NORMAL_TRIGGER = "PlayNormal";
    private const string V_ECTOPIC_TRIGGER = "PlayV-Ectopic";
    private const string S_ECTOPIC_TRIGGER = "PlayS-Ectopic";

    void Awake()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject!");
        }
    }

    /// <summary>
    /// Triggers a specific animation based on the arrhythmia type string.
    /// </summary>
    /// <param name="arrhythmiaType">The name of the arrhythmia, received from the API.</param>
    public void TriggerAnimation(string arrhythmiaType)
    {
        if (animator == null) return;

        Debug.Log("Triggering animation for: " + arrhythmiaType);

        switch (arrhythmiaType)
        {
            case "Normal":
                animator.SetTrigger(NORMAL_TRIGGER);
                break;
            case "Ventricular Ectopic":
                animator.SetTrigger(V_ECTOPIC_TRIGGER);
                break;
            case "Supraventricular Ectopic":
                animator.SetTrigger(S_ECTOPIC_TRIGGER);
                break;
            case "Fusion":
                // Example: Defaulting Fusion to a normal beat animation
                animator.SetTrigger(NORMAL_TRIGGER);
                break;
            case "Unknown":
                 // Example: Defaulting Unknown to a normal beat animation
                animator.SetTrigger(NORMAL_TRIGGER);
                break;
            default:
                Debug.LogWarning("Unknown arrhythmia type: " + arrhythmiaType + ". Defaulting to Normal animation.");
                animator.SetTrigger(NORMAL_TRIGGER);
                break;
        }
    }
}
