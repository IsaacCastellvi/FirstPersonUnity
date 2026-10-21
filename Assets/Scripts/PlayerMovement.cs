using System.Threading;
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

    private bool IsSprinting;
    private Ray FeetCast;
    [SerializeField] private LayerMask FloorHitMask;





    private void Awake() => Input = new _InputActions(); 
    private void OnEnable(){
        Input.Enable();
        Input.Player.Jump.performed += Jump;
        
    }
    private void OnDisable() => Input.Disable();
    private void ProcessRotation()
    {
        Vector2 lookDir = Input.Player.Look.ReadValue<Vector2>();

        //HOR - body 
        Body.localEulerAngles += new Vector3(0, lookDir.x * sensitivity * Time.deltaTime, 0);   
        //VER - camera                          
        CameraTransform.localEulerAngles += new Vector3(-lookDir.y * sensitivity * Time.deltaTime,0, 0);   
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


    private void Start()
    {
        FeetCast = new Ray(transform.position,-transform.up);
    }

    void FixedUpdate()
    {
        ProcessMovement();
        ProcessRotation();

    }
}
