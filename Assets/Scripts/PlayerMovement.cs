using System.Collections;
using UnityEngine;

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private int moveDistance = 1;
    [SerializeField] private float animateSpeed;
    private Vector2Int targetPos;
    public Vector3 TargetPosV3 => new Vector3(targetPos.x, targetPos.y, 0);
    public bool UpPressed => Input.GetKey(KeyCode.W);
    public bool DownPressed => Input.GetKey(KeyCode.S);
    public bool RightPressed => Input.GetKey(KeyCode.D);
    public bool LeftPressed => Input.GetKey(KeyCode.A);

    [SerializeField] private EnvironmentManager environmentManager;
    [SerializeField] private Transform coin;
    [SerializeField] private ScoreAgent agent;

    [SerializeField] private float mlMovedCloserReward = 0.1f;
    [SerializeField] private float mlMovedFurtherReward = -.05f;

    public void ResetTargetPos(Vector2Int resetTo)
    {
        targetPos = resetTo;
    }

    public void TryMove(Direction d)
    {
        // if cannot move, don't!
        if (!environmentManager.CanMove(targetPos, d)) return;

        // Determine the current number of steps to target to get to the target
        int prevCurrentNumStepsToCoin = environmentManager.GetNumStepsToPos(TargetPosV3, coin.transform.position);

        // Move depending on the sought after direction
        switch (d)
        {
            case Direction.Up:
                targetPos += new Vector2Int(0, moveDistance);
                break;
            case Direction.Down:
                targetPos -= new Vector2Int(0, moveDistance);
                break;
            case Direction.Right:
                targetPos += new Vector2Int(moveDistance, 0);
                break;
            case Direction.Left:
                targetPos -= new Vector2Int(moveDistance, 0);
                break;
        }

        int newNumStepsToCoin = environmentManager.GetNumStepsToPos(TargetPosV3, coin.transform.position);

        // Either add or remove a small reward depending on whether the agent has moved closer to the target or not
        if (prevCurrentNumStepsToCoin > newNumStepsToCoin)
        {
            // Debug.Log("Closer");
            agent.AddReward(mlMovedCloserReward);
        }
        else
        {
            // Debug.Log("Further");
            agent.AddReward(mlMovedFurtherReward);
        }
    }

    private void Update()
    {
        // Move
        transform.position = Vector3.Lerp(transform.position, TargetPosV3, animateSpeed * Time.deltaTime);
    }
}
