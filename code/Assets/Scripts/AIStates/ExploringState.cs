using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploringState : AbstractState
{
    private int nearbyExplored;

    public override void OnStateEnter(AIController aI)
    {
        aI.nextAction = 1;
        aI.moveQueue.Clear();
        aI.nodeQueue.Clear();
        nearbyExplored = 0;
    }

    public override void Update(AIController aI)
    {
        // Explore nearby nodes for aI.profile.nearbyExplorationDuration turns and then target an unexplored node 
        if (aI.moveQueue.Count == 0)
        {
            if (aI.profile.hideChance > Random.Range(0f, 1)) {
                aI.currentState = new HidingState();
                aI.currentState.OnStateEnter(aI);
            }

            if (aI.profile.coverMateChance > Random.Range(0f, 1) && aI.teamMates[0] != null) {
                aI.currentState = new CoveringMateState();
                aI.currentState.OnStateEnter(aI);
            }

            if (aI.nodeQueue.Count == 0)
            {
                if (nearbyExplored < aI.profile.nearbyExploringDuration)
                {
                    // FIND NEAREST
                    aI.GetNeighbourTarget();
                    nearbyExplored++;
                }
                else
                {
                    aI.GetNewTarget();
                    nearbyExplored = 0;
                }
            }
            else
            {
                aI.CalculateMovementToNextTarget(aI.nodeQueue.Dequeue());
            }
        }
    }
}
