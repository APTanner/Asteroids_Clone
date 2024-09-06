using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController : MonoBehaviour
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

    private PlayerInput m_playerInput;
    private Rigidbody2D m_rb;

    private float m_turnInput;
    private float m_forwardInput;

    private void Awake()
    {
        m_playerInput = new PlayerInput();
        m_rb = GetComponent<Rigidbody2D>();
        if (m_rb == null)
        {
            Debug.LogError("Ship controller requires a rigidbody to function");
        }
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
    }

    private void FixedUpdate()
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
}
