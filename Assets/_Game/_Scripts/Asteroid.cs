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
        m_rb.mass = typeData.Health;

        transform.localScale = Vector3.one * typeData.Size;
        this.gameObject.SetActive(true);
    }

    public void SetMovement(Vector2 position, Vector2 velocity)
    {
        m_rb.velocity = velocity;
        this.transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    public void FixedUpdate()
    {
        if (Helpers.OutOfBounds(transform.position, Globals.ASTEROID_TYPE_MAP[AsteroidType].Size, true))
        {
            GameManager.Instance.LeaveAsteroid(this);
        }
    }

    public void TakeDamage()
    {
        if (--Health <= 0)
        {
            GameManager.Instance.ExplodeAsteroid(this, m_rb.velocity);
        }
    }
}

public enum AsteroidType 
{
    None,
    Small,
    Medium,
    Large
}
