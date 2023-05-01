using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject circleBegin;
    public GameObject circleEnd;

    public GameObject container;
    public GameObject wire;
    public GameObject outline;

    // public ParticleSystem bombParticle;
    public ParticleSystem wireParticle;
    public GameObject explosionPrefab;

    public float radius = 3.0f;
    public float strength = 3.0f;
    public float explodeTime = 2.0f;

    public float density = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator CountDown()
    {
        float t = 0;
        var line = wire.GetComponent<LineRenderer>();
        float realCount = line.positionCount;
        while (t < explodeTime)
        {
            int newCount = (int)(realCount * (1 - t / explodeTime));
            Vector3[] pos = new Vector3[line.positionCount];
            line.GetPositions(pos);

            line.positionCount = newCount;
            line.SetPositions(pos[0 .. newCount]);

            if (line.positionCount > 0)
            {
                wireParticle.transform.localPosition = line.GetPosition(line.positionCount - 1);
            }
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        Explode();
        yield break;
    }

    public void Light()
    {
        // Invoke("Explode", 1.0f);
        StartCoroutine(CountDown());
    }

    public void Explode()
    {
        var line = container.GetComponent<LineRenderer>();
        List<Vector3> positions = new List<Vector3>();
        positions.Add(Vector3.zero);
        if (line.positionCount > 0)
        {
            float total = 0.0f;
            float dura = 0.25f;
            for (int i = 1; i < line.positionCount; i++)
            {
                var delta = line.GetPosition(i) - line.GetPosition(i - 1);
                var start = line.GetPosition(i - 1);
                if (total + delta.magnitude > dura)
                {
                    positions.Add(start += (dura - total) * delta.normalized);
                    total = delta.magnitude - (dura - total);
                }
                while (total >= dura)
                {
                    positions.Add(start += dura * delta.normalized);
                    total -= dura;
                }
            }
        }
        container.GetComponent<LineRenderer>().enabled = false;
        Destroy(circleBegin);
        Destroy(circleEnd);
        GetComponent<CompositeCollider2D>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        positions = positions.Select((v) => transform.TransformPoint(v)).ToList();
        int layerMask = LayerMask.GetMask("Default") | LayerMask.GetMask("Bombs") | LayerMask.GetMask("StageObjects");
        foreach (var position in positions)
        {
            var other = Physics2D.OverlapCircleAll(position, radius, layerMask);
            foreach (Collider2D collider in other)
            {
                if (collider.gameObject == gameObject) continue;
                if (collider.attachedRigidbody == null) continue;
                Vector3 delta = collider.transform.position - position;
                Vector3 hitPoint = collider.attachedRigidbody.ClosestPoint(position);
                // hitPoint = collider.transform.TransformPoint(hitPoint);
                collider.attachedRigidbody.AddForceAtPosition(strength * delta / delta.magnitude / Mathf.Max(1f, delta.magnitude), hitPoint, ForceMode2D.Impulse);
            }
            var explosion = Instantiate(explosionPrefab);
            explosion.transform.position = position;
        }

        if (wireParticle != null)
            wireParticle.Stop();
        Destroy(gameObject);
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponent<Rigidbody2D>() != null
            && GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Dynamic
            && collision.gameObject.GetComponent<MapBorder>() != null)
        {
            Destroy(gameObject);
        }
    }
}
