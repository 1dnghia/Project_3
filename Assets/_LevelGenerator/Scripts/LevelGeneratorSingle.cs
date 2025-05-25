using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelGeneratorSingle : MonoBehaviour
{
    [SerializeField] private TMP_Text _counterText;
    [SerializeField] private int speed;
    private int tempCount;
    private int counter;

    public IEnumerator Generate()//giống như void nhưng sẽ thực hiện được các lệnh yield 
    {
        counter = 0;
        tempCount = 0;
        var Instance = GetComponent<LevelGenerator>();
        GridNode CurrentNode;
        CurrentNode = new GridNode(Instance.levelSize);
        CurrentNode = CurrentNode.Next();
        GridNode nextNode;

        while (CurrentNode != null)
        {
            if (counter > 5000)
            {
                yield return Generate();
                yield break;
            }

            if (CurrentNode.Data.IsGridComplete())
            {
                Instance.result = CurrentNode.Data;
                yield break;
            }

            nextNode = CurrentNode.Next();

            if (nextNode != null)
            {
                CurrentNode = nextNode;
            }
            else
            {
                CurrentNode = CurrentNode.Prev;
                if (CurrentNode == null) { yield break; }
            }

            Instance.RenderGrid(CurrentNode.Data._grid);
            counter++;
            tempCount++;
            _counterText.text = counter.ToString();
            if (tempCount > speed * Time.deltaTime)
            {
                tempCount = 0;
                yield return null;
            }
        }
    }

    public static bool IsSolvable(HashSet<Point> points)
    {
        if (points.Count < 3) return false;
        if (points.Count == 3) return true;
        if (points.Count > 9) return true;

        GridNode CurrentCheckNode = new GridNode(GridData.LevelSize, points);
        GridNode NextNode;

        while (CurrentCheckNode != null)
        {
            if (CurrentCheckNode.Data.IsGridComplete())
            {
                return true;
            }
            NextNode = CurrentCheckNode.Next();
            if (NextNode != null)
            {
                CurrentCheckNode = NextNode;
            }
            else
            {
                CurrentCheckNode = CurrentCheckNode.Prev;
            }

        }

        return false;
    }
}

public class GridNode//Khởi tạo GridData trống, Tạo danh sách toàn bộ điểm trên lưới, sau đó trộn ngẫu nhiên (Shuffle) để tăng tính bất định.
{
    public GridNode Prev;//Lưu lại bước trước đó để có thể quay lại nếu không còn bước hợp lệ nào tại node hiện tại
    public GridData Data;//Lưu lại toàn bộ thông tin của lưới tại bước hiện tại
    private int neighborIndex;//Chỉ số của ô hiện tại trong danh sách neighbors
    private int emptyIndex;//Chỉ số của ô hiện tại trong danh sách emptyPositions
    private List<Point> neighbors;//Danh sách ô kề hợp lệ, có thể mở rộng
    private List<Point> emptyPositions; //Danh sách ô trống hợp lệ, có thể mở rộng
    public GridNode(int LevelSize)
    {
        Prev = null;
        Data = new GridData(LevelSize);
        neighbors = new List<Point>();  
        emptyPositions = new List<Point>();
        neighborIndex = 0;
        emptyIndex = 0;
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                emptyPositions.Add(new Point(i, j));
            }
        }
        Shuffle(emptyPositions);
    }

    public GridNode(GridData data, GridNode prev = null)
    {
        Data = data;//Lưu lại toàn bộ thông tin của lưới tại bước hiện tại
        Prev = prev;//Quay lại nếu không còn bước hợp lệ nào tại node hiện tại
        neighborIndex = 0;
        emptyIndex = 0;
        neighbors = new List<Point>();
        emptyPositions = new List<Point>();
        Data.GetResultsList(neighbors, emptyPositions);
        Shuffle(neighbors);
        Shuffle(emptyPositions);
    }

    public GridNode(int levelSize, HashSet<Point> points)
    {
        Prev = null;
        Data = new GridData(levelSize, points);
        neighborIndex = 0;
        emptyIndex = 0;
        neighbors = new List<Point>();
        emptyPositions = new List<Point>();

        foreach (var item in points)
        {
            emptyPositions.Add(item);
        }

        Shuffle(emptyPositions);
    }


    public GridNode Next()
    {
        GridData tempGrid;

        if (neighborIndex < neighbors.Count && emptyIndex < emptyPositions.Count)
        {
            if (UnityEngine.Random.Range(0, GridData.LevelSize) != 0)
            {
                tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
                neighborIndex++;
                return new GridNode(tempGrid, this);
            }
            else
            {
                tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
                emptyIndex++;
                return new GridNode(tempGrid, this);
            }
        }
        else if (neighborIndex < neighbors.Count)
        {
            tempGrid = new GridData(neighbors[neighborIndex].x, neighbors[neighborIndex].y, Data.ColorId, Data);
            neighborIndex++;
            return new GridNode(tempGrid, this);
        }
        else if (emptyIndex < emptyPositions.Count)
        {
            tempGrid = new GridData(emptyPositions[emptyIndex].x, emptyPositions[emptyIndex].y, Data.ColorId + 1, Data);
            emptyIndex++;
            return new GridNode(tempGrid, this);
        }

        return null;
    }

    public static void Shuffle(List<Point> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
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

    public int[,] _grid;
    public bool IsSolved;
    public Point CurrentPos;
    public int ColorId;
    public static int LevelSize;
    public List<Edge> Edges;

    public GridData(int levelSize)
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
        ColorId = -1;
        LevelSize = levelSize;
        Edges = new List<Edge>();
    }

    public GridData(int i, int j, int passedColor, GridData gridCopy)
    {
        _grid = new int[LevelSize, LevelSize];

        for (int a = 0; a < LevelSize; a++)
        {
            for (int b = 0; b < LevelSize; b++)
            {
                _grid[a, b] = gridCopy._grid[a, b];
            }
        }

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
        if (passedColor == ColorId)
        {
            Edges[Edges.Count - 1].Points.Add(new Vector2Int(i, j));
        }
        else
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

    public GridData(int levelSize, HashSet<Point> points)
    {
        _grid = new int[levelSize, levelSize];

        for (int i = 0; i < levelSize; i++)
        {
            for (int j = 0; j < levelSize; j++)
            {
                _grid[i, j] = -2;
            }
        }

        foreach (var point in points)
        {
            _grid[point.x, point.y] = -1;
        }

        IsSolved = false;
        ColorId = -1;
        LevelSize = levelSize;
        Edges = new List<Edge>();
    }

    public bool IsInsideGrid(Point pos)
    {
        return pos.IsPointValid(LevelSize);
    }

    public bool IsGridComplete()
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

    public bool IsNotNeighbour(Point pos)
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

    public int FlowLength()
    {
        int result = 0;
        foreach (var item in _grid)
        {
            if (item == ColorId)
                result++;
        }

        return result;
    }

    public void GetResultsList(List<Point> neighbors, List<Point> emptyPositions)
    {
        int[,] emptyGrid = new int[LevelSize, LevelSize];
        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                emptyGrid[i, j] = -1;
            }
        }

        for (int i = 0; i < LevelSize; i++)
        {
            for (int j = 0; j < LevelSize; j++)
            {
                if (_grid[i, j] == -1)
                {
                    emptyGrid[i, j] = 0;
                    for (int k = 0; k < directionChecks.Length; k++)
                    {
                        Point tempPoint = new Point(directionChecks[k].x + i, directionChecks[k].y + j);
                        if (IsInsideGrid(tempPoint) && _grid[tempPoint.x, tempPoint.y] == -1)
                        {
                            emptyGrid[i, j]++;
                        }
                    }
                }
            }
        }

        List<Point> zeroNeighbours = new List<Point>();
        List<Point> allNeighbours = new List<Point>();

        for (int i = 0; i < directionChecks.Length; i++)
        {
            Point tempPoint = CurrentPos + directionChecks[i];
            if (IsInsideGrid(tempPoint) &&
                IsNotNeighbour(tempPoint) &&
                emptyGrid[tempPoint.x, tempPoint.y] != -1)
            {
                if (emptyGrid[tempPoint.x, tempPoint.y] == 0)
                {
                    zeroNeighbours.Add(tempPoint);
                    emptyGrid[tempPoint.x, tempPoint.y] = -1;
                }
                allNeighbours.Add(tempPoint);
            }
        }

        List<Point> zeroEmpty = new List<Point>();
        List<Point> oneEmpty = new List<Point>();
        List<Point> allEmpty = new List<Point>();

        for (int i = 0; i < LevelSize; i++)
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

        List<HashSet<Point>> connectedSet = new List<HashSet<Point>>();
        HashSet<Point> minSet = FindMinConnectedSet(new List<Point>(allEmpty), connectedSet);
        List<HashSet<Point>> tempSet = new List<HashSet<Point>>();

        foreach (var item in connectedSet)
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

        if (zeroEmpty.Count > 0 || zeroNeighbours.Count > 1)
        {
            return;
        }

        foreach (var item in connectedSet)
        {
            if (!LevelGeneratorSingle.IsSolvable(item))
            {
                return;
            }
        }

        if (zeroNeighbours.Count == 1)
        {
            neighbors.Add(zeroNeighbours[0]);
            return;
        }

        foreach (var item in allNeighbours)
        {
            neighbors.Add(item);
        }

        if (FlowLength() < 3) return;

        if (oneEmpty.Count > 0)
        {
            foreach (var item in oneEmpty)
            {
                if (minSet.Contains(item))
                    emptyPositions.Add(item);
            }

            return;
        }

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
