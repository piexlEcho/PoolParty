using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCounter : MonoBehaviour
{
    [SerializeField] Mesh rice;
    int points;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "RiceGrain")
        {
            points++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "RiceGrain")
        {
            points--;
        }
    }
}
