using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerT;

    Wave CurrentWave; 
    int currentWaveNum; // 현재 웨이브 넘버

    int enemiesRemainingToSpawn; // 남아있는 스폰할 적
    int enemiesRemainingAlive; // 살아있는 적의 수
  
    float nextSpawnTime; // 다음 스폰 시간 

    MapGenerator map;

    public event System.Action<int> OnNewWave;


    // 캠핑 방지 스폰 변수
    float timeBetweenCampingChecks = 2f; // 캠핑 체크 시간
    float campThresholdDistance = 1.5f; // 캠프한계거리 (1.5f 이상을 움직여야 캠핑 간주 X)    
    float nextCampCheckTime; // 다음 캠핑 체크 시간
    Vector3 campPositionOnOld; // 이전 캠핑 위치 
    bool isCamping;

    bool isDisabled;

    private void Start()
    {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOnOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }
    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOnOld) < campThresholdDistance);
                campPositionOnOld = playerT.position;
            }

            if (enemiesRemainingToSpawn > 0 & Time.time > nextSpawnTime) // 스폰해야 할 적 > 0 && 현재 시간 > 다음 스폰 시간
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + CurrentWave.timeBetweenSpawn;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1f;
        float tileFlashSpeed = 4f;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initalColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initalColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath; // 적을 스폰할때마다 OnEnemyDeath() 추가 

    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

        if(enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void NextWave()
    {
        currentWaveNum++;
        print("wave :" + currentWaveNum);
        
        if(currentWaveNum -1 < waves.Length) // currentWaveNum -1이 웨이브 배열 길이보다 작아야함. (배열은 0부터 시작)
            CurrentWave = waves[currentWaveNum - 1];

        enemiesRemainingToSpawn = CurrentWave.enemyCount; // 현재 웨이브만큼 Enemy 생성
        enemiesRemainingAlive = enemiesRemainingToSpawn; // 남아있는 스폰할 적을 살아있는 적의 수에 할당

        if(OnNewWave != null)
        {
            OnNewWave(currentWaveNum);
        }

        ResetPlayerPosition();
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 2;
    }


    [System.Serializable]
    public class Wave   
    {
        public int enemyCount;
        public float timeBetweenSpawn; // Enemy 스폰 시간
         
    }
    
}
