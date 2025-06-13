using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetGameManager : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager; // Reference to the PlayerManager to reset player state if needed
    [SerializeField] List<NPC_Root> NPCs = new List<NPC_Root>();
    [SerializeField] private float baseResetLoadTime = 2f;

    [SerializeField] bool isResetting = false; // Flag to check if the reset is in progress

    Coroutine resetCoroutine;

    public void ResetAllNPCs()
    {
        foreach (var npc in NPCs)
        {
            npc.ResetEnemy();
        }
    }

    public void ResetAllNPCsInDelay(float delay)
    {
        StartCoroutine(ResetAllNPCsDelayed(delay));
    }

    IEnumerator ResetAllNPCsDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAllNPCs();
    }

    public void ResetGameWorld(Action onResetComplete = null)
    {
        ResetAllNPCs();
        playerManager.ResetPlayer();

        // Optionally, invoke the callback if provided
        onResetComplete?.Invoke();
    }

    public void ResetGameOnPlayerDeath()
    {
        if (isResetting) return;
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine); // Stop any ongoing reset coroutine
        }
        resetCoroutine = StartCoroutine(ResetGameWorldCoroutine(true));
    }

    public void ResetGameOnResting()
    {
        if (isResetting) return;
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine); // Stop any ongoing reset coroutine
        }
        resetCoroutine = StartCoroutine(ResetGameWorldCoroutine(false));
    }

    IEnumerator ResetGameWorldCoroutine(bool canShowYouDiedScreen = false)
    {
        isResetting = true;
        if (canShowYouDiedScreen)
        {
            yield return new WaitForSeconds(1f); // Wait for the fade-in to complete
            HandleYouDiedCanvas.Instance.FadeInYouDiedScreen();
            yield return new WaitForSeconds(HandleYouDiedCanvas.Instance.FadeDuration + 2f); // Wait for the fade-in to complete

            HandleYouDiedCanvas.Instance.FadeOutYouDiedScreen();
            yield return new WaitForSeconds(HandleYouDiedCanvas.Instance.FadeDuration + 1f); // Wait for the fade-out to complete
        }

        HandleLoadingScreen.Instance.FadeInLoadingScreen();
        yield return new WaitForSeconds(HandleLoadingScreen.Instance.FadeDuration); // Wait for the fade-in to complete
        yield return new WaitForSeconds(baseResetLoadTime); // Additional wait time if needed.

        ResetGameWorld(() =>
        {
            HandleLoadingScreen.Instance.FadeOutLoadingScreen();

        });

        isResetting = false;
        resetCoroutine = null;
        yield return null; // Ensure the coroutine completes
    }
}
