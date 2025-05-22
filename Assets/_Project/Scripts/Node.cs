using System.Collections;
using System.Collections.Generic;
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

    [HideInInspector] public int colorID;
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
            if(_point.activeSelf)
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
}
