using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [SerializeField] private Material asteroidMaterial;
    [SerializeField] private Vector2 speedRange;

    private struct Asteroid
    {
        public int size;
        public GameObject gameObject;

        public Asteroid(int _size, GameObject _gameObject)
        {
            size = _size;
            gameObject = _gameObject;
        }
    }

    private static int ASTEROID_SEGMENTS = 10;
    private static int ASTEROID_MAX = 200;
    private static float ARENA_BUFFER = 5f;

    // maps asteroid sizes to their actual diameter
    private static Dictionary<int, float> m_asteroidSizeMap = new Dictionary<int, float>
    {
        {1, 1},
        {2, 2.2f},
        {3, 4f}
    };
    private static int[] m_asteroidSizeOptions = m_asteroidSizeMap.Keys.ToArray();

    private Vector2 m_cameraBounds;
    private Asteroid[] m_asteroids = new Asteroid[ASTEROID_MAX];
    private Queue<int> m_freeAsteroidIdxs = new Queue<int>();

    private void Start()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        m_cameraBounds = new Vector2(camWidth, camHeight);
        Debug.Log(m_cameraBounds);

        PopulateAsteroids();
        StartCoroutine(SpawnAsteroids(1f));
    }

    IEnumerator SpawnAsteroids(float seconds)
    {
        while (true)
        {
            TrySpawnAsteroid(out Asteroid _);
            yield return new WaitForSeconds(seconds);
        }
    }

    private bool TrySpawnAsteroid(out Asteroid asteroid, int size = -1, bool sendIntoTheFray = true)
    {
        asteroid = m_asteroids[0];
        if (m_freeAsteroidIdxs.Count == 0)
        {
            return false;
        }

        int idx = m_freeAsteroidIdxs.Dequeue();
        asteroid = m_asteroids[idx];

        if (size == -1)
        {
            size = m_asteroidSizeOptions[Random.Range(0, m_asteroidSizeOptions.Length)];
        }
        asteroid.size = size;

        float physicalSize = m_asteroidSizeMap[asteroid.size];
        asteroid.gameObject.transform.localScale = Vector3.one * physicalSize;

        Rigidbody2D rb = asteroid.gameObject.GetComponent<Rigidbody2D>();
        rb.mass = physicalSize * physicalSize;

        asteroid.gameObject.SetActive(true);

        if (sendIntoTheFray)
        {
            float xSpawnBounds = m_cameraBounds.x + ARENA_BUFFER * 2;
            float ySpawnBounds = m_cameraBounds.y + ARENA_BUFFER * 2;

            bool clampX = Random.value > .5f;

            float xPos;
            float yPos;

            if (clampX)
            {
                bool right = Random.value > .5f;
                xPos = m_cameraBounds.x + ARENA_BUFFER + Random.Range(0, ARENA_BUFFER) * (right ? 1 : -1);
                yPos = Random.Range(-ySpawnBounds, ySpawnBounds);
            }
            else
            {
                xPos = Random.Range(-xSpawnBounds, xSpawnBounds);
                bool above = Random.value > .5f; 
                yPos = m_cameraBounds.y + ARENA_BUFFER + Random.Range(0, ARENA_BUFFER) * (above ? 1 : -1);
            }

            Vector2 position = new Vector2(xPos, yPos);
            Debug.Log(position);

            float xTarget = Random.Range(-m_cameraBounds.x, m_cameraBounds.x);
            float yTarget = Random.Range(-m_cameraBounds.y, m_cameraBounds.y);
            Vector2 target = new Vector2(xTarget, yTarget);

            Vector2 heading = (target - position).normalized;
            Vector2 velocity = heading * Random.Range(speedRange.x, speedRange.y);

            asteroid.gameObject.transform.position = position;
            rb.velocity = velocity;
        }

        return true;
    }

    private void PopulateAsteroids()
    {
        for (int i = 0; i < ASTEROID_MAX; ++i)
        {
            m_freeAsteroidIdxs.Enqueue(i);
            GameObject asteroid = CreateAsteroid();
            asteroid.SetActive(false);
            m_asteroids[i] = new Asteroid(1, asteroid);
        }
    }

    private GameObject CreateAsteroid()
    {
        GameObject asteroid = new GameObject();
        asteroid.transform.SetParent(transform, false);

        MeshFilter meshFilter = asteroid.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = asteroid.AddComponent<MeshRenderer>();
        PolygonCollider2D polygonCollider = asteroid.AddComponent<PolygonCollider2D>();

        {
            Mesh mesh = CreateAsteroidMesh();
            meshFilter.sharedMesh = mesh;

            Vector2[] vertices2D = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < vertices2D.Length; ++i)
            {
                vertices2D[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
            }
            polygonCollider.SetPath(0, vertices2D);
        }
        
        if (asteroidMaterial != null)
        {
            meshRenderer.material = asteroidMaterial;
        }
        else
        {
            Debug.LogError("Asteroid Material is null!");
        }
        
        Rigidbody2D rb = asteroid.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        return asteroid;
    }

    private Mesh CreateAsteroidMesh()
    {
        Mesh res = new Mesh();

        Vector3[] vertices = new Vector3[ASTEROID_SEGMENTS];
        for (int i = 0; i < ASTEROID_SEGMENTS; ++i)
        {
            float dist = Random.Range(.5f, 1);
            float theta = (2 * Mathf.PI) / ASTEROID_SEGMENTS * i;
            vertices[i] = new Vector3(dist * Mathf.Cos(theta), dist * Mathf.Sin(theta), 0);
        }

        Color[] colors = new Color[ASTEROID_SEGMENTS];
        for (int i = 0; i < ASTEROID_SEGMENTS; ++i)
        {
            colors[i] = new Color(0, 1, 0, 0);
        }

        int[] indicies = new int[ASTEROID_SEGMENTS * 2];
        for (int i = 0; i < ASTEROID_SEGMENTS; ++i)
        {
            int idxIdx = i * 2;
            indicies[idxIdx] = i;
            indicies[idxIdx + 1] = (i + 1) % ASTEROID_SEGMENTS;
        }

        res.vertices = vertices;
        res.colors = colors;
        res.SetIndices(indicies, MeshTopology.Lines, 0);

        return res;
    }

    private void FixedUpdate()
    {
        // Iterate backwards through the list so we can remove correctly
        for (int i = ASTEROID_MAX-1; i >= 0; --i)
        {
            Asteroid asteroid = m_asteroids[i];
            if (OutOfBounds(asteroid.gameObject.transform.position, asteroid.size, true)) {
                // When an asteroid leaves return it to the pool
                asteroid.gameObject.SetActive(false);
                m_freeAsteroidIdxs.Enqueue(i);
            }
        }
    }

    private void ExplodeAsteroid(Asteroid asteroid)
    {
        // spawn 2 more, smaller, in the same general direction
    }

    private bool OutOfBounds(Vector3 position, float size, bool useBuffer = false)
    {
        float x = position.x;
        float y = position.y;
        float buffer = useBuffer ? ARENA_BUFFER*2 : 0;
        float xDist = m_cameraBounds.x + size / 2 + buffer;
        float yDist = m_cameraBounds.y + size / 2 + buffer;
        Debug.Log($"X: {xDist} Y: {yDist}");
        return x < -xDist || x > xDist || y < -yDist || y > yDist;
    }
}
