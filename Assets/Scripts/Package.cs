using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Package : MonoBehaviour
{
    public PackageManager manager;
    public AudioSource se;
    public GameObject congEffect;
    private bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (triggered) return;
        if (collision.gameObject.GetComponent<House>() != null)
        {
            triggered = true;
            Instantiate(congEffect).transform.position = transform.position;
            if (manager)
            {
                manager.GetPackage(this);
            }
            if (se != null)
                se.Play();
            for (int i = 0; i < gameObject.transform.childCount; i++)
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollisionStay2D(collision);
    }
}
