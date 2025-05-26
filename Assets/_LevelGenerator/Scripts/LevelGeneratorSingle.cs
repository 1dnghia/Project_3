using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelGeneratorSingle : NMonoBehaviour
{
    [SerializeField] private TMP_Text _counterText;
    [SerializeField] private int speed;
    private int tempCount;
    private int counter;// Tổng số lần lặp qua các bước trong Generate

    public IEnumerator Generate()//thực hiện tạo level theo từng bước, trả lại điều khiển (yield) để không làm đứng game
    {
        counter = 0;
        tempCount = 0;
        var Instance = GetComponent<LevelGenerator>();
        GridNode CurrentNode;
        CurrentNode = new GridNode(Instance.levelSize);
        CurrentNode = CurrentNode.Next();// Di chuyển sang bước tiếp theo của node để bắt đầu mở rộng lưới
        GridNode nextNode;  

        while (CurrentNode != null)// Vòng lặp chính để tiếp tục tạo level
        {
            if (counter > 5000)// Nếu số bước vượt quá 5000, gọi lại Generate() để làm mới
            {
                yield return Generate();
                yield break;
            }

            if (CurrentNode.Data.IsGridComplete())// Nếu lưới hiện tại đã hoàn chỉnh
            {
                Instance.result = CurrentNode.Data;// Lưu kết quả lưới hoàn chỉnh vào instance
                yield break;
            }

            nextNode = CurrentNode.Next();

            if (nextNode != null)
            {
                CurrentNode = nextNode;// Nếu có bước tiếp theo hợp lệ thì đi tiếp
            }
            else
            {
                CurrentNode = CurrentNode.Prev;// Nếu không có bước tiếp theo, lùi lại node trước đó để tìm hướng khác
                if (CurrentNode == null) { yield break; }// Nếu không thể lùi lại nữa (đã về đầu) thì dừng
            }

            Instance.RenderGrid(CurrentNode.Data._grid);// Vẽ lại lưới hiện tại lên màn hình
            counter++;
            tempCount++;
            _counterText.text = counter.ToString(); // Cập nhật số đếm lên UI
            if (tempCount > speed * Time.deltaTime)// Nếu đã chạy đủ số bước theo tốc độ đã định, tạm dừng 1 frame
            {
                tempCount = 0;
                yield return null;// trả lại điều khiển cho Unity, frame tiếp theo sẽ chạy tiếp
            }
        }
    }

    public static bool IsSolvable(HashSet<Point> points)// Hàm kiểm tra tính khả thi của một tập điểm (hashset) để sinh lưới
    {
        if (points.Count < 3) return false;
        if (points.Count == 3) return true;
        if (points.Count > 9) return true;

        GridNode CurrentCheckNode = new GridNode(GridData.LevelSize, points);
        GridNode NextNode;

        while (CurrentCheckNode != null)// Dùng vòng lặp để mở rộng từng bước trong GridNode
        {
            if (CurrentCheckNode.Data.IsGridComplete())// Nếu lưới đã hoàn chỉnh thì trả về true
            {
                return true;
            }
            NextNode = CurrentCheckNode.Next();// Tạo bước tiếp theo
            if (NextNode != null)
            {
                CurrentCheckNode = NextNode;// đi tiếp
            }
            else
            {
                CurrentCheckNode = CurrentCheckNode.Prev;// backtrack nếu không có bước mới
            }

        }

        return false;// Nếu hết mà không hoàn chỉnh được thì không khả thi
    }
}

public class GridNode//Khởi tạo GridData trống, Tạo danh sách toàn bộ điểm trên lưới, sau đó trộn ngẫu nhiên (Shuffle) để tăng tính bất định.
{
    public GridNode Prev;// Lưu lại node trước đó để hỗ trợ quay lui khi cần
    public GridData Data;// Lưu trạng thái của lưới tại thời điểm hiện tại
    private int neighborIndex;// Biến đếm để duyệt qua danh sách neighbors
    private int emptyIndex;// Biến đếm để duyệt qua danh sách emptyPositions
    private List<Point> neighbors;//Danh sách ô kề hợp lệ, có thể mở rộng
    private List<Point> emptyPositions; //Danh sách ô trống hợp lệ, có thể mở rộng
    public GridNode(int LevelSize)//Tạo GridNode mới với lưới trống và danh sách toàn bộ các điểm
    {
        Prev = null;
        Data = new GridData(LevelSize);// Tạo GridData trống với kích thước LevelSize
        neighbors = new List<Point>();
        emptyPositions = new List<Point>();
        neighborIndex = 0;
        emptyIndex = 0;
        for (int i = 0; i < LevelSize; i++)// Duyệt toàn bộ các ô trong lưới và thêm vào danh sách emptyPositions
        {
            for (int j = 0; j < LevelSize; j++)
            {
                emptyPositions.Add(new Point(i, j));
            }
        }
        Shuffle(emptyPositions);// Trộn danh sách để đảm bảo tính ngẫu nhiên trong việc chọn ô
    }
    //Tạo GridNode từ một GridData có sẵn và node trước đó (Prev)
    public GridNode(GridData data, GridNode prev = null)
    {
        Data = data;
        Prev = prev;
        neighborIndex = 0;
        emptyIndex = 0;
        neighbors = new List<Point>();
        emptyPositions = new List<Point>();
        Data.GetResultsList(neighbors, emptyPositions);// Lấy ra các ô kề và ô trống hợp lệ từ dữ liệu hiện tại
        Shuffle(neighbors);// Trộn ngẫu nhiên danh sách để tăng độ bất định
        Shuffle(emptyPositions);
    }

    public GridNode(int levelSize, HashSet<Point> points)//Khởi tạo với danh sách điểm cụ thể (không tạo toàn bộ lưới)
    {
        Prev = null;
        Data = new GridData(levelSize, points);// Khởi tạo GridData với các điểm cụ thể
        neighborIndex = 0;
        emptyIndex = 0;
        neighbors = new List<Point>();
        emptyPositions = new List<Point>();

        foreach (var item in points)// Chuyển từ HashSet sang List để có thể duyệt và trộn
        {
            emptyPositions.Add(item);
        }

        Shuffle(emptyPositions);
    }


    public GridNode Next()// Phương thức tạo ra GridNode kế tiếp dựa trên các điểm hợp lệ
    {
        GridData tempGrid;//Biến tạm để tạo GridData

        if (neighborIndex < neighbors.Count && emptyIndex < emptyPositions.Count) // Nếu còn ô kề và ô trống
        {
            if (UnityEngine.Random.Range(0, GridData.LevelSize) != 0)// Xác suất cao chọn ô kề để mở rộng tiếp vùng hiện tại
            {
                tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);// Tạo node mới từ ô kề (giữ nguyên màu)
                neighborIndex++;
                return new GridNode(tempGrid, this);
            }
            else
            {//Trường hợp ngẫu nhiên chọn ô trống → tạo màu mới.
                tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
                emptyIndex++;
                return new GridNode(tempGrid, this);
            }
        }
        else if (neighborIndex < neighbors.Count)// Nếu chỉ còn ô kề→ tiếp tục mở theo đường đi hiện tại.
        {
            tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
            neighborIndex++;
            return new GridNode(tempGrid, this);
        }
        else if (emptyIndex < emptyPositions.Count)// Nếu chỉ còn ô trống→ bắt đầu một đường đi/màu mới.
        {
            tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
            emptyIndex++;
            return new GridNode(tempGrid, this);
        }

        return null;// Không còn bước đi nào nữa → trả về null
    }

    public static void Shuffle(List<Point> list)// Hàm trộn danh sách để tạo thứ tự ngẫu nhiên
    {
        System.Random rng = new System.Random();// Tạo bộ sinh số ngẫu nhiên
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);// Chọn chỉ số ngẫu nhiên từ 0 đến n
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

public class GridData
{
    private static Point[] directionChecks = new Point[]
    { Point.up,Point.down,Point.left,Point.right };

    public int[,] _grid;// Mảng 2D lưu trạng thái mỗi ô trong lưới
    public bool IsSolved;// Đánh dấu lưới đã giải xong hay chưa
    public Point CurrentPos;
    public int ColorId;
    public static int LevelSize;
    public List<Edge> Edges;// Danh sách các đường đã vẽ

    public GridData(int levelSize)//tạo lưới mới kích thước levelSize x levelSize, toàn bộ ô = -1 (trống)
    {
        _grid = new int[levelSize, levelSize];

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                _grid[i, j] = -1;
            }
        }

        IsSolved = false;
        ColorId = -1;// Chưa chọn màu nào
        LevelSize = levelSize;
        Edges = new List<Edge>();
    }

    public GridData(int i, int j, int passedColor, GridData gridCopy)//tạo bản sao từ gridCopy, đồng thời đặt điểm (i, j) thành ColorId
    {
        _grid = new int[LevelSize, LevelSize];

        for (int a = 0; a < LevelSize; a++)
        {
            for (int b = 0; b < LevelSize; b++)
            {
                _grid[a, b] = gridCopy._grid[a, b];
            }
        }
        // Sao chép danh sách các đường (Edges)
        Edges = new List<Edge>();

        foreach (var item in gridCopy.Edges)
        {
            Edge temp = new Edge();
            temp.Points = new List<Vector2Int>();
            foreach (var point in item.Points)
            {
                temp.Points.Add(point);
            }
            Edges.Add(temp);
        }

        ColorId = gridCopy.ColorId;
        if (passedColor == ColorId)// Nếu tiếp tục vẽ cùng màu, thêm vào đường hiện tại
        {
            Edges[Edges.Count - 1].Points.Add(new Vector2Int(i, j));
        }
        else// Nếu bắt đầu đường mới
        {
            Edges.Add(new Edge()
            {
                Points = new List<Vector2Int>() { new Vector2Int(i, j) }
            });
        }

        CurrentPos = new Point(i, j);
        ColorId = passedColor;
        _grid[CurrentPos.x, CurrentPos.y] = ColorId;
        IsSolved = false;
    }

    public GridData(int levelSize, HashSet<Point> points)//Khởi tạo lưới có sẵn các ô hợp lệ, các ô còn lại bị khóa (-2)
    {
        _grid = new int[levelSize, levelSize];

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                _grid[i, j] = -2;//-2 là ô bị khóa (không dùng)
            }
        }

        foreach (var point in points)
        {
            _grid[point.x, point.y] = -1;// các ô hợp lệ được đánh dấu là trống
        }

        IsSolved = false;
        ColorId = -1;
        LevelSize = levelSize;
        Edges = new List<Edge>();
    }

    public bool IsInsideGrid(Point pos)// Kiểm tra một điểm có nằm trong biên lưới hay không
    {
        return pos.IsPointValid(LevelSize);
    }

    public bool IsGridComplete()// Kiểm tra lưới đã hoàn thành chưa (không còn ô trống và mỗi màu >= 3 ô)
    {
        foreach (var item in _grid)
        {
            if (item == -1) return false;
        }

        for (int i = 0; i <= ColorId; i++)
        {
            int result = 0;

            foreach (var item in _grid)
            {
                if (item == i)
                    result++;
            }

            if (result < 3)
                return false;

        }

        return true;
    }

    public bool IsNotNeighbour(Point pos)// Kiểm tra điểm pos có phải là hàng xóm hợp lệ (không trùng các điểm cũ)
    {

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (_grid[i, j] == ColorId && new Point(i, j) != CurrentPos)
                {
                    for (int p = 0; p < directionChecks.Length; p++)
                    {
                        if (pos - new Point(i, j) == directionChecks[p])
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public int FlowLength()// Tính độ dài đường đi hiện tại
    {
        int result = 0;
        foreach (var item in _grid)
        {
            if (item == ColorId)
                result++;
        }

        return result;
    }
    //Phân tích lưới _grid hiện tại và tìm ra các: neighbor(vị trí kế bên điểm đang đi mà có thể đi tiếp),emptyPositions(vị trí trống có thể bắt đầu một đường đi mới)
    public void GetResultsList(List<Point> neighbors, List<Point> emptyPositions)
    {//neighbors: danh sách vị trí hàng xóm để mở rộng đường hiện tại, emptyPositions: danh sách vị trí trống để bắt đầu đường mới
        int[,] emptyGrid = new int[LevelSize, LevelSize];
        for (int i = 0; i < LevelSize; i++)//Khởi tạo emptyGrid với -1, nghĩa là ô chưa được xét hoặc không phải ô trống
        {
            for (int j = 0; j < LevelSize; j++)
            {
                emptyGrid[i, j] = -1;
            }
        }

        for (int i = 0; i < LevelSize; i++)//Xác định các ô trống và đếm số hàng xóm trống xung quanh
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (_grid[i, j] == -1)
                {
                    emptyGrid[i, j] = 0;// Gán giá trị 0 tức là đã tìm thấy ô trống
                    for (int k = 0; k < directionChecks.Length; k++) //Với mỗi ô trống, đếm số lượng ô trống liền kề 4 hướng
                    {
                        Point tempPoint = new Point(directionChecks[k].x + i, directionChecks[k].y + j);
                        if (IsInsideGrid(tempPoint) && _grid[tempPoint.x, tempPoint.y] == -1)//// Nếu ô lân cận nằm trong lưới và cũng là ô trống
                        {
                            emptyGrid[i, j]++;// Tăng số đếm ô trống kế bên của ô hiện tại = 1
                        }
                    }
                }
            }
        }

        List<Point> zeroNeighbours = new List<Point>();// Danh sách lưu các ô hàng xóm (neighbors) có số hàng xóm trống = 0
        List<Point> allNeighbours = new List<Point>();//// Danh sách lưu tất cả các ô hàng xóm trống quanh điểm hiện tại

        for (int i = 0; i < directionChecks.Length; i++)//Tìm các hàng xóm hợp lệ (neighbor)
        {
            Point tempPoint = CurrentPos + directionChecks[i];
            if (IsInsideGrid(tempPoint) &&
                IsNotNeighbour(tempPoint) &&
                emptyGrid[tempPoint.x, tempPoint.y] != -1)// Nếu ô nằm trong lưới, chưa phải hàng xóm hiện tại, và là ô trống (emptyGrid != -1)
            {
                if (emptyGrid[tempPoint.x, tempPoint.y] == 0)// Nếu ô trống này không có ô trống nào bên cạnh (count == 0)
                {
                    zeroNeighbours.Add(tempPoint);
                    emptyGrid[tempPoint.x, tempPoint.y] = -1;
                }
                allNeighbours.Add(tempPoint);// Lưu tất cả hàng xóm trống được tìm thấy
            }
        }

        List<Point> zeroEmpty = new List<Point>();//ô trống không có ô trống bên cạnh
        List<Point> oneEmpty = new List<Point>();//ô trống chỉ có 1 ô trống bên cạnh
        List<Point> allEmpty = new List<Point>();//tất cả các ô trống

        for (int i = 0; i < LevelSize; i++)//Phân loại các ô trống
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (emptyGrid[i, j] == 0)
                {
                    zeroEmpty.Add(new Point(i, j));
                }

                if (emptyGrid[i, j] == 1)
                {
                    oneEmpty.Add(new Point(i, j));
                }

                if (emptyGrid[i, j] != -1)
                {
                    allEmpty.Add(new Point(i, j));
                }
            }
        }

        List<HashSet<Point>> connectedSet = new List<HashSet<Point>>();// Tìm các vùng liên thông (connected sets) của các ô trống
        HashSet<Point> minSet = FindMinConnectedSet(new List<Point>(allEmpty), connectedSet);// Tìm vùng liên thông nhỏ nhất (minSet) trong toàn bộ ô trống
        List<HashSet<Point>> tempSet = new List<HashSet<Point>>();// Lọc lại các vùng liên thông loại bỏ vùng dính vào neighbor hiện tại

        foreach (var item in connectedSet)//Bỏ qua các vùng liên thông nếu không thể giải được
        {
            bool canAdd = true;

            foreach (var neighbor in allNeighbours)
            {
                if (item.Contains(neighbor))
                    canAdd = false;
            }
            if (canAdd)
            {
                tempSet.Add(item);
            }
        }
        connectedSet = tempSet;

        if (zeroEmpty.Count > 0 || zeroNeighbours.Count > 1) // Nếu có ít nhất 1 ô trống không có ô trống lân cận (zeroEmpty > 0) Hoặc có nhiều hơn 1 ô hàng xóm không có ô trống lân cận (zeroNeighbours > 1)
        {
            return;
        }

        foreach (var item in connectedSet)// Kiểm tra tính khả giải của từng vùng liên thông còn lại, Nếu có vùng không thể giải (đường đi ≥ 3), thoát hàm luôn
        {
            if (!LevelGeneratorSingle.IsSolvable(item))
            {
                return;
            }
        }

        if (zeroNeighbours.Count == 1)// Nếu chỉ có đúng 1 ô hàng xóm không có ô trống bên cạnh, ưu tiên chọn ô đó để mở rộng đường đi
        {
            neighbors.Add(zeroNeighbours[0]);
            return;
        }

        foreach (var item in allNeighbours)//// Thêm tất cả các ô hàng xóm trống được tìm thấy vào danh sách neighbor 
        {
            neighbors.Add(item);
        }

        if (FlowLength() < 3) return;// Nếu chiều dài đường hiện tại nhỏ hơn 3, không xét đến ô trống bắt đầu đường mới

        if (oneEmpty.Count > 0)// Nếu có ô trống chỉ có 1 hàng xóm trống
        {
            foreach (var item in oneEmpty)// Chỉ thêm những ô nằm trong vùng liên thông nhỏ nhất
            {
                if (minSet.Contains(item))
                    emptyPositions.Add(item);
            }

            return;
        }
        // Nếu không có ô trống loại trên thì thêm tất cả ô trống nằm trong vùng liên thông nhỏ nhất
        foreach (var item in allEmpty)
        {
            if (minSet.Contains(item))
                emptyPositions.Add(item);
        }

    }
    //***HashSet một kiểu dữ liệu tập hợp trong C# — nó lưu trữ các phần tử duy nhất, không cho phép trùng lặp, và cho phép truy cập, kiểm tra nhanh nhờ sử dụng băm (hashing).
    public static HashSet<Point> FindMinConnectedSet(List<Point> points, List<HashSet<Point>> connectedSet)
    {//phân nhóm các điểm rời rạc trong danh sách points thành các cụm (HashSet<Point>), rồi trả về cụm nhỏ nhất, điền các cụm tìm được vào connectedSet
        HashSet<Point> visited = new HashSet<Point>();
        HashSet<Point> allPoints = new HashSet<Point>(points);

        foreach (var point in points)
        {
            if (!visited.Contains(point))
            {
                HashSet<Point> connected = new HashSet<Point>();
                Queue<Point> queue = new Queue<Point>();//***khởi tạo một hàng đợi (queue) chứa các đối tượng kiểu Point.

                queue.Enqueue(point);//***thêm phần tử vào cuối hàng đợi.

                while (queue.Count > 0)
                {
                    Point current = queue.Dequeue();//***Lấy và loại bỏ phần tử ở đầu hàng đợi.

                    if (!visited.Contains(current))//kiểm tra xem current đã được thăm chưa, nếu chưa thì thêm vào connected và visited
                    {
                        connected.Add(current);
                        visited.Add(current);

                        foreach (var neighbor in GetNeighbors(current))//lấy 4 node point lân cận của current, thhàng đợi queue nếu chưa được thăm và có trong allPoints
                        {
                            if (!visited.Contains(neighbor) && allPoints.Contains(neighbor))
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }

                connectedSet.Add(connected);
            }
        }

        HashSet<Point> minSet = null;

        foreach (var item in connectedSet)
        {
            if (minSet == null || item.Count < minSet.Count)
            {
                minSet = item;
            }
        }

        return minSet;
    }

    private static List<Point> GetNeighbors(Point point)//Trả về danh sách các ô lân cận theo 4 hướng
    {
        List<Point> result = new List<Point>
            {
                new Point(point.x, point.y + 1),
                new Point(point.x, point.y - 1),
                new Point(point.x + 1, point.y),
                new Point(point.x - 1, point.y)
            };

        return result;
    }
}
