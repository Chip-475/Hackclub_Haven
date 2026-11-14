using UnityEngine;
using System.Collections.Generic;

public class Pathfinder
{
    public class Node
    {
        public int x;
        public int y;

        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;

        public Node parent;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    Node[,] grid;

    public Pathfinder(int width, int height) => grid = new Node[width, height];

    public List<Node> FindPath(Vector3 start, Vector3 end)
    {
        Node startNode = grid[(int)start.x, (int)start.y];
        Node endNode = grid[(int)end.x, (int)end.y];

        List<Node> openSet = new();
        HashSet<Node> closedSet = new();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    currentNode = openSet[i];
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == endNode)
                return RetracePath(startNode, endNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    float GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.x - b.x);
        int dstY = Mathf.Abs(a.y - b.y);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);

        return 14 * dstX + 10 * (dstY - dstX);
    }
    Node[] GetNeighbors(Node node)
    {
        List<Node> neighbors = new();
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.x + x;
                int checkY = node.y + y;
                if (checkX >= 0 && checkX < grid.GetLength(0) && checkY >= 0 && checkY < grid.GetLength(1))
                    neighbors.Add(grid[checkX, checkY]);
            }

        return neighbors.ToArray();
    }
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
}
