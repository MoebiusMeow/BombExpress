using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    public float range = 6.0f;
    public float velocity = 0.01f;
    public float velocityRatio = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        velocityRatio = Random.value + 0.5f;
        transform.localScale = new Vector3(velocityRatio, velocityRatio, velocityRatio);
    }

    private void FixedUpdate()
    {
        transform.position += Vector3.right * velocity * velocityRatio;
        if (transform.position.x > range)
            transform.position += Vector3.left * 2 * range;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
