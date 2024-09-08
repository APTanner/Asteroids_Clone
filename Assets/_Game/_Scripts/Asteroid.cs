using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public int Health { get; set; }

    public AsteroidType AsteroidType { get; private set; }

    private Rigidbody2D m_rb;
    private MeshFilter m_mf;
    private PolygonCollider2D m_pc;

    public void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_mf = GetComponent<MeshFilter>();
        m_pc = GetComponent<PolygonCollider2D>();

        Mesh mesh = GameManager.Instance.AsteroidBuilder.CreateAsteroidMesh();
        m_mf.sharedMesh = mesh;

        Vector3[] vertices = mesh.vertices;
        Vector2[] vertices2D = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices2D[i] = new Vector2(vertices[i].x, vertices[i].y);
        }
        m_pc.SetPath(0, vertices2D);
    }

    public void Enable(AsteroidType type)
    {
        AsteroidType = type;
        Globals.AsteroidTypeData typeData = Globals.ASTEROID_TYPE_MAP[type];

        Health = typeData.Health;
        transform.localScale = Vector3.one * typeData.Size;

        this.gameObject.SetActive(true);

        ResetVertexData();
        m_rb.mass = typeData.Health;
    }

    public void SetMovement(Vector2 position, Vector2 velocity)
    {
        m_rb.velocity = velocity;
        this.transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
    }

    public void FixedUpdate()
    {
        if (Helpers.OutOfBounds(transform.position, Globals.ASTEROID_TYPE_MAP[AsteroidType].Size, true))
        {
            GameManager.Instance.LeaveAsteroid(this);
        }
    }

    public void TakeDamage(Vector2 position)
    {
        ApplyVertexData(transform.InverseTransformPoint(position));

        if (--Health <= 0)
        {
            GameManager.Instance.ExplodeAsteroid(this, m_rb.velocity);
        }
    }

    public void Update()
    {
        UpdateVertexData();
    }

    private void UpdateVertexData()
    {
        Color[] colors = m_mf.sharedMesh.colors;
        for (int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new Color(
                colors[i].r * (1 - (0.5f * Time.deltaTime)),
                1 - ((float)Health / Globals.ASTEROID_TYPE_MAP[AsteroidType].Health), 
                0, 0);
        }
        m_mf.sharedMesh.colors = colors;
    }

    private void ApplyVertexData(Vector2 localPosition)
    {
        // To find the closest vertex to the impact point,
        // I find the angle around the asteroid circle that the localPosition vector points in then find the closest vertex index to that point
        float angle = Mathf.Atan2(localPosition.y, localPosition.x);
        if (angle < 0)
        {
            angle += 2 * Mathf.PI;
        }
        const float angleBetweenVertices = (2 * Mathf.PI) / Globals.ASTEROID_SEGMENTS;

        float approxIdx = angle / angleBetweenVertices;
        int idx = (int)Mathf.Round(approxIdx) % 10;

        Color[] colors = m_mf.sharedMesh.colors;
        colors[idx] = new Color(1, 0, 0, 0);
        m_mf.sharedMesh.colors = colors;
    }

    private void ResetVertexData()
    {
        Color[] colors = m_mf.sharedMesh.colors;
        for (int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new Color(0, 0, 0, 0);
        }
        m_mf.sharedMesh.colors = colors;
    }
}

public enum AsteroidType 
{
    None,
    Small,
    Medium,
    Large
}
