using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameplayManager : NMonoBehaviour
{
    [HideInInspector] public bool hasGameFinished;// Biến cờ để kiểm tra game đã kết thúc chưa

    [SerializeField] private TMP_Text _titleText;// UI hiển thị tên màn chơi
    [SerializeField] private GameObject _winText;// UI hiển thị khi chiến thắng
    [SerializeField] private SpriteRenderer _clickHighlight; // Hiệu ứng highlight khi click
    [SerializeField] private SpriteRenderer _boardPrefab, _bgCellPrefab;// Prefab bảng nền và ô nền
    [SerializeField] private Node _nodePrefab;

    private LevelData CurrentLevelData;
    private List<Node> _nodes;
    private Node startNode;
    private SoundManager _soundManager;

    public Dictionary<Vector2Int, Node> _nodeGrid;// Bảng node theo vị trí 2D
    public List<Color> NodeColors;

    public static GameplayManager Instance => _instance;
    private static GameplayManager _instance;

    protected override void Awake()//hiển thị tên màn khi vào level
    {
        _instance = this;

        hasGameFinished = false;
        _winText.SetActive(false);
        _titleText.gameObject.SetActive(true);
        _titleText.text = GameManager.Instance.StageName +
            " - " + GameManager.Instance.CurrentLevel.ToString();

        CurrentLevelData = GameManager.Instance.GetLevel();
        _soundManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();

        SpawnBoard();

        SpawnNodes();

    }

    private void Update()
    {
        if (hasGameFinished) return;// Nếu game đã kết thúc thì bỏ qua

        if (Input.GetMouseButtonDown(0))// Khi bắt đầu nhấn chuột
        {
            startNode = null;
            return;
        }

        if (Input.GetMouseButton(0))// Khi giữ chuột
        { 

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);//Raycast2D được dùng để kiểm tra xem chuột đang "đè lên" đối tượng nào trong thế giới 2D.

            if (startNode == null)
            {// Nếu chưa có node bắt đầu, kiểm tra va chạm
                if (hit && hit.collider.gameObject.TryGetComponent(out Node tNode)
                    && tNode.IsClickable)
                {
                    startNode = tNode;
                    _clickHighlight.gameObject.SetActive(true);
                    _clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;
                    _clickHighlight.color = GetHighLightColor(tNode.colorId);// Đặt màu highlight
                }

                return;
            }
            // Cập nhật vị trí highlight theo chuột
            _clickHighlight.gameObject.transform.position = (Vector3)mousePos2D;

            if (hit && hit.collider.gameObject.TryGetComponent(out Node tempNode)
                && startNode != tempNode)
            {
                if (startNode.colorId != tempNode.colorId && tempNode.IsEndNode)
                {
                    return;// Không cho kết nối nếu màu khác nhau và là nút cuối
                }

                startNode.UpdateInput(tempNode);// Cập nhật liên kết giữa node
                _soundManager.PlaySFX(_soundManager.connectClip);
                CheckWin();// Kiểm tra thắng
                startNode = null;
            }

            return;
        }

        if (Input.GetMouseButtonUp(0))// Khi thả chuột
        {
            startNode = null;
            _clickHighlight.gameObject.SetActive(false); // Ẩn highlight
        }

    }

    protected void SpawnBoard()//sinh bảng
    {
        int currentLevelSize = GameManager.Instance.CurrentStage + 4;
        //Instantiate(...) Là hàm dùng để tạo ra một GameObject mới từ một Prefab có sẵn.
        var board = Instantiate(_boardPrefab,
            new Vector3(currentLevelSize / 2f, currentLevelSize / 2f, 0f),
            Quaternion.identity);// Sinh bảng ở giữa màn hình

        board.size = new Vector2(currentLevelSize + 0.08f, currentLevelSize + 0.08f);// Điều chỉnh kích thước bảng

        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {
                Instantiate(_bgCellPrefab, new Vector3(i + 0.5f, j + 0.5f, 0f), Quaternion.identity);// Sinh các ô nền
            }
        }

        Camera.main.orthographicSize = currentLevelSize + 2f;// Cập nhật camera theo kích thước bảng
        Camera.main.transform.position = new Vector3(currentLevelSize / 2f, currentLevelSize / 2f, -10f); //di chuyển camera đến giữa bảng

        _clickHighlight.size = new Vector2(currentLevelSize / 4f, currentLevelSize / 4f);
        _clickHighlight.transform.position = Vector3.zero;
        _clickHighlight.gameObject.SetActive(false);
    }

    protected void SpawnNodes()//sinh node
    {
        _nodes = new List<Node>();
        _nodeGrid = new Dictionary<Vector2Int, Node>();

        int currentLevelSize = GameManager.Instance.CurrentStage + 4;
        Node spawnedNode;
        Vector3 spawnPos;

        for (int i = 0; i < currentLevelSize; i++)
        {
            for (int j = 0; j < currentLevelSize; j++)
            {// Vị trí spawn node
                spawnPos = new Vector3(i + 0.5f, j + 0.5f, 0f);
                spawnedNode = Instantiate(_nodePrefab, spawnPos, Quaternion.identity);
                spawnedNode.Init();// Khởi tạo node

                int colorIdForSpawnedNode = GetColorId(i, j);// Lấy ID màu

                if (colorIdForSpawnedNode != -1)//khi ko có màu đc gán vào
                {// Gán màu cho node nếu hợp lệ
                    spawnedNode.SetColorForPoint(colorIdForSpawnedNode);
                }

                _nodes.Add(spawnedNode);// Thêm vào danh sách node
                _nodeGrid.Add(new Vector2Int(i, j), spawnedNode);// Thêm vào grid
                spawnedNode.gameObject.name = i.ToString() + j.ToString();// Đặt tên cho node
                spawnedNode.Pos2D = new Vector2Int(i, j);

            }
        }
        // Danh sách hướng kiểm tra lân cận
        List<Vector2Int> offsetPos = new List<Vector2Int>()
            {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right };

        foreach (var item in _nodeGrid)
        {
            foreach (var offset in offsetPos)
            {
                var checkPos = item.Key + offset;//item.Key là tọa độ (Vector2Int) của node.
                if (_nodeGrid.ContainsKey(checkPos))
                {
                    item.Value.SetEdge(offset, _nodeGrid[checkPos]);// Gán các cạnh liên kết với node lân cận    //item.Value là đối tượng Node tương ứng tại vị trí đó.
                }
            }
        }


    }

    public int GetColorId(int i, int j)
    {
        List<Edge> edges = CurrentLevelData.Edges;
        Vector2Int point = new Vector2Int(i, j);

        for (int colorId = 0; colorId < edges.Count; colorId++)
        {
            if (edges[colorId].StartPoint == point ||
                edges[colorId].EndPoint == point)
            {
                return colorId;// Trả về ID màu nếu node là điểm đầu hoặc cuối của đường nối
            }
        }

        return -1;
    }

    public Color GetHighLightColor(int colorID)
    {
        Color result = NodeColors[colorID % NodeColors.Count];// Lấy màu tương ứng
        result.a = 0.4f;// Đặt độ trong suốt
        return result;
    }

    private void CheckWin()
    {
        bool IsWinning = true;

        foreach (var item in _nodes)
        {
            item.SolveHighlight();// Hiển thị hiệu ứng đúng sai
        }

        foreach (var item in _nodes)
        {
            IsWinning &= item.IsWin;// Kiểm tra tất cả node có đúng không
            if (!IsWinning)
            {
                return;
            }
        }

        GameManager.Instance.UnlockLevel();

        _winText.gameObject.SetActive(true);
        _clickHighlight.gameObject.SetActive(false);
        _soundManager.PlaySFX(_soundManager.winClip);

        hasGameFinished = true;// Đặt cờ kết thúc game
    }

    public void ClickedBack()
    {
        _soundManager.PlaySFX(_soundManager.connectClip);
        GameManager.Instance.GoToMainMenu();
    }

    public void ClickedRestart()
    {
        _soundManager.PlaySFX(_soundManager.connectClip);
        GameManager.Instance.GoToGameplay();
    }

    public void ClickedNextLevel()
    {
        if (!hasGameFinished) return;

        _soundManager.PlaySFX(_soundManager.connectClip);
        GameManager.Instance.GoToGameplay();
    }
}
