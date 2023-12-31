using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class BrushControllerv2 : MonoBehaviour
{
    //This script is used with co-ordination with the Unity's new input system and character controller
    // Start is called before the first frame update
    private Player player;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private int killRequirement = 10;
    [SerializeField] private bool isTop = true;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private GameObject skidMark;
    [SerializeField] private float skidMarkMultiplier;
    private Vector3 move;
    private int killed;

    //helper matrix
    Matrix4x4 matrixRot = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    Matrix4x4 matrixMove = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));

    private void Awake()
    {
        player = new Player();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        player.Enable();
    }

    private void OnDisable()
    {
        player.Disable();
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 movement = player.Move.Movement.ReadValue<Vector2>();
        move = new Vector3(movement.x, 0f, movement.y);
        //For top Down
        if (isTop) { 
            controller.Move(playerSpeed * Time.deltaTime * move);
        }
        //For isometric
        else
        {
            Vector3 skewedInput = matrixMove.MultiplyPoint3x4(move);
            Quaternion rot = Quaternion.LookRotation(matrixRot.MultiplyPoint3x4(move), Vector3.up);

            controller.Move(skewedInput * Time.deltaTime * playerSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
            
        }

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with an object tagged as "Enemy"
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector3 pointOfContact = collision.GetContact(0).point;
            pointOfContact.Set(pointOfContact.x, 0.05f, pointOfContact.z);
            GameObject skid = Instantiate(skidMark, pointOfContact, Quaternion.Euler(-90f, transform.rotation.eulerAngles.y + 270, 0f));
            skid.transform.localScale = transform.localScale * skidMarkMultiplier;
            skid.GetComponent<Renderer>().material = collision.gameObject.GetComponentInChildren<Renderer>().material;
            killed++;
        }

        if (killed == killRequirement)
        {
            Debug.Log("Going to Next Level");
            GetComponent<GameManager>().LevelComplete();
        }
    }
}
