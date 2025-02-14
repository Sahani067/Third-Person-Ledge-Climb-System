using System.Collections;
using UnityEngine;

public class LedgeToRoofClimb : MonoBehaviour
{
    PlayerClimb playerClimb;
    ShimmyController shimmyController;

    public LayerMask ledgeGroundLayer;

    public float rayHeight = 1.8f;

    RaycastHit ledgeToClimbHit;

    public bool foundLedgeToRoofClimb;

    public GameObject climbPointObjPrefab;
    public GameObject climbPointObj;


    private void Start()
    {
        playerClimb = GetComponent<PlayerClimb>();
        shimmyController = GetComponent<ShimmyController>();
    }

    private void Update()
    {
        if (playerClimb.isClimbing)
        {
            Debug.DrawRay(transform.position + new Vector3(0, rayHeight, 0), transform.forward, Color.blue);
            if (Physics.Raycast(transform.position + new Vector3(0, rayHeight, 0), transform.forward, 1))
            {
                foundLedgeToRoofClimb = false;
            }
            else
            {
                Debug.DrawRay(shimmyController.ledgeHit.point + new Vector3(0, 0.7f, 0), Vector3.down, Color.blue);
                if (Physics.Raycast(shimmyController.ledgeHit.point + new Vector3(0, 0.7f, 0), Vector3.down, out ledgeToClimbHit, 1, ledgeGroundLayer))
                {
                    foundLedgeToRoofClimb = true;
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        climbPointObj = Instantiate(climbPointObjPrefab, ledgeToClimbHit.point, Quaternion.identity);
                        StartCoroutine(LedgeToClimb());
                    }
                }
            }
        }



        // Hop Down Target Match
        if (playerClimb.animator.GetCurrentAnimatorStateInfo(0).IsName("Braced Hang To Crouch") && !playerClimb.animator.IsInTransition(0))
        {
            playerClimb.animator.MatchTarget(climbPointObj.transform.position , transform.rotation, AvatarTarget.RightFoot, new MatchTargetWeightMask(new Vector3(0, 1, 1), 0), 0.41f, 0.87f);
        }
    }


    IEnumerator LedgeToClimb()
    {
        playerClimb.animator.CrossFade("Braced Hang To Crouch", 0.2f);

        yield return new WaitForSeconds(1);

        climbPointObj = null;


        playerClimb.isClimbing = false;
        playerClimb.playerState = PlayerState.NormalState;
    }
}
