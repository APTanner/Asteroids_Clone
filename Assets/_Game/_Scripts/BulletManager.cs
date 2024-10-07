using System.Collections;
using UnityEngine;

public class BulletManager : Singleton<BulletManager>
{
    [Header("Prefabs")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private ParticleSystem bulletHitParticle;

    private ObjectPool<Bullet> m_bulletPool;
    private ObjectPool<ParticleSystem> m_hitParticlePool;

    protected override void Awake()
    {
        base.Awake();
        m_bulletPool = new ObjectPool<Bullet>(bulletPrefab, Globals.BULLET_MAX);
        m_hitParticlePool = new ObjectPool<ParticleSystem>(bulletHitParticle, Globals.BULLET_MAX);
    }

    public void Restart()
    {
        m_bulletPool.Reset();
        m_hitParticlePool.Reset();
    }

    public void Fire(Transform t, Vector3 velocity)
    {
        Bullet bullet;
        if (!m_bulletPool.TryGet(out bullet))
        {
            return;
        }

        bullet.Fire(t.position, t.up, velocity);
    }

    public void Release(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);

        if (m_hitParticlePool.TryGet(out ParticleSystem ps))
        {
            ps.gameObject.SetActive(true);
            ps.transform.position = bullet.transform.position;
            ps.transform.up = -bullet.transform.up;
            StartCoroutine(PlayParticle(ps));
        }
        m_bulletPool.Release(bullet);
    }

    private IEnumerator PlayParticle(ParticleSystem ps)
    {
        ps.Play();

        while (ps.isPlaying)
        {
            yield return null;
        }

        ps.gameObject.SetActive(false);
        m_hitParticlePool.Release(ps);
    }
}
