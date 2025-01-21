using System;
using UnityEngine;

public class ForceCheck : MonoBehaviour
{
    [SerializeField] float forceVal = 5f;
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.X))
        {
            AddForceUp();
        }*/
    }

    private void AddForceUp()
    {
        //rb.AddForce(Vector3.up * forceVal,ForceMode.VelocityChange);
        //rb.linearVelocity = Vector3.up * forceVal;
        rb.AddExplosionForce(forceVal, transform.position, forceVal * 2);
    }
}
