using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShadowRaycaster : MonoBehaviour
{
    // TODO: tuşu salınca arkada ışık bırak o ışık süre dolunca yok olsun o kadar
    [Header("Referanslar")]
    [Tooltip("Gölge kenar prefab’ı. Bu prefab, BoxCollider2D ve (varsa) MeshFilter/MeshCollider içermelidir.")]
    public GameObject shadowPrefab;
    public Material shadowMaterial;

    [Header("Raycast Ayarları")]
    public float angleMin = 10f;
    public float angleMax = 45f;
    public int rayCount = 20;
    public float rayDistance = 10f;
    public LayerMask targetLayer;

    [Header("Gölge Ayarları")]
    public float shadowScaleX = 15f;
    public float shadowDuration = 2f;

    private float releaseTime = 0f;
    private bool isCasting = false;

    private Dictionary<Collider2D, ShadowData> activeShadows = new Dictionary<Collider2D, ShadowData>();
    private Rigidbody2D rb;

    private Vector2[] rayDirections;

    private ShadowPool shadowPool;
    [SerializeField] GameObject lightColumn;


    #region Yardımcı Sınıflar

    private class HitData
    {
        public int minIndex;
        public Vector2 minPoint;
        public int maxIndex;
        public Vector2 maxPoint;
        public HitData(int index, Vector2 point)
        {
            minIndex = index;
            maxIndex = index;
            minPoint = point;
            maxPoint = point;
        }
        public void Update(int index, Vector2 point)
        {
            if (index < minIndex)
            {
                minIndex = index;
                minPoint = point;
            }
            if (index > maxIndex)
            {
                maxIndex = index;
                maxPoint = point;
            }
        }
    }

    private class ShadowData
    {
        public GameObject topEdge;
        public GameObject bottomEdge;
        public GameObject shadowMeshObj;
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;
    }

    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PrecomputeRayDirections();

        shadowPool = new ShadowPool(shadowPrefab);
    }

    void Update()
    {

        if (Input.GetKey(KeyCode.Mouse1))
        {
            isCasting = true;
            ProcessRaycasts();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            releaseTime = Time.time;
            GameObject lc = Instantiate(lightColumn, transform.position, lightColumn.transform.rotation);
            Destroy(lc, shadowDuration);
        }

        if (!Input.GetKey(KeyCode.Mouse1) && isCasting && Time.time - releaseTime >= shadowDuration)
        {
            ClearShadows();
            isCasting = false;
        }
    }

    void PrecomputeRayDirections()
    {
        rayDirections = new Vector2[rayCount];
        for (int i = 0; i < rayCount; i++)
        {
            float t = (float)i / (rayCount - 1);
            float ang = Mathf.Lerp(angleMin, angleMax, t);
            rayDirections[i] = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
        }
    }

    void ProcessRaycasts()
    {
        Dictionary<Collider2D, HitData> hitDataDict = new Dictionary<Collider2D, HitData>();

        for (int i = 0; i < rayCount; i++)
        {
            Vector2 localDirection = rayDirections[i];
            Vector2 direction = transform.TransformDirection(localDirection);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, targetLayer);

#if UNITY_EDITOR

            Debug.DrawRay(transform.position, direction * rayDistance, Color.green);

#endif

            if (hit.collider != null)
            {
                BoxCollider2D box = hit.collider.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    if (!hitDataDict.ContainsKey(hit.collider))
                        hitDataDict[hit.collider] = new HitData(i, hit.point);
                    else
                        hitDataDict[hit.collider].Update(i, hit.point);
                }
            }
        }

        foreach (var kvp in hitDataDict)
        {
            Collider2D col = kvp.Key;
            HitData data = kvp.Value;

            BoxCollider2D box = col.GetComponent<BoxCollider2D>();
            if (box == null) continue;

            Bounds bounds = box.bounds;
            Vector2 lowerEndpoint = (data.minIndex == 0) ? data.minPoint : transform.rotation.y == 0 ? bounds.min : new Vector2(bounds.max.x, bounds.min.y);
            Vector2 upperEndpoint = (data.maxIndex == rayCount - 1) ? data.maxPoint : transform.rotation.y == 0 ? new Vector2(bounds.min.x, bounds.max.y) : bounds.max;

            if (!activeShadows.ContainsKey(col))
                activeShadows[col] = CreateShadowData(col);

            ShadowData shadowData = activeShadows[col];

            UpdateShadowEdge(shadowData.bottomEdge, lowerEndpoint, false);
            UpdateShadowEdge(shadowData.topEdge, upperEndpoint, true);

            UpdateShadowMesh(shadowData, col);
        }

        List<Collider2D> keysToRemove = new List<Collider2D>();
        foreach (var kvp in activeShadows)
        {
            if (!hitDataDict.ContainsKey(kvp.Key))
                keysToRemove.Add(kvp.Key);
        }
        foreach (var key in keysToRemove)
        {
            ClearShadowFor(key);
        }
    }

    ShadowData CreateShadowData(Collider2D target)
    {
        ShadowData sd = new ShadowData();
        sd.topEdge = shadowPool.GetShadow();
        sd.bottomEdge = shadowPool.GetShadow();
        sd.topEdge.transform.SetParent(target.transform);
        sd.bottomEdge.transform.SetParent(target.transform);


        shadowMaterial.color = Color.black.WithAlpha(0.75f);

        sd.shadowMeshObj = new GameObject("ShadowMesh");
        sd.shadowMeshObj.transform.SetParent(target.transform);
        sd.meshFilter = sd.shadowMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = sd.shadowMeshObj.AddComponent<MeshRenderer>();
        meshRenderer.material = shadowMaterial;
        sd.meshCollider = sd.shadowMeshObj.AddComponent<MeshCollider>();

        return sd;
    }

    void UpdateShadowEdge(GameObject edge, Vector2 endpoint, bool isTop)
    {
        edge.transform.position = endpoint;
        Vector2 direction = (endpoint - (Vector2)transform.position).normalized;
        float computedAngle = CalculateAngle(endpoint, endpoint + direction);
        float finalAngle = isTop ? (computedAngle > 0 ? computedAngle : 0) : (computedAngle < 0 ? computedAngle : 0);
        edge.transform.rotation = Quaternion.Euler(0, 0, finalAngle);
        edge.transform.localScale = new Vector3(shadowScaleX, edge.transform.localScale.y, 0);
    }

    void UpdateShadowMesh(ShadowData shadowData, Collider2D target)
    {
        BoxCollider2D topBox = shadowData.topEdge.GetComponent<BoxCollider2D>();
        BoxCollider2D bottomBox = shadowData.bottomEdge.GetComponent<BoxCollider2D>();
        if (topBox == null || bottomBox == null)
        {
            ClearShadows();
            return;
        }

        Vector3[] vertices = new Vector3[4];

        GameObject child = target.GetComponentInChildren<MeshRenderer>().gameObject;
        if (transform.rotation.y == 0)
        {
            vertices[0] = child.transform.InverseTransformPoint(shadowData.topEdge.transform.position);
            vertices[1] = child.transform.InverseTransformPoint(topBox.bounds.max);
            vertices[2] = child.transform.InverseTransformPoint(new Vector2(bottomBox.bounds.max.x, bottomBox.bounds.min.y));
            vertices[3] = child.transform.InverseTransformPoint(shadowData.bottomEdge.transform.position);
        }
        else
        {
            vertices[0] = child.transform.InverseTransformPoint(new Vector2(topBox.bounds.min.x, topBox.bounds.max.y));
            vertices[1] = child.transform.InverseTransformPoint(shadowData.topEdge.transform.position);
            vertices[2] = child.transform.InverseTransformPoint(shadowData.bottomEdge.transform.position);
            vertices[3] = child.transform.InverseTransformPoint(bottomBox.bounds.min);
        }

        int[] triangles = new int[6] { 0, 1, 2, 2, 3, 0 };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        shadowData.meshFilter.mesh = mesh;
        shadowData.meshCollider.sharedMesh = mesh;

        if (shadowData.topEdge.transform.position.y < target.GetComponent<BoxCollider2D>().bounds.max.y)
        {
            ClearShadows();
        }
    }

    void ClearShadowFor(Collider2D col)
    {
        if (activeShadows.ContainsKey(col))
        {
            ShadowData sd = activeShadows[col];
            if (sd.topEdge) shadowPool.ReturnShadow(sd.topEdge);
            if (sd.bottomEdge) shadowPool.ReturnShadow(sd.bottomEdge);
            if (sd.shadowMeshObj) Destroy(sd.shadowMeshObj);
            activeShadows.Remove(col);
        }
    }

    void ClearShadows()
    {
        List<Collider2D> keys = new List<Collider2D>(activeShadows.Keys);
        foreach (var key in keys)
        {
            ClearShadowFor(key);
        }
    }

    public float CalculateAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;
        return Mathf.Atan2(deltaY, deltaX) * Mathf.Rad2Deg;
    }


}

public class ShadowPool
{
    private GameObject shadowPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public ShadowPool(GameObject prefab)
    {
        shadowPrefab = prefab;
    }

    public GameObject GetShadow()
    {
        if (pool.Count > 0)
        {
            GameObject shadow = pool.Dequeue();
            shadow.SetActive(true);
            return shadow;
        }
        return Object.Instantiate(shadowPrefab);
    }

    public void ReturnShadow(GameObject shadow)
    {
        shadow.SetActive(false);
        pool.Enqueue(shadow);
    }
}
