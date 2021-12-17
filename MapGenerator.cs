using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps; // 맵의 갯수(Size)
    public int mapIndex; // 맵 번호

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshMaskPrefab;
    public Transform navmeshFloor;
    

    public Vector2 maxMapSize; // 맵의 최대 크기에 따라 내브메쉬를 분할

    [Range(0,1)] // 0과 1사이의 범위로 한정
    public float outlinePercent; // 개별 타일들을 구별하기 위해 테두리를 만듬
    public float tileSize; // 맵의 전체 크기

    List<Coord> allTileCoord; // 모든 타일 좌표에 대한 리스트 
    Queue<Coord> shuffledTileCoords; // 셔플된 좌표들을 저장할 새 변수 
    Queue<Coord> shuffledOpenTileCoords; // 셔플된 좌표들을 저장할 새 변수 
    Transform[,] tileMap;

    Map currentMap;
     

    private void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GeneratorMap();
    }

    public void GeneratorMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y]; 
        System.Random prng = new System.Random(currentMap.seed); // System.Random 오브젝트를 생성하고, 랜덤 숫자 생성
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize); // 플레이어가 걸어다닐 수 있는 콜라이더를 맵 사이즈 크기로 만들기

        // 좌표(Coord)들을 생성
        allTileCoord = new List<Coord>(); // 모든 타일을 거쳐서 루프 
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoord.Add(new Coord(x, y)); 
            }
        }

        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray(allTileCoord.ToArray(), currentMap.seed));
        // 셔플된 좌표 Queue 값
        // Queue는 데이터나 작업을 입력한 순서대로 처리할 때 사용
       


        // 맵 홀더 오브젝트 생성
        string holdername = "Generator Map"; // 타일들을 자식으로 묶을 오브젝트 

        if (transform.Find(holdername))
        {
            DestroyImmediate(transform.Find(holdername).gameObject); // DestroyImmediate는 게임 오브젝트만 파괴, 에디터에서 호출하기 때문에 Destoy대신 사용
        }

        Transform mapHolder = new GameObject(holdername).transform;
        mapHolder.parent = transform; // transform의 자식으로 추가 



        // 타일들을 스폰
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordPosition(x,y);
                // -currentMap.mapSize.x / 2를 할당하면 x좌표 0을 중심으로 맵의 가로 길이의 절반 만큼 왼쪽으로 이동한 점에서 부터 타일 생성
                // 0.5f를 더해서 타일의 중점이 아닌 타일의 모서리에 위치할 수 있도록 설정, Vector3로 변환
                
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform; // as Transform = (Transform newTile = new Transform)
                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize; // outlinePercent가 1이면, localScale이 0이 되서 전체 영역이 테두리가 된다. 
                newTile.parent = mapHolder; // mapHolder의 자식으로 추가 
                tileMap[x, y] = newTile;
            }
        }


        // Obstacle 스폰
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y]; // 새 장애물을 인스턴스화 하기 전에, 이 맵에 장애물이 어디에 위치할 것인가를 갱신

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent); // 전체 타일의 갯수
        int CurrentObstacleCount = 0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoord); 

        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true; // obstalceMap을 갱신 
            CurrentObstacleCount++; // 장애물을 인스턴스화 시킬 때 마다 1 증가 

            if(randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, CurrentObstacleCount)) // 맵 중앙에 아무것도 없거나 && 맵 전체가 접근 가능하면 실행
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 ObstaclePosition = CoordPosition(randomCoord.x, randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab, ObstaclePosition + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder; // mapHolder의 자식으로 추가 
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1- outlinePercent) * tileSize);

                // 맵 컬러 설정
                Renderer obstalceRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstalceRenderer.sharedMaterial);
                float colourPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colourPercent); 
                obstalceRenderer.sharedMaterial = obstacleMaterial;

                allOpenCoords.Remove(randomCoord); 
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false; 
                CurrentObstacleCount--; 

            }
        }

        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(allOpenCoords.ToArray(), currentMap.seed));


        // 내브메쉬 마스크 스폰
        // navmeshMaskPrefab의 구역 정하기 => Enemy가 맵 밖으로 돌아나가는걸 방지 
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x , 1, (maxMapSize.x - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.x - currentMap.mapSize.y) / 2f) * tileSize;


        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;

    }


    // 맵 전체가 접근 가능한지에 대한 메서드
    bool MapIsFullyAccessible(bool[,] obstalceMap, int CurrentObstacleCount) 
    {
        bool[,] mapFlags = new bool[obstalceMap.GetLength(0), obstalceMap.GetLength(1)]; // 2차원 배열
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCenter); // 정중앙에 타일을 넣음
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true; // 이미 중앙이 비어있으니 obstalceMap.GetLength(0) 

        int accessibleTileCount = 1; // 접근 가능한 타일의 수 지정, mapCenter가 접근이 가능하다는 것을 알고 있기 때문에 1을 할당.

        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue(); // Queue의 첫번째 아이템을 가져오고, 그것을 Queue에서 제거. 

            for (int x = -1; x <= 1; x++) // 좌표에 근접한 네 개의 이웃 타일들을 루프, 8개의 이웃한 타일들을 순환.
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighborX = tile.x + x; // 이웃 타일의 좌표x를 나타냄.
                    int neighborY = tile.y + y; // 이웃 타일의 좌표y를 나타냄.

                    if (x == 0 || y == 0) // 대각선 방향은 체크하지 않음.
                    {
                        if(neighborX >= 0 && neighborX < obstalceMap.GetLength(0) && neighborY >= 0 && neighborY < obstalceMap.GetLength(1))
                        {
                            if(!mapFlags[neighborX, neighborY] && !obstalceMap[neighborX, neighborY]) // 이 타일을 이전에 체크하지 않았다면 && 이것이 장애물이 아닌지 체크 
                            {
                                mapFlags[neighborX, neighborY] = true; // 아직 검사하지 않은 타일을 찾았고, 그것이 장애물 타일이라면 타일을 체크.
                                queue.Enqueue(new Coord(neighborX, neighborY)); // 타일의 이웃 타일들을 체크, 루프 반복
                                accessibleTileCount++; // 접근 가능한 타일을 찾고 accessibleTileCount을 하나 증가.
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - CurrentObstacleCount); // 모든 것이 끝나면, 본래 장애물이 아닌 타일이 얼마나 존재했는지.
        return targetAccessibleTileCount == accessibleTileCount; // 조건문으로 리턴.
    }

    Vector3 CoordPosition(int x, int y)  
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
        return tileMap[x, y];
    }

    public Coord GetRandomCoord() // Queue로 부터 다음 아이템을 얻어 랜덤한 좌표를 반환 
    {
        Coord randomCoord = shuffledTileCoords.Dequeue(); // 셔플된 좌표 Queue의 첫 Item을 가져옴
        shuffledTileCoords.Enqueue(randomCoord); // 얻은 랜덤 좌표 Queue 값을 마지막으로 되돌림
        return randomCoord; 

    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord // Class는 참조형식, struct는 값 형식
    {
        public int x;
        public int y;
        
        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static bool operator == (Coord c1, Coord c2) // Coord에 대한 == 과 != 연산자를 정의. 
        {
            return c1.x == c2.x && c1.y == c2.y;
        }
        public static bool operator != (Coord c1, Coord c2) // Coord에 대한 == 과 != 연산자를 정의. 
        {
            return !(c1 == c2); 
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coord mapSize;

        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;
        
        public Coord mapCenter
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
