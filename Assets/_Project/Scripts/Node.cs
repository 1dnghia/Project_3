using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Node : NMonoBehaviour
{
    [SerializeField] private GameObject _point;// Vòng tròn ở giữa node (điểm đầu/cuối)
    [SerializeField] private GameObject _topEdge;
    [SerializeField] private GameObject _bottomEdge;
    [SerializeField] private GameObject _leftEdge;
    [SerializeField] private GameObject _rightEdge;
    [SerializeField] private GameObject _highLight;

    protected Dictionary<Node, GameObject> ConnectedEdges;// Lưu các cạnh kết nối với node khác

    [HideInInspector] public int colorId;
    [HideInInspector] public List<Node> ConnectedNodes;// Danh sách các node đang kết nối tới node này


    public bool IsWin//điều kiện cho số cạnh của 1 node
    {
        get
        {
            if (_point.activeSelf)
            {
                return ConnectedNodes.Count == 1;
            }
            return ConnectedNodes.Count == 2;
        }
    }

    public bool IsClickable // kiểm tra xem node có thể click hay không
    {
        get
        {
            if (_point.activeSelf)
            {
                return true;
            }
            return ConnectedNodes.Count > 0;
        }
    }

    public bool IsEndNode
    {
        get
        {
            return _point.activeSelf;
        }
    }

    public Vector2Int Pos2D { get; set; } // Tọa độ của node trong lưới (Grid)

    public void Init()// Hàm khởi tạo node: ẩn mọi thành phần và làm sạch danh sách kết nối
    {
        _point.SetActive(false);
        _topEdge.SetActive(false);
        _bottomEdge.SetActive(false);
        _leftEdge.SetActive(false);
        _rightEdge.SetActive(false);
        _highLight.SetActive(false);
        ConnectedEdges = new Dictionary<Node, GameObject>();
        ConnectedNodes = new List<Node>();
    }

    public void SetColorForPoint(int colorIdForSpawnedNode)// Hàm đặt màu cho node
    {
        colorId = colorIdForSpawnedNode;
        _point.SetActive(true);
        _point.GetComponent<SpriteRenderer>().color =
                GameplayManager.Instance.NodeColors[colorId % GameplayManager.Instance.NodeColors.Count];
    }

    public void SetEdge(Vector2Int offset, Node node) //// Gán edge tương ứng với node khác dựa vào offset (hướng)
    {
        if (offset == Vector2Int.up)
        {
            ConnectedEdges[node] = _topEdge;
            return;
        }

        if (offset == Vector2Int.down)
        {
            ConnectedEdges[node] = _bottomEdge;
            return;
        }

        if (offset == Vector2Int.right)
        {
            ConnectedEdges[node] = _rightEdge;
            return;
        }

        if (offset == Vector2Int.left)
        {
            ConnectedEdges[node] = _leftEdge;
            return;
        }
    }

    public void UpdateInput(Node connectedNode)
    {
        if(!ConnectedEdges.ContainsKey(connectedNode))
        {
            return;
        }

        if (ConnectedNodes.Contains(connectedNode))
        {
            ConnectedNodes.Remove(connectedNode);
            connectedNode.ConnectedNodes.Remove(this);

        }
    }

    protected void AddEdge(Node connectedNode)// Thêm kết nối giữa node hiện tại và node được đang kết nối
    {
        connectedNode.colorId = colorId;
        connectedNode.ConnectedNodes.Add(this);
        ConnectedNodes.Add(connectedNode);
        GameObject connectedEdge = ConnectedEdges[connectedNode];
        connectedEdge.SetActive(true);
        connectedEdge.GetComponent<SpriteRenderer>().color =
            GameplayManager.Instance.NodeColors[colorId % GameplayManager.Instance.NodeColors.Count];
    }

    protected void RemoveEdge(Node node)// ẩn egde kết nôi node hiện tại và node được đang kết nối
    {
        GameObject edge = ConnectedEdges[node];
        edge.SetActive(false);
        edge = node.ConnectedEdges[this];
        edge.SetActive(false);
    }

    protected void DeleteNode()
    {
        Node startNode = this;

        if (startNode.IsConnectedToEndNode())
        {
            return;
        }

        while (startNode != null)
        {
            Node tempNode = null;
            if (startNode.ConnectedNodes.Count != 0)
            {
                tempNode = startNode.ConnectedNodes[0];
                startNode.ConnectedNodes.Clear();
                tempNode.ConnectedNodes.Remove(startNode);
                startNode.RemoveEdge(tempNode);
            }
            startNode = tempNode;
        }
    }

    public bool IsConnectedToEndNode(List<Node> checkedNode = null)
    {
        if(checkedNode == null)
        {
            checkedNode = new List<Node>();
        }

        if (IsEndNode)
        {
            return true;
        }

        foreach(var item in ConnectedNodes)
        {
            if (!checkedNode.Contains(item))
            {
                checkedNode.Add(item);
                return item.IsConnectedToEndNode(checkedNode);
            }
        }

        return false;
    }
}
