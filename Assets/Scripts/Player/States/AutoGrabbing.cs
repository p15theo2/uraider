using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGrabbing : StateBase<PlayerController>
{
    private float timeTracker = 0f;
    private float distanceToGo = 0f;
    private float distanceTravelled = 0f;

    private Vector3 grabPoint;
    private Vector3 startPosition;
    private LedgeDetector ledgeDetector = LedgeDetector.Instance;

    public override void OnEnter(PlayerController player)
    {
        player.MinimizeCollider();

        player.Anim.SetBool("isAutoGrabbing", true);

        grabPoint = new Vector3(ledgeDetector.GrabPoint.x - (player.transform.forward.x * player.grabForwardOffset),
                        ledgeDetector.GrabPoint.y - player.grabUpOffset,
                        ledgeDetector.GrabPoint.z - (player.transform.forward.z * player.grabForwardOffset));

        Vector3 multiplier = new Vector3(1f, 0f, 1f);

        distanceToGo = Mathf.Abs(Vector3.Distance(
            Vector3.Scale(grabPoint, multiplier), 
            Vector3.Scale(player.transform.position, multiplier))) - 0.01f;

        startPosition = player.transform.position;

        player.Velocity = UMath.VelocityToReachPoint(player.transform.position,
            grabPoint,
            player.gravity,
            player.grabTime);

        timeTracker = Time.time;
    }

    public override void OnExit(PlayerController player)
    {
        player.MaximizeCollider();

        player.Anim.SetBool("isAutoGrabbing", false);
    }

    public override void Update(PlayerController player)
    {
        player.ApplyGravity(player.gravity);

        Vector3 multiplier = new Vector3(1f, 0f, 1f);

        distanceTravelled = Mathf.Abs(Vector3.Distance(
            Vector3.Scale(player.transform.position, multiplier), 
            Vector3.Scale(startPosition, multiplier)));

        if (Time.time - timeTracker >= player.grabTime)
        {
            player.transform.position = grabPoint;

            if (ledgeDetector.WallType == LedgeType.Free)
                player.StateMachine.GoToState<Freeclimb>();
            else
                player.StateMachine.GoToState<Climbing>();
        }
    }
}
