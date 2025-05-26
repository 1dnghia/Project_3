using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    #region START_METHODS

    [SerializeField] private bool canGeneratorOnce;// Cho biết có tạo 1 level hay nhiều level

    [SerializeField] private int stage;

    public int levelSize => stage + 4;

    private void Awake()
    {
        SpawnBoard();// Tạo bảng nền
        SpawnNodes();// Tạo các node trong bảng
    }
    //tạo bảng SpawnBoard
    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab; // Prefab bảng chính và ô nền

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
    //tạo spawmnode
    [SerializeField] private NodeRenderer _nodePrefab; // Prefab của node

    public Dictionary<Point, NodeRenderer> nodeGrid;   // Lưu node theo tọa độ
    private NodeRenderer[,] nodeArray;                 // Mảng 2 chiều node

    private void SpawnNodes()
    {
        nodeGrid = new Dictionary<Point, NodeRenderer>();           // Khởi tạo từ điển
        nodeArray = new NodeRenderer[levelSize, levelSize];         // Khởi tạo mảng 2 chiều
        Vector3 spawnPos;
        NodeRenderer spawnedNode;

        for (int i = 0; i < levelSize; i++) // Duyệt từng dòng
        {
            for (int j = 0; j < levelSize; j++) // Duyệt từng cột
            {
                spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f); // Vị trí spawn node
                spawnedNode = Instantiate(_nodePrefab, spawnPos, Quaternion.identity); // Tạo node
                spawnedNode.Init(); // Khởi tạo node
                nodeGrid.Add(new Point(i, j), spawnedNode); // Thêm vào từ điển
                nodeArray[i, j] = spawnedNode; // Thêm vào mảng
                spawnedNode.gameObject.name = i.ToString() + j.ToString(); // Đặt tên node
            }
        }
    }



    #endregion

    #region BUTTON_FUNCTION

    [SerializeField] private GameObject _simulateButton; // Nút mô phỏng

    public void ClickedSimulate()
    {
        Levels = new Dictionary<string, LevelData>(); // Khởi tạo danh sách level

        foreach (var item in _allLevelList.Levels) // Nạp dữ liệu từ danh sách có sẵn
        {
            if (!string.IsNullOrEmpty(item.LevelName))
            {
                Levels[item.LevelName] = item;
            }
            else
            {
                Debug.LogWarning($"LevelData có LevelName bị null hoặc rỗng! (object: {item})");
            }
        }

        if (canGeneratorOnce) // Nếu chỉ tạo 1 level
        {
            GenerateDefault(); // Tạo 1 level
        }
        else
        {
            GenerateAll(); // Tạo nhiều level
        }

        _simulateButton.SetActive(false); // Ẩn nút mô phỏng
    }

    //Tạo 1 level
    [SerializeField] private LevelList _allLevelList; // Danh sách các level hiện có
    private Dictionary<string, LevelData> Levels;     // Dictionary lưu thông tin level

    public LevelData currentLevelData; // Dữ liệu level đang được tạo

    private void GenerateDefault()
    {
        GenerateLevelData(); // Gọi hàm tạo dữ liệu level
    }

    private void GenerateLevelData(int level = 0)
    {
        string currentLevelName = "Level" + stage.ToString() + level.ToString(); // Tên level dạng LevelXY

        if (!Levels.ContainsKey(currentLevelName)) // Nếu chưa tồn tại
        {
#if UNITY_EDITOR
            currentLevelData = ScriptableObject.CreateInstance<LevelData>(); // Tạo ScriptableObject mới
            AssetDatabase.CreateAsset(currentLevelData, "Assets/_Common/Prefabs/Levels/" +
                currentLevelName + ".asset"); // Lưu asset
            AssetDatabase.SaveAssets(); // Lưu thay đổi
#endif
            Levels[currentLevelName] = currentLevelData; // Thêm vào Dictionary
            _allLevelList.Levels.Add(currentLevelData);  // Thêm vào danh sách hiện có
        }

        currentLevelData = Levels[currentLevelName];    // Gán level hiện tại
        currentLevelData.LevelName = currentLevelName;  // Cập nhật tên
        currentLevelData.Edges = new List<Edge>();      // Xóa các đường nối cũ

        GetComponent<GenerateMethod>().Generate();      // Gọi phương thức Generate để tạo dữ liệu
    }


    #endregion

    #region GENERATE_ALL_LEVELS 
    //Tạo nhiều level liên tiếp

    [SerializeField] private TMP_Text _counterText; // Text hiển thị số lượng level đã tạo
    public GridData result;                         // Lưu kết quả tạm thời của level

    private void GenerateAll()
    {
        StartCoroutine(GenerateAllLevels()); // Bắt đầu coroutine
    }

    private IEnumerator GenerateAllLevels()
    {
        for (int i = 1; i < 51; i++) // Lặp từ level 1 đến 50
        {
            yield return GenerateSingleLevelData(i); // Tạo từng level một
            _counterText.text = i.ToString();        // Cập nhật số level đã tạo lên UI
            yield return null;                       // Chờ 1 frame
        }
    }

    private IEnumerator GenerateSingleLevelData(int level = 0)
    {
        string currentLevelName = "Level" + stage.ToString() + level.ToString(); // Tên level

        if (!Levels.ContainsKey(currentLevelName)) // Nếu level chưa tồn tại
        {
#if UNITY_EDITOR
            currentLevelData = ScriptableObject.CreateInstance<LevelData>(); // Tạo mới
            AssetDatabase.CreateAsset(currentLevelData, "Assets/_Common/Prefabs/Levels/" +
                currentLevelName + ".asset"); // Lưu asset
            AssetDatabase.SaveAssets(); // Lưu thay đổi
#endif
            Levels[currentLevelName] = currentLevelData; // Lưu vào Dictionary
            _allLevelList.Levels.Add(currentLevelData);  // Lưu vào danh sách
        }

        currentLevelData = Levels[currentLevelName];    // Gán lại level hiện tại
        currentLevelData.LevelName = currentLevelName;  // Gán tên
        currentLevelData.Edges = new List<Edge>();      // Xóa các đường nối cũ

        yield return GetComponent<LevelGeneratorSingle>().Generate(); // Gọi coroutine tạo level

        currentLevelData.Edges = result.Edges;           // Gán dữ liệu đường nối từ kết quả sinh
        RenderGrid(result._grid);                        // Hiển thị dữ liệu lên lưới
    }


    #endregion

    #region NODE_RENDERING
    ///Hiển thị dữ liệu lên lưới
    private List<Point> directions = new List<Point>()
        { Point.up,Point.down,Point.left,Point.right }; // 4 hướng liền kề

    public void RenderGrid(Dictionary<Point, int> grid)
    {
        int currentColor; // Màu tại vị trí hiện tại
        int numOfConnectedNodes; // Số node nối kề cùng màu

        foreach (var item in nodeGrid) // Duyệt từng node trong từ điển
        {
            item.Value.Init(); // Reset node
            if (item.Key == null || grid == null || !grid.ContainsKey(item.Key))
                continue;

            currentColor = grid[item.Key]; // Lấy màu tại điểm
            numOfConnectedNodes = 0;

            if (currentColor != -1) // Nếu node không phải là ô trống
            {
                foreach (var direction in directions) // Duyệt 4 hướng
                {
                    if (grid.ContainsKey(item.Key + direction) &&
                        grid[item.Key + direction] == currentColor) 
                    {
                        item.Value.SetEdge(currentColor, direction); // Nếu lân cận có cùng màu → vẽ cạnh nối
                        numOfConnectedNodes++; // Tăng đếm
                    }
                }

                if (numOfConnectedNodes <= 1) // Nếu chỉ có 1 hướng, là điểm đầu/cuối
                {
                    item.Value.SetEdge(currentColor, Point.zero); // Thiết lập hướng là 0 (không nối)
                }
            }
        }
    }


    private Point[] neighbourPoints = new Point[]
    {
            Point.up, Point.left, Point.down, Point.right // Hướng liền kề
    };

    public void RenderGrid(int[,] grid)//Giống bản Dictionary, nhưng dùng mảng 2D để truy cập theo [i, j].
    {
        int currentColor;
        int numOfConnectedNodes;

        for (int i = 0; i < levelSize; i++) // Duyệt hàng
        {
            for (int j = 0; j < levelSize; j++) // Duyệt cột
            {
                nodeArray[i, j].Init(); // Reset node
                currentColor = grid[i, j]; // Màu hiện tại
                numOfConnectedNodes = 0;

                if (currentColor != -1)
                {
                    for (int p = 0; p < neighbourPoints.Length; p++) // Duyệt 4 hướng
                    {
                        Point tempPoint = new Point(i, j) + neighbourPoints[p]; // Điểm liền kề

                        if (tempPoint.IsPointValid(levelSize) &&
                            grid[tempPoint.x, tempPoint.y] == currentColor) // Nếu hợp lệ và cùng màu
                        {
                            nodeArray[i, j].SetEdge(currentColor, neighbourPoints[p]); // Vẽ cạnh
                            numOfConnectedNodes++;
                        }
                    }

                    if (numOfConnectedNodes <= 1) // Nếu chỉ nối 1 hướng
                    {
                        nodeArray[i, j].SetEdge(currentColor, Point.zero); // Là điểm đầu/cuối
                    }
                }
            }
        }
    }


    #endregion

}

public interface GenerateMethod
{
    public void Generate();
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool IsPointValid(int maxCount)
    {
        return x < maxCount && y < maxCount && x > -1 && y > -1; // Kiểm tra hợp lệ trong lưới
    }

    // Cộng điểm
    public static Point operator +(Point p1, Point p2)
    {
        return new Point(p1.x + p2.x, p1.y + p2.y);
    }

    // Trừ điểm
    public static Point operator -(Point p1, Point p2)
    {
        return new Point(p1.x - p2.x, p1.y - p2.y);
    }

    // Các hướng tiện dụng
    public static Point up => new Point(0, 1);
    public static Point left => new Point(-1, 0);
    public static Point down => new Point(0, -1);
    public static Point right => new Point(1, 0);
    public static Point zero => new Point(0, 0);

    // So sánh bằng
    public static bool operator ==(Point p1, Point p2) => p1.x == p2.x && p1.y == p2.y;
    public static bool operator !=(Point p1, Point p2) => p1.x != p2.x || p1.y != p2.y;

    public override bool Equals(object obj)
    {
        Point a = (Point)obj;
        return x == a.x && y == a.y;
    }

    public override int GetHashCode()
    {
        return (100 * x + y).GetHashCode(); // Hash theo công thức đơn giản
    }
}
