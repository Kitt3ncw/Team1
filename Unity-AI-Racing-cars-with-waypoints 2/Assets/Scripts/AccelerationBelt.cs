using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationBelt : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("playerCar"))
        {
            //Debug.LogError("other:"+other);
            other.transform.GetComponentInParent<MovementControls>().ActivateBoost();
        }
    }
}
