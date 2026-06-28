using System;
using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    public static List<Node> Run(
        Node initialNode,
        Func<Node, bool> isSatisfied,
        Func<Node, List<Node>> getConnections,
        Func<Node, Node, float> getCosts,
        Func<Node, float> heuristic)
    {
        PriorityQueue<Node> pending = new PriorityQueue<Node>();
        HashSet<Node> visited = new HashSet<Node>();
        Dictionary<Node, Node> parents = new Dictionary<Node, Node>();
        Dictionary<Node, float> costs = new Dictionary<Node, float>();

        costs[initialNode] = 0f;
        pending.Enqueue(initialNode, 0f);

        while (!pending.IsEmpty)
        {
            Node node = pending.Dequeue();

            if (visited.Contains(node)) continue;
            visited.Add(node);

            if (isSatisfied(node))
                return BuildPath(parents, node);

            foreach (Node child in getConnections(node))
            {
                if (visited.Contains(child)) continue;

                float newCost = costs[node] + getCosts(node, child);

                if (costs.ContainsKey(child) && newCost >= costs[child]) continue;

                costs[child] = newCost;
                parents[child] = node;
                pending.Enqueue(child, newCost + heuristic(child));
            }
        }

        return new List<Node>();
    }

    private static List<Node> BuildPath(Dictionary<Node, Node> parents, Node goal)
    {
        var path = new List<Node>();
        Node current = goal;

        while (parents.ContainsKey(current))
        {
            path.Add(current);
            current = parents[current];
        }
        path.Add(current);
        path.Reverse();
        return path;
    }
}