using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostEffects : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject screenQuad;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (screenQuad == null) return;
        screenQuad.transform.position = new (Camera.main.transform.position.x, Camera.main.transform.position.y, 0);
        screenQuad.transform.localScale = new (Camera.main.aspect * Camera.main.orthographicSize * 2, Camera.main.orthographicSize * 2, 1);
        Debug.Log(Camera.main.orthographicSize);
        Debug.Log(screenQuad.transform.position);
        Debug.Log(Camera.main.transform.position);
    }
}
