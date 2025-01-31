using System;
using System.Collections;
using UnityEngine;

public enum PlayerState { NormalState, ClimbingState }

public class PlayerClimb : MonoBehaviour
{
    ThirdPersonController thirdPersonController;
    Animator animator;

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



    private void Start()
    {
        playerState = PlayerState.NormalState;
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        CheckingMainRay(); // 1
        Inputs(); // 2
        StateConditionsCheck(); // 3
        MatchTargetToLedge(); // 4

    }


    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.C)) // Pressing Button
        {
            if (!isClimbing && rayLedgeDownHit.point != Vector3.zero) // if Not Climbing 
            {
                if (canGrabLedge)
                {
                    Quaternion lookRot = Quaternion.LookRotation(-rayLedgeForwardHit.normal);
                    transform.rotation = lookRot;


                    StartCoroutine(GrabLedge()); // Climb
                }

            }
            else // if Climbing 
            {
                // Drop from ledge
                StartCoroutine(DropLedge());
            }
        }
    }
    private void CheckingMainRay()
    {
        if (!isClimbing && thirdPersonController.Grounded) // if player is not climbing and player is on the ground
        {
            for (int i = 0; i < rayAmount; i++)
            {
                Vector3 rayPosition = transform.position + Vector3.up * rayHeight + Vector3.up * rayOffset * i;

                Debug.DrawRay(rayPosition, transform.forward, Color.cyan);

                if (Physics.Raycast(rayPosition, transform.forward, out rayLedgeForwardHit, rayLength, ledgeLayer, QueryTriggerInteraction.Ignore))
                {
                    canGrabLedge = true;

                    Debug.DrawRay(rayLedgeForwardHit.point + Vector3.up * 0.5f, Vector3.down * 0.7f);
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
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle To Braced Hang") && !animator.IsInTransition(0))
        {
            Vector3 handPos = transform.forward * rayZHandCorrection + transform.up * rayYHandCorrection;
            animator.MatchTarget(rayLedgeDownHit.point + handPos, transform.rotation, AvatarTarget.RightHand, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.36f, 0.57f);
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

    private void OnDrawGizmos()
    {
        if (rayLedgeDownHit.point != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rayLedgeDownHit.point, 0.05f);
        }
    }
}
