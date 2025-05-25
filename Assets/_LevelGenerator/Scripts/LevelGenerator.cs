using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class LevelGenerator : NMonoBehaviour
{
    [SerializeField] private bool canGeneratorOnce;
    [SerializeField] private int stage;

    public int levelSize => stage + 4;

    protected override void Awake()
    {
        SpawnBoard();
        SpawnNodes();
    }

    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;

    private void SpawnBoard()
    {
        var board = Instantiate(_boardPrefab,
                new Vector3(levelSize / 2f, levelSize / 2f, 0f),// Đặt ở giữa màn hình
                Quaternion.identity);// Không xoay

        board.size = new Vector2(levelSize + 0.08f, levelSize + 0.08f);// Chỉnh kích thước phù hợp


        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                Instantiate(_bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);// Sinh ô nền tại từng vị trí
            }
        }

        Camera.main.orthographicSize = levelSize / 1.6f + 1f;// Phóng camera vừa với bảng
        Camera.main.transform.position = new Vector3(levelSize / 2f, levelSize / 2f, -10f);// Đặt camera ở giữa bảng
    }

    [SerializeField] private NodeRenderer _nodePrefab;

    public Dictionary<Point, NodeRenderer> nodeGrid;   // Lưu node theo tọa độ
    private NodeRenderer[,] nodeArray;// Mảng 2 chiều node

    private void SpawnNodes()
    {
        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                var node = new GameObject($"Node_{i}_{j}");// Tạo GameObject cho node
                node.transform.position = new Vector3(i + 0.5f, j + 0.5f, 0f);// Đặt vị trí của node
                node.AddComponent<NodeRenderer>().Init();// Thêm NodeRenderer và khởi tạo
            }
        }
    }
}
