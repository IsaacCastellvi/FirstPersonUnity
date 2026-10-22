using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [Header("references")]
    [SerializeField] private Transform Body;
    [SerializeField] private Transform CameraTransform;
    [SerializeField] private Rigidbody Rb;

    public _InputActions Input;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float WalkSpeed = 5;
    [SerializeField] private float GrabDistance = 5;
    [SerializeField] private float HoldDistance = 2f; 
    [SerializeField] private float PullStrength = 15f;
    [SerializeField] private float MaxPullSpeed = 20f;

    [Header("Directional speed")]
    [SerializeField] private float SprintMultiplier = 1;
    [SerializeField] private float ForwardMultiplier = 1f;
    [SerializeField] private float SideMultiplier = 0.75f;
    [SerializeField] private float BackwardMultiplier = 0.5f;
    [SerializeField] private float AirMultiplier = 0.5f;


    private bool IsSprinting;
    private Ray FeetCast;
    private Ray GrabCast;
    private bool isGrabbing;
    public bool isGrounded;
    private Rigidbody GrabTarget;
    [SerializeField] private LayerMask FloorHitMask;
    [SerializeField] private LayerMask GrabHitMask;







    private void Awake() => Input = new _InputActions(); 
    private void OnEnable(){
        Input.Enable();
        Input.Player.Jump.performed += Jump;
        Input.Player.Attack.performed += Grab;
        Input.Player.Attack.canceled += Grab;
        //Input.Player.Sprint.performed 


        
    }
    private void OnDisable() => Input.Disable();
        
    
    private float clampedRotation;
    private void ProcessRotation()
    {
        //TODO: block rotations
        Vector2 lookDir = Input.Player.Look.ReadValue<Vector2>();

        //HOR - body 
        Body.localEulerAngles += new Vector3(0, lookDir.x * sensitivity * Time.deltaTime, 0);   
        //VER - camera                          
        clampedRotation = Mathf.Clamp(clampedRotation - lookDir.y * sensitivity * Time.deltaTime, -90f, 90f);
        CameraTransform.localEulerAngles = new Vector3(clampedRotation, 0f, 0f);
    }
    private void ProcessMovement()
    {
        Vector2 MoveInput = Input.Player.Move.ReadValue<Vector2>();

        float speed = WalkSpeed * (IsSprinting ? SprintMultiplier : 1f);

        //half
        if (!isGrounded) speed *= AirMultiplier;
        //z check
        float forwardMult = MoveInput.y >= 0f ? ForwardMultiplier : BackwardMultiplier;

        //both sides equal
        Vector3 localMove = new Vector3(MoveInput.x * SideMultiplier,0f,MoveInput.y * forwardMult) * speed;
        //dont surpass speed
        localMove = Vector3.ClampMagnitude(localMove, speed);

        Vector3 worldMove = Body.TransformDirection(localMove);
        Rb.linearVelocity = new Vector3(worldMove.x, Rb.linearVelocity.y, worldMove.z);

    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        //needed to follow the anch pos
        FeetCast.origin = transform.position;
        FeetCast.direction = Vector3.down;
        if (isGrounded) Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 2, Rb.linearVelocity.z);
    }
    private void Grab(InputAction.CallbackContext ctx)
    {


        if (ctx.performed) GrabObject();
        else if (ctx.canceled) ReleaseObject();
    }

    private void GrabObject()
    {

        
        if (Physics.Raycast(GrabCast,out RaycastHit hit, 2f, GrabHitMask))
        {
            Debug.Log("grabed");
            GrabTarget = hit.rigidbody;
        }
        
    }
    private void ReleaseObject()
    {
        if (GrabTarget == null) return;

        GrabTarget.useGravity = true;
        GrabTarget = null;
    }

    private void HoldObject()
    {
        if (GrabTarget == null) return;

        Vector3 targetPos = CameraTransform.position + CameraTransform.forward * HoldDistance;
        Vector3 toTarget = targetPos - GrabTarget.position;

        GrabTarget.linearVelocity = Vector3.ClampMagnitude(toTarget * PullStrength, MaxPullSpeed);
        GrabTarget.angularVelocity = Vector3.zero; // optional: stop it spinning
    }



    private void Start()
    {
        FeetCast = new Ray(transform.position,-transform.up);
    }

    void FixedUpdate()
    {
        GrabCast.origin = CameraTransform.position;
        GrabCast.direction = CameraTransform.forward;
        if (Physics.Raycast(FeetCast, 2f, FloorHitMask)) isGrounded = true;
        else isGrounded = false;
        ProcessMovement();
        ProcessRotation();
        HoldObject();

    }
}
