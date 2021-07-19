using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoveringMateState : AbstractState
{
    public GameObject mateToCover;
    public GraphNode target;

    public override void OnStateEnter(AIController aI)
    {
        mateToCover = aI.teamMates[0];
        aI.nodeQueue.Clear();
        aI.moveQueue.Clear();
    }

    public override void Update(AIController aI)
    {
        // If no teammate is alive, return to exploring
        if (mateToCover == null)
        {
            // GO TO EXPLORE
            aI.currentState = new ExploringState();
            aI.currentState.OnStateEnter(aI);
            return;
        }
        if (aI.nodeQueue.Count == 0 && aI.moveQueue.Count == 0)
        {
            if (aI.currentNode.Equals(target))
            {
                // After we have reached our destination for covering mate GO TO EXPLORE
                aI.currentState = new ExploringState();
                aI.currentState.OnStateEnter(aI);
                return;
            }
            // Go towards teammate last visited node, or if we don't know the node yet, go towards closest goal to teammate
            if (aI.graph.Contains(mateToCover.GetComponent<AIController>().lastVisited))
            {
                aI.GetShortestPathToTarget(aI.graph.Find(g => g.Equals(mateToCover.GetComponent<AIController>().lastVisited)));
                target = mateToCover.GetComponent<AIController>().lastVisited;
            } else
            {
                var minimalDistance = Mathf.Infinity;
                GraphNode closestNode = null;
                foreach (GraphNode g in aI.graph)
                {
                    if (Vector2.Distance(new Vector2(g.x, g.y), mateToCover.transform.position) < minimalDistance)
                    {
                        minimalDistance = Vector2.Distance(new Vector2(g.x, g.y), mateToCover.transform.position);
                        closestNode = g;
                    }
                }
                if (!closestNode.Equals(aI.currentNode))
                    aI.GetShortestPathToTarget(closestNode);
                target = closestNode;
            }

        } else if (aI.moveQueue.Count == 0)
        {
            aI.CalculateMovementToNextTarget(aI.nodeQueue.Dequeue());
        }
    }
}
