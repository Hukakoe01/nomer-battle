using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// GameManager.cs

public class GameManager : MonoBehaviour  // Наследуем от MonoBehaviour
{
    public List<int[]>[,] islands;
    public int gridSize = 10;
    public int hp_enemy;

    public static GameManager Instance;
    public static int selectedCardId = -1;

    public static List<BuildingData> allBuildings = new List<BuildingData>();
    public static int kolvo_gun = 0;
    public static int max_gun = 2;
    public TMP_Text TText;

    public Button shootButton;

    public BuildingInstance[,] buildingsGrid;
    public GunInstance[,] gunsGrid;

    public static bool isGunPlacementPending = false;
    public static BuildingData tempGunData;
    public static Vector2Int tempGunPosition;

    public List<GunInstance> gunArray = new List<GunInstance>();

    public enum BuildMode { None, SelectingCell, SelectingDirection }
    public BuildMode currentBuildMode = BuildMode.None;

    public GameObject[,] cellObjects;

    public static bool isPlacingGun = false;
    public static Vector2Int? selectedGunPosition = null;
    public Dictionary<Vector2Int, string> cellEffects = new Dictionary<Vector2Int, string>();

    public List<EnemyData> allEnemies = new List<EnemyData>();

    public Image enemyImage;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemyStatsText;

    public EnemyData currentEnemy;
    public int currentEnemyHP;

    public RectTransform hpBarImage;
    public int hpBarFullWidth = 330;

    private List<int> enemyQueue = new List<int> { 0, 1, 2, 3, 4, 5 };

    public int playerHP = 50;
    public int maxPlayerHP = 50;
    public TextMeshProUGUI playerHPText; // Привяжи в инспекторе

    public TextMeshProUGUI goldText;
    public int playerGold = 40;


    public void CancelBuildingMode()
    {
        selectedCardId = -1;
        isPlacingGun = false;
        selectedGunPosition = null;
    }



    private void Start()
    {
        UpdateGoldUI();
        InitEnemies();
    }



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadBuildingsFromDB();
            UpdatePlayerHPUI();
        }
        else
        {
            Destroy(gameObject);
        }
        islands = new List<int[]>[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                islands[x, y] = new List<int[]>();
            }
        }
        buildingsGrid = new BuildingInstance[gridSize, gridSize];
        gunsGrid = new GunInstance[gridSize, gridSize];
        InitEnemies();

    }


    void LoadBuildingsFromDB()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "data.db");
        string dbPath = "URI=file:" + DatabaseManager.GetDatabasePath();


        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, name, about, price, image, type, damage, lifetime FROM towers";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BuildingData building = new BuildingData();

                        building.id = reader.GetInt32(0);
                        building.name = reader.GetString(1);
                        building.description = reader.GetString(2);
                        building.price = reader.GetInt32(3);
                        building.imageName = reader.GetString(4);
                        building.type = reader.GetString(5);
                        building.damage = reader.GetInt32(6);
                        building.lifetime = reader.GetInt32(7);

                        building.icon = Resources.Load<Sprite>("photo/" + building.imageName);
                        Debug.Log($"Здание загружено: {building.name}, ID: {building.id}, Цена: {building.price}");

                        allBuildings.Add(building);
                    }
                }
            }
        }
    }

    public List<BuildingData> GetBuildingsByType(string type)
    {
        return allBuildings.FindAll(building => building.type == type);
    }

    void PrintAllBuildings()
        {
            foreach (var building in allBuildings)
            {
                Debug.Log($"Building ID: {building.id}, Name: {building.name}, Price: {building.price}, Type: {building.type}");
            }
        }
    public BuildingData GetBuildingById(int id)
    {
        return allBuildings.Find(building => building.id == id);
    }
    public void AddGun(BuildingData data, Vector2Int pos, int direction)
    {
        GunInstance gun = new GunInstance()
        {
            id = data.id,
            position = pos,
            direction = direction,
            damage = data.damage,
            cost = data.cost,
            lifetime = data.lifetime
        };

        gunArray.Add(gun);
    }
    public bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < islands.GetLength(0) && y >= 0 && y < islands.GetLength(1);
    }
    public GameObject GetCellObject(int x, int y)
    {
        if (IsValidCell(x, y))
            return cellObjects[x, y];
        return null;
    }


    public void ApplyBuffs(int centerX, int centerY, int buildingId)
    {
        List<Vector2Int> targetCells = new List<Vector2Int>();

        switch (buildingId)
        {
            case 1: 
                targetCells = GetOrthogonalNeighbors(centerX, centerY);
                break;

            case 2: 
                targetCells = GetDiagonalNeighbors(centerX, centerY);
                break;

            case 3: 
                targetCells = GetKnightMoves(centerX, centerY);
                break;
        }

        foreach (var pos in targetCells)
        {
            if (IsValidCell(pos.x, pos.y))
            {
                Debug.Log($"Клетка ({pos.x}, {pos.y}) получила баф от здания {buildingId}");
                cellEffects[new Vector2Int(pos.x, pos.y)] = "power";

            }
        }
    }

    public List<Vector2Int> GetOrthogonalNeighbors(int x, int y)
    {
        return new List<Vector2Int>
    {
        new Vector2Int(x + 1, y),
        new Vector2Int(x - 1, y),
        new Vector2Int(x, y + 1),
        new Vector2Int(x, y - 1)
    };
    }

    public List<Vector2Int> GetDiagonalNeighbors(int x, int y)
    {
        return new List<Vector2Int>
    {
        new Vector2Int(x + 1, y + 1),
        new Vector2Int(x - 1, y + 1),
        new Vector2Int(x + 1, y - 1),
        new Vector2Int(x - 1, y - 1)
    };
    }

    public List<Vector2Int> GetKnightMoves(int x, int y)
    {
        return new List<Vector2Int>
    {
        new Vector2Int(x + 1, y + 2),
        new Vector2Int(x - 1, y + 2),
        new Vector2Int(x + 1, y - 2),
        new Vector2Int(x - 1, y - 2),
        new Vector2Int(x + 2, y + 1),
        new Vector2Int(x - 2, y + 1),
        new Vector2Int(x + 2, y - 1),
        new Vector2Int(x - 2, y - 1)
    };
    }

    public void InitEnemies()
    {
        allEnemies.Clear();

        allEnemies.Add(new EnemyData
        {
            id = 0,
            name = "манекен",
            maxHP = 10,
            armor = 0,
            damage = 1,
            gold = 5,
            exp = 0,
            special = "",
            sprite = Resources.Load<Sprite>("Enemies/enemy_0")
        });

        allEnemies.Add(new EnemyData
        {
            id = 1,
            name = "ученик",
            maxHP = 50,
            armor = 3,
            damage = 5,
            gold = 20,
            exp = 1,
            special = "",
            sprite = Resources.Load<Sprite>("Enemies/enemy_1")
        });

        allEnemies.Add(new EnemyData
        {
            id = 2,
            name = "ведьмочка",
            maxHP = 150,
            armor = 1,
            damage = 10,
            gold = 40,
            exp = 10,
            special = "poison",
            sprite = Resources.Load<Sprite>("Enemies/enemy_2")
        });

        allEnemies.Add(new EnemyData
        {
            id = 3,
            name = "страж",
            maxHP = 500,
            armor = 10,
            damage = 20,
            gold = 100,
            exp = 200,
            special = "krit",
            sprite = Resources.Load<Sprite>("Enemies/enemy_3")
        });

        allEnemies.Add(new EnemyData
        {
            id = 4,
            name = "живое дерево",
            maxHP = 1000,
            armor = 0,
            damage = 50,
            gold = 150,
            exp = 10000,
            special = "destroy_towe",
            sprite = Resources.Load<Sprite>("Enemies/enemy_4")
        });
        allEnemies.Add(new EnemyData
        {
            id = 5,
            name = "дракон",
            maxHP = 2000,
            armor = 0,
            damage = 50,
            gold = 150,
            exp = 10000,
            special = "destroy_towe",
            sprite = Resources.Load<Sprite>("Enemies/enemy_4")
        });
        allEnemies.Add(new EnemyData
        {
            id = 6,
            name = "король",
            maxHP = 3500,
            armor = 8,
            damage = 100,
            gold = 50,
            exp = 1000000,
            special = "half_field",
            sprite = Resources.Load<Sprite>("Enemies/enemy_5")
        });

        Debug.Log("Враги загружены: " + allEnemies.Count);
    }

    public void SpawnEnemy(int enemyId)
    {
        currentEnemy = allEnemies.Find(e => e.id == enemyId);
        if (currentEnemy == null)
        {
            Debug.LogError("Враг не найден!");
            return;
        }

        currentEnemyHP = currentEnemy.maxHP;
        enemyImage.sprite = currentEnemy.sprite;
        enemyHPText.text = $"{currentEnemyHP} / {currentEnemy.maxHP}";

        if (enemyStatsText != null)
        {
            enemyStatsText.text =
                $"Имя: {currentEnemy.name}\n" +
                $"Урон: {currentEnemy.damage}\n";
        }

        Debug.Log($"Появился враг: {currentEnemy.name}");
    }

    public int currentEnemyIndex = 0;

    public void EnemyDefeated()
    {
        StartCoroutine(EnemyDefeatedSequence());
    }

    private IEnumerator EnemyDefeatedSequence()
    {
        GameManager.Instance.TText.gameObject.SetActive(true);
        TText.text = "Враг повержен!";
        yield return new WaitForSeconds(1.5f);

        if (TText != null) TText.text = $"Вы получили {currentEnemy.gold} золота!";
        yield return new WaitForSeconds(1.5f);

        if (TText != null) TText.text = $"Получено {currentEnemy.exp} опыта!";
        yield return new WaitForSeconds(1.5f);

        // Очистка текста
        if (TText != null) TText.text = "";

        // Следующий враг
        currentEnemyIndex++;
        if (currentEnemyIndex < allEnemies.Count)
        {
            SpawnEnemy(enemyQueue[currentEnemyIndex]);
        }
        else
        {
            if (TText != null) TText.text = "Все враги побеждены!";
        }
    }

    public void UpdateEnemyUI()
    {
        if (enemyHPText != null)
        {
            enemyHPText.text = $"{currentEnemyHP} / {currentEnemy.maxHP}";
        }

        if (hpBarImage != null && currentEnemy != null)
        {
            float oneHPWidth = (float)hpBarFullWidth / currentEnemy.maxHP;
            float targetOffset = oneHPWidth * (currentEnemy.maxHP - currentEnemyHP);
            StartCoroutine(AnimateHPBar(-targetOffset));
        }
    }

    private IEnumerator AnimateHPBar(float targetX)
    {
        float duration = 0.5f;
        float time = 0f;

        Vector2 startPos = hpBarImage.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, startPos.y);

        while (time < duration)
        {
            time += Time.deltaTime;
            hpBarImage.anchoredPosition = Vector2.Lerp(startPos, endPos, time / duration);
            yield return null;
        }

        hpBarImage.anchoredPosition = endPos;
    }

    public void SpawnNextEnemy()
    {
        currentEnemyIndex++;
        if (currentEnemyIndex < enemyQueue.Count)
        {
            SpawnEnemy(enemyQueue[currentEnemyIndex]);
        }
    }
    public void UpdatePlayerHPUI()
    {
        if (playerHPText != null)
        {
            playerHPText.text = $"HP: {playerHP} / {maxPlayerHP}";
        }
    }
    public void TakeDamageFromEnemy()
    {
        if (currentEnemy == null) return;

        playerHP -= currentEnemy.damage;
        if (playerHP < 0) playerHP = 0;

        UpdatePlayerHPUI();

        if (playerHP <= 0)
        {
            Debug.Log("Игрок погиб!");
            OnPlayerDeath();
            SceneManager.LoadScene("SampleScene"); // Имя сцены главного меню
        }
    }

    public void OnPlayerDeath()
    {
        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        GameManager.Instance.TText.text = "Вы проиграли!";
        GameManager.Instance.TText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);
        ClearAllCards();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }




    public void ResetGameState()
    {
        gunArray.Clear();
        cellEffects.Clear();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                islands[x, y]?.Clear();
            }
        }

        playerHP = maxPlayerHP;
        currentEnemyHP = 0;
    }
    public void ClearAllCards()
    {
        foreach (var card in GameObject.FindObjectsOfType<BuildingCard>())
        {
            card.ClearCard(); // Очищаем визуально
            Destroy(card.gameObject); // Или сохраняем для пула, если хочешь
        }
    }


    public void AddGold(int amount)
    {
        playerGold += amount;
        UpdateGoldUI();
    }

    public void SpendGold(int amount)
    {
        playerGold -= amount;
        UpdateGoldUI();
    }

    public void UpdateGoldUI()
    {
        goldText.text = "Монеты: " + playerGold; // goldText — привязанный TextMeshPro
    }
    public void ShowMessage(string message)
    {
        if (TText != null)
        {
            TText.text = message;
            TText.gameObject.SetActive(true);
            StartCoroutine(HideMessageAfterDelay());
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (TText != null)
            TText.gameObject.SetActive(false);
    }
}
