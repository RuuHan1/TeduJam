using UnityEngine;

public class Test : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public GameObject platform;
    private GameObject shadowTopEdge;
    private GameObject shadowBottomEdge;
    private Mesh mesh;
    private MeshFilter meshFilter;

    [Header("Raycast Ayarlarý")]
    public float angleMin = -10f;
    public float angleMax = 10f;
    public int rayCount = 20;
    public float rayDistance = 10f;
    public float infiniteLength = 1000f;
    public LayerMask targetLayer;

    private Vector2? boundaryLower = null;
    private Vector2? boundaryUpper = null;

    private float countdown = 2;
    private float currentTime = 0;

    private void Start()
    {
        shadowTopEdge = Instantiate(platform);
        shadowTopEdge.SetActive(false);

        shadowBottomEdge = Instantiate(platform);
        shadowBottomEdge.SetActive(false);
    }

    void Update()
    {
        if (currentTime == 0)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                shadowTopEdge.SetActive(false);
                shadowBottomEdge.SetActive(false);
                for (int i = 0; i < rayCount; i++)
                {
                    float t = (float)i / (rayCount - 1);
                    float angle = Mathf.Lerp(angleMin, angleMax, t);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, targetLayer);

                    if (i == 0 || i == rayCount - 1)
                        Debug.DrawRay(transform.position, direction * rayDistance, Color.green);


                    if (i == 0)
                        boundaryLower = hit.collider != null ? hit.point : null;
                    else if (i == rayCount - 1)
                        boundaryUpper = hit.collider != null ? hit.point : null;


                    if (hit.collider != null)
                    {
                        BoxCollider2D box = hit.collider.GetComponent<BoxCollider2D>();
                        if (box != null)
                        {

                            Bounds bounds = box.bounds;
                            Vector2 bottomLeft = bounds.min;
                            Vector2 topLeft = new Vector2(bounds.min.x, bounds.max.y);

                            Vector2 redLowerEndpoint = bottomLeft;
                            Vector2 redUpperEndpoint = topLeft;


                            if (boundaryLower != null)
                            {
                                redLowerEndpoint = boundaryLower.Value;
                                Debug.DrawLine(transform.position, redLowerEndpoint, Color.blue);
                            }
                            else
                            {
                                Vector2 lowerDir = (redLowerEndpoint - (Vector2)transform.position).normalized;
                                //Debug.DrawLine(transform.position, redLowerEndpoint + lowerDir * infiniteLength, Color.red);
                                shadowBottomEdge.SetActive(true);
                                shadowBottomEdge.transform.position = redLowerEndpoint;
                                float z = CalculateAngle(redLowerEndpoint, redLowerEndpoint + lowerDir);
                                shadowBottomEdge.transform.rotation = Quaternion.Euler(0, 0, z < 0 ? z : 0);
                                shadowBottomEdge.transform.localScale = new Vector3(15, shadowTopEdge.transform.localScale.y, 0);
                            }


                            if (boundaryUpper != null)
                            {
                                redUpperEndpoint = boundaryUpper.Value;
                                Debug.DrawLine(transform.position, redUpperEndpoint, Color.blue);
                            }
                            else
                            {
                                Vector2 upperDir = (redUpperEndpoint - (Vector2)transform.position).normalized;
                                //Debug.Log("Start:" + redUpperEndpoint + " / End:" + (redUpperEndpoint + upperDir));
                                shadowTopEdge.SetActive(true);
                                shadowTopEdge.transform.position = redUpperEndpoint;
                                float z = CalculateAngle(redUpperEndpoint, redUpperEndpoint + upperDir);
                                shadowTopEdge.transform.rotation = Quaternion.Euler(0, 0, z > 0 ? z : 0);
                                shadowTopEdge.transform.localScale = new Vector3(15, shadowTopEdge.transform.localScale.y, 0);
                            }

                            if (shadowTopEdge.activeInHierarchy)
                            {
                                //lineRenderer.positionCount = 5;

                                //Vector3[] points = new Vector3[]
                                //{
                                //    shadowTopEdge.transform.position,
                                //    shadowTopEdge.GetComponent<BoxCollider2D>().bounds.max,
                                //    new Vector2(shadowBottomEdge.GetComponent<BoxCollider2D>().bounds.max.x,shadowBottomEdge.GetComponent<BoxCollider2D>().bounds.min.y),
                                //    shadowBottomEdge.transform.position,
                                //    shadowTopEdge.transform.position,
                                //};

                                //lineRenderer.SetPositions(points);

                                mesh = new Mesh();
                                meshFilter = hit.collider.GetComponentInChildren<MeshFilter>();
                                // 4 noktayý (vertex) tanýmlýyoruz
                                Vector3[] vertices = new Vector3[4];
                                vertices[0] = meshFilter.transform.InverseTransformPoint(shadowTopEdge.transform.position);
                                vertices[1] = meshFilter.transform.InverseTransformPoint(shadowTopEdge.GetComponent<BoxCollider2D>().bounds.max);
                                vertices[2] = meshFilter.transform.InverseTransformPoint(new Vector2(shadowBottomEdge.GetComponent<BoxCollider2D>().bounds.max.x, shadowBottomEdge.GetComponent<BoxCollider2D>().bounds.min.y));
                                vertices[3] = meshFilter.transform.InverseTransformPoint(shadowBottomEdge.transform.position);

                                // Alaný kaplamak için iki üçgen oluþturuyoruz.
                                int[] triangles = new int[6]
                                {
                                    0, 1, 2,  // Birinci üçgen
                                    2, 3, 0   // Ýkinci üçgen
                                };

                                mesh.vertices = vertices;
                                mesh.triangles = triangles;

                                // Opsiyonel: Mesh normalleri ve UV koordinatlarý ayarlanabilir.
                                mesh.RecalculateNormals();

                                // Mesh'i MeshFilter bileþenine atýyoruz.
                                hit.collider.GetComponentInChildren<MeshFilter>().mesh = mesh;
                                hit.collider.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
                            }
                        }
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                currentTime = Time.time;
            }
        }
        else
        {
            if (Time.time - currentTime >= countdown)
            {
                currentTime = 0;
                shadowTopEdge.SetActive(false);
                shadowBottomEdge.SetActive(false);
                mesh = new Mesh();
                meshFilter.mesh = mesh;
            }
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(h, v) * 5;
        //transform.Translate(new Vector2(h, v) * 5 * Time.deltaTime);
    }

    public float CalculateAngle(Vector2 point1, Vector2 point2)
    {
        float deltaY = point2.y - point1.y;
        float deltaX = point2.x - point1.x;

        float angleRad = Mathf.Atan2(deltaY, deltaX); // Radyan cinsinden açý
        float angleDeg = angleRad * Mathf.Rad2Deg;   // Dereceye çevir

        return angleDeg;
    }
}
