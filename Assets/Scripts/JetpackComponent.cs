using Unity.Mathematics;
using UnityEngine;

public class JetpackComponent : MonoBehaviour
{
    [SerializeField] private float Boost = 0.5f;
    [SerializeField] private float FuelDuration = 1f;
    [SerializeField] private float RecharcheWaitTime = 0.5f;
    [SerializeField] private float RechargeTime = 0.5f;
    [SerializeField] private Rigidbody Body;

    private PlayerMovement pm;
    private _InputActions controls => pm.Input;
    private float fuel = 1f;
    private float waitTimer;
    private bool depleted;
    private bool holdingJump;

    public bool _isGrounded => pm.isGrounded;
    public float Fuel => fuel;
    void Start() => pm = GetComponent<PlayerMovement>();

    void Impulse()
    {
        if (_isGrounded || !holdingJump || fuel <= 0f) return;

        Body.linearVelocity += Vector3.up * Boost;

        fuel = math.max(fuel - Time.fixedDeltaTime / FuelDuration,0);

        if (fuel <= 0f)
        {
            depleted = true;
            waitTimer = RecharcheWaitTime;
        }
    }

    void Recharge()
    {
        if (!_isGrounded) return;
        if (depleted)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer > 0f) return;
            depleted = false;
        }
        fuel = Mathf.Min(1f, fuel + Time.fixedDeltaTime / RechargeTime);
    }
    void Update()
    {
        holdingJump = controls.Player.Jump.IsInProgress();
    }
    void FixedUpdate()
    {
        Impulse();
        Recharge();
    }
}