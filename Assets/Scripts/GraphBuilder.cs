using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraphNode : IEquatable<GraphNode>, IComparable
{
    public float x;
    public float y;
    public GraphNode up;
    private float distanceUp;
    public GraphNode down;
    private float distanceDown;
    public GraphNode left;
    private float distanceLeft;
    public GraphNode right;
    private float distanceRight;
    [SerializeField]
    public GraphNodeType type;
    public bool visited;

    public GraphNode previous;
    public float nodeTotalCost;
    public float estimatedCost;

    public GraphNode(float x, float y)
    {
        this.estimatedCost = 0.0f;
        this.nodeTotalCost = 1.0f;
        this.previous = null;
        visited = false;
        this.x = (float)Math.Round(x);
        this.y = (float)Math.Round(y);
    }

    public void ResetPrevious()
    {
        this.previous = null;
    }

    public bool Equals(GraphNode other)
    {
        if (other == null)
        {
            return false;
        } else
        {
            return (x - other.x == 0 && y - other.y == 0);
        }
    }

    public override string ToString()
    {
        return (x + ", " + y + "; " + previous);
    }

    public int CompareTo(object obj)
    {
        GraphNode node = (GraphNode)obj;
        if (this.estimatedCost < node.estimatedCost)
            return -1;
        if (this.estimatedCost > node.estimatedCost)
            return 1;
        return 0;
    }

    public void UpdateGraphNodeType()
    {
        var numberOfNeighbours = 0;
        if (this.up != null)
            numberOfNeighbours++;
        if (this.down != null)
            numberOfNeighbours++;
        if (this.left != null)
            numberOfNeighbours++;
        if (this.right != null)
            numberOfNeighbours++;

        switch (numberOfNeighbours)
        {
            case 1:
                this.type = GraphNodeType.Hole;
                break;
            case 2:
                this.type = GraphNodeType.Curve;
                break;
            case 3:
                this.type = GraphNodeType.Junction;
                break;
            case 4:
                this.type = GraphNodeType.Open;
                break;
        }

    }

    public GraphNodeType UpdateGraphNodeType(int hits, bool isFirstOrLast)
    {
        switch (hits)
        {
            case 0:
                if (isFirstOrLast)
                    this.type = GraphNodeType.Hole;
                else
                    this.type = GraphNodeType.Straight;
                break;
            case 1:
                if (isFirstOrLast)
                    this.type = GraphNodeType.Curve;
                else
                    this.type = GraphNodeType.Junction;
                break;
            case 2:
                if (isFirstOrLast)
                    this.type = GraphNodeType.Junction;
                else
                    this.type = GraphNodeType.Open;
                break;

        }
        return this.type;
    }

    public GraphNode[] GetNeighbours()
    {
        GraphNode[] neighbours = new GraphNode[4];
        neighbours[0] = this.up;
        neighbours[1] = this.down;
        neighbours[2] = this.left;
        neighbours[3] = this.right;

        return neighbours;
    }

    public GraphNode GetNeighbour(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return this.up;
            case Direction.Down:
                return this.down;
            case Direction.Left:
                return this.left;
            case Direction.Right:
                return this.right;
            default:
                return null;
        }
    }

    public float[] GetDistances()
    {
        float[] distances = new float[4];
        distances[0] = this.distanceUp;
        distances[1] = this.distanceDown;
        distances[2] = this.distanceLeft;
        distances[3] = this.distanceRight;

        return distances;
    }

    public void SetNeighbour(GraphNode node, Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                this.up = node;
                this.distanceUp = node.y - y;
                break;
            case Direction.Down:
                this.down = node;
                this.distanceDown = y - node.y;
                break;
            case Direction.Right:
                this.right = node;
                this.distanceRight = node.x - x;
                break;
            case Direction.Left:
                this.left = node;
                this.distanceLeft = x - node.x;
                break;
        }
    }

}

public enum GraphNodeType
{
    Open,
    Junction,
    Curve,
    Hole,
    Straight
}

public static class GraphBuilder
{
    static int layerMask = ~(LayerMask.GetMask("Player", "Bullet"));

    // Fairly comlicated function, it simulates exploration of graph, by determining all neighbouring nodes of current nodes and their neighbours
    public static List<List<GraphNode>> ExploreGraph(Vector3 position, List<GraphNode> graph, List<GraphNode> unvisitedNodes, List<GraphNode> junctionNodes, List<GraphNode> holeNodes)
    {
        List<GraphNode> xScan = new List<GraphNode>();
        List<GraphNode> yScan = new List<GraphNode>();
        var xDim = new int[2];
        var yDim = new int[2];

        var pos = new Vector2(position.x, position.y);
        Vector2[] casts = new Vector2[4];
        var hits = new RaycastHit2D[4];

        casts[0] = new Vector2(0, 1000f);
        casts[1] = new Vector2(0, -1000f);
        casts[2] = new Vector2(1000f, 0);
        casts[3] = new Vector2(-1000f, 0);
        for (int i = 0; i < 4; i++)
        {
            hits[i] = Physics2D.Raycast(pos, pos + casts[i], float.MaxValue, layerMask);
        }
        yDim[0] = Mathf.FloorToInt(pos.y + hits[0].distance);
        yDim[1] = Mathf.CeilToInt(pos.y - hits[1].distance);
        xDim[0] = Mathf.FloorToInt(pos.x + hits[2].distance);
        xDim[1] = Mathf.CeilToInt(pos.x - hits[3].distance);

        for (int i = yDim[0]; i >= yDim[1]; i--)
        {
            var xHits = 0;
            var origin = new Vector2(pos.x, i);
            var hit = Physics2D.Raycast(origin, origin + casts[2], float.MaxValue, layerMask);
            if (hit.distance >= 1.0f)
                xHits++;
            hit = Physics2D.Raycast(origin, origin + casts[3], float.MaxValue, layerMask);
            if (hit.distance >= 1.0f)
                xHits++;
            if (xHits > 0 || (xHits == 0 && i == yDim[0]) || (xHits == 0 && i == yDim[1]))
            {
                GraphNode node = new GraphNode(origin.x, origin.y);
                yScan.Add(node);
                if (!graph.Contains(node))
                {
                    graph.Add(node);
                    unvisitedNodes.Add(node);
                }
                GraphNodeType nodeType;
                // UPDATE THE NODE TYPE
                if (i == yDim[0] || i == yDim[1])
                    nodeType = node.UpdateGraphNodeType(xHits, true);
                else
                    nodeType = node.UpdateGraphNodeType(xHits, false);
                switch (nodeType)
                {
                    case GraphNodeType.Open:
                    case GraphNodeType.Junction:
                    case GraphNodeType.Curve:
                        if (!junctionNodes.Contains(node))
                            junctionNodes.Add(node);
                        break;
                    case GraphNodeType.Hole:
                        if (!holeNodes.Contains(node))
                            holeNodes.Add(node);
                        break;
                }
            }
        }
        yScan.Sort(CompareByY);
        if (yScan.Count > 1)
        {
            for (int i = 0; i < yScan.Count; i++)
            {
                if (i == 0)
                {
                    graph.FindLast(a => a.Equals(yScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(yScan[i + 1])), Direction.Down);
                }
                else if (i == yScan.Count - 1)
                {
                    graph.FindLast(a => a.Equals(yScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(yScan[i - 1])), Direction.Up);
                }
                else
                {
                    graph.FindLast(a => a.Equals(yScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(yScan[i + 1])), Direction.Down);
                    graph.FindLast(a => a.Equals(yScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(yScan[i - 1])), Direction.Up);
                }
            }
        }

        for (int i = xDim[0]; i >= xDim[1]; i--)
        {
            var yHits = 0;
            var origin = new Vector2(i, pos.y);
            if (Physics2D.Raycast(origin, origin + casts[0], float.MaxValue, layerMask).distance >= 1.0f)
                yHits++;
            if (Physics2D.Raycast(origin, origin + casts[1], float.MaxValue, layerMask).distance >= 1.0f)
                yHits++;
            if (yHits > 0 || (yHits == 0 && i == xDim[0]) || (yHits == 0 && i == xDim[1]))
            {
                GraphNode node = new GraphNode(origin.x, origin.y);
                xScan.Add(node);
                if (!graph.Contains(node))
                {
                    graph.Add(node);
                    unvisitedNodes.Add(node);
                }
                GraphNodeType nodeType;
                // UPDATE THE NODE TYPE
                if (i == xDim[0] || i == xDim[1])
                    nodeType = node.UpdateGraphNodeType(yHits, true);
                else
                    nodeType = node.UpdateGraphNodeType(yHits, false);
                switch (nodeType)
                {
                    case GraphNodeType.Open:
                    case GraphNodeType.Junction:
                    case GraphNodeType.Curve:
                        if (!junctionNodes.Contains(node))
                            junctionNodes.Add(node);
                        break;
                    case GraphNodeType.Hole:
                        if (!holeNodes.Contains(node))
                            holeNodes.Add(node);
                        break;
                }
            }
        }
        xScan.Sort(CompareByX);
        if (xScan.Count > 1)
        {
            for (int i = 0; i < xScan.Count; i++)
            {
                if (i == 0)
                {
                    graph.FindLast(a => a.Equals(xScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(xScan[i + 1])), Direction.Left);
                }
                else if (i == xScan.Count - 1)
                {
                    graph.FindLast(a => a.Equals(xScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(xScan[i - 1])), Direction.Right);
                }
                else
                {
                    graph.FindLast(a => a.Equals(xScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(xScan[i + 1])), Direction.Left);
                    graph.FindLast(a => a.Equals(xScan[i])).SetNeighbour(graph.FindLast(a => a.Equals(xScan[i - 1])), Direction.Right);
                }
            }
        }
        var lists = new List<List<GraphNode>>();
        lists.Add(graph);
        lists.Add(unvisitedNodes);
        lists.Add(junctionNodes);
        lists.Add(holeNodes);
        return lists;
    }


    public static int CompareByX(GraphNode a, GraphNode b)
    {
        return (int)(b.x - a.x);
    }

    public static int CompareByY(GraphNode a, GraphNode b)
    {
        return (int)(b.y - a.y);
    }

}
