# Tower Defense — звіт по проєкту

> Двовимірна Tower Defense гра на Unity 6 (URP 2D), згенерована повністю у коді (без префабів). Запускається у браузері через WebGL.
>
> 🎮 **Грати:** https://devorius-nagara.github.io/ToweDefenseProj/

---

## 1. Короткий опис правил і геймплею

Гра поділена на **раунди**. Кожен раунд складається з двох фаз:

1. **Preparation (підготовка)** — тривалість 35/30/25 секунд (залежно від рівня). Гравець може ставити, апгрейдити та продавати вежі.
2. **Battle (бій)** — спавниться хвиля ворогів, яких сформував AI під свій бюджет. Гравець може й далі ставити/апгрейдити вежі під час бою.

**Мета:** не дати ворогам пройти весь шлях. Кожен ворог, що доходить до кінця шляху, знімає `baseDamage` HP з бази (база має 20 HP).

**Перемога:** пережити всі `MaxRounds` хвиль рівня (8/10/12 для рівнів 1/2/3).
**Поразка:** HP бази впало до 0.

**Дії гравця:**

| Дія | Керування |
|------|-----------|
| Вибрати вежу для розставлення | клік по кнопці вежі у нижній панелі |
| Поставити вежу на клітинку | LMB на вільній клітинці (не на шляху) |
| Скасувати розставлення | RMB |
| Апгрейдити вежу (макс. 2 апгрейди) | LMB на вже поставленій вежі |
| Продати вежу (повертає 50% вартості) | RMB на вже поставленій вежі |

Усі вежі мають **range indicator** (жовте кільце), що з'являється при наведенні мишки. У бою вежі автоматично шукають ціль і стріляють по cooldown'у; режим націлювання — `FirstInPath` (по дефолту найближчий до бази).

Доступно **3 рівні** з різною геометрією шляху та параметрами економіки.

---

## 2. Схема станів (state machine) та ключові скрипти

State machine оголошена в [`GameEnums.cs`](Assets/Scripts/Gameplay/GameEnums.cs:1) і керується одним методом `SetState()` у [`GameManager.cs`](Assets/Scripts/Gameplay/GameManager.cs:119).

```
                            ┌─────────────────┐
                            │   MainMenu      │ ◄────────────────┐
                            └────────┬────────┘                  │
                                     │ StartGame()               │ GoToMainMenu()
                                     ▼                           │
                            ┌─────────────────┐                  │
              ┌────────────►│  Preparation    │                  │
              │             │  (35/30/25 s    │                  │
              │             │   timer + UI)   │                  │
              │             └────────┬────────┘                  │
              │                      │ StartBattle()             │
              │                      │ (timer→0 or button)       │
              │                      ▼                           │
   NextRound()│             ┌─────────────────┐                  │
              │             │     Battle      │                  │
              │             │  (waves spawn,  │                  │
              │             │   towers fire)  │                  │
              │             └────────┬────────┘                  │
              │                      │                           │
              │      ┌───────────────┼────────────────┐          │
              │      │               │                │          │
              │ OnWaveComplete       │       DamageBase(BaseHP=0)│
              │      │               │                │          │
              │      ▼               ▼                ▼          │
              │ ┌──────────┐  ┌──────────┐     ┌──────────┐      │
              └─│ RoundEnd │  │ Victory  │     │ Defeat   │──────┘
                └──────────┘  └──────────┘     └──────────┘
                 (round         (round ≥          (Restart /
                  < max)         MaxRounds)        GoToMainMenu)
```

### Ключові скрипти

| Скрипт | Призначення |
|--------|-------------|
| [`GameBootstrap.cs`](Assets/Scripts/Bootstrap/GameBootstrap.cs:9) | Корінь усього — будує сцену в коді: створює менеджери, дані ворогів/веж, пули. |
| [`GameManager.cs`](Assets/Scripts/Gameplay/GameManager.cs:3) | Singleton зі state machine, лічильник раундів, HP бази, керування переходами. |
| [`LevelManager.cs`](Assets/Scripts/Gameplay/LevelManager.cs:8) | Каталог із 3 рівнів (`LevelConfig`), DontDestroyOnLoad. |
| [`EnemySpawner.cs`](Assets/Scripts/Gameplay/EnemySpawner.cs:5) | Корутиною спавнить чергу `Queue<EnemyData>` з інтервалом 1с, відстежує `ActiveEnemies` і завершення хвилі. |
| [`EnemyController.cs`](Assets/Scripts/Gameplay/EnemyController.cs:5) | Рух по waypoint'ах, HP-бар, slow-ефект, hit-flash, death animation. |
| [`TowerController.cs`](Assets/Scripts/Gameplay/TowerController.cs:4) | Пошук цілі (4 режими targeting), cooldown, апгрейд, range ring, спалах при пострілі. |
| [`Projectile.cs`](Assets/Scripts/Gameplay/Projectile.cs:3) | Політ до цілі, попадання, AoE-радіус, накладення `ApplySlow`. |
| [`TowerPlacer.cs`](Assets/Scripts/Gameplay/TowerPlacer.cs:9) | Обробка миші: LMB/RMB на клітинках і на вежах. |
| [`AIWaveBuilder.cs`](Assets/Scripts/Gameplay/AIWaveBuilder.cs:9) | Генерація хвиль під `AttackerBudget`. |
| [`EconomyManager.cs`](Assets/Scripts/Gameplay/EconomyManager.cs:3) | Золото гравця + бюджет AI. |
| [`PoolManager.cs`](Assets/Scripts/Gameplay/PoolManager.cs:4) | Object pooling (Dictionary&lt;string, Queue&lt;GameObject&gt;&gt;). |
| [`UIManager.cs`](Assets/Scripts/UI/UIManager.cs:1) | IMGUI HUD, головне меню, статистика, екрани перемоги/поразки. |
| [`AudioManager.cs`](Assets/Scripts/Gameplay/AudioManager.cs:1) | Музика (menu/gameplay) + sfx (постріли, смерть, золото, тощо). |
| [`StatisticsManager.cs`](Assets/Scripts/Gameplay/StatisticsManager.cs:6) | Session + all-time stats, збереження у `PlayerPrefs`. |

Перехід `Preparation → Battle` керується таймером у [`GameManager.Update()`](Assets/Scripts/Gameplay/GameManager.cs:109). Перехід `Battle → RoundEnd/Victory` ініціюється з [`EnemySpawner.CheckWaveComplete()`](Assets/Scripts/Gameplay/EnemySpawner.cs:60). Перехід у `Defeat` — з [`GameManager.DamageBase()`](Assets/Scripts/Gameplay/GameManager.cs:69), коли `BaseHP` падає до 0.

---

## 3. Структура «префабів» і ScriptableObject-данні

**Особливість проєкту: фізичних префабів і `.asset`-файлів ScriptableObject у репозиторії нема.** Усе створюється кодом у `GameBootstrap.Awake()`. Так зроблено навмисно — щоб мінімізувати залежність від Editor-серіалізації та полегшити балансування числами в одному місці.

### ScriptableObject-класи

**[`EnemyData`](Assets/Scripts/Data/EnemyData.cs:3)** — характеристики ворога:
```csharp
string enemyName;       // Goblin / Orc / Ghost
float  maxHealth;       // HP ворога
float  moveSpeed;       // швидкість руху
int    goldReward;      // золото за вбивство
int    waveCost;        // ціна у бюджеті AI
int    baseDamage;      // урон базі при доходженні
bool   immuneToSlow;    // Ghost — true
Color  enemyColor;      // tint для процедурного fallback-спрайта
```

**[`TowerData`](Assets/Scripts/Data/TowerData.cs:3)** — характеристики вежі:
```csharp
string towerName;       // Archer / Mage / Freezer / Cannon
int    cost;            // ціна побудови
float  attackCooldown;  // секунди між пострілами
float  range;
float  damage;
bool   isAoE;           // Mage — true
float  aoeRadius;       // радіус для AoE
bool   isSlowing;       // Freezer — true
float  slowFactor;      // 0–1 множник до швидкості
float  slowDuration;    // тривалість slow
float  projectileSpeed;
Color  towerColor;
string description;     // tooltip
```

Обидва класи мають `[CreateAssetMenu(menuName = "TowerDefense/...")]`, тож при потребі можна створити `.asset` через меню. Зараз інстанси утворюються через `ScriptableObject.CreateInstance<T>()` у методах [`GameBootstrap.MakeEnemy`](Assets/Scripts/Bootstrap/GameBootstrap.cs:261) і [`MakeTower`](Assets/Scripts/Bootstrap/GameBootstrap.cs:271).

### «Префаби» (динамічно створювані прототипи)

| Тип | Метод створення | Що містить |
|-----|-----------------|------------|
| **Enemy** (3 шт.) | [`CreateEnemyProto`](Assets/Scripts/Bootstrap/GameBootstrap.cs:173) | SpriteRenderer (PNG з Resources або процедурне коло) + `EnemyController` + HP-бар (BG/FG) + literal-символ імені + дочірній `HitFlash` SpriteRenderer |
| **Projectile** (5 пулів) | [`CreateProjectileProto`](Assets/Scripts/Bootstrap/GameBootstrap.cs:200) / [`CreateMageProjectileProto`](Assets/Scripts/Bootstrap/GameBootstrap.cs:211) / [`CreateCustomProjectileProto`](Assets/Scripts/Bootstrap/GameBootstrap.cs:231) | SpriteRenderer + `Projectile`. Mage/Freezer/Cannon — обертаються, Archer-стріла дивиться на ціль (`faceTarget`). |
| **Tower** (4 шт.) | [`TowerPlacer.CreateTowerObject`](Assets/Scripts/Gameplay/TowerPlacer.cs:140) у момент розставлення | SpriteRenderer (PNG або процедурна фігура: rounded-square для Archer, circle для Mage, diamond для Freezer, triangle для Cannon) + `BoxCollider2D` + `TowerController` + literal-символ імені (для fallback-фігур) + дочірня `UpgradeLabel` (TextMesh, "|" за рівень) + `RangeIndicator` (SpriteRenderer з кільцем) |

Дані ворогів і веж задаються прямо у `GameBootstrap.Awake()`, рядки [65-74](Assets/Scripts/Bootstrap/GameBootstrap.cs:65):

```csharp
var goblin = MakeEnemy("Goblin", hp:40, speed:2.5f, gold:6,  cost:10, dmg:1, immune:false, ...);
var orc    = MakeEnemy("Orc",    hp:150,speed:1.2f, gold:15, cost:25, dmg:3, immune:false, ...);
var ghost  = MakeEnemy("Ghost",  hp:80, speed:1.8f, gold:13, cost:20, dmg:2, immune:true,  ...);

var archer  = MakeTower("Archer",  cost:100, cd:1.0f, range:2.5f, dmg:20,  ...);
var mage    = MakeTower("Mage",    cost:150, cd:2.2f, range:2.0f, dmg:35,  aoe:true,  aoeR:1.2f, ...);
var freezer = MakeTower("Freezer", cost:120, cd:1.5f, range:2.5f, dmg:10,  slow:true, slowF:0.40f, slowD:2.5f, ...);
var cannon  = MakeTower("Cannon",  cost:200, cd:3.0f, range:3.5f, dmg:70,  ...);
```

---

## 4. Економіка і формування хвиль

### Економіка

Два паралельних бюджети у [`EconomyManager`](Assets/Scripts/Gameplay/EconomyManager.cs:3):

* **`Gold`** — гроші гравця. Витрачаються на побудову/апгрейд веж. Поповнюються за вбивства (`goldReward` ворога) і за продаж веж (50% від ціни).
* **`AttackerBudget`** — «гроші» AI. Кожна хвиля заповнюється до повного бюджету. Після кожної хвилі бюджет збільшується на `budgetIncrease` ([`IncreaseAttackerBudget`](Assets/Scripts/Gameplay/EconomyManager.cs:31)).

Стартові значення задаються у [`LevelConfig`](Assets/Scripts/Gameplay/LevelManager.cs:98) кожного рівня:

| Рівень | StartGold | StartBudget | BudgetIncrease | PrepDuration | MaxRounds |
|--------|-----------|-------------|----------------|--------------|-----------|
| 1. Forest Path | 400 | 150 | +20 | 35 c | 8  |
| 2. Crossroads  | 300 | 200 | +30 | 30 c | 10 |
| 3. Labyrinth   | 250 | 260 | +40 | 25 c | 12 |

Економіка апгрейдів і продажу — у [`TowerController.UpgradeCost`](Assets/Scripts/Gameplay/TowerController.cs:12) та [`TowerPlacer.SellAt`](Assets/Scripts/Gameplay/TowerPlacer.cs:118):

* **Upgrade cost** = `baseCost × (0.5 + 0.25 × currentLevel)`. Максимум 2 апгрейди → 3 рівні.
* Кожен апгрейд: **+30% range**, **−20% cooldown**.
* **Sell refund** = `baseCost × 0.5` (50% від базової ціни).

### Формування хвиль

[`AIWaveBuilder.BuildWave(budget, round)`](Assets/Scripts/Gameplay/AIWaveBuilder.cs:20) — greedy-алгоритм:

1. Сортує типи ворогів за `waveCost` (зростання).
2. Розблоковує тири за раундом: `maxTierIndex = min(round - 1, типів - 1)`. На раунді 1 — лише Goblin (cost 10); на раунді 2 додається наступний за вартістю; з 3 раунду доступні всі.
3. Кожен крок:
   * Бере афордабельні типи (waveCost ≤ remaining budget).
   * Кидає `Random.value`. Якщо `roll < 0.3 + round × 0.05` → бере **найдорожчого** афордабельного (у пізніх раундах все частіше). Інакше — випадкового з афордабельних.
   * Віднімає `waveCost` від `remaining`, додає ворога в чергу.
4. Зупиняється при `remaining ≤ найменшої waveCost` або при `count >= 50` (хард-кеп розміру хвилі).

Приклад: рівень 3, раунд 5 → бюджет = 260 + 4×40 = 420 золотих, на 5 раунді ймовірність вибрати найдорожчого ~55%, тож формується суміш ~12 Goblin + 4 Orc + 2 Ghost ≈ 420 cost.

---

## 5. Pooling і оптимізація для WebGL

### Object pooling

Власна реалізація у [`PoolManager.cs`](Assets/Scripts/Gameplay/PoolManager.cs:4) (без `UnityEngine.Pool`):

```csharp
Dictionary<string, Queue<GameObject>> pools;     // активний пул
Dictionary<string, GameObject>        prototypes; // прототип для recreate
```

- `CreatePool(key, prototype, initialSize)` — створює `initialSize` неактивних копій прототипу.
- `Get(key)` — якщо черга непорожня, дістає з неї і `SetActive(true)`. Якщо порожня — `Instantiate` нового з прототипу (graceful overflow, ніколи не падає).
- `Return(key, obj)` — `SetActive(false)` + повернення у чергу + `SetParent(pool)`.

Усі пули реєструються у [`GameBootstrap.Awake()`](Assets/Scripts/Bootstrap/GameBootstrap.cs:78), рядки 78-106:

| Pool key | Початковий розмір | Об'єкт |
|----------|-------------------|--------|
| `enemy_Goblin` / `enemy_Orc` / `enemy_Ghost` | 15 кожен | Ворог з усією дочірньою ієрархією |
| `projectile` | 40 | Загальний жовтий снаряд (fallback) |
| `mage_projectile` | 20 | Вогняна куля (з обертанням) |
| `archer_projectile` | 30 | Стріла (faceTarget) |
| `freezer_projectile` | 20 | Крижана куля (rotSpeed 120°/c) |
| `cannon_projectile` | 20 | Ядро (rotSpeed 180°/c) |

Жоден ворог чи снаряд не **знищується** під час раунду — лише деактивується і повертається у пул:
- Ворог: `EnemyController.Finish()` → `PoolManager.Return(...)` ([`EnemyController.cs:222`](Assets/Scripts/Gameplay/EnemyController.cs:222))
- Снаряд: `Projectile.ReturnToPool()` ([`Projectile.cs:90`](Assets/Scripts/Gameplay/Projectile.cs:90))

### Інші оптимізації під WebGL

1. **Compression вимкнено** в [`WebGLBuilder.cs`](Assets/Editor/WebGLBuilder.cs:16): `PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled`. Це збільшує розмір білда ~×3, але дозволяє GitHub Pages віддавати файли без потреби налаштовувати `Content-Encoding` (Pages не вміє).

2. **Процедурні спрайти** ([`SpriteFactory`](Assets/Scripts/Gameplay/SpriteFactory.cs:4)) для всієї карти (12×8 = 96 тайлів — окремі унікальні Texture2D з seed на основі координат) і для всіх fallback-форм веж. Це **збільшує** runtime-memory, але **зменшує** розмір білда (бо в Resources лежить ~12 PNG-картинок замість сотень).

3. **`SpriteLoader.Load`** ([`Assets/Scripts/Gameplay/SpriteLoader.cs:15`](Assets/Scripts/Gameplay/SpriteLoader.cs:15)) має дві стратегії: спочатку `Resources.Load<Sprite>`, потім `Resources.Load<Texture2D>` (з обгортанням у `Sprite.Create`), і третя — `System.IO.File.ReadAllBytes` — обгорнута в `#if UNITY_EDITOR`, тому в WebGL білді її код **повністю видаляється**, бо в браузері нема доступу до файлової системи.

4. **`PlayerPrefs.Save()`** викликається явно у [`GameManager.GoToMainMenu()`](Assets/Scripts/Gameplay/GameManager.cs:90), бо WebGL зберігає prefs у IndexedDB і не flush'ить автоматично.

5. **`TextMesh` (legacy)** замість TextMeshPro — менший runtime overhead і відсутність TMP-assemblies.

6. **Жодних фізичних колайдерів/Rigidbody** на ворогах і снарядах — все рухається `Vector3.MoveTowards`, попадання детектиться через `Vector3.Distance < 0.15f` у [`Projectile.Update`](Assets/Scripts/Gameplay/Projectile.cs:33).

7. **AoE-урон** проходить через `EnemySpawner.ActiveEnemies.ToArray()` (один-раз-копія), а не Physics2D.OverlapCircle — швидше і без allocation у рамках Physics-системи.

8. **Один Update-цикл на сутність**, ніяких корутин для основної логіки руху (корутини використовуються тільки для спавну хвиль і анімацій смерті/спалаху).

---

## 6. Інструкція запуску та збірки WebGL

### Локальний запуск у Editor

1. Встановити **Unity 6000.4.1f1** (або сумісну 6000.4.x) через Unity Hub. Додати модуль **WebGL Build Support**.
2. Клонувати репозиторій:
   ```bash
   git clone https://github.com/Devorius-Nagara/ToweDefenseProj.git
   cd ToweDefenseProj
   ```
3. Відкрити проєкт у Unity Hub → `Open` → вибрати теку.
4. Завантажити сцену `Assets/Scenes/SampleScene.unity`.
5. Натиснути ▶ **Play** — гра запуститься. На сцені лежить лише один GameObject з `GameBootstrap`, решта створюється кодом у `Awake()`.

## 7. Використані ассети та ШІ-генерація

### Зовнішні ассети (під ліцензіями для повторного використання)

Розташовані в [`Assets/Resources/`](Assets/Resources/):

* **Музика** ([`Assets/Resources/Music/`](Assets/Resources/Music/)):
  * `menu_music.mp3` — фонова музика головного меню.
  * `gameplay_music.mp3` — фонова музика гри.

* **PNG-спрайти** ([`Assets/Resources/Sprites/`](Assets/Resources/Sprites/)) — для веж, ворогів, снарядів:
  * Вежі: `archer_tower.png`, `magtower.png`, `freezer_tower.png`, `cannon_tower.png`.
  * Снаряди: `arrow_proj.png`, `magfiresplash.png`, `ice_proj.png`, `cannonball_proj.png`.
  * Вороги: `green-goblin.png`, `blue-goblin.png`, `red-goblin.png`, `goblin-hit.png` (overlay-спалах при попаданні).

### Згенеровано ШІ

* **Спрайти** у `Assets/Resources/Sprites/` — згенеровані помічником на основі промпта про візуальний стиль гри (medieval cartoon, низька деталізація). Власне Python-скрипт-генератор лежить у [`gen_sprites2.py`](gen_sprites2.py) (не виконується під час збірки, лишений як reference).
* **Музика** у `Assets/Resources/Music/` — згенерована музичним ШІ-сервісом (8-bit / medieval ambient).

### Згенеровано процедурно у рантаймі

Через [`SpriteFactory`](Assets/Scripts/Gameplay/SpriteFactory.cs:4) — *без* ШІ, чистий код:

* **Карта** (12×8 = 96 унікальних тайлів): `CreateMedievalGrass(seed)` для трав'яних клітинок, `CreateMedievalCobblestone(seed)` для шляху. Кожна клітинка має власний seed (на основі `col*100 + row`) — текстура унікальна.
* **Геометричні fallback-форми** для веж, коли PNG не завантажились: `CreateRoundedSquare`, `CreateCircle`, `CreateDiamond`, `CreateTriangle`.
* **Кільця радіусу** веж: `CreateRing`.
* **HP-бари** ворогів: пара `CreateSquare` (червоний фон + зелений foreground).

### Unity-пакети / залежності

Стандартні пакети Unity (з `Packages/manifest.json`):

* **Universal Render Pipeline** (URP 2D) — для 2D рендерингу.
* **Input System** (`UnityEngine.InputSystem`) — обробка миші у `TowerPlacer`.
* **TextMeshPro** — встановлений, але не використовується (увесь текст на legacy `TextMesh`).

Жодних сторонніх ассетів з Asset Store не використано.

---

## Ліцензія

Проєкт навчальний. Усі сторонні ассети (музика, базові спрайти) — використовуються в рамках відповідних ліцензій (CC0 / CC-BY або згенеровані ШІ).
