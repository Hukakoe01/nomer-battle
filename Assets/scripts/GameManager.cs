using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public enum GameMode { Single, Multiplayer }
    public enum BuildMode { None, Build }

    public BuildMode currentBuildMode = BuildMode.None;
    public GameMode CurrentMode = GameMode.Single;

    public List<int[]>[,] islands;
    public int gridSize = 10;

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
    public static bool isPlacingGun = false;
    public static Vector2Int? selectedGunPosition = null;
    public Dictionary<Vector2Int, string> cellEffects = new Dictionary<Vector2Int, string>();

    public GameObject[,] cellObjects;
    public List<EnemyData> allEnemies = new List<EnemyData>();

    public Image enemyImage;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemyStatsText;

    public EnemyData currentEnemy;
    public int currentEnemyHP;

    public RectTransform hpBarImage;
    public int hpBarFullWidth = 330;

    public int playerHP = 50;
    public int maxPlayerHP = 50;
    public TextMeshProUGUI playerHPText;

    public TextMeshProUGUI goldText;
    public int playerGold = 40;

    public int currentEnemyIndex = 0;
    private List<int> enemyQueue = new List<int> { 0, 1, 2, 3, 4, 5 };
    public bool isRemovingMode = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadBuildingsFromDB();
            UpdatePlayerHPUI();
            ShowStory();
        }
        else
        {
            Destroy(gameObject);
        }

        islands = new List<int[]>[gridSize, gridSize];
        buildingsGrid = new BuildingInstance[gridSize, gridSize];
        gunsGrid = new GunInstance[gridSize, gridSize];
        cellObjects = new GameObject[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                islands[x, y] = new List<int[]>();
            }
        }

        if (CurrentMode == GameMode.Single)
        {
            InitEnemies();
        }
    }

    private void Start()
    {
        UpdateGoldUI();

        if (CurrentMode == GameMode.Single)
        {
            SpawnEnemy(enemyQueue[0]);
        }
        StartCoroutine(HandlePassiveStructures());
    }

    public void StartRemoveMode()
    {
        CancelBuildingMode(); // сбрасываем другие режимы
        isRemovingMode = true;

        foreach (GameObject obj in cellObjects)
        {
            if (obj == null) continue;

            Cell cell = obj.GetComponent<Cell>();
            if (cell == null) continue;

            if (islands[cell.x, cell.y].Count > 0) // если есть постройки
            {
                cell.EnableRemoveButton(); // показать ту же кнопку
            }
            else
            {
                cell.HideButton(); // скрыть для пустых клеток
            }
        }

        ShowMessage("Кликните на иконку здания, чтобы удалить");
    }

    public void CancelRemoveMode()
    {
        isRemovingMode = false;

        foreach (GameObject obj in cellObjects)
        {
            if (obj == null) continue;

            Cell cell = obj.GetComponent<Cell>();
            if (cell != null)
                cell.HideButton();
        }

        ShowMessage("Удаление отменено");
    }

    void LoadBuildingsFromDB()
    {
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
                        building.lifetime = reader.GetFloat(7);
                        building.icon = Resources.Load<Sprite>("photo/" + building.imageName);

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

    public BuildingData GetBuildingById(int id)
    {
        return allBuildings.Find(building => building.id == id);
    }

    public void AddGun(BuildingData data, Vector2Int pos, int dir)
    {
        GunInstance gun = new GunInstance
        {
            id = data.id,
            position = pos,
            direction = dir,
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
        return IsValidCell(x, y) ? cellObjects[x, y] : null;
    }

    public void ApplyBuffs(int x, int y, int id)
    {
        List<Vector2Int> targets = GetOrthogonalNeighbors(x, y);

        foreach (var pos in targets)
        {
            if (!IsValidCell(pos.x, pos.y)) continue;

            Vector2Int cell = new Vector2Int(pos.x, pos.y);

            if (id == 1) // home
            {
                cellEffects[cell] = "power";
            }
            else if (id == 9) // statue
            {
                if (cellEffects.ContainsKey(cell))
                {
                    AddGold(2); // усиливает эффект
                    Debug.Log("Statue усилил бонус на клетке " + cell);
                }
            }
        }
    }

    public List<Vector2Int> GetOrthogonalNeighbors(int x, int y)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(x+1, y),
            new Vector2Int(x-1, y),
            new Vector2Int(x, y+1),
            new Vector2Int(x, y-1)
        };
    }

    public void InitEnemies()
    {
        if (CurrentMode != GameMode.Single) return;

        allEnemies.Clear();
        allEnemies.Add(new EnemyData { id = 0, name = "манекен", maxHP = 10, armor = 0, damage = 1, gold = 20, exp = 0, special = "", sprite = Resources.Load<Sprite>("Enemies/enemy_0") });
        allEnemies.Add(new EnemyData { id = 1, name = "ученик", maxHP = 40, armor = 3, damage = 3, gold = 80, exp = 1, special = "", sprite = Resources.Load<Sprite>("Enemies/enemy_1") });
        allEnemies.Add(new EnemyData { id = 2, name = "ведьмочка", maxHP = 100, armor = 1, damage = 6, gold = 100, exp = 10, special = "poison", sprite = Resources.Load<Sprite>("Enemies/enemy_2") });
        allEnemies.Add(new EnemyData { id = 3, name = "страж", maxHP = 200, armor = 10, damage = 10, gold = 200, exp = 200, special = "krit", sprite = Resources.Load<Sprite>("Enemies/enemy_3") });
        allEnemies.Add(new EnemyData { id = 4, name = "живое дерево", maxHP = 250, armor = 0, damage = 14, gold = 150, exp = 10000, special = "destroy_towe", sprite = Resources.Load<Sprite>("Enemies/enemy_4") });
        allEnemies.Add(new EnemyData { id = 5, name = "дракон", maxHP = 300, armor = 0, damage = 20, gold = 150, exp = 10000, special = "destroy_towe", sprite = Resources.Load<Sprite>("Enemies/enemy_4") });
    }

    public void SpawnEnemy(int id)
    {
        if (CurrentMode != GameMode.Single) return;

        currentEnemy = allEnemies.Find(e => e.id == id);
        currentEnemyHP = currentEnemy.maxHP;

        if (enemyImage != null) enemyImage.sprite = currentEnemy.sprite;
        if (enemyHPText != null) enemyHPText.text = $"{currentEnemyHP} / {currentEnemy.maxHP}";
        if (enemyStatsText != null) enemyStatsText.text = $"Имя: {currentEnemy.name}\nУрон: {currentEnemy.damage}";
    }

    public void EnemyDefeated()
    {
        if (CurrentMode != GameMode.Single) return;

        StartCoroutine(EnemyDefeatedSequence());
    }

    private IEnumerator EnemyDefeatedSequence()
    {
        TText.text = "Враг повержен!";
        TText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        TText.text = $"Получено {currentEnemy.gold} золота!";
        yield return new WaitForSeconds(1.5f);

        TText.text = $"Получено {currentEnemy.exp} опыта!";
        yield return new WaitForSeconds(1.5f);
        TText.text = "";

        currentEnemyIndex++;
        if (currentEnemyIndex < allEnemies.Count)
        {
            SpawnEnemy(enemyQueue[currentEnemyIndex]);
        }
        else
        {
            TText.text = "Все враги побеждены!";
        }
    }

    public void UpdateEnemyUI()
    {
        if (CurrentMode != GameMode.Single) return;

        if (enemyHPText != null)
            enemyHPText.text = $"{currentEnemyHP} / {currentEnemy.maxHP}";

        if (hpBarImage != null && currentEnemy != null)
        {
            float oneHP = (float)hpBarFullWidth / currentEnemy.maxHP;
            float offsetX = oneHP * (currentEnemy.maxHP - currentEnemyHP);
            StartCoroutine(AnimateHPBar(-offsetX));
        }
    }

    private IEnumerator AnimateHPBar(float targetX)
    {
        float duration = 0.5f;
        float time = 0f;

        Vector2 start = hpBarImage.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);

        while (time < duration)
        {
            time += Time.deltaTime;
            hpBarImage.anchoredPosition = Vector2.Lerp(start, end, time / duration);
            yield return null;
        }

        hpBarImage.anchoredPosition = end;
    }

    public void TakeDamageFromEnemy()
    {
        if (CurrentMode != GameMode.Single) return;
        if (currentEnemy == null) return;

        int enemyDamage = currentEnemy.damage;

        // проверка spire
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                foreach (var building in islands[x, y])
                {
                    if (building[0] == 8)
                    {
                        enemyDamage -= 4;
                        Debug.Log("Spire уменьшил урон врага на 4");
                    }
                }
            }
        }

        playerHP -= Mathf.Max(0, enemyDamage);

        UpdatePlayerHPUI();

        if (playerHP <= 0) OnPlayerDeath();
    }


    public void OnPlayerDeath()
    {
        if (CurrentMode != GameMode.Single) return;
        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        TText.text = "Вы проиграли!";
        TText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);

        ClearAllCards();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdatePlayerHPUI()
    {
        if (playerHPText != null)
            playerHPText.text = $"HP: {playerHP} / {maxPlayerHP}";
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
            card.ClearCard();
            Destroy(card.gameObject);
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
        if (goldText != null)
            goldText.text = "Монеты: " + playerGold;
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
    public void CancelBuildingMode()
    {
        currentBuildMode = BuildMode.None;
        selectedCardId = -1;

        // Очистить курсор, если нужно
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        // Можно скрыть кнопки у всех Cell
        if (cellObjects != null)
        {
            foreach (var obj in cellObjects)
            {
                if (obj != null)
                {
                    Cell cell = obj.GetComponent<Cell>();
                    if (cell != null)
                        cell.HideButton();
                }
            }
        }
    }


    private IEnumerator HandlePassiveStructures()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);  // интервал логики

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    foreach (var building in islands[x, y])
                    {
                        int id = building[0];
                        BuildingData bdata = GetBuildingById(id);
                        if (bdata == null) continue;

                        // Coiner (id = 2)
                        if (bdata.id == 2)
                        {
                            AddGold(4);
                            Debug.Log("coiner дал 4 монеты");
                        }

                        // Shield (id = 3)
                        if (bdata.id == 3)
                        {
                            var neighbors = GetOrthogonalNeighbors(x, y);
                            foreach (var pos in neighbors)
                            {
                                if (!IsValidCell(pos.x, pos.y)) continue;

                                foreach (var obj in islands[pos.x, pos.y])
                                {
                                    BuildingData neighbor = GetBuildingById(obj[0]);
                                    if (neighbor != null && neighbor.type == "gun")
                                    {
                                        playerHP = Mathf.Min(maxPlayerHP, playerHP + 3); // 3 хп
                                        Debug.Log("shield восстановил 3 HP");
                                        UpdatePlayerHPUI();
                                        break;
                                    }
                                }
                            }
                        }

                        // Spire (id = 8)
                        if (bdata.id == 8)
                        {
                            Debug.Log("spire активен — враг получит меньше урона"); // логика ниже
                        }
                    }
                }
            }
        }
    }

    public void ProcessBuildingLifetimes()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                List<int[]> toRemove = new List<int[]>();
                List<int[]> cellBuildings = islands[x, y];

                foreach (var data in cellBuildings)
                {
                    int id = data[0];
                    int age = data[1];

                    BuildingData building = GetBuildingById(id);
                    if (building == null || building.type != "tower") continue;

                    if (building.lifetime <= 0) continue; // бесконечные здания

                    data[1]++; // увеличить возраст

                    if (data[1] >= building.lifetime)
                    {
                        Debug.Log($"Здание {building.name} на клетке ({x},{y}) разрушено по сроку жизни");
                        toRemove.Add(data);

                        // Очистить эффект, если был
                        Vector2Int pos = new Vector2Int(x, y);
                        if (cellEffects.ContainsKey(pos))
                            cellEffects.Remove(pos);

                        // Убрать иконку
                        GameObject cell = GetCellObject(x, y);
                        if (cell != null)
                        {
                            Cell cellComp = cell.GetComponent<Cell>();
                            if (cellComp != null && cellComp.ImgOnIsland != null)
                            {            Sprite skySprite = Resources.Load<Sprite>("sky");
            cellComp.ImgOnIsland.sprite = skySprite;
                            }
                        }
                    }
                }

                // Удаляем отработавшие здания
                foreach (var b in toRemove)
                {
                    cellBuildings.Remove(b);
                }
            }
        }
    }

    public void OnDeleteButtonClicked()
    {
        GameManager.Instance.StartRemoveMode();
    }
    private int storyIndex = 0;
    private string[] story = new string[]
    {
        "Когда-то давно, на затерянных островах, жил мудрый строитель.",
        "Однажды из глубин моря поднялись враждебные механизмы.",
        "Жители построили пушки, чтобы защитить свои дома.",
        "Каждое уничтоженное зло приносило частичку победы и ресурсов.",
        "Мудро расставленные сооружения остановили натиск врагов.",
        "Острова снова наполнились светом, и страх исчез.",
        "Теперь этот мир живёт под защитой тех, кто не сдался."
    };


    public void ShowStory()
    {
        if (storyIndex < story.Length)
        {
            ShowMessage(story[storyIndex]);
            storyIndex++;
        }
    }

}