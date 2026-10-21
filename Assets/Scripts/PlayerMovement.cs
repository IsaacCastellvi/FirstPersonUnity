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

    private _InputActions Input;

    [Header("Settings")]
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float WalkSpeed = 5;
    [SerializeField] private float SprintMultiplier = 1;
    [SerializeField] private float GrabDistance = 5;
    [SerializeField] private float HoldDistance = 2f; 
    [SerializeField] private float PullStrength = 15f;
    [SerializeField] private float MaxPullSpeed = 20f;
    private bool IsSprinting;
    private Ray FeetCast;
    private Ray GrabCast;
    private bool isGrabbing;
    private Rigidbody GrabTarget;
    [SerializeField] private LayerMask FloorHitMask;
    [SerializeField] private LayerMask GrabHitMask;







    private void Awake() => Input = new _InputActions(); 
    private void OnEnable(){
        Input.Enable();
        Input.Player.Jump.performed += Jump;
        Input.Player.Attack.performed += Grab;
        Input.Player.Attack.canceled += Grab;


        
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
        clampedRotation = Mathf.Clamp(clampedRotation - lookDir.y * sensitivity, -90f, 90f);
        CameraTransform.localEulerAngles = new Vector3(clampedRotation, 0f, 0f);
    }
    private void ProcessMovement()
    {
        Vector2 MoveInput = Input.Player.Move.ReadValue<Vector2>();

        float speed = WalkSpeed * (IsSprinting ? SprintMultiplier : 1f);

        Vector3 localMove = new Vector3(MoveInput.x * speed, 0f, MoveInput.y * speed);
        Vector3 worldMove = Body.TransformDirection(localMove);

        Rb.linearVelocity = new Vector3(worldMove.x, Rb.linearVelocity.y, worldMove.z);


    }

    private void Jump(InputAction.CallbackContext ctx)
    {
        //needed to follow the anch pos
        FeetCast.origin = transform.position;
        FeetCast.direction = Vector3.down;
        if (Physics.Raycast(FeetCast, 2f, FloorHitMask))
        {
            Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 20, Rb.linearVelocity.z);
        }
    }
    private void Grab(InputAction.CallbackContext ctx)
    {


        if (ctx.performed) GrabObject();
        else if (ctx.canceled) ReleaseObject();
    }

    private void GrabObject()
    {
        GrabCast.origin = CameraTransform.position;
        GrabCast.direction = CameraTransform.forward;
        
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
        ProcessMovement();
        ProcessRotation();
        HoldObject();

    }
}
