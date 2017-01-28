using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AStar : MonoBehaviour
{
    public class RootPosition
    {
        public int x;
        public int y;
        public RootPosition(int x_ = 0, int y_ = 0)
        {
            x = x_;
            y = y_;
        }
    }

    public class Node
    {
        public enum Status
        {
            NONE,
            OPEN,
            CLOSE,
        }
        public Status status;

        public int cost = 0;
        public int heuristic_cost = 0;
        public Node parent = null;

        public int x;
        public int y;

        public Node(int x_, int y_)
        {
            x = x_;
            y = y_;
        }

        public int getScore()
        {
            return cost + heuristic_cost;
        }

        public void calcHeuristicCost(int goal_x_, int goal_y_)
        {
            goal_x_ = Mathf.Abs(goal_x_ - x);
            goal_y_ = Mathf.Abs(goal_y_ - y);
        }

        public bool isNone()
        {
            return status == Status.NONE;
        }

        public void openNode(Node parentnode_, int cost_)
        {
            status = Status.OPEN;
            cost = cost_;
            parent = parentnode_;
        }

        public void close()
        {
            status = Status.CLOSE;
        }

        public void getRootList(List<RootPosition> r_list_)
        {
            r_list_.Add(new RootPosition(x, y));
            if (parent != null)
                parent.getRootList(r_list_);
        }
    }


    public class NodeManager
    {
        List<Node> opennodelist;
        Dictionary<int, Node> nodedic;
        MapChipController map;
        int goal_x;
        int goal_y;

        public NodeManager(int goal_x_, int goal_y_, MapChipController mapchip_)
        {
            opennodelist = new List<Node>();
            nodedic = new Dictionary<int, Node>();
            goal_x = goal_x_;
            goal_y = goal_y_;
            map = mapchip_;
        }

        public Node getNode(int x_, int y_)
        {
            var index = map.blocks[(int)LayerController.Layer.FLOOR] [y_] [x_].GetComponent<Block>().index;
            if (nodedic.ContainsKey(index))
            {
                return nodedic[index];
            }

            var node = new Node(x_, y_);
            nodedic.Add(index, node);
            node.calcHeuristicCost(goal_x, goal_y);

            return node;
        }

        public void addOpenNode(Node node_)
        {
            opennodelist.Add(node_);
        }

        public void removeOpenNode(Node node_)
        {
            opennodelist.Remove(node_);
        }

        public Node openNode(int x_, int y_, int cost_, Node parent_)
        {
            if (map.isOutOfRange(x_, y_))
                return null;

            int wallnum = map.blocks[(int)LayerController.Layer.WALL] [y_] [x_].GetComponent<Block>().number;
            if (wallnum != -1)
                return null;

            var node = getNode(x_, y_);
            if (node.isNone() == false)
                return null;

            node.openNode(parent_, cost_);
            addOpenNode(node);

            return node;
        }

        public void openAround(Node parent_)
        {
            var x = parent_.x;
            var y = parent_.y;
            var cost = parent_.cost;
            cost += 1;

            openNode(x, y + 1, cost, parent_);
            openNode(x, y - 1, cost, parent_);
            openNode(x + 1, y, cost, parent_);
            openNode(x - 1, y, cost, parent_);
        }

        public Node searchMinScoreNodeFromOpenNodeList()
        {
            int min_score = int.MaxValue;
            int min_cost = int.MaxValue;

            Node min_node = null;
            foreach (Node node in opennodelist)
            {
                int score = node.getScore();
                if (score > min_score)
                    continue;
                if (score == min_score && node.cost >= min_cost)
                    continue;

                min_score = score;
                min_cost = node.cost;
                min_node = node;
            }
            return min_node;
        }



    }
}
