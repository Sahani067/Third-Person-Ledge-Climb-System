using System.Collections;
using UnityEngine;

public class RoofLedgeDetection : MonoBehaviour
{
    PlayerClimb playerClimbScript;

    public bool isRoofLedgeDetected;

    public int rayAmount = 10;

    public float rayLength = 0.5f;
    public float rayOffset = 0.15f;    
    public RaycastHit rayLedgeFwdHit;
    public RaycastHit rayLedgeDwnHit;


    private void Start()
    {
        playerClimbScript = GetComponent<PlayerClimb>();
    }
    private void Update()
    {
        if (!playerClimbScript.isClimbing)
        {
            for (int i = 0; i < rayAmount; i++)
            {
                Vector3 rayPos = transform.position + Vector3.up * 0.5f + transform.forward * rayOffset * i;

               // Debug.DrawRay(rayPos, Vector3.down * rayLength, Color.yellow);

                if (Physics.Raycast(rayPos, Vector3.down, out rayLedgeDwnHit, rayLength, playerClimbScript.ledgeLayer))
                {
                    isRoofLedgeDetected = true;
                    //Debug.DrawRay(rayLedgeDwnHit.point + transform.forward * 0.5f, -transform.forward * 1, Color.yellow);
                    if (Physics.Raycast(rayLedgeDwnHit.point + rayLedgeDwnHit.transform.forward * 0.5f, -rayLedgeDwnHit.transform.forward, out rayLedgeFwdHit, 1, playerClimbScript.ledgeLayer))
                    {
                        
                        if (Input.GetKeyDown(KeyCode.C) && rayLedgeFwdHit.point != Vector3.zero)
                        {
                            StartCoroutine(DropToLedgeHang());
                        }
                    }
                    


                    break;
                }
                else
                {
                    isRoofLedgeDetected = false;
                }
            }
        }
        else
        {
            isRoofLedgeDetected = false;
        }
        
    }

    public bool isDropingFromRoof;

    IEnumerator DropToLedgeHang()
    {
        isDropingFromRoof = true;
        playerClimbScript.animator.CrossFade("Drop To FreeHang", 0.2f);
        playerClimbScript.isClimbing = true;
        playerClimbScript.playerState = PlayerState.ClimbingState;

        yield return new WaitForSeconds(0.2f);
        Quaternion lookRot = Quaternion.LookRotation(-rayLedgeFwdHit.normal);
        transform.rotation = lookRot;

        yield return new WaitForSeconds(1);

        isDropingFromRoof = false;
    }

    private void OnDrawGizmos()
    {
        if (rayLedgeFwdHit.point != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(rayLedgeFwdHit.point, 0.05f);
        }
    }
}
