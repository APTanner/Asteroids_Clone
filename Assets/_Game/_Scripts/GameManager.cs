using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Options")]
    [SerializeField] private Asteroid asteroidPrefab;
    [SerializeField] private Vector2 speedRange;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem asteroidDestruction;

    public Vector2 CameraBounds { get; private set; }
    public AsteroidBuilder AsteroidBuilder { get; private set; } = new AsteroidBuilder();

    private ObjectPool<Asteroid> m_asteroidPool;
    private ObjectPool<ParticleSystem> m_destructionParticlePool;

    private int m_score;

    private GameObject m_playerGameObject;

    protected override void Awake()
    {
        base.Awake();
        m_asteroidPool = new ObjectPool<Asteroid>(asteroidPrefab, Globals.ASTEROID_MAX);
        m_destructionParticlePool = new ObjectPool<ParticleSystem>(asteroidDestruction, 20);
    }

    private void Start()
    {
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        CameraBounds = new Vector2(camWidth, camHeight);

        m_playerGameObject = ShipController.Instance.gameObject;

        StartCoroutine(SpawnAsteroids(1f));
    }

    IEnumerator SpawnAsteroids(float seconds)
    {
        while (true)
        {
            if (TrySpawnAsteroid(out Asteroid asteroid))
            {
                MoveAndLaunchAsteroid(asteroid);
            }
            yield return new WaitForSeconds(seconds);
        }
    }

    private bool TrySpawnAsteroid(out Asteroid asteroid, AsteroidType asteroidType = AsteroidType.None)
    {
        if (!m_asteroidPool.TryGet(out asteroid))
        {
            return false;
        }

        if (asteroidType == AsteroidType.None)
        {
            asteroidType = Globals.ASTEROID_TYPES[Random.Range(1, Globals.ASTEROID_TYPES.Length)];
        }

        asteroid.Enable(asteroidType);

        return true;
    }

    private void MoveAndLaunchAsteroid(Asteroid asteroid)
    {
        float arenaBuffer = Globals.ARENA_BUFFER;
        float xSpawnBounds = CameraBounds.x + arenaBuffer * 2;
        float ySpawnBounds = CameraBounds.y + arenaBuffer * 2;

        bool clampX = Random.value > .5f;

        float xPos;
        float yPos;

        if (clampX)
        {
            bool right = Random.value > .5f;
            xPos = CameraBounds.x + arenaBuffer + Random.Range(0, arenaBuffer) * (right ? 1 : -1);
            yPos = Random.Range(-ySpawnBounds, ySpawnBounds);
        }
        else
        {
            xPos = Random.Range(-xSpawnBounds, xSpawnBounds);
            bool above = Random.value > .5f;
            yPos = CameraBounds.y + arenaBuffer + Random.Range(0, arenaBuffer) * (above ? 1 : -1);
        }

        Vector2 position = new Vector2(xPos, yPos);

        float xTarget = Random.Range(-CameraBounds.x, CameraBounds.x);
        float yTarget = Random.Range(-CameraBounds.y, CameraBounds.y);
        Vector2 target = new Vector2(xTarget, yTarget);

        Vector2 heading = (target - position).normalized;
        Vector2 velocity = heading * Random.Range(speedRange.x, speedRange.y);

        asteroid.SetMovement(position, velocity);
    }

    public void LeaveAsteroid(Asteroid asteroid)
    {
        asteroid.gameObject.SetActive(false);
        m_asteroidPool.Release(asteroid);
    }

    public void ExplodeAsteroid(Asteroid asteroid, Vector2 velocity)
    {
        AsteroidType oldType = asteroid.AsteroidType;

        ++m_score;
        scoreText.text = m_score.ToString();

        if (m_destructionParticlePool.TryGet(out ParticleSystem ps))
        {
            ps.gameObject.SetActive(true);
            ps.transform.position = asteroid.transform.position;
            float scale = Globals.ASTEROID_TYPE_MAP[asteroid.AsteroidType].Size / 3f;
            StartCoroutine(PlayParticleSystem(ps, scale));
        }


        if (asteroid.AsteroidType == AsteroidType.Small)
        {
            asteroid.gameObject.SetActive(false);
            m_asteroidPool.Release(asteroid);
            return;
        }

        AsteroidType newType = (AsteroidType)((int)asteroid.AsteroidType - 1);

        Vector2 oldPos = asteroid.transform.position;

        Vector2 pos1 = new Vector2(Random.Range(0, Globals.ASTEROID_TYPE_MAP[oldType].Size), Random.Range(0, Globals.ASTEROID_TYPE_MAP[oldType].Size));
        Vector2 v1 = new Vector2(Random.Range(2, 4f), Random.Range(2, 4f));

        asteroid.Enable(newType);
        asteroid.SetMovement(oldPos + pos1, velocity + v1);

        Asteroid otherAsteroid;
        if (!TrySpawnAsteroid(out otherAsteroid, newType))
        {
            return;
        }

        Vector2 pos2 = new Vector2(Random.Range(-Globals.ASTEROID_TYPE_MAP[oldType].Size, 0), Random.Range(-Globals.ASTEROID_TYPE_MAP[oldType].Size, 0));
        Vector2 v2 = new Vector2(Random.Range(-4f, -2), Random.Range(-4f, -2));

        otherAsteroid.SetMovement(oldPos + pos2, velocity + v2);
    }

    private IEnumerator PlayParticleSystem(ParticleSystem ps, float scale)
    {
        ps.transform.localScale = scale * Vector3.one;
        ps.Play();

        while (ps.isPlaying)
        {
            yield return null;
        }

        ps.gameObject.SetActive(false);
        m_destructionParticlePool.Release(ps);
    }

    public void Destroyed()
    {
        m_playerGameObject.SetActive(false);
        StartCoroutine(Destruction());
    }

    private IEnumerator Destruction()
    {
        yield return new WaitForSeconds(3f);

        BulletManager.Instance.Restart();
        m_asteroidPool.Reset();
        m_destructionParticlePool.Reset();

        m_score = 0;
        scoreText.text = m_score.ToString();

        m_playerGameObject.SetActive(true);
        ShipController.Instance.Reset();
    }
}

