using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float countdown = 5.0f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, countdown);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
