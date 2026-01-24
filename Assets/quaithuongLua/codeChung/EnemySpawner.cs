using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;
    
    [Header("Spawn Offset (Tránh chồng lên nhau)")]
    public float minSpawnOffset = 1.5f; // Khoảng cách tối thiểu giữa các enemy
    public float maxSpawnOffset = 3f; // Khoảng cách tối đa (chỉ theo chiều ngang X)
    public float groundCheckDistance = 2f; // Khoảng cách kiểm tra ground phía dưới
    public LayerMask enemyLayer; // Layer để kiểm tra enemy đã có ở vị trí chưa
    public LayerMask groundLayer; // Layer của ground để kiểm tra

    [Header("Total Spawn Count (Tổng số lượng cần spawn)")]
    public int meleeSpawnCount = 5;
    public int rangedSpawnCount = 5;

    [Header("Initial Spawn (Số lượng spawn ban đầu)")]
    public int initialMeleeCount = 2;
    public int initialRangedCount = 1;

    [Header("Auto Spawn")]
    public float checkInterval = 0.5f;
    public int triggerEnemyCount = 2; // Khi còn đúng số này thì spawn tiếp
    public int spawnPerWave = 5; // Số lượng spawn mỗi đợt
    public bool enableDebugLog = true; // Bật log để debug

    float checkTimer;
    int meleeSpawned; // Số lượng melee đã spawn
    int rangedSpawned; // Số lượng ranged đã spawn
    bool hasInitialSpawned; // Đã spawn lần đầu chưa
    bool isSpawningWave; // Đang spawn một đợt

    // ================= START =================
    void Start()
    {
        // Spawn ban đầu
        SpawnInitial();
    }

    // ================= UPDATE =================
    void Update()
    {
        // phím 2 → melee (manual spawn)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnMelee();
        }

        // phím 3 → ranged (manual spawn)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnRanged();
        }

        // ⭐ Auto spawn khi gần hết quái
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            CheckAndSpawn();
        }
    }

    // ================= INITIAL SPAWN =================
    void SpawnInitial()
    {
        if (hasInitialSpawned) return;

        // Spawn số lượng ban đầu
        for (int i = 0; i < initialMeleeCount; i++)
        {
            if (meleeSpawned < meleeSpawnCount)
            {
                SpawnMelee();
                meleeSpawned++;
            }
        }

        for (int i = 0; i < initialRangedCount; i++)
        {
            if (rangedSpawned < rangedSpawnCount)
            {
                SpawnRanged();
                rangedSpawned++;
            }
        }

        hasInitialSpawned = true;
    }

    // ================= AUTO SPAWN =================
    void CheckAndSpawn()
    {
        // ⭐ Đã spawn hết → ngừng
        if (meleeSpawned >= meleeSpawnCount && rangedSpawned >= rangedSpawnCount)
        {
            isSpawningWave = false;
            return;
        }

        int currentEnemyCount = CountEnemies();

        if (enableDebugLog)
        {
            int totalRemaining = (meleeSpawnCount - meleeSpawned) + (rangedSpawnCount - rangedSpawned);
            Debug.Log($"[EnemySpawner] Enemy count: {currentEnemyCount}, Melee: {meleeSpawned}/{meleeSpawnCount}, Ranged: {rangedSpawned}/{rangedSpawnCount}, Remaining: {totalRemaining}, isSpawning: {isSpawningWave}");
        }

        // ⭐ Reset flag nếu enemy count tăng lên (có thể đã spawn xong đợt)
        if (currentEnemyCount > triggerEnemyCount)
        {
            isSpawningWave = false;
            return;
        }

        // ⭐ Khi còn <= triggerEnemyCount con → spawn một đợt
        if (currentEnemyCount <= triggerEnemyCount && !isSpawningWave)
        {
            int totalRemaining = (meleeSpawnCount - meleeSpawned) + (rangedSpawnCount - rangedSpawned);
            
            // Chỉ spawn nếu còn số lượng cần spawn
            if (totalRemaining > 0)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[EnemySpawner] Trigger spawn wave! Enemy count: {currentEnemyCount} <= {triggerEnemyCount}, Remaining: {totalRemaining}");
                }
                
                isSpawningWave = true;
                SpawnWave();
            }
        }
    }

    // ================= SPAWN WAVE =================
    void SpawnWave()
    {
        int meleeRemaining = meleeSpawnCount - meleeSpawned;
        int rangedRemaining = rangedSpawnCount - rangedSpawned;
        int totalRemaining = meleeRemaining + rangedRemaining;

        // Số lượng spawn trong đợt này (không vượt quá số còn lại)
        int spawnThisWave = Mathf.Min(spawnPerWave, totalRemaining);

        if (enableDebugLog)
        {
            Debug.Log($"[EnemySpawner] Spawning wave: {spawnThisWave} enemies (Remaining: {totalRemaining}, Melee: {meleeRemaining}, Ranged: {rangedRemaining})");
        }

        // Spawn ngẫu nhiên melee/ranged trong số lượng còn lại
        for (int i = 0; i < spawnThisWave; i++)
        {
            // Kiểm tra còn loại nào để spawn
            bool canSpawnMelee = meleeSpawned < meleeSpawnCount;
            bool canSpawnRanged = rangedSpawned < rangedSpawnCount;

            if (!canSpawnMelee && !canSpawnRanged)
                break; // Đã spawn hết

            if (!canSpawnMelee)
            {
                // Chỉ còn ranged
                SpawnRanged();
                rangedSpawned++;
            }
            else if (!canSpawnRanged)
            {
                // Chỉ còn melee
                SpawnMelee();
                meleeSpawned++;
            }
            else
            {
                // Còn cả 2 loại → spawn ngẫu nhiên
                if (Random.Range(0, 2) == 0)
                {
                    SpawnMelee();
                    meleeSpawned++;
                }
                else
                {
                    SpawnRanged();
                    rangedSpawned++;
                }
            }
        }

        if (enableDebugLog)
        {
            Debug.Log($"[EnemySpawner] Wave complete! Spawned: {spawnThisWave}, Total: {meleeSpawned} melee + {rangedSpawned} ranged");
        }

        isSpawningWave = false;
    }

    // ================= COUNT =================
    int CountEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        return enemies.Length;
    }

    // ================= SPAWN =================
    void SpawnMelee()
    {
        if (!meleeEnemyPrefab || spawnPoints.Length == 0) return;

        Vector2 spawnPosition = GetSafeSpawnPosition();
        if (spawnPosition == Vector2.zero) return;

        GameObject newEnemy = Instantiate(
            meleeEnemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // ⭐ Ignore collision với tất cả enemy khác
        IgnoreEnemyCollisions(newEnemy);
    }

    void SpawnRanged()
    {
        if (!rangedEnemyPrefab || spawnPoints.Length == 0) return;

        Vector2 spawnPosition = GetSafeSpawnPosition();
        if (spawnPosition == Vector2.zero) return;

        GameObject newEnemy = Instantiate(
            rangedEnemyPrefab,
            spawnPosition,
            Quaternion.identity
        );

        // ⭐ Ignore collision với tất cả enemy khác
        IgnoreEnemyCollisions(newEnemy);
    }

    // ================= HELPER =================
    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // ================= SAFE SPAWN POSITION =================
    Vector2 GetSafeSpawnPosition()
    {
        if (spawnPoints.Length == 0) return Vector2.zero;

        // Thử tìm vị trí an toàn (tối đa 20 lần thử)
        for (int attempt = 0; attempt < 20; attempt++)
        {
            // Chọn spawn point ngẫu nhiên
            Transform basePoint = GetRandomSpawnPoint();
            if (basePoint == null) continue;

            Vector2 basePos = basePoint.position;

            // ⭐ CHỈ OFFSET THEO X (ngang), giữ nguyên Y của spawn point
            float offsetX = Random.Range(-maxSpawnOffset, maxSpawnOffset);
            Vector2 candidatePos = basePos + new Vector2(offsetX, 0);

            // Kiểm tra xem vị trí có enemy nào không
            bool hasEnemyNearby = !IsPositionSafe(candidatePos);
            
            // Kiểm tra có ground ở dưới không
            bool hasGround = HasGroundBelow(candidatePos);

            if (!hasEnemyNearby && hasGround)
            {
                return candidatePos;
            }
        }

        // Nếu không tìm được vị trí an toàn sau nhiều lần thử, 
        // spawn ở chính xác vị trí spawn point (không offset)
        Transform fallbackPoint = GetRandomSpawnPoint();
        if (fallbackPoint != null)
        {
            Vector2 basePos = fallbackPoint.position;
            
            // Kiểm tra lại xem spawn point có ground không
            if (HasGroundBelow(basePos))
            {
                return basePos;
            }
        }

        // Cuối cùng, thử tất cả spawn points để tìm vị trí có ground
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;
            if (HasGroundBelow(point.position))
            {
                return point.position;
            }
        }

        // Nếu vẫn không tìm được, trả về spawn point đầu tiên
        if (spawnPoints.Length > 0 && spawnPoints[0] != null)
        {
            return spawnPoints[0].position;
        }

        return Vector2.zero;
    }

    // ================= CHECK SAFE POSITION =================
    bool IsPositionSafe(Vector2 position)
    {
        // Kiểm tra xem có enemy nào trong bán kính minSpawnOffset không
        Collider2D[] colliders;
        
        if (enemyLayer.value != 0)
        {
            // Sử dụng layer mask nếu đã được set
            colliders = Physics2D.OverlapCircleAll(
                position,
                minSpawnOffset,
                enemyLayer
            );
        }
        else
        {
            // Nếu chưa set layer, kiểm tra tất cả collider
            colliders = Physics2D.OverlapCircleAll(
                position,
                minSpawnOffset
            );
        }

        // Lọc ra chỉ enemy (có tag "Enemy")
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                return false; // Có enemy ở gần, không an toàn
            }
        }

        return true; // Vị trí an toàn
    }

    // ================= CHECK GROUND =================
    bool HasGroundBelow(Vector2 position)
    {
        // Nếu không set ground layer, bỏ qua kiểm tra
        if (groundLayer.value == 0)
            return true;

        // Kiểm tra có ground ở phía dưới không
        RaycastHit2D hit = Physics2D.Raycast(
            position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    // ================= IGNORE ENEMY COLLISIONS =================
    void IgnoreEnemyCollisions(GameObject newEnemy)
    {
        if (newEnemy == null) return;

        // Lấy tất cả collider của enemy mới
        Collider2D[] newEnemyColliders = newEnemy.GetComponentsInChildren<Collider2D>();
        if (newEnemyColliders.Length == 0)
            newEnemyColliders = newEnemy.GetComponents<Collider2D>();

        // Lấy tất cả enemy hiện có
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Ignore collision giữa enemy mới và tất cả enemy cũ
        foreach (GameObject existingEnemy in existingEnemies)
        {
            if (existingEnemy == newEnemy) continue; // Bỏ qua chính nó

            Collider2D[] existingColliders = existingEnemy.GetComponentsInChildren<Collider2D>();
            if (existingColliders.Length == 0)
                existingColliders = existingEnemy.GetComponents<Collider2D>();

            // Ignore collision giữa tất cả collider
            foreach (Collider2D newCol in newEnemyColliders)
            {
                foreach (Collider2D existingCol in existingColliders)
                {
                    if (newCol != null && existingCol != null)
                    {
                        Physics2D.IgnoreCollision(newCol, existingCol, true);
                    }
                }
            }
        }
    }
}
