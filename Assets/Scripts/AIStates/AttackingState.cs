using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingState : AbstractState
{
    public bool directSight;

    public int waitTurns;
    public override void OnStateEnter(AIController aI)
    {
        waitTurns = UnityEngine.Random.Range(1, 4);
        aI.moveQueue.Clear();
        aI.nodeQueue.Clear();
    }

    public override void Update(AIController aI)
    {
        if (aI.enemies.Count == 0)
        {
            aI.currentState = new ExploringState();
            aI.currentState.OnStateEnter(aI);
            return;
        }

        if (aI.enemies.Count > 1)
        {
            AttackMultiplePlayers(aI);
        } else
        {
            AttackOnePlayer(aI);
        }

    }

    private void AttackMultiplePlayers(AIController aI)
    {
        // We have multiple enemies on sight, we will find the closest one and attack him
        foreach (GameObject enemy in aI.enemies)
        {
            if (enemy.gameObject.transform.position == aI.transform.position)
            {
                aI.moveQueue.Clear();
                aI.moveQueue.Enqueue(aI.direction);
                return;
            }
            else
            {
                aI.nextAction = 1;
            }
        }

        var aiPos = new Vector2(aI.transform.position.x, aI.transform.position.y);
        var enemyPos = Vector2.positiveInfinity;
        foreach (GameObject enemy in aI.enemies)
        {
            enemyPos = new Vector2(enemy.transform.position.x, enemy.transform.position.y);
            var hit = Physics2D.Raycast(aiPos, (enemyPos - aiPos).normalized);
            if (hit.rigidbody.gameObject.tag == "Player" && (aiPos.x == enemyPos.x || aiPos.y == enemyPos.y))
            {
                directSight = true;
                break;
            }
            else
                directSight = false;
        }
        // In case we have at least one enemy in direct sight
        if (directSight)
        {
            DirectSight(aI, aiPos, enemyPos);
        } else
        {
            if ((Mathf.Abs(aiPos.x - aI.enemies[0].gameObject.transform.position.x) == 1 && Mathf.Abs(aiPos.y - aI.enemies[0].gameObject.transform.position.y) == 1)
                || (Mathf.Abs(aiPos.x - aI.enemies[1].gameObject.transform.position.x) == 1 && Mathf.Abs(aiPos.y - aI.enemies[1].gameObject.transform.position.y) == 1))
            {
                // In case enemy position is "around the corner", which means one difference on both axis, we wait a few turns for him to come at us
                if (waitTurns > 0)
                {
                    aI.nextAction = 3;
                    waitTurns--;
                    return;
                }
                else
                {
                    waitTurns = UnityEngine.Random.Range(1, 4);
                }
            }
            aI.nextAction = 1;

            // We don't attack everytime, but it is random and dependent on profile.attackChance
            if (aI.profile.attackChance > UnityEngine.Random.Range(0f, 1) && aI.moveQueue.Count == 0 && aI.nodeQueue.Count == 0)
            {
                var closestEnemyDistance = Mathf.Infinity;
                var closestEnemy = aI.enemies[0];
                foreach (GameObject enemy in aI.enemies)
                {
                    if (Mathf.Abs(aiPos.x - enemy.gameObject.transform.position.x) + Mathf.Abs(aiPos.y - enemy.gameObject.transform.position.y) < closestEnemyDistance)
                    {
                        closestEnemy = enemy;
                        closestEnemyDistance = Mathf.Abs(aiPos.x - enemy.gameObject.transform.position.x) + Mathf.Abs(aiPos.y - enemy.gameObject.transform.position.y);
                    }
                }
                AttackClosestEnemy(aI, closestEnemy);
            }
            else
            {
                // Not interested in attacking, we ignore enemy for now (but on direct sight we shoot)
                if (aI.moveQueue.Count == 0)
                {
                    aI.GetNeighbourTarget();
                }

            }
        }


    }

    private void AttackOnePlayer(AIController aI)
    {
        // There is only one enemy nearby, we attack him
        if (aI.enemies.Count == 0)
        {
            return;
        }
        try {
        if (aI.enemies[0].gameObject.transform.position == aI.transform.position)
        {

            aI.moveQueue.Clear();
            aI.moveQueue.Enqueue(aI.direction);
            return;
        }
        else
        {
            aI.nextAction = 1;
        }

        var aiPos = new Vector2(aI.transform.position.x, aI.transform.position.y);
        var enemyPos = new Vector2(aI.enemies[0].transform.position.x, aI.enemies[0].transform.position.y);
        var hit = Physics2D.Raycast(aiPos, (enemyPos - aiPos).normalized);
        if (hit.rigidbody.gameObject.tag == "Player" && (aiPos.x == enemyPos.x || aiPos.y == enemyPos.y))
            directSight = true;
        else
            directSight = false;

            if (directSight)
            {
                DirectSight(aI, aiPos, enemyPos);
            }
            else
            {
                if (Mathf.Abs(aiPos.x - enemyPos.x) == 1 && Mathf.Abs(aiPos.y - enemyPos.y) == 1)
                {
                    if (waitTurns > 0)
                    {
                        aI.nextAction = 3;
                        waitTurns--;
                        return;
                    } else
                    {
                        waitTurns = UnityEngine.Random.Range(1, 4);
                    }
                }
                aI.nextAction = 1;
                // We don't attack everytime, but it is random and dependent on profile.attackChance
                if (aI.profile.attackChance > UnityEngine.Random.Range(0f, 1) && aI.moveQueue.Count == 0 && aI.nodeQueue.Count == 0)
                {
                    AttackClosestEnemy(aI, aI.enemies[0]);
                }
                else
                {
                    // Not interested in attacking, we ignore enemy for now (but on direct sight we shoot)
                    if (aI.moveQueue.Count == 0)
                    {
                        aI.GetNeighbourTarget();
                    }

                }
            }
        } catch (Exception)
        {
            // Sometimes line 123 throws nullReference exception, which is wierd, cause line 118 should prevent this
            // This exception is unhandled, next update aI.enemies are correct
            return;
        }
    }

    private void AttackClosestEnemy(AIController aI, GameObject closestEnemy)
    {
        // In case of multiple enemies nearby, we attack the closest one
        var enemyPos = new Vector2(closestEnemy.gameObject.transform.position.x, closestEnemy.gameObject.transform.position.y);
        if (aI.graph.Contains(new GraphNode(enemyPos.x, enemyPos.y)))
        {
            var enemyNode = aI.graph.FindLast(a => a.Equals(new GraphNode(enemyPos.x, enemyPos.y)));
            var neighbours = new List<GraphNode>();
            foreach (GraphNode g in enemyNode.GetNeighbours())
            {
                if (g != null)
                {
                    neighbours.Add(g);
                }

            }
            aI.GetClosestTarget(neighbours);
            aI.CalculateMovementToNextTarget(aI.nodeQueue.Peek());
        }
        else
        {
            var minimalDistance = Mathf.Infinity;
            GraphNode closestNode = null;
            foreach (GraphNode g in aI.graph)
            {
                if (Vector2.Distance(new Vector2(g.x, g.y), enemyPos) < minimalDistance)
                {
                    minimalDistance = Vector2.Distance(new Vector2(g.x, g.y), enemyPos);
                    closestNode = g;
                }
            }
            aI.GetShortestPathToTarget(closestNode);

        }
    }

    private void DirectSight(AIController aI, Vector2 aiPos, Vector2 enemyPos)
    {
        // If we have direct sight of enemy, we should turn towards him and shoot
        aI.moveQueue.Clear();
        aI.nodeQueue.Clear();
        var difference = aiPos - enemyPos;
        if (difference.x > 0 && (Mathf.Abs(difference.y) < 0.1f))
        {
            if (aI.direction == Direction.Left)
            {
                aI.nextAction = 2;
            }
            else
            {
                aI.moveQueue.Enqueue(Direction.Left);
            }
            // ENEMY ON LEFT
        }
        else if (difference.x < 0 && (Mathf.Abs(difference.y) < 0.1f))
        {
            if (aI.direction == Direction.Right)
            {
                aI.nextAction = 2;
            }
            else
            {
                aI.moveQueue.Enqueue(Direction.Right);
            }
            // ENEMY ON RIGHT
        }
        else if (difference.y < 0 && (Mathf.Abs(difference.x) < 0.1f))
        {
            if (aI.direction == Direction.Up)
            {
                aI.nextAction = 2;
            }
            else
            {
                aI.moveQueue.Enqueue(Direction.Up);
            }
            // ENEMY ON DOWM
        }
        else if (difference.y > 0 && (Mathf.Abs(difference.x) < 0.1f))
        {
            if (aI.direction == Direction.Down)
            {
                aI.nextAction = 2;
            }
            else
            {
                aI.moveQueue.Enqueue(Direction.Down);
            }
            // ENEMY ON UP
        }
    }

}
