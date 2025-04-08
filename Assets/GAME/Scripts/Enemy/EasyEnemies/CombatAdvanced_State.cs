using UnityEngine;

public class CombatAdvanced_State : MonoBehaviour
{
    public bool canBackStep = false; // Can the enemy backstep?
}

public enum CombatStrategies
{
    Attack,  //Standard single attack
    BackOff, //Move back to avoid damage, backstepping if taking damage is continuous and if backstepping is possible
}
