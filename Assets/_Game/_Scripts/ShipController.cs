using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : Singleton<ShipController>
{
    [Header("Movement Behavior")]
    [Min(0f)]
    [SerializeField] private float maxSpeed;
    [Min(0f)]
    [SerializeField] private float acceleration;
    [Range(0, 10)]
    [SerializeField] private float drag;
    [Min(0f)]
    [SerializeField] private float maxTurnSpeed;
    [Range(0, 10)]
    [SerializeField] private float turnDrag;
    [Min(0f)]
    [SerializeField] private float turnAcceleration;

    [Header("Bullets")]
    public float bulletSpeed;
    [SerializeField] private Bullet bulletPrefab;

    private PlayerInput m_playerInput;
    private Rigidbody2D m_rb;

    private float m_turnInput;
    private float m_forwardInput;
    private bool m_shoot;

    private ObjectPool<Bullet> m_bulletPool;

    private Transform m_barrel;

    protected override void Awake()
    {
        base.Awake();

        m_playerInput = new PlayerInput();
        m_rb = GetComponent<Rigidbody2D>();
        if (m_rb == null)
        {
            Debug.LogError("Ship controller requires a rigidbody to function");
        }
        m_bulletPool = new ObjectPool<Bullet>(bulletPrefab, Globals.BULLET_MAX);
        m_barrel = transform.Find("Barrel");
    }

    private void OnEnable()
    {
        m_playerInput.Enable();
    }

    private void OnDisable()
    {
        m_playerInput.Disable();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        Vector2 temp = m_playerInput.Player.Move.ReadValue<Vector2>();
        m_turnInput = -temp.x;
        m_forwardInput = temp.y;
        // Clamp input so we always move forward
        m_forwardInput = Mathf.Max(m_forwardInput, 0f);

        m_shoot = m_playerInput.Player.Fire.IsPressed();
    }

    private void FixedUpdate()
    {
        HandleInput();
        Wrap();
        Fire();
    }

    private void HandleInput()
    {
        // If the player is making an input don't apply the drag
        // drag can lower the max speed if it has a high coefficient

        float newAngularVelocity = m_rb.angularVelocity;
        newAngularVelocity += m_turnInput * turnAcceleration * Time.fixedDeltaTime;

        if (Mathf.Approximately(m_turnInput, 0f))
        {
            newAngularVelocity *= (1 - turnDrag / 10f);
        }

        newAngularVelocity = Mathf.Clamp(newAngularVelocity, -maxTurnSpeed, maxTurnSpeed);
        m_rb.angularVelocity = newAngularVelocity;

        Vector2 newVelocity = m_rb.velocity;
        newVelocity += (Vector2)m_rb.transform.up * m_forwardInput * acceleration * Time.fixedDeltaTime;

        if (Mathf.Approximately(m_forwardInput, 0f))
        {
            newVelocity *= (1 - drag / 10f);
        }

        newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
        m_rb.velocity = newVelocity;
    }

    private void Wrap()
    {
        Vector3 pos = this.transform.position;
        if (!Helpers.OutOfBounds(pos, 1f))
        {
            return;
        }

        Vector2 camBounds = GameManager.Instance.CameraBounds;

        float newX = Mathf.Clamp(pos.x, -(camBounds.x + 1), camBounds.x + 1);
        float newY = Mathf.Clamp(pos.y, -(camBounds.y + 1), camBounds.y + 1);

        // if the newX or newY is different than the old, it was clamped (outside) and should be moved to the other side

        if (pos.x != newX)
        {
            pos.x = -newX;
        }
        if (pos.y != newY)
        {
            pos.y = -newY;
        }
        this.transform.position = pos;
    }

    private void Fire()
    {
        if (!m_shoot)
        {
            return;
        }

        m_shoot = false;

        Bullet bullet;
        if (!m_bulletPool.TryGet(out bullet)) {
            return;
        }

        bullet.Fire(m_barrel.position, transform.up, m_rb.velocity);
    }

    public void ReleaseBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        m_bulletPool.Release(bullet);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        m_rb.velocity = Vector2.zero;
        this.transform.position = Vector2.zero;

        Bullet[] bullets = FindObjectsOfType<Bullet>();
        for (int i = 0; i < bullets.Length; ++i)
        {
            ReleaseBullet(bullets[i]);
        }

        GameManager.Instance.Restart();
    }
}
