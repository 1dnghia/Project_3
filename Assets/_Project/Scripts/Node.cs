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

    public Vector2Int Pos2D { get; set; } // Tọa độ của node trong lưới (Grid)


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
        // Nếu node được chọn không có kết nối edge tương ứng thì thoát
        if (!ConnectedEdges.ContainsKey(connectedNode))
        {
            return;
        }

        // Nếu đã kết nối rồi thì gỡ kết nối giữa 2 node

        if (ConnectedNodes.Contains(connectedNode))
        {
            ConnectedNodes.Remove(connectedNode);// Xóa node kia khỏi danh sách kết nối
            connectedNode.ConnectedNodes.Remove(this);            // Ngược lại cũng xóa node hiện tại khỏi node kia
            RemoveEdge(connectedNode);                            // Ẩn edge giữa 2 node
            DeleteNode();                                         // Xóa các node trung gian nếu cần
            connectedNode.DeleteNode();                           // Xóa node bên kia nếu cần
            return;
        }

        // Nếu node hiện tại đã có 2 kết nối rồi
        if (ConnectedNodes.Count == 2)
        {
            Node tempNode = ConnectedNodes[0];

            if (!tempNode.IsConnectedToEndNode())// Nếu node đầu tiên không kết nối tới điểm cuối thì xóa
            {
                ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(this);
                RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }
            else// Nếu không thì xóa node còn lại
            {
                tempNode = ConnectedNodes[1];
                ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(this);
                RemoveEdge(tempNode);
                tempNode.DeleteNode();
            }
        }

        // Nếu node được chọn đã có 2 kết nối rồi thì gỡ cả 2
        if (connectedNode.ConnectedNodes.Count == 2)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();

            tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }

        // Nếu node được chọn có màu khác nhưng đã có 1 kết nối, thì xóa kết nối đó
        if (connectedNode.ConnectedNodes.Count == 1 && connectedNode.colorId != colorId)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }

        // Nếu node hiện tại là điểm đầu/cuối và đã có 1 kết nối rồi thì xóa
        if (ConnectedNodes.Count == 1 && IsEndNode)
        {
            Node tempNode = ConnectedNodes[0];
            ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(this);
            RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }

        // Nếu node được chọn là điểm đầu/cuối và đã có 1 kết nối rồi thì xóa
        if (connectedNode.ConnectedNodes.Count == 1 && connectedNode.IsEndNode)
        {
            Node tempNode = connectedNode.ConnectedNodes[0];
            connectedNode.ConnectedNodes.Remove(tempNode);
            tempNode.ConnectedNodes.Remove(connectedNode);
            connectedNode.RemoveEdge(tempNode);
            tempNode.DeleteNode();
        }

        AddEdge(connectedNode);// Thêm edge giữa node hiện tại và node được chọn

        // Nếu khác màu thì không tiếp tục xử lý
        if (colorId != connectedNode.colorId)
        {
            return;
        }
        // Kiểm tra xem việc thêm có tạo thành vòng kín không
        List<Node> checkingNodes = new List<Node>() { this };
        List<Node> resultNodes = new List<Node>() { this };
        // Tìm toàn bộ mạng lưới node đang kết nối
        while (checkingNodes.Count > 0)
        {
            foreach (var item in checkingNodes[0].ConnectedNodes)
            {
                if (!resultNodes.Contains(item))
                {
                    resultNodes.Add(item);
                    checkingNodes.Add(item);
                }
            }

            checkingNodes.Remove(checkingNodes[0]);
        }
        // Kiểm tra các node trong mạng lưới vừa thu được có node nào có bậc 3 không (tức là tạo vòng)
        foreach (var item in resultNodes)
        {
            if (!item.IsEndNode && item.IsDegreeThree(resultNodes))
            { // Nếu có thì xóa các kết nối để phá vòng
                Node tempNode = item.ConnectedNodes[0];
                item.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(item);
                item.RemoveEdge(tempNode);
                tempNode.DeleteNode();

                if (item.ConnectedNodes.Count == 0) return;

                tempNode = item.ConnectedNodes[0];
                item.ConnectedNodes.Remove(tempNode);
                tempNode.ConnectedNodes.Remove(item);
                item.RemoveEdge(tempNode);
                tempNode.DeleteNode();

                return;
            }
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

    protected void DeleteNode()//Xóa node hiện tại khỏi hệ thống nếu nó không kết nối tới EndNode
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

    public bool IsConnectedToEndNode(List<Node> checkedNode = null)//kiểm tra xem node hiện tại có kết nối (trực tiếp hoặc gián tiếp) đến EndNode không.
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

    public void SolveHighlight()//Highlight node hiện tại nếu nó nằm giữa 2 EndNodes
    {
        if (ConnectedNodes.Count == 0)
        {
            _highLight.SetActive(false);
            return;
        }

        List<Node> checkingNodes = new List<Node>() { this };//hàng đợi để kiểm tra các node.
        List<Node> resultNodes = new List<Node>() { this };//danh sách lưu tất cả các node mà node hiện tại có thể truy cập tới qua các kết nối.

        while (checkingNodes.Count > 0)
        {
            foreach (var item in checkingNodes[0].ConnectedNodes)
            {
                if (!resultNodes.Contains(item))
                {
                    resultNodes.Add(item);//thêm node vào danh sách kết quả nếu chưa có trong danh sách
                    checkingNodes.Add(item);//thêm node vào hàng đợi để kiểm tra các node lân cận
                }
            }

            checkingNodes.Remove(checkingNodes[0]);
        }

        checkingNodes.Clear();

        foreach (var item in resultNodes)
        {
            if (item.IsEndNode)
            {
                checkingNodes.Add(item);
            }
        }

        if (checkingNodes.Count == 2)
        {
            _highLight.SetActive(true);
            _highLight.GetComponent<SpriteRenderer>().color =
                GameplayManager.Instance.GetHighLightColor(colorId);
        }
        else
        {
            _highLight.SetActive(false);
        }

    }

    private List<Vector2Int> directionCheck = new List<Vector2Int>()
        {
            Vector2Int.up,Vector2Int.left,Vector2Int.down,Vector2Int.right
        };

    public bool IsDegreeThree(List<Node> resultNodes)//Kiểm tra xem node hiện tại có 3 node liên tiếp cùng hàng hoặc cột không
    {
        bool isdegreethree = false;

        int numOfNeighbours = 0;

        for (int i = 0; i < directionCheck.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int checkingPos = Pos2D + directionCheck[(i + j) % directionCheck.Count];

                if (GameplayManager.Instance._nodeGrid.TryGetValue(checkingPos, out Node result))//Kiểm tra xem tại vị trí checkingPos có node không. Nếu có, node đó được lưu vào biến result.
                {
                    if (resultNodes.Contains(result))
                    {
                        numOfNeighbours++;
                    }
                    else
                    {
                        break;
                    }   
                }
            }

            if (numOfNeighbours == 3)
            {
                break;
            }

            numOfNeighbours = 0;
        }

        if (numOfNeighbours >= 3)
        {
            isdegreethree = true;
        }

        return isdegreethree;
    }
}
