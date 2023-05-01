using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;

public class BombPainter : MonoBehaviour
{
    public GameObject bombPrefab = null;
    public Material lineMaterial;
    public Material wireMaterial;

    public GameObject brushObject = null;
    public Text costText = null;

    public float pipeLength = 2.0f;
    public float costPerBomb = 10f;
    public float costPerUnit = 10f;

    private GameObject workingGO = null;
    private int workingCount = 0;
    private Vector2[] workingStartPath = null;
    private Vector2[] workingStartPathInner = null;

    public float totalCost = 0f;
    public float workingCost = 0f;
    private float visualTotalCost = 0f;
    private float visualWorkingCost = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    List<Vector2> PathFromLine(LineRenderer line, float lineWidth)
    {
        if (line.positionCount <= 0) return new ();
        Vector3[] pos = new Vector3[line.positionCount];
        line.GetPositions(pos);

        var p1 = pos.SkipLast(1).Zip(pos.Skip(1), (a, b) => 
            (Quaternion.Euler(0, 0, 90) * (b - a).normalized * lineWidth * 0.5f + a,
             Quaternion.Euler(0, 0, 90) * (b - a).normalized * lineWidth * 0.5f + b)
        ).Aggregate(new List<Vector3>(), (s, v) => s.Append(v.Item1).Append(v.Item2).ToList());

        var p2 = pos.SkipLast(1).Zip(pos.Skip(1), (a, b) => 
            (Quaternion.Euler(0, 0, -90) * (b - a).normalized * lineWidth * 0.5f + a,
             Quaternion.Euler(0, 0, -90) * (b - a).normalized * lineWidth * 0.5f + b)
        ).Aggregate(new List<Vector3>(), (s, v) => s.Append(v.Item1).Append(v.Item2).ToList());

        return p1.Concat(p2.Reverse<Vector3>()).Select((v) => new Vector2(v.x, v.y)).ToList();
    }
    void UpdateWorkingCost(LineRenderer line)
    {
        if (line == null || line.positionCount <= 0)
        {
            workingCost = 0;
            return;
        }
        if (line.positionCount <= 1)
        {
            workingCost = costPerBomb;
            return;
        }
        workingCost = costPerBomb;
        Vector3[] pos = new Vector3[line.positionCount];
        line.GetPositions(pos);

        float totalLength = pos.SkipLast(1).Zip(pos.Skip(1), (a, b) => (a - b).magnitude ).Sum();
        workingCost += totalLength * costPerUnit;
    }

    private void FixedUpdate()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (brushObject != null)
        {
            var length = pipeLength;
            if (brushObject.transform.parent.InverseTransformPoint(mousePos).magnitude <= length)
                brushObject.transform.position = mousePos;
            else
                brushObject.transform.localPosition = (brushObject.transform.parent.InverseTransformPoint(mousePos)).normalized * length;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos = brushObject.transform.position;
        var outlineWidth = 0.25f;

        int layerMask = LayerMask.GetMask("StageObjects");
        var other = Physics2D.OverlapCircleAll(mousePos, outlineWidth * 0.5f, layerMask);
        bool canAdd = true;
        foreach (Collider2D collider in other)
            if (collider.attachedRigidbody.bodyType == RigidbodyType2D.Static)
            {
                canAdd = false;
                break;
            }

        if (Input.GetMouseButtonDown(0) && !canAdd)
        {
            workingGO = null;
        }
        if ((Input.GetMouseButtonDown(0) && canAdd) || (Input.GetMouseButton(0) && workingGO == null && canAdd))
        {
            workingGO = GameObject.Instantiate(bombPrefab);
            workingGO.transform.position = mousePos;
            workingCount = 0;
            var body = workingGO.GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            body.gravityScale = 0.0f;

            var line = workingGO.GetComponent<Bomb>().container.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = workingCount + 1;
            line.SetPosition(workingCount++, workingGO.transform.InverseTransformPoint(mousePos));
            line.widthMultiplier = 0.20f;
            line.material = lineMaterial;
            line.material.color = Color.gray;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            /*
            var outline = workingGO.GetComponent<Bomb>().outline.AddComponent<LineRenderer>();
            outline.useWorldSpace = false;
            outline.positionCount = workingCount;
            outline.SetPosition(workingCount - 1, workingGO.transform.InverseTransformPoint(mousePos));
            outline.widthMultiplier = 0.2f;
            outline.material.color = Color.black;
            outline.numCapVertices = 10;
            outline.numCornerVertices = 0;
            */

            var edge = workingGO.AddComponent<EdgeCollider2D>();
            edge.edgeRadius = outlineWidth * 0.5f;
            edge.SetPoints(new List<Vector2>(new Vector2[] { Vector2.zero, Vector2.zero }));

            var collider = workingGO.AddComponent<PolygonCollider2D>();
            collider.usedByComposite = true;
            collider.pathCount = 1;
            /*
            var edgeMesh = edge.CreateMesh(false, false);
            collider.pathCount = 1;
            collider.SetPath(0, edgeMesh.vertices.Select((v) => new Vector2(v.x, v.y)).ToArray());
            */
            // ColliderCreator.CreateFromMesh(collider, edge.CreateMesh(false, false));
            collider.CreatePrimitive(20, Vector2.one * outlineWidth * 0.5f, Vector2.zero);
            // workingStartPath = collider.GetPath(0);
            // workingGO.GetComponent<Bomb>().outline.GetComponent<MeshFilter>().mesh = collider.CreateMesh(false, false);

            /*
            collider.CreatePrimitive(20, Vector2.one * line.startWidth * 0.5f, Vector2.zero);
            workingStartPathInner = collider.GetPath(0);
            workingGO.GetComponent<Bomb>().lineBack.GetComponent<MeshFilter>().mesh = collider.CreateMesh(false, false);
            */

            workingGO.GetComponent<Bomb>().circleBegin.transform.localScale = Vector3.one * line.startWidth * 5;
            workingGO.GetComponent<Bomb>().circleBegin.AddComponent<CircleCollider2D>().radius = outlineWidth * 0.5f;
            workingGO.GetComponent<Bomb>().circleBegin.GetComponent<CircleCollider2D>().density = workingGO.GetComponent<Bomb>().density;
            //workingGO.GetComponent<Bomb>().circleBegin.GetComponent<SpriteRenderer>().color = Color.gray;
            workingGO.GetComponent<Bomb>().circleEnd.transform.localScale = Vector3.one * line.startWidth * 5;
            workingGO.GetComponent<Bomb>().circleEnd.AddComponent<CircleCollider2D>().radius = outlineWidth * 0.5f;
            workingGO.GetComponent<Bomb>().circleBegin.GetComponent<CircleCollider2D>().density = workingGO.GetComponent<Bomb>().density;
            // workingGO.GetComponent<Bomb>().circleEnd.GetComponent<SpriteRenderer>().color = Color.gray;
            var composite = workingGO.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.vertexDistance = 0.0015f;
            composite.generationType = CompositeCollider2D.GenerationType.Manual;
            UpdateWorkingCost(line);
        }
        if (Input.GetMouseButton(0) && canAdd)
        {
            var line = workingGO.GetComponent<Bomb>().container.GetComponent<LineRenderer>();
            // var outline = workingGO.GetComponent<Bomb>().outline.GetComponent<LineRenderer>();
            var newPos = workingGO.transform.InverseTransformPoint(mousePos);
            if ((line.GetPosition(line.positionCount - 1) - newPos).magnitude >= 0.1f)
            {
                line.positionCount = workingCount + 1;
                line.SetPosition(workingCount++, newPos);
                /*
                outline.positionCount = workingCount;
                outline.SetPosition(workingCount - 1, newPos);
                */
                var edge = workingGO.GetComponent<EdgeCollider2D>();
                Vector3[] posList = new Vector3[line.positionCount];
                line.GetPositions(posList);
                edge.SetPoints(posList.Select((v) => new Vector2(v.x, v.y)).ToList());

                var collider = workingGO.GetComponent<PolygonCollider2D>();
                // collider.pathCount = 0;
                // ColliderCreator.CreateFromMesh(collider, edge.CreateMesh(false, false));
                /*
                var edgeMesh = edge.CreateMesh(false, false);
                collider.pathCount = 1;
                collider.SetPath(0, edgeMesh.vertices.Select((v) => new Vector2(v.x, v.y)).ToArray());
                */
                /*
                collider.CreatePrimitive(20, Vector2.one * line.startWidth * 0.5f, newPos);
                collider.pathCount = 3;
                collider.SetPath(1, PathFromLine(line, line.startWidth));
                collider.SetPath(2, workingStartPathInner);
                workingGO.GetComponent<Bomb>().lineBack.GetComponent<MeshFilter>().mesh = collider.CreateMesh(false, false);
                */

                // collider.CreatePrimitive(20, Vector2.one * outlineWidth * 0.5f, newPos);
                // collider.pathCount = 1;
                // collider.SetPath(0, PathFromLine(line, outlineWidth));
                // collider.SetPath(2, workingStartPath);
                // workingGO.GetComponent<Bomb>().outline.GetComponent<MeshFilter>().mesh = collider.CreateMesh(false, false);
                workingGO.GetComponent<Bomb>().circleEnd.transform.position = mousePos;
                UpdateWorkingCost(line);
            }
        }
        if (Input.GetMouseButtonUp(0) && workingGO != null)
        {
            var line = workingGO.GetComponent<Bomb>().container.GetComponent<LineRenderer>();
            /*
            line.Simplify(0.001f);
            var collider = workingGO.GetComponent<PolygonCollider2D>();
            collider.SetPath(0, PathFromLine(line, outlineWidth));
            workingGO.GetComponent<Bomb>().outline.GetComponent<MeshFilter>().mesh = collider.CreateMesh(false, false);
            */
            var edge = workingGO.GetComponent<EdgeCollider2D>();
            var collider = workingGO.GetComponent<PolygonCollider2D>();
            collider.pathCount = 1;
            // if (edge.pointCount > 1) ColliderCreator.CreateFromMesh(collider, edge.CreateMesh(false, false));
            collider.SetPath(0, PathFromLine(line, outlineWidth));
            var composite = workingGO.GetComponent<CompositeCollider2D>();
            composite.density = workingGO.GetComponent<Bomb>().density;
            composite.GenerateGeometry();
            composite.density = workingGO.GetComponent<Bomb>().density;

            workingGO.GetComponent<EdgeCollider2D>().enabled = false;
            workingGO.GetComponent<Rigidbody2D>().gravityScale = 1.0f;
            workingGO.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            workingGO.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            workingGO.GetComponent<Rigidbody2D>().useAutoMass = true;

            var wire = workingGO.GetComponent<Bomb>().wire.AddComponent<LineRenderer>();
            wire.useWorldSpace = false;
            wire.material = wireMaterial;
            wire.startWidth = 0.03f;
            List<Vector3> list = new List<Vector3>();
            float seed = Random.value * 0.5f - 1;
            for (int i = 0; i < 20; i++)
            {
                float y = i * 0.09f;
                float x = 1.2f * Mathf.Sin(y * seed * 0.4f) - 0.05f * Mathf.Sin(y * 3 * 20);
                list.Add(new Vector3(x, y + outlineWidth, 0));
            }
            wire.positionCount = list.Count;
            wire.SetPositions(list.ToArray());

            var bomb = workingGO.GetComponent<Bomb>();
            if (bomb != null)
                bomb.Light();
            totalCost += workingCost;
            workingCost = 0;
            visualWorkingCost = 0;
        }

        float bestCost = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "_best", -1);
        string displayText = "";
        visualTotalCost = Mathf.Lerp(visualTotalCost, totalCost, 0.1f);
        visualWorkingCost = Mathf.Lerp(visualWorkingCost, workingCost, 0.1f);
        if (bestCost >= 0)
        {
            displayText += string.Format("Best: ${0:N0}\n", bestCost);
        }
        displayText += string.Format("Cost: ${0:N0}", visualTotalCost);
        if (visualWorkingCost > 0.5f)
        {
            displayText += string.Format(" + {0:N0}", visualWorkingCost);
        }
        if (costText != null)
            costText.text = displayText;
    }
}
