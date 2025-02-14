using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerState { NormalState, ClimbingState }

public class PlayerClimb : MonoBehaviour
{
    ThirdPersonController thirdPersonController;
    RoofLedgeDetection roofLedgeDetection;
    public Animator animator;

    [Header("Climbing")]

    public PlayerState playerState;
    

    public bool isClimbing;
    public bool canGrabLedge;

    public int rayAmount = 10;

    public float rayLength = 0.5f;
    public float rayOffset = 0.15f;
    public float rayHeight = 1.7f;

    public RaycastHit rayLedgeForwardHit;
    public RaycastHit rayLedgeDownHit;

    public LayerMask ledgeLayer;

    [Space(5)]
    public float rayYHandCorrection;
    public float rayZHandCorrection;
    [Space(5)]
    public float yDropToHangPos = -0.1f;
    public float zDropToHangPos = -0.05f;
    [Space(5)]
    public float upHopPos = -0.1f;
    public float frowardHopPos = -0.05f;



    private void Start()
    {
        playerState = PlayerState.NormalState;
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
        roofLedgeDetection = GetComponent<RoofLedgeDetection>();
    }

    private void Update()
    {
        CheckingMainRay(); // 1
        Inputs(); // 2
        StateConditionsCheck(); // 3
        MatchTargetToLedge(); // 4


        if (isClimbing)
        {
            HopUpDown();
        }
    }

    

    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.C) && !roofLedgeDetection.isRoofLedgeDetected) // Pressing Button and roof ledge is not detected
        {
            if (!isClimbing ) // if Not Climbing 
            {
                if (canGrabLedge && rayLedgeDownHit.point != Vector3.zero)
                {
                    Quaternion lookRot = Quaternion.LookRotation(-rayLedgeForwardHit.normal);
                    transform.rotation = lookRot;


                    StartCoroutine(GrabLedge()); // Climb
                }

            }
            else // if Climbing 
            {
                // Drop from ledge
                if(verticalInp == 0)
                    StartCoroutine(DropLedge());
            }
        }
    }
    private void CheckingMainRay()
    {
        if(!isClimbing && thirdPersonController.Grounded) // if player is not climbing and player is on the ground
        {
            for (int i = 0; i < rayAmount; i++)
            {
                Vector3 rayPosition = transform.position + Vector3.up * rayHeight + Vector3.up * rayOffset * i;

                //Debug.DrawRay(rayPosition, transform.forward, Color.cyan);

                if (Physics.Raycast(rayPosition, transform.forward, out rayLedgeForwardHit, rayLength, ledgeLayer, QueryTriggerInteraction.Ignore))
                {
                    canGrabLedge = true;

                    //Debug.DrawRay(rayLedgeForwardHit.point + Vector3.up * 0.5f, Vector3.down * 0.7f);
                    Physics.Raycast(rayLedgeForwardHit.point + Vector3.up * 0.5f, Vector3.down, out rayLedgeDownHit, 0.7f, ledgeLayer);

                    return;

                }
                else
                {
                    canGrabLedge = false;
                }
            }
        }
    }
    private void StateConditionsCheck()
    {
        if (playerState == PlayerState.NormalState)
        {
            animator.applyRootMotion = false;

            thirdPersonController._controller.enabled = true;
            thirdPersonController.enabled = true;
        }
        else if (playerState == PlayerState.ClimbingState)
        {
            animator.applyRootMotion = true;

            thirdPersonController._controller.enabled = false;
            thirdPersonController.enabled = false;
        }
    }


    private void MatchTargetToLedge() // Matching Target To Ledge
    {
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle To Braced Hang") && !animator.IsInTransition(0))
        {
            Vector3 handPos = transform.forward * rayZHandCorrection + transform.up * rayYHandCorrection;
            animator.MatchTarget(rayLedgeDownHit.point + handPos, transform.rotation, AvatarTarget.RightHand, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.36f, 0.57f);
        }

        // Drop To Hang Ledge Target Match
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Drop To FreeHang") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * zDropToHangPos + transform.up * yDropToHangPos;
            animator.MatchTarget(roofLedgeDetection.rayLedgeFwdHit.point + handDropPos, transform.rotation, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.65f, 0.71f);
        }

        /////////////////////////////////////
        // Hop Up Target Match
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang Hop Up") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * frowardHopPos + transform.up * upHopPos;
            animator.MatchTarget(hopLedgeDownHit.point + handDropPos, transform.rotation, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.39f, 0.59f);
        }


        // Hop Down Target Match
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("HopDown") && !animator.IsInTransition(0))
        {
            Vector3 handDropPos = transform.forward * frowardHopPos + transform.up * upHopPos;
            animator.MatchTarget(hopLedgeDownHit.point + handDropPos, transform.rotation, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.31f, 0.56f);
        }
    }

    float verticalInp;
    public float rayHopOffset = 0.1f;
    public float rayHopLength = 1f;
    public int hopRayAmount = 7;
    public float rayVerticalGap;
    public float rayHopHeight = 1.6f;

    RaycastHit hopLedgeForwardHit;
    RaycastHit hopLedgeDownHit;


    private void HopUpDown()
    {
        verticalInp = Input.GetAxis("Vertical");

        if (verticalInp < -0.1f)
        {
            HopDownRayCheck();
        }
        else if (verticalInp > 0.1f)
        {
            HopUpRayCheck();
        }
        
    }

    private void HopUpRayCheck()
    {
        for (int i = 0; i < hopRayAmount; i++)
        {
            Vector3 rayPosition = transform.position + Vector3.up * rayHopHeight + Vector3.up * rayVerticalGap + Vector3.up * rayHopOffset * i;
            Debug.DrawRay(rayPosition, transform.forward, Color.green);

            if (Physics.Raycast(rayPosition, transform.forward, out rayLedgeForwardHit, rayHopLength, ledgeLayer, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(rayLedgeForwardHit.point + Vector3.up * 0.35f, Vector3.down, Color.green);

                if (Physics.Raycast(rayLedgeForwardHit.point + Vector3.up * 0.35f, Vector3.down, out hopLedgeDownHit, 0.5f, ledgeLayer))
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        StartCoroutine(HopUp());
                    }
                }


                break;
            }
        }
    }

    private void HopDownRayCheck()
    {
        for (int i = 0; i < hopRayAmount; i++)
        {
            Vector3 rayPosition = transform.position + Vector3.up * rayHopHeight - Vector3.up * rayVerticalGap - Vector3.up * rayHopOffset * i;
            Debug.DrawRay(rayPosition, transform.forward, Color.green);

            if (Physics.Raycast(rayPosition, transform.forward, out rayLedgeForwardHit, rayHopLength, ledgeLayer, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(rayLedgeForwardHit.point + Vector3.up * 0.35f, Vector3.down, Color.green);

                if (Physics.Raycast(rayLedgeForwardHit.point + Vector3.up * 0.35f, Vector3.down, out hopLedgeDownHit, 0.5f, ledgeLayer))
                {
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        StartCoroutine(HopDown());
                    }
                }


                break;
            }
        }
    }

    IEnumerator GrabLedge()
    {
        playerState = PlayerState.ClimbingState;
        isClimbing = true;
        animator.CrossFade("Idle To Braced Hang", 0.2f);
        yield return null;  

    }
    public IEnumerator DropLedge()
    {
        animator.CrossFade("Braced Hang Drop To Ground", 0.2f);
        yield return new WaitForSeconds(0.5f);
        playerState = PlayerState.NormalState;
        isClimbing = false;
    }

    IEnumerator HopUp()
    {
        animator.CrossFade("Braced Hang Hop Up", 0.2f);

        yield return null;
    }

    IEnumerator HopDown()
    {
        animator.CrossFade("HopDown", 0.2f);

        yield return null;
    }


/*    private void OnDrawGizmos()
    {
        if (rayLedgeDownHit.point != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rayLedgeDownHit.point, 0.05f);
        }
    }*/
}
