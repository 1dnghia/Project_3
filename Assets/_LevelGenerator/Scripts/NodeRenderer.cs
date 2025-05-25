using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class NodeRenderer : MonoBehaviour
{
    [SerializeField] private List<Color> NodeColors;

    [SerializeField] private GameObject _point;
    [SerializeField] private GameObject _topEdge;
    [SerializeField] private GameObject _bottomEdge;
    [SerializeField] private GameObject _leftEdge;
    [SerializeField] private GameObject _rightEdge;


    public void Init()
    {
        _point.SetActive(false);
        _topEdge.SetActive(false);
        _bottomEdge.SetActive(false);
        _leftEdge.SetActive(false);
        _rightEdge.SetActive(false);
    }

    public void SetEdge(int colorId, Point direction)
    {
        GameObject connectedNode = _point;// Mặc định chọn điểm trung tâm
                                          // Kiểm tra hướng và gán GameObject tương ứng
        if (direction == Point.up)
        {
            connectedNode = _topEdge;// Nếu hướng là lên thì chọn cạnh trên
        }

        else if (direction == Point.down)
        {
            connectedNode = _bottomEdge;
        }

        else if (direction == Point.left)
        {
            connectedNode = _leftEdge;
        }

        else if (direction == Point.right)
        {
            connectedNode = _rightEdge;
        }

        connectedNode.SetActive(true);// Hiện cạnh được chọn
        connectedNode.GetComponent<SpriteRenderer>().color = NodeColors[colorId % NodeColors.Count];// Lấy SpriteRenderer và gán màu từ danh sách NodeColors, dùng phép chia lấy dư để đảm bảo không vượt quá chỉ số danh sách
    }
}
