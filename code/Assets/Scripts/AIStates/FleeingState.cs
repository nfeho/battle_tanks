using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeingState : AbstractState
{
    public Direction aiDirection;
    public Direction bulletDirection;

    public override void OnStateEnter(AIController aI)
    {
        // Determine the bullet direction
        aiDirection = aI.direction;
        if (1 - Mathf.Abs(Quaternion.Dot(aI.threat.transform.rotation,new Quaternion(0, 0, 0, 1))) < 0.1f)
        {
            bulletDirection = Direction.Left;
        }
        else if (1 - Mathf.Abs(Quaternion.Dot(aI.threat.transform.rotation, new Quaternion(0, 0, -1, 0))) < 0.1f)
        {
            bulletDirection = Direction.Right;
        }
        else if (1 - Mathf.Abs(Quaternion.Dot(aI.threat.transform.rotation, new Quaternion(0, 0, 0.7f, 0.7f))) < 0.1f)
        {
            bulletDirection = Direction.Down;
        }
        else if (1 - Mathf.Abs(Quaternion.Dot(aI.threat.transform.rotation, new Quaternion(0, 0, -0.7f, 0.7f))) < 0.1f)
        {
            bulletDirection = Direction.Up;
        } else
        {
            bulletDirection = Direction.None;
        }

    }

    public override void Update(AIController aI)
    {
        // If no bullets around, we go back to exploring
        if (aI.threat.gameObject == null || Vector3.Distance(aI.threat.gameObject.transform.position, aI.transform.position) > 3.5f)
        {
            GoToExplore(aI);
            return;
        }
        var aiPos = new Vector2(aI.transform.position.x, aI.transform.position.y);
        var bullPos = new Vector2(aI.threat.transform.position.x, aI.threat.transform.position.y);
        var hit = Physics2D.Raycast(aiPos, (bullPos-aiPos).normalized);

        // Only in debug mode
        Debug.DrawLine(aiPos, bullPos, Color.white, 5f, false);

        if (hit.rigidbody.name.Contains("Bullet") && ((bullPos-aiPos).x == 0 || (bullPos-aiPos).y == 0))
        {
            // BULLET CAN REACH YOU
            switch (bulletDirection)
            {
                case Direction.Left:
                    if (bullPos.x < aiPos.x)
                        Flee(bulletDirection, aI);
                    else
                        GoToExplore(aI);
                    break;
                case Direction.Right:
                    if (bullPos.x > aiPos.x)
                        Flee(bulletDirection, aI);
                    else
                        GoToExplore(aI);
                    break;
                case Direction.Up:
                    if (bullPos.y > aiPos.y)
                        Flee(bulletDirection, aI);
                    else
                        GoToExplore(aI);
                    break;
                case Direction.Down:
                    if (bullPos.y < aiPos.y)
                        Flee(bulletDirection, aI);
                    else
                        GoToExplore(aI);
                    break;
            }
        } else
        {
            if (aI.moveQueue.Count > 0)
            {
                // Prevent stepping in the bullet path
                switch (aI.moveQueue.Peek())
                {
                    case Direction.Left:
                        if (aiPos.x - 1 == bullPos.x)
                            aI.nextAction = 3;
                        break;
                    case Direction.Right:
                        if (aiPos.x + 1 == bullPos.x)
                            aI.nextAction = 3;
                        break;
                    case Direction.Down:
                        if (aiPos.y - 1 == bullPos.y)
                            aI.nextAction = 3;
                        break;
                    case Direction.Up:
                        if (aiPos.y + 1 == bullPos.y)
                            aI.nextAction = 3;
                        break;
                }
            }
        }
    }

    private void Flee(Direction bulletDirection, AIController aI)
    {
        if (aI.ContainsCurrent() && aI.graph.Find(g => g.Equals(aI.currentNode)).type != GraphNodeType.Hole)
        {
            if (aI.direction != bulletDirection && aI.graph.Find(g => g.Equals(aI.currentNode)).GetNeighbour(aI.direction) != null)
            {
                // If we can move forward to dodge the bullet, we need to move just once
                aI.moveQueue.Enqueue(aI.direction);
            } else
            {
                // We try to find some neighbour to run to, this requires 2 move cycles
                var neighbours = new List<GraphNode>();
                if (bulletDirection == Direction.Up || bulletDirection == Direction.Down)
                {
                    var curr = aI.graph.Find(g => g.Equals(aI.currentNode)).GetNeighbour(Direction.Right);
                    if (curr != null)
                        neighbours.Add(curr);
                    curr = aI.graph.Find(g => g.Equals(aI.currentNode)).GetNeighbour(Direction.Left);
                    if (curr != null)
                        neighbours.Add(curr);
                    var random = UnityEngine.Random.Range(0, neighbours.Count - 1);
                    aI.moveQueue.Clear();
                    aI.CalculateMovementToNextTarget(neighbours[random]);
                }
                else
                {
                    var curr = aI.graph.Find(g => g.Equals(aI.currentNode)).GetNeighbour(Direction.Up);
                    if (curr != null)
                        neighbours.Add(curr);
                    curr = aI.graph.Find(g => g.Equals(aI.currentNode)).GetNeighbour(Direction.Down);
                    if (curr != null)
                        neighbours.Add(curr);
                    var random = UnityEngine.Random.Range(0, neighbours.Count - 1);
                    aI.moveQueue.Clear();
                    aI.CalculateMovementToNextTarget(neighbours[random]);
                }
            }

        } else
        {
            // If we are in narrow corridor, we have nowhere to move we just try to shoot the enemy too, as we get shot
            if (aI.direction == bulletDirection)
                aI.nextAction = 2;
            else
                aI.moveQueue.Enqueue(bulletDirection);
        }
    }

    private void GoToExplore(AIController aI)
    {
        aI.currentState = new ExploringState();
        aI.currentState.OnStateEnter(aI);
    }
    
}
