using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PackageManager : MonoBehaviour
{
    public GameObject packageHolder;
    public ParticleSystem particle;
    public Text packageText;

    private int totalPackages = 999;
    private int currentPackages = 0;
    private int ended = 0;

    void Start()
    {
        totalPackages = currentPackages = 0;
        ended = 0;
        for (int i = 0; i < packageHolder.transform.childCount; i++)
        {
            var package = packageHolder.transform.GetChild(i).GetComponent<Package>();
            if (!package) continue;
            package.manager = this;
            totalPackages++;
        }
    }

    void Update()
    {
        packageText.text = string.Format("{0}/{1}", currentPackages, totalPackages);
        if (ended == 1) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            particle.Play();
            Invoke("RestartLevel", 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            particle.Play();
            Invoke("BackToTitle", 0.5f);
        }
    }

    public void GetPackage(Package package)
    {
        currentPackages++;
        if (currentPackages >= totalPackages)
        {
            PlayerPrefs.SetInt(string.Format("{0}_done", SceneManager.GetActiveScene().name), 1);
            var best = PlayerPrefs.GetFloat(string.Format("{0}_best", SceneManager.GetActiveScene().name), -1);
            var cost = Camera.main.GetComponent<BombPainter>().totalCost;
            if (best < 0 || cost < best)
            {
                PlayerPrefs.SetFloat(string.Format("{0}_best", SceneManager.GetActiveScene().name), cost);
            }
            if (ended == 1) return;
            Invoke("ReadyBackToTitle", 1.0f);
        }
    }

    private void RestartLevel()
    {
        if (ended == 1) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void ReadyBackToTitle()
    {
        if (ended == 1) return;
        particle.Play();
        Invoke("BackToTitle", 0.5f);
    }

    private void BackToTitle()
    {
        if (ended == 1) return;
        SceneManager.LoadScene("Title");
    }
    // Start is called before the first frame update
}
