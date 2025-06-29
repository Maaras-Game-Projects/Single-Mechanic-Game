using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetGameManager : MonoBehaviour
{
    [SerializeField] PlayerManager playerManager; // Reference to the PlayerManager to reset player state if needed
    [SerializeField] PlayerAnimationManager playerAnimationManager; // Reference to the PlayerAnimationManager to reset player animations if needed

    [SerializeField] PickUpItemDataContainer pickUpItemDataContainer; // Reference to the container holding item data
    [SerializeField] EnemyDataContainer enemyDataContainer; // Reference to the container holding enemy data
    [SerializeField] private float baseResetLoadTime = 2f;
    [SerializeField] private float defaultRestTime = 5f;

    [SerializeField] bool isResetting = false; // Flag to check if the reset is in progress

    public bool IsResetting => isResetting;

    [SerializeField] UnityEvent onGameReset;
    [SerializeField] UnityEvent onGameResetComplete;

    Coroutine resetCoroutine;

    public void ResetAllNPCs()
    {
        foreach (var npc in enemyDataContainer.GetEnemies)
        {
            IEnemyReset enemyReset = npc.GetComponent<IEnemyReset>();
            if (enemyReset != null)
            {
                enemyReset.ResetEnemy(); // Reset each enemy in the container
            }
            
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
        playerManager.gameObject.SetActive(true); // Ensure the player is active before resetting
        playerManager.ResetPlayer();
        
        foreach (ItemPickUp item in pickUpItemDataContainer.GetItemPickUps)
        {
            item.ResetItemPickUp(); // Reset each item in the container
        }

        SaveSystem.ResetSave(); // Reset the save data

        onGameReset?.Invoke();

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
        resetCoroutine = StartCoroutine(ResetGameWorldCoroutineOnPlayerDeath());
    }

    public void ResetGameOnResting()
    {
        if (isResetting) return;
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine); // Stop any ongoing reset coroutine
        }
        resetCoroutine = StartCoroutine(ResetGameWorldCoroutineOnPlayerRest());
    }

    IEnumerator ResetGameWorldCoroutineOnPlayerDeath()
    {
        isResetting = true;
        yield return new WaitForSeconds(1f); // Wait for the fade-in to complete
        HandleYouDiedCanvas.Instance.FadeInYouDiedScreen();
        yield return new WaitForSeconds(HandleYouDiedCanvas.Instance.FadeDuration + 2f); // Wait for the fade-in to complete

        HandleYouDiedCanvas.Instance.FadeOutYouDiedScreen();
        yield return new WaitForSeconds(HandleYouDiedCanvas.Instance.FadeDuration + 1f); // Wait for the fade-out to complete

        HandleLoadingScreen.Instance.FadeInLoadingScreen();
        yield return new WaitForSeconds(HandleLoadingScreen.Instance.FadeDuration); // Wait for the fade-in to complete
        yield return new WaitForSeconds(baseResetLoadTime); // Additional wait time if needed.

        ResetGameWorld(() =>
        {
            HandleLoadingScreen.Instance.FadeOutLoadingScreen();
            onGameResetComplete?.Invoke();
            isResetting = false;
            
        });

        
        resetCoroutine = null;
        yield return null; // Ensure the coroutine completes
    }

    IEnumerator ResetGameWorldCoroutineOnPlayerRest()
    {
        isResetting = true;

        playerAnimationManager.PlayRestAnimation();
        
        yield return new WaitForSeconds(defaultRestTime); // play rest animation for a default time

        HandleLoadingScreen.Instance.FadeInLoadingScreen();
        yield return new WaitForSeconds(HandleLoadingScreen.Instance.FadeDuration); // Wait for the fade-in to complete
        yield return new WaitForSeconds(baseResetLoadTime); // Additional wait time if needed.

        ResetGameWorld(() =>
        {

            playerAnimationManager.PlaySitToIdleAnimation(); // Play sit to idle animation after reset
            HandleLoadingScreen.Instance.FadeOutLoadingScreen(1f);
            //isResetting = false;
        });

        yield return new WaitForSeconds(7f); // Wait for the sit to idle animation to complete
        isResetting = false;
        resetCoroutine = null;
        yield return null; // Ensure the coroutine completes
    }
}
