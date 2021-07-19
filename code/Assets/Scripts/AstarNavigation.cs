using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue
{
    // Just priority queue used for Astar
    private ArrayList nodes = new ArrayList();

    public int getLength()
    {
        return this.nodes.Count;
    }

    public bool Contains(object node)
    {
        return this.nodes.Contains(node);
    }

    public GraphNode First()
    {
        if (this.nodes.Count > 0)
        {
            return (GraphNode)this.nodes[0];
        }
        return null;
    }
    public void Push(GraphNode node)
    {
        this.nodes.Add(node);
        this.nodes.Sort();
    }
    public void Remove(GraphNode node)
    {
        this.nodes.Remove(node);
        this.nodes.Sort();
    }
}

public class AstarNavigation
{
    public PriorityQueue openList;
    public PriorityQueue closedList;

    // Heurestic function for one goal (Euclidean distance)
    private static float HeuristicEstimateCost(GraphNode current, GraphNode goal)
    {
        Vector2 euclideanCost = new Vector2(current.x - goal.x, current.y - goal.y);
        return euclideanCost.magnitude;
    }

    // Heurestic function for multiple goals (Euclidean distance), returns closest one
    private static float MultiGoalHeurestic(GraphNode current, List<GraphNode> goals)
    {
        var closest = float.MaxValue;
        foreach (GraphNode g in goals)
        {
            Vector2 euclideanCost = new Vector2(current.x - g.x, current.y - g.y);
            closest = Mathf.Min(closest, euclideanCost.magnitude);
        }
        return closest;
    }

    public List<GraphNode> FindPath(GraphNode start, GraphNode goal)
    {
        openList = new PriorityQueue();
        openList.Push(start);
        start.nodeTotalCost = 0f;
        start.estimatedCost = HeuristicEstimateCost(start, goal);
        closedList = new PriorityQueue();

        GraphNode currentNode = null;

        while (openList.getLength() != 0)
        {
            currentNode = openList.First();
            if (currentNode == goal)
            {
                return GetPath(currentNode);
            }

            GraphNode[] neighbours = currentNode.GetNeighbours();
            float[] distances = currentNode.GetDistances();
            for (int i = 0; i < 4 ; i++)
            {
                if (neighbours[i] != null)
                {
                    if (!closedList.Contains(neighbours[i]))
                    {
                        float totalCost = currentNode.nodeTotalCost + distances[i];
                        float neighbourNodeEstimatedCost = HeuristicEstimateCost(neighbours[i], goal);
                        neighbours[i].nodeTotalCost = totalCost;
                        neighbours[i].previous = currentNode;
                        neighbours[i].estimatedCost = totalCost + neighbourNodeEstimatedCost;
                        if (!openList.Contains(neighbours[i]))
                        {
                            openList.Push(neighbours[i]);
                        }

                    }

                }

            }
            closedList.Push(currentNode);
            openList.Remove(currentNode);
        }
        if (currentNode.x != goal.x || currentNode.y != goal.y)
        {
            //Debug.LogError("Goal not found");
            return null;
        }
        return GetPath(currentNode);
    }

    public List<GraphNode> FindClosestGoalPath(GraphNode start, List<GraphNode> goals)
    {
        // USES MULTIGOAL HEURESTIC, ON REACHING ANY OF THEM, RETURN PATH
        openList = new PriorityQueue();
        openList.Push(start);
        start.nodeTotalCost = 0f;
        start.estimatedCost = MultiGoalHeurestic(start, goals);
        closedList = new PriorityQueue();

        GraphNode currentNode = null;

        while (openList.getLength() != 0)
        {
            currentNode = openList.First();
            if (goals.Contains(currentNode))
            {
                return GetPath(currentNode);
            }

            GraphNode[] neighbours = currentNode.GetNeighbours();
            float[] distances = currentNode.GetDistances();
            for (int i = 0; i < 4; i++)
            {
                if (neighbours[i] != null)
                {
                    if (!closedList.Contains(neighbours[i]))
                    {
                        float totalCost = currentNode.nodeTotalCost + distances[i];
                        float neighbourNodeEstimatedCost = MultiGoalHeurestic(neighbours[i], goals);
                        neighbours[i].nodeTotalCost = totalCost;
                        neighbours[i].previous = currentNode;
                        neighbours[i].estimatedCost = totalCost + neighbourNodeEstimatedCost;
                        if (!openList.Contains(neighbours[i]))
                        {
                            openList.Push(neighbours[i]);
                        }

                    }

                }

            }
            closedList.Push(currentNode);
            openList.Remove(currentNode);
        }
        if (!goals.Contains(currentNode))
        {
            //Debug.LogError("Goal not found");
            return null;
        }
        return GetPath(currentNode);
    }


    private List<GraphNode> GetPath(GraphNode currentNode)
    {
        // Retrieve the path after finding the target node
        List<GraphNode> list = new List<GraphNode>();
        while (currentNode != null)
        {
            list.Add(currentNode);
            currentNode = currentNode.previous;
        }
        list.Reverse();
        return list;
    }
}
