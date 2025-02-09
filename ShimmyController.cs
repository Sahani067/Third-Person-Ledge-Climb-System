using UnityEngine;

public class ShimmyController : MonoBehaviour
{
    PlayerClimb playerClimbScript;

    public float sphereRadius;
    public float sphereGap;

    public float rayHeight = 1.6f;
    public float rayLength = 1.0f;

    public bool canMoveRight;
    public bool canMoveLeft;

    RaycastHit ledgeHit;

    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
    }

    private void Update()
    {
        while (playerClimbScript.isClimbing)
        {
            Debug.DrawRay(transform.position + Vector3.up * rayHeight, transform.forward * rayLength, Color.magenta);

            Physics.Raycast(transform.position + Vector3.up * rayHeight, transform.forward, out ledgeHit, rayLength, playerClimbScript.ledgeLayer);

            CheckSphere();

            break;
        }
    }

    public bool leftBtn;
    public bool rightBtn;
    public float ledgeMoveSpeed = 0.5f;
    float horizontalInp;
    float horizontalValue;

    void CheckSphere()
    {
        if (ledgeHit.point != Vector3.zero)
        {
            // Right Hand Sphere check if it still ledge to move
            if (Physics.CheckSphere(ledgeHit.point + transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer))
            {
                canMoveRight = true;

                rightBtn = Input.GetKey(KeyCode.D);
            }
            else 
            {
                rightBtn = false;

                leftBtn = Input.GetKey(KeyCode.A);
                canMoveRight = false;
            }

            // Left Hand Sphere check if it still ledge to move
            if (Physics.CheckSphere(ledgeHit.point - transform.right * sphereGap, sphereRadius, playerClimbScript.ledgeLayer))
            {
                canMoveLeft = true;

                leftBtn = Input.GetKey(KeyCode.A);
            }
            else
            {
                leftBtn = false;
                canMoveLeft = false;
                rightBtn = Input.GetKey(KeyCode.D);
            }
        }

        // Horizontal Value
        if (leftBtn)
        {
            horizontalValue = -1;
        }
        else if (rightBtn)
        {
            horizontalValue = 1;
        }
        else
        {
            horizontalValue = 0;
        }
        playerClimbScript.animator.SetFloat("Speed", horizontalValue, 0.05f, Time.deltaTime);
        transform.position += transform.right * horizontalValue * ledgeMoveSpeed * Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        if (ledgeHit.point != Vector3.zero)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(ledgeHit.point + transform.right * sphereGap, sphereRadius);
            Gizmos.DrawSphere(ledgeHit.point - transform.right * sphereGap, sphereRadius);
            
            
        }
    }
}
