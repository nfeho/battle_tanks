using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingState : AbstractState
{
    public GraphNode target;
    public int hideDuration;
    public bool turnAround;

    public override void OnStateEnter(AIController aI)
    {
        turnAround = false;
        aI.moveQueue.Clear();
        aI.nodeQueue.Clear();
        hideDuration = aI.profile.hideDuration;
        target = aI.GetClosestTarget(aI.holeNodes);
        if (aI.lastVisited.type == GraphNodeType.Hole)
        {
            GoToExplore(aI);
            return;
        }
    }

    public override void Update(AIController aI)
    {
        // We just find closest Hole node to hide in, we reach it, turn around to look at the direction of possible enemies and wait a few turns
        // After that, we just return to exploring
        if (aI.moveQueue.Count == 0)
        {
            if (aI.currentNode.Equals(target))
            {
                if (!turnAround)
                {
                    switch (aI.direction)
                    {
                        case Direction.Up:
                            aI.moveQueue.Enqueue(Direction.Down);
                            break;
                        case Direction.Down:
                            aI.moveQueue.Enqueue(Direction.Up);
                            break;
                        case Direction.Right:
                            aI.moveQueue.Enqueue(Direction.Left);
                            break;
                        case Direction.Left:
                            aI.moveQueue.Enqueue(Direction.Right);
                            break;
                    }
                    turnAround = true;

                }
                else
                {
                    if (hideDuration <= 0)
                    {
                        // Go-to state explore
                        GoToExplore(aI);
                    }
                    else
                    {
                        aI.nextAction = 3;
                        hideDuration--;
                    }
                }

            } else
            {
                if (aI.nodeQueue.Count == 0)
                {
                    aI.CalculateMovementToNextTarget(target);
                } else
                {
                    aI.CalculateMovementToNextTarget(aI.nodeQueue.Dequeue());
                }
            }
        }
    }

    public void GoToExplore(AIController aI)
    {
        aI.currentState = new ExploringState();
        aI.currentState.OnStateEnter(aI);
    }
}
