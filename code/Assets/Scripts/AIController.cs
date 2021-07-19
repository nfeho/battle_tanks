using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AIController : AbstractController
{
    public Queue<GraphNode> nodeQueue;
    public Queue<Direction> moveQueue;
   
    public List<GraphNode> junctionNodes;
    public List<GraphNode> holeNodes;
    public List<GraphNode> unvisitedNodes;

    public List<GameObject> teamMates;

    public GraphNode lastVisited;
    public int nextAction; // #1 MOVE, #2 SHOOT, #3 STAY

    public AbstractState currentState;

    public GraphNode currentNode;
    public BotProfile profile;
    public GameObject threat;
    public List<GameObject> enemies;

    private AITriggerDetector trigger;

    [SerializeField]
    public List<GraphNode> graph = new List<GraphNode>();

    // Start is called before the first frame update
    public override void Start()
    {
        //profile = GlobalManager.Instance.tacticalProfile;
        trigger = this.GetComponentInChildren<AITriggerDetector>();
        threat = null;
        enemies = new List<GameObject>();
        nodeQueue = new Queue<GraphNode>();
        moveQueue = new Queue<Direction>();
        timer = inputTimer;
        moveXY = transform.position;
        newAngle = transform.eulerAngles;

        nextAction = 1;
        currentState = new ExploringState();
        currentState.OnStateEnter(this);

        
        var newLists = GraphBuilder.ExploreGraph(transform.position, graph, unvisitedNodes, junctionNodes, holeNodes);
        graph = newLists[0];
        unvisitedNodes = newLists[1];
        junctionNodes = newLists[2];
        holeNodes = newLists[3];

        graph.Find(a => a.Equals(new GraphNode(transform.position.x, transform.position.y))).visited = true;
        graph.Find(a => a.Equals(new GraphNode(transform.position.x, transform.position.y))).UpdateGraphNodeType();
        unvisitedNodes.Remove(new GraphNode(transform.position.x, transform.position.y));
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Handles AI actions
        // AI will perform an action periodically after inputTimer seconds
        // Shooting takes twice as much as other actions
        if (timer >= inputTimer)
        {
            transform.position = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));

            currentNode = new GraphNode(transform.position.x, transform.position.y);

            if (graph.Contains(currentNode))
                lastVisited = graph.FindLast(a => a.Equals(currentNode));

            // If we are in node we haven't visited yet
            if (graph.Contains(currentNode) && !graph.Find(a => a.Equals(currentNode)).visited)
            {
                var newLists = GraphBuilder.ExploreGraph(transform.position, graph, unvisitedNodes, junctionNodes, holeNodes);
                graph = newLists[0];
                unvisitedNodes = newLists[1];
                junctionNodes = newLists[2];
                holeNodes = newLists[3];
                
                graph.Find(a => a.Equals(new GraphNode(transform.position.x, transform.position.y))).visited = true;
                unvisitedNodes.Remove(new GraphNode(transform.position.x, transform.position.y));
            }

            // Every action update check enemies
            if (enemies.Count != 0)
                CheckEnemies();
            currentState.Update(this);

            switch (nextAction)
            {
                case 1:
                    var desiredMovement = Direction.None;
                    if (moveQueue.Count != 0)
                    {
                        desiredMovement = moveQueue.Dequeue();
                    }
                    
                    if (desiredMovement == Direction.Up)
                    {
                        if (direction != Direction.Up)
                        {
                            newAngle = new Vector3(0, 0, 90);
                            direction = Direction.Up;
                        }
                        else
                        {
                            moveXY = transform.position + new Vector3(0, 1f, 0);
                        }
                        timer = 0f;
                    }
                    else if (desiredMovement == Direction.Down)
                    {
                        if (direction != Direction.Down)
                        {
                            newAngle = new Vector3(0, 0, 270);
                            direction = Direction.Down;
                        }
                        else
                        {
                            moveXY = transform.position + new Vector3(0, -1f, 0);
                        }
                        timer = 0f;
                    }
                    else if (desiredMovement == Direction.Left)
                    {
                        if (direction != Direction.Left)
                        {
                            newAngle = new Vector3(0, 0, 180);
                            direction = Direction.Left;
                        }
                        else
                        {
                            moveXY = transform.position + new Vector3(-1f, 0, 0);
                        }
                        timer = 0f;
                    }
                    else if (desiredMovement == Direction.Right)
                    {
                        if (direction != Direction.Right)
                        {
                            newAngle = new Vector3(0, 0, 0);
                            direction = Direction.Right;
                        }
                        else
                        {
                            moveXY = transform.position + new Vector3(1f, 0, 0);
                        }
                        timer = 0f;
                    }
                    timer = 0f;
                    break;
                case 2:
                    Shoot();
                    timer = -inputTimer;
                    break;
                case 3:
                    timer = 0f;
                    break;
            }

        }

        timer += Time.deltaTime;
        transform.eulerAngles = AngleLerp(transform.eulerAngles, newAngle, timer/ 1.6f);
        transform.position = Vector3.Lerp(transform.position, moveXY, timer/ 2f);
    }

    private void CheckEnemies()
    {
        // Function for checking if enemy is present, if not, remove from list of enemies
        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (enemy.gameObject == null || Vector3.Distance(transform.position, enemy.transform.position) > 3.5f)
                enemies.Remove(enemy);
        }
    }

    private void Shoot()
    {
        // Instatiates bullet in direction AI is facing, sets ignore collision of bullet collider with AI collider
        Vector3 bulletPosition;
        switch (direction)
        {
            case Direction.Up:
                bulletPosition = new Vector3(0, 0.5f, 0);
                break;
            case Direction.Down:
                bulletPosition = new Vector3(0, -0.5f, 0);
                break;
            case Direction.Right:
                bulletPosition = new Vector3(0.5f, 0, 0);
                break;
            case Direction.Left:
                bulletPosition = new Vector3(-0.5f, 0, 0);
                break;
            default:
                bulletPosition = new Vector3(0.5f, 0, 0);
                break;
        }
        var myBullet = GameObject.Instantiate(Bullet, transform.position + bulletPosition, transform.rotation).GetComponent<BulletController>();
        Physics2D.IgnoreCollision(this.GetComponent<Collider2D>(), myBullet.GetComponent<Collider2D>());
        Physics2D.IgnoreCollision(trigger.GetComponent<Collider2D>(), myBullet.GetComponent<Collider2D>());
    }


    
    public void GetNewTarget()
    {
        // Tries to make some unvisited node a target
        // If all nodes explored, target random node
        AstarNavigation a = new AstarNavigation();
        var f = graph.Find(q => q.Equals(new GraphNode(transform.position.x, transform.position.y)));
        if (f == null)
        {
            // WE ARE NOT INSIDE A NODE
            f = CreateTemporaryNode();
        }
        List<GraphNode> path = new List<GraphNode>();
        int random = 0;
        if (unvisitedNodes.Count > 0)
        {
            random = UnityEngine.Random.Range(0, unvisitedNodes.Count);
            path = a.FindPath(f, unvisitedNodes[random]);
        } else
        {
            random = UnityEngine.Random.Range(0, graph.Count);
            path = a.FindPath(f, graph[random]);
        }

        foreach (GraphNode g in graph)
        {
            g.ResetPrevious();
        }

        foreach (GraphNode p in path)
        {
            nodeQueue.Enqueue(p);
        }

        CalculateMovementToNextTarget(nodeQueue.Dequeue());
    }

    public GraphNode GetClosestTarget(List<GraphNode> listOfNodes)
    {
        // Finds the closest node in list of target nodes
        AstarNavigation a = new AstarNavigation();
        var f = graph.Find(q => q.Equals(new GraphNode(transform.position.x, transform.position.y)));
        if (f == null)
        {
            // WE ARE NOT INSIDE A NODE
            f = CreateTemporaryNode();
        }

        var path = a.FindClosestGoalPath(f, listOfNodes);
        foreach (GraphNode g in graph)
        {
            g.ResetPrevious();
        }

        foreach (GraphNode p in path)
        {
            nodeQueue.Enqueue(p);
        }
        return path[path.Count-1];
    }

    public void GetShortestPathToTarget(GraphNode target)
    {
        // Finds shortest path to a target node from current position
        AstarNavigation a = new AstarNavigation();
        var f = graph.Find(q => q.Equals(new GraphNode(transform.position.x, transform.position.y)));
        if (f == null)
        {
            // WE ARE NOT INSIDE A NODE
            f = CreateTemporaryNode();
        }
        var path =  a.FindPath(f, target);
        foreach (GraphNode g in graph)
        {
            g.ResetPrevious();
        }

        foreach (GraphNode p in path)
        {
            nodeQueue.Enqueue(p);
        }
    }

    public void GetNeighbourTarget()
    {
        // Goes to some neighbour of current node, primarily targets unvisited neighbours
        var f = graph.Find(q => q.Equals(new GraphNode(transform.position.x, transform.position.y)));
        if (f == null)
        {
            // WE ARE NOT INSIDE A NODE
            f = CreateTemporaryNode();
        }

        var neighbours = f.GetNeighbours();
        var elligible = new List<GraphNode>();
        var unexplored = new List<GraphNode>();
        for (int i = 0; i < 4; i++)
        {
            if (neighbours[i] != null && neighbours[i].type != GraphNodeType.Hole)
            {
                elligible.Add(neighbours[i]);
                if (!neighbours[i].visited)
                {
                    unexplored.Add(neighbours[i]);
                }
            }
        }
        if (unexplored.Count != 0)
            CalculateMovementToNextTarget(unexplored[UnityEngine.Random.Range(0, unexplored.Count)]);
        else
            CalculateMovementToNextTarget(elligible[UnityEngine.Random.Range(0, elligible.Count)]);
    }

    public void CalculateMovementToNextTarget(GraphNode target)
    {
        // Calculates how many moves it takes to move from current next node in nodeQueue, takes rotating in account
        if (currentNode.Equals(target))
        {
            if (nodeQueue.Count != 0)
            {
                var newTarget = nodeQueue.Dequeue();
                CalculateMovementToNextTarget(newTarget);
            }
            return;
        }

        var distance = 0;
        var distanceBonus = 0;
        var direction = Direction.None;
        if (currentNode.x == target.x)
        {
            if (currentNode.y > target.y)
            { // DOWN
                if (this.direction != Direction.Down)
                    distanceBonus = 1;
                distance = Mathf.RoundToInt(currentNode.y - target.y);
                direction = Direction.Down;
            }
            else
            { //UP
                if (this.direction != Direction.Up)
                    distanceBonus = 1;
                distance = Mathf.RoundToInt(target.y - currentNode.y);
                direction = Direction.Up;
            }
        }
        else
        {
            if (currentNode.x > target.x)
            { // LEFT
                if (this.direction != Direction.Left)
                    distanceBonus = 1;
                distance = Mathf.RoundToInt(currentNode.x - target.x);
                direction = Direction.Left;
            }
            else
            { // RIGHT
                if (this.direction != Direction.Right)
                    distanceBonus = 1;
                distance = Mathf.RoundToInt(target.x - currentNode.x);
                direction = Direction.Right;
            }
        }
        distance += distanceBonus;
        for (int i = 0; i < distance; i++)
        {
            moveQueue.Enqueue(direction);
        }

    }

    public bool ContainsCurrent()
    {
        // Simple function to determine if our positions is node in our graph (whether the node in our position is represented in graph, or if it is just a transtition node)
        var current = new GraphNode(Mathf.Round(transform.position.x),Mathf.Round(transform.position.y));
        return graph.Contains(current);
    }

    public GraphNode CreateTemporaryNode()
    {
        // Creates temporary node in current position, so we can pathfind even if we are not in a node of our graph
        var current = new GraphNode(transform.position.x, transform.position.y);
        if (current.x == lastVisited.x)
        {
            if (current.y > lastVisited.y)
            {
                current.SetNeighbour(lastVisited, Direction.Down);
                current.SetNeighbour(lastVisited.GetNeighbours()[0], Direction.Up);
            }
            else
            {
                current.SetNeighbour(lastVisited, Direction.Up);
                current.SetNeighbour(lastVisited.GetNeighbours()[1], Direction.Down);
            }
        }
        else
        {
            if (current.x > lastVisited.x)
            {
                current.SetNeighbour(lastVisited, Direction.Left);
                current.SetNeighbour(lastVisited.GetNeighbours()[3], Direction.Right);
            }
            else
            {
                current.SetNeighbour(lastVisited, Direction.Right);
                current.SetNeighbour(lastVisited.GetNeighbours()[2], Direction.Left);
            }
        }
        return current;
    }

    public void OnBulletDetected(GameObject bullet)
    {
        // If bullet is detected, we always flee
        threat = bullet;
        currentState = new FleeingState();
        currentState.OnStateEnter(this);
    }

    public void OnEnemyDetected(GameObject player)
    {
        // If player is detected, we check if it is our teammate, or enemy, if it is enemy and we are not fleeing or attacking, we enter attacking state
        if (player.GetComponent<AbstractController>().team == team)
            return;
        if (!enemies.Contains(player))
        {
            enemies.Add(player);
            if (!(currentState.GetType() == typeof(FleeingState)) && !(currentState.GetType() == typeof(AttackingState)))
            {
                currentState = new AttackingState();
                currentState.OnStateEnter(this);
            }
        }
    }

    public void SetProfile(int id)
    {
        // Sets the profile of this AI player
        switch (id)
        {
            case 0:
                return;
            case 1:
                profile = GlobalManager.Instance.tacticalProfile;
                break;
            case 2:
                profile = GlobalManager.Instance.aggresiveProfile;
                break;
            case 3:
                profile = GlobalManager.Instance.passiveProfile;
                break;
        }
    }

    public void SetMates(List<GameObject> team)
    {
        // Sets the teammates of this AI player
        foreach (GameObject member in team)
        {
            if (member != this.gameObject)
                teamMates.Add(member);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On collision with bullet, death occurs
        if (collision.gameObject.name.Contains("Bullet"))
        {
            GameObject.Destroy(this.gameObject);
        }

        if (collision.gameObject.name == "Walls")
        {
            moveXY = new Vector3(currentNode.x, currentNode.y, transform.position.z);
            timer = 0f;
        }
    }

}
