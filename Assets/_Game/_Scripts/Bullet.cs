using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D m_rb;

    public void Fire(Vector2 position, Vector2 heading, Vector2 shipVelocity)
    {
        this.transform.position = position;
        this.transform.up = heading;

        this.gameObject.SetActive(true);
        m_rb.AddForce(heading * ShipController.Instance.bulletSpeed, ForceMode2D.Impulse);
        m_rb.AddForce(shipVelocity, ForceMode2D.Impulse);
    }

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();    
    }

    void FixedUpdate()
    {
        if (Helpers.OutOfBounds(transform.position, 0, true))
        {
            ShipController.Instance.ReleaseBullet(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            return;
        }

        asteroid.TakeDamage(transform.position);

        ShipController.Instance.ReleaseBullet(this);
    }
}
