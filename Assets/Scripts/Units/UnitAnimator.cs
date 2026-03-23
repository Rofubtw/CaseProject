using UnityEngine;

/// <summary>
/// Controls unit animations via CrossFadeInFixedTime.
/// Attach to the same GameObject as the Animator (child of Unit prefab).
/// </summary>
public class UnitAnimator : MonoBehaviour
{
    private Animator animator;
    private string currentState;
    private bool isPlayingOneShot;

    private static readonly float CrossFadeDuration = 0.1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Plays a looping animation (Idle, Run). Interrupts one-shot if playing.
    /// </summary>
    public void PlayLoop(string stateName)
    {
        if (currentState == stateName && !isPlayingOneShot) return;

        isPlayingOneShot = false;
        currentState = stateName;
        animator.CrossFadeInFixedTime(stateName, CrossFadeDuration);
    }

    /// <summary>
    /// Plays a one-shot animation (Attack, Shoot, Guard).
    /// Returns to the given fallback state when finished.
    /// </summary>
    public void PlayOneShot(string stateName, string fallbackState = "Idle")
    {
        isPlayingOneShot = true;
        currentState = stateName;
        animator.CrossFadeInFixedTime(stateName, CrossFadeDuration);
        // Fallback is handled in Update by checking normalizedTime
        oneShotFallback = fallbackState;
    }

    private string oneShotFallback;

    private void Update()
    {
        if (!isPlayingOneShot) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName(currentState) && info.normalizedTime >= 1f)
        {
            isPlayingOneShot = false;
            PlayLoop(oneShotFallback);
        }
    }

    public bool IsPlayingOneShot => isPlayingOneShot;
}
