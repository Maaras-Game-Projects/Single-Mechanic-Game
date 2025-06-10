using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawnManager : MonoBehaviour
{
    [SerializeField] List<NPC_Root> NPCs = new List<NPC_Root>();

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
}
