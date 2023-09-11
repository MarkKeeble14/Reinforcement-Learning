using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class ScoreAgent : Agent
{
    // Reference to the Coin Target
    [SerializeField] private Transform CoinTarget;

    // Reference to the PlayerMovement script so we can tell the Agent to Move
    [SerializeField] private PlayerMovement p_Movement;

    // Determines if the AI should make all the correct moves or not
    [SerializeField] private bool forceUseHardCodedDirections;
    [SerializeField] private bool allowHumanIntervention;

    // Callback
    public Action CallOnEpisodeBegin;

    // This function determines what information the Agent has to work with
    public override void CollectObservations(VectorSensor sensor)
    {
        // The information the Agent currently has is it's own position
        sensor.AddObservation(transform.position);

        // as well as the position of its target
        sensor.AddObservation(CoinTarget.position);


        // Testing only providing the Relative Distance between the two points as opposed to just the two points
        // sensor.AddObservation(Vector3.Distance(transform.position, CoinTarget.position));
    }

    // This function determines the actions that the Agent takes
    public override void OnActionReceived(ActionBuffers actions)
    {
        // The Agent only truly works in numbers, it is up to us to determine what those numbers mean
        // in this case, we are defining each number 0-4 as either some direction to move, or simply an instruction to remain still

        // the value passed through the DiscreteActions array will be a random number from 0-4, as defined in the Behaviour Parameters Component on the Entity
        int moveDirection = actions.DiscreteActions[0];

        // Debug.Log("OnActionRecieved: " + moveDirection);

        // if that value is 0, we take it as the Agent deciding not to move
        if (moveDirection == 0)
        {
            return;
        }

        // otherwise, that value was some number between 1-4, we can simply cast it to whatever the respective Direction enum value is (Up, Down, Right, Left) and attempt to move in that direction 
        p_Movement.TryMove((Direction)(moveDirection - 1));
    }

    // Reset the Game State
    public override void OnEpisodeBegin()
    {
        CallOnEpisodeBegin?.Invoke();
    }

    // Allows us to override the ActionBuffers to test
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (forceUseHardCodedDirections)
        {
            // Manually set the correct direction to move in
            if (CoinTarget.position.y > p_Movement.TargetPosV3.y) // Up
            {
                discreteActions[0] = 1;
            }
            else if (CoinTarget.position.x > p_Movement.TargetPosV3.x) // Right
            {
                discreteActions[0] = 2;
            }
            else if (CoinTarget.position.y < p_Movement.TargetPosV3.y) // Down
            {
                discreteActions[0] = 3;
            }
            else if (CoinTarget.position.x < p_Movement.TargetPosV3.x) // Left
            {
                discreteActions[0] = 4;
            }
        }

        if (allowHumanIntervention)
        {
            // if the player attempts to move, alter the value in our discrete actions to the appropriate value
            if (p_Movement.UpPressed) // Up
            {
                discreteActions[0] = 1;
            }
            else if (p_Movement.RightPressed) // Right
            {
                discreteActions[0] = 2;
            }
            else if (p_Movement.DownPressed) // Down
            {
                discreteActions[0] = 3;
            }
            else if (p_Movement.LeftPressed) // Left
            {
                discreteActions[0] = 4;
            }
        }
    }
}
