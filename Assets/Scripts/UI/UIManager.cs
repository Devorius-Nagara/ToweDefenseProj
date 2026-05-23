using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections.Generic;

/// <summary>
/// Builds and manages ALL UI screens at runtime.
/// Screens: MainMenu · LevelSelect · Statistics · Settings · HUD · TowerShop · RoundEnd · GameOver
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ── Live references ───────────────────────────────────────────────
    private Text txtGold, txtBaseHP, txtRound, txtTimer, txtStatus, txtWaveInfo;
    private Slider sliderVolume;

    // ── Panels ────────────────────────────────────────────────────────
    private GameObject scrMainMenu, scrLevelSelect, scrStats, scrSettings;
    private GameObject panHUD, panTowerShop, panRoundEnd, panGameOver;

    // Round-end / game-over labels
    private Text txtRoundEndBody, txtGameOverTitle, txtGameOverBody;

    private List<TowerData> towerDataList;
    private Font _font;

    void Awake() => Instance = this;

    // ── Entry point called from GameBootstrap (next-frame coroutine) ──
    public void BuildUI(List<TowerData> towers)
    {
        towerDataList = towers;
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 14);
        if (_font == null) _font = Font.CreateDynamicFontFromOSFont(Font.GetOSInstalledFontNames()[0], 14);

        EnsureEventSystem();

        var root = CreateRootCanvas();
        BuildMainMenu(root);
        BuildLevelSelect(root);
        BuildStats(root);
        BuildSettings(root);
        BuildHUD(root);
        BuildTowerShop(root);
        BuildRoundEnd(root);
        BuildGameOver(root);

        ShowScreen(scrMainMenu);
    }

    // ══════════════════════════════════════════════════════════════════
    //  EventSystem
    // ══════════════════════════════════════════════════════════════════
    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    // ══════════════════════════════════════════════════════════════════
    //  Root Canvas
    // ══════════════════════════════════════════════════════════════════
    private Canvas CreateRootCanvas()
    {
        var go = new GameObject("Canvas");
        go.transform.SetParent(transform);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1280, 720);
        scaler.screenMatchMode      = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight   = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    // ══════════════════════════════════════════════════════════════════
    //  MAIN MENU
    // ══════════════════════════════════════════════════════════════════
    private void BuildMainMenu(Canvas root)
    {
        scrMainMenu = FullOverlay(root.transform, "ScrnMainMenu", new Color(0f, 0f, 0f, 0.82f));
        var card = Card(scrMainMenu.transform, 420, 360);

        // Title
        AddLabel(card, "TOWER DEFENSE", 0f, 130f, 400, 50, 40, FontStyle.Bold, new Color(1f, 0.85f, 0.1f));
        AddLabel(card, "Defend your base from enemy waves!", 0f, 90f, 400, 28, 16, FontStyle.Normal, new Color(0.75f, 0.75f, 0.75f));

        // Buttons stacked
        float by = 40f;
        AddBtn(card, "▶  PLAY", 0f, by,       360, 48, new Color(0.20f, 0.65f, 0.20f), OnClickPlay);       by -= 58f;
        AddBtn(card, "🗺  SELECT LEVEL", 0f, by, 360, 48, new Color(0.15f, 0.45f, 0.70f), OnClickLevels);  by -= 58f;
        AddBtn(card, "📊  STATISTICS",   0f, by, 360, 48, new Color(0.45f, 0.25f, 0.65f), OnClickStats);    by -= 58f;
        AddBtn(card, "⚙  SETTINGS",     0f, by, 360, 48, new Color(0.35f, 0.35f, 0.35f), OnClickSettings);

        // Version
        AddLabel(card, "v1.0  |  Tower Defense 2D", 0f, -165f, 400, 22, 12, FontStyle.Normal, new Color(0.4f, 0.4f, 0.4f));
    }

    private void OnClickPlay()
    {
        AudioManager.Instance?.PlayButton();
        GameManager.Instance.StartGame();
    }
    private void OnClickLevels()  { AudioManager.Instance?.PlayButton(); ShowScreen(scrLevelSelect); }
    private void OnClickStats()   { AudioManager.Instance?.PlayButton(); RefreshStats(); ShowScreen(scrStats); }
    private void OnClickSettings(){ AudioManager.Instance?.PlayButton(); ShowScreen(scrSettings); }

    // ══════════════════════════════════════════════════════════════════
    //  LEVEL SELECT
    // ══════════════════════════════════════════════════════════════════
    private void BuildLevelSelect(Canvas root)
    {
        scrLevelSelect = FullOverlay(root.transform, "ScrnLevelSelect", new Color(0f, 0f, 0f, 0.82f));
        var card = Card(scrLevelSelect.transform, 560, 420);

        AddLabel(card, "SELECT LEVEL", 0f, 175f, 520, 40, 30, FontStyle.Bold, Color.white);

        var levels = LevelManager.Levels;
        float startY = 95f;
        for (int i = 0; i < levels.Length; i++)
        {
            var lvl = levels[i];
            int idx = lvl.Index;
            float y = startY - i * 90f;

            // Row background
            var row = SubPanel(card.transform, new Color(0.18f, 0.22f, 0.30f, 0.95f), 0f, y, 520, 78);

            // Level name
            AddLabel(row, $"Level {lvl.Index}: {lvl.Name}", -60f, 18f, 320, 28, 18, FontStyle.Bold,
                lvl.Difficulty == 1 ? new Color(0.3f, 0.9f, 0.3f) :
                lvl.Difficulty == 2 ? new Color(1.0f, 0.8f, 0.1f) :
                                      new Color(1.0f, 0.35f, 0.35f));
            // Description
            AddLabel(row, lvl.Description, -60f, -8f, 320, 22, 12, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f));
            // Stars
            AddLabel(row, lvl.DifficultyStars, -60f, -26f, 320, 22, 13, FontStyle.Normal, Color.yellow);
            // Play button
            var playColor = new Color(0.20f, 0.60f, 0.20f);
            AddBtn(row.transform, "PLAY", 195f, 0f, 88, 52, playColor, () =>
            {
                AudioManager.Instance?.PlayButton();
                LevelManager.Instance.SelectLevel(idx);
                LevelManager.Instance.LoadSelectedLevel();
            });
        }

        AddBtn(card, "◀  BACK", 0f, -180f, 200, 44, new Color(0.3f, 0.3f, 0.3f),
            () => { AudioManager.Instance?.PlayButton(); ShowScreen(scrMainMenu); });
    }

    // ══════════════════════════════════════════════════════════════════
    //  STATISTICS
    // ══════════════════════════════════════════════════════════════════
    private Text[] statValueTexts = new Text[5];
    private readonly string[] statLabels = { "Games Played", "Victories", "Win Rate", "Enemies Killed", "Highest Round" };

    private void BuildStats(Canvas root)
    {
        scrStats = FullOverlay(root.transform, "ScrnStats", new Color(0f, 0f, 0f, 0.82f));
        var card = Card(scrStats.transform, 480, 420);

        AddLabel(card, "STATISTICS", 0f, 175f, 440, 40, 30, FontStyle.Bold, Color.white);

        float y = 105f;
        for (int i = 0; i < statLabels.Length; i++)
        {
            // Label
            AddLabel(card, statLabels[i] + ":", -60f, y, 200, 28, 16, FontStyle.Normal, new Color(0.7f, 0.7f, 0.7f));
            // Value (we keep a reference to update it)
            var vt = AddLabel(card, "—", 120f, y, 160, 28, 18, FontStyle.Bold, Color.white);
            statValueTexts[i] = vt;
            y -= 40f;
        }

        AddBtn(card, "🗑  Reset Statistics", 0f, -155f, 260, 44, new Color(0.65f, 0.15f, 0.15f), () =>
        {
            AudioManager.Instance?.PlayButton();
            StatisticsManager.Instance?.ResetAllTimeStats();
            RefreshStats();
        });

        AddBtn(card, "◀  BACK", 0f, -205f, 200, 44, new Color(0.3f, 0.3f, 0.3f),
            () => { AudioManager.Instance?.PlayButton(); ShowScreen(scrMainMenu); });
    }

    private void RefreshStats()
    {
        if (statValueTexts[0] == null || StatisticsManager.Instance == null) return;
        var s = StatisticsManager.Instance;
        statValueTexts[0].text = s.TotalGamesPlayed.ToString();
        statValueTexts[1].text = s.TotalWins.ToString();
        statValueTexts[2].text = $"{s.WinRate:0}%";
        statValueTexts[3].text = s.TotalKills.ToString();
        statValueTexts[4].text = s.HighestRound.ToString();
    }

    // ══════════════════════════════════════════════════════════════════
    //  SETTINGS
    // ══════════════════════════════════════════════════════════════════
    private void BuildSettings(Canvas root)
    {
        scrSettings = FullOverlay(root.transform, "ScrnSettings", new Color(0f, 0f, 0f, 0.82f));
        var card = Card(scrSettings.transform, 440, 340);

        AddLabel(card, "SETTINGS", 0f, 140f, 400, 40, 30, FontStyle.Bold, Color.white);

        // Volume slider
        AddLabel(card, "SFX Volume", -80f, 65f, 200, 28, 16, FontStyle.Normal, Color.white);
        sliderVolume = CreateSlider(card.transform, 80f, 65f, 200, 28,
            PlayerPrefs.GetFloat("sfx_vol", 0.7f), v =>
            {
                if (AudioManager.Instance) AudioManager.Instance.SFXVolume = v;
                PlayerPrefs.SetFloat("sfx_vol", v);
                PlayerPrefs.Save(); // required for WebGL (writes to browser IndexedDB)
            });

        // Difficulty label
        AddLabel(card, "Difficulty is set per Level in Level Select.", 0f, 10f, 400, 24, 12,
            FontStyle.Normal, new Color(0.6f, 0.6f, 0.6f));

        AddBtn(card, "🗑  Reset Statistics", 0f, -55f, 270, 44, new Color(0.60f, 0.15f, 0.15f), () =>
        {
            AudioManager.Instance?.PlayButton();
            StatisticsManager.Instance?.ResetAllTimeStats();
        });

        AddBtn(card, "◀  BACK", 0f, -110f, 200, 44, new Color(0.30f, 0.30f, 0.30f),
            () => { AudioManager.Instance?.PlayButton(); ShowScreen(scrMainMenu); });
    }

    // ══════════════════════════════════════════════════════════════════
    //  HUD  (top bar — always visible during game)
    // ══════════════════════════════════════════════════════════════════
    private void BuildHUD(Canvas root)
    {
        panHUD = new GameObject("PanHUD");
        panHUD.transform.SetParent(root.transform);
        var rt = panHUD.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(0f, 52f);
        rt.anchoredPosition = Vector2.zero;
        var img = panHUD.AddComponent<Image>();
        img.color = new Color(0.06f, 0.08f, 0.12f, 0.93f);

        // Stretch child anchors so elements fill the HUD bar
        float[] xs = { 0.08f, 0.28f, 0.50f, 0.72f, 0.90f };

        txtGold   = HudText("💰 Gold: 300",       xs[0]);
        txtBaseHP = HudText("❤ HP: 20/20",        xs[1]);
        txtRound  = HudText("Round: 0/10",         xs[2]);
        txtTimer  = HudText("",                    xs[3]);
        // MENU button at far right of HUD bar
        AddHudMenuButton(xs[4]);
        // Status text (bottom of screen)
        txtStatus = HudText("",                    0f, bottom: true);
        // Wave info (just below top bar)
        txtWaveInfo = HudText("",                  0f, belowBar: true);

        panHUD.SetActive(false);
    }

    private Text HudText(string text, float anchorX, bool bottom = false, bool belowBar = false)
    {
        var go = new GameObject("HT_" + text.Substring(0, Mathf.Min(8, text.Length)));
        if (bottom)
        {
            go.transform.SetParent(panHUD.transform.parent); // sibling of HUD, not child
            var rt2 = go.AddComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0f, 0f);
            rt2.anchorMax = new Vector2(1f, 0f);
            rt2.pivot     = new Vector2(0.5f, 0f);
            rt2.sizeDelta = new Vector2(0f, 28f);
            rt2.anchoredPosition = new Vector2(0f, 4f);
            var t2 = go.AddComponent<Text>();
            if (_font != null) t2.font = _font; t2.fontSize = 13; t2.color = Color.white;
            t2.alignment = TextAnchor.LowerCenter; t2.text = text;
            return t2;
        }
        if (belowBar)
        {
            go.transform.SetParent(panHUD.transform.parent);
            var rt2 = go.AddComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0f, 1f);
            rt2.anchorMax = new Vector2(1f, 1f);
            rt2.pivot     = new Vector2(0.5f, 1f);
            rt2.sizeDelta = new Vector2(0f, 26f);
            rt2.anchoredPosition = new Vector2(0f, -54f);
            var t2 = go.AddComponent<Text>();
            if (_font != null) t2.font = _font; t2.fontSize = 13; t2.color = new Color(1f, 0.85f, 0.4f);
            t2.alignment = TextAnchor.UpperCenter; t2.text = text;
            return t2;
        }

        go.transform.SetParent(panHUD.transform);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorX - 0.09f, 0f);
        rt.anchorMax = new Vector2(anchorX + 0.09f, 1f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>();
        if (_font != null) t.font = _font; t.fontSize = 16; t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = 10; t.resizeTextMaxSize = 18;
        t.text = text;
        return t;
    }

    // ══════════════════════════════════════════════════════════════════
    //  TOWER SHOP  (right panel — visible during Prep AND Battle)
    // ══════════════════════════════════════════════════════════════════
    private void BuildTowerShop(Canvas root)
    {
        panTowerShop = new GameObject("PanTowerShop");
        panTowerShop.transform.SetParent(root.transform);
        var rt = panTowerShop.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(165f, 0f);
        rt.anchoredPosition = Vector2.zero;
        panTowerShop.AddComponent<Image>().color = new Color(0.07f, 0.09f, 0.14f, 0.95f);

        // "TOWERS" header
        var hdr = MakeGO("ShopHdr", panTowerShop.transform);
        var hdrRT = hdr.AddComponent<RectTransform>();
        hdrRT.anchorMin = new Vector2(0f, 1f); hdrRT.anchorMax = new Vector2(1f, 1f);
        hdrRT.pivot = new Vector2(0.5f, 1f); hdrRT.sizeDelta = new Vector2(0f, 32f);
        hdrRT.anchoredPosition = Vector2.zero;
        hdr.AddComponent<Image>().color = new Color(0.12f, 0.15f, 0.22f);
        // Text must be on a CHILD — Unity forbids Image+Text on the same GameObject
        var hdrTxtGo = MakeGO("ShopHdrTxt", hdr.transform);
        var hdrTxtRT = hdrTxtGo.AddComponent<RectTransform>();
        hdrTxtRT.anchorMin = Vector2.zero; hdrTxtRT.anchorMax = Vector2.one;
        hdrTxtRT.offsetMin = hdrTxtRT.offsetMax = Vector2.zero;
        var hdrTxt = hdrTxtGo.AddComponent<Text>();
        if (_font != null) hdrTxt.font = _font; hdrTxt.text = "— TOWERS —";
        hdrTxt.fontSize = 13; hdrTxt.color = Color.white; hdrTxt.alignment = TextAnchor.MiddleCenter;

        // Tower buttons
        if (towerDataList != null)
        {
            for (int i = 0; i < towerDataList.Count; i++)
            {
                var data = towerDataList[i];
                int captureI = i;
                float topOffset = -36f - i * 115f;

                // Button
                var btn = MakeGO($"BtnTower_{data.towerName}", panTowerShop.transform);
                var bRT = btn.AddComponent<RectTransform>();
                bRT.anchorMin = new Vector2(0f, 1f); bRT.anchorMax = new Vector2(1f, 1f);
                bRT.pivot = new Vector2(0.5f, 1f); bRT.sizeDelta = new Vector2(-10f, 56f);
                bRT.anchoredPosition = new Vector2(0f, topOffset);
                btn.AddComponent<Image>().color = Color.Lerp(data.towerColor, new Color(0.1f, 0.1f, 0.1f), 0.5f);
                var btnComp = btn.AddComponent<Button>();
                var cs = btnComp.colors;
                cs.highlightedColor = Color.Lerp(data.towerColor, Color.white, 0.3f);
                cs.pressedColor = Color.Lerp(data.towerColor, Color.black, 0.3f);
                btnComp.colors = cs;
                btnComp.onClick.AddListener(() =>
                {
                    AudioManager.Instance?.PlayButton();
                    TowerPlacer.Instance.SelectTower(towerDataList[captureI]);
                });
                // Name + cost
                var nm = MakeText(btn.transform, $"{data.towerName}\n{data.cost}g", 0f, 8f, 150, 40, 14, FontStyle.Bold, data.towerColor);

                // Description
                float descY = topOffset - 58f;
                var desc = MakeGO($"Desc_{data.towerName}", panTowerShop.transform);
                var dRT = desc.AddComponent<RectTransform>();
                dRT.anchorMin = new Vector2(0f, 1f); dRT.anchorMax = new Vector2(1f, 1f);
                dRT.pivot = new Vector2(0.5f, 1f); dRT.sizeDelta = new Vector2(-10f, 50f);
                dRT.anchoredPosition = new Vector2(0f, descY);
                var dt = desc.AddComponent<Text>();
                if (_font != null) dt.font = _font; dt.text = data.description;
                dt.fontSize = 10; dt.color = new Color(0.65f, 0.65f, 0.65f);
                dt.alignment = TextAnchor.UpperCenter;
            }
        }

        // Start Battle button (bottom)
        var sbGO = MakeGO("BtnStartBattle", panTowerShop.transform);
        var sbRT = sbGO.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0f, 0f); sbRT.anchorMax = new Vector2(1f, 0f);
        sbRT.pivot = new Vector2(0.5f, 0f); sbRT.sizeDelta = new Vector2(-8f, 52f);
        sbRT.anchoredPosition = new Vector2(0f, 6f);
        sbGO.AddComponent<Image>().color = new Color(0.75f, 0.18f, 0.18f);
        var sbBtn = sbGO.AddComponent<Button>();
        sbBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButton();
            GameManager.Instance.StartBattle();
        });
        MakeText(sbGO.transform, "▶ START\nBATTLE", 0f, 0f, 155, 48, 14, FontStyle.Bold, Color.white);

        panTowerShop.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  ROUND END
    // ══════════════════════════════════════════════════════════════════
    private void BuildRoundEnd(Canvas root)
    {
        panRoundEnd = FullOverlay(root.transform, "PanRoundEnd", new Color(0f, 0f, 0f, 0.70f));
        var card = Card(panRoundEnd.transform, 430, 250);

        AddLabel(card, "Round Complete!", 0f, 90f, 400, 36, 28, FontStyle.Bold, Color.yellow);
        txtRoundEndBody = AddLabel(card, "", 0f, 20f, 400, 60, 15, FontStyle.Normal, Color.white);
        txtRoundEndBody.alignment = TextAnchor.MiddleCenter;

        AddBtn(card, "▶  NEXT ROUND", 0f, -75f, 220, 50, new Color(0.2f, 0.6f, 0.2f), () =>
        {
            AudioManager.Instance?.PlayButton();
            panRoundEnd.SetActive(false);
            GameManager.Instance.NextRound();
        });

        panRoundEnd.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  GAME OVER
    // ══════════════════════════════════════════════════════════════════
    private void BuildGameOver(Canvas root)
    {
        panGameOver = FullOverlay(root.transform, "PanGameOver", new Color(0f, 0f, 0f, 0.80f));
        var card = Card(panGameOver.transform, 480, 310);

        txtGameOverTitle = AddLabel(card, "",  0f, 115f, 450, 44, 34, FontStyle.Bold, Color.white);
        txtGameOverBody  = AddLabel(card, "",  0f,  40f, 450, 65, 15, FontStyle.Normal, Color.white);
        txtGameOverBody.alignment = TextAnchor.MiddleCenter;

        AddBtn(card, "▶  PLAY AGAIN", -100f, -85f, 180, 48, new Color(0.2f, 0.6f, 0.2f), () =>
        {
            AudioManager.Instance?.PlayButton();
            panGameOver.SetActive(false);
            GameManager.Instance.RestartGame();
        });
        AddBtn(card, "⬅  MAIN MENU", 100f, -85f, 180, 48, new Color(0.3f, 0.3f, 0.3f), () =>
        {
            AudioManager.Instance?.PlayButton();
            panGameOver.SetActive(false);
            panTowerShop.SetActive(false);
            panHUD.SetActive(false);
            GameManager.Instance.GoToMainMenu();
        });

        panGameOver.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  PUBLIC API (called by GameManager)
    // ══════════════════════════════════════════════════════════════════

    public void RefreshHUD()
    {
        if (txtGold   != null) txtGold.text   = $"💰 {EconomyManager.Instance.Gold} g";
        if (txtBaseHP != null) txtBaseHP.text = $"❤ {GameManager.Instance.BaseHP}/{GameManager.Instance.MaxBaseHP}";
        if (txtRound  != null) txtRound.text  = $"Round {GameManager.Instance.CurrentRound}/{GameManager.Instance.MaxRounds}";
    }

    public void SetPrepTimer(float t)
    {
        if (txtTimer) txtTimer.text = t > 0 ? $"⏱ {Mathf.CeilToInt(t)}s" : "";
    }

    public void SetStatusText(string msg) { if (txtStatus) txtStatus.text = msg; }

    public void ShowWaveInfo(int count, int budget)
    {
        if (txtWaveInfo) txtWaveInfo.text = $"Incoming: {count} enemies  |  Attacker budget: {budget}";
    }

    public void OnPreparationStart(int round)
    {
        HideAllScreens();
        if (panHUD)       panHUD.SetActive(true);
        if (panTowerShop) panTowerShop.SetActive(true);
        RefreshHUD();
        SetStatusText("Preparation — place towers (LMB). LMB placed tower: upgrade. RMB placed tower: sell (50%).");
    }

    public void OnBattleStart()
    {
        // Tower shop stays visible so player can buy/sell mid-battle
        if (panTowerShop) panTowerShop.SetActive(true);
        if (txtTimer) txtTimer.text = "";
        if (txtWaveInfo) txtWaveInfo.text = "";
        SetStatusText("Battle! Place towers any time with your remaining gold.");
    }

    public void OnRoundEnd(int round, int baseHP, int gold)
    {
        var s = StatisticsManager.Instance;
        int kills = s?.SessionKills ?? 0;
        txtRoundEndBody.text =
            $"Base HP: {baseHP}/{GameManager.Instance.MaxBaseHP}    Gold: {gold}\n" +
            $"Enemies killed this session: {kills}";
        panRoundEnd.SetActive(true);
    }

    public void OnStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                ShowScreen(scrMainMenu);
                if (panHUD)       panHUD.SetActive(false);
                if (panTowerShop) panTowerShop.SetActive(false);
                break;
            case GameState.Victory:
                ShowGameOver(true);
                break;
            case GameState.Defeat:
                ShowGameOver(false);
                break;
        }
    }

    private void ShowGameOver(bool victory)
    {
        panRoundEnd.SetActive(false);
        panTowerShop.SetActive(false);
        panGameOver.SetActive(true);
        if (panHUD) panHUD.SetActive(true);

        int kills = StatisticsManager.Instance?.SessionKills       ?? 0;
        int gold  = StatisticsManager.Instance?.SessionGoldEarned  ?? 0;

        txtGameOverTitle.text  = victory ? "VICTORY!" : "DEFEAT!";
        txtGameOverTitle.color = victory ? Color.yellow : new Color(1f, 0.32f, 0.32f);
        txtGameOverBody.text   = victory
            ? $"You survived all {GameManager.Instance.MaxRounds} rounds!\nKills: {kills}  |  Gold earned: {gold}"
            : $"Your base was destroyed on round {GameManager.Instance.CurrentRound}.\nKills: {kills}  |  Gold earned: {gold}";

        if (victory) AudioManager.Instance?.PlayVictory();
        else         AudioManager.Instance?.PlayDefeat();

        StatisticsManager.Instance?.OnGameEnd(victory, GameManager.Instance.CurrentRound);
    }

    // ══════════════════════════════════════════════════════════════════
    //  HELPERS
    // ══════════════════════════════════════════════════════════════════

    private void ShowScreen(GameObject screen)
    {
        HideAllScreens();
        if (screen) screen.SetActive(true);
    }

    private void HideAllScreens()
    {
        foreach (var s in new[] { scrMainMenu, scrLevelSelect, scrStats, scrSettings })
            if (s) s.SetActive(false);
    }

    /// <summary>Full-screen semi-transparent overlay panel.</summary>
    private GameObject FullOverlay(Transform parent, string name, Color col)
    {
        var go = MakeGO(name, parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = col;
        return go;
    }

    /// <summary>Centered card panel inside a parent (usually a full-screen overlay).</summary>
    private GameObject Card(Transform parent, float w, float h)
    {
        var go = MakeGO("Card", parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.10f, 0.13f, 0.20f, 0.97f);
        return go;
    }

    /// <summary>Small sub-panel anchored by center position relative to its parent card.</summary>
    private GameObject SubPanel(Transform parent, Color col, float cx, float cy, float w, float h)
    {
        var go = MakeGO("SubPanel", parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(cx, cy);
        go.AddComponent<Image>().color = col;
        return go;
    }

    private Text AddLabel(GameObject parent, string text, float cx, float cy,
        float w, float h, int size, FontStyle style, Color col)
    {
        return MakeText(parent.transform, text, cx, cy, w, h, size, style, col);
    }

    private Text MakeText(Transform parent, string text, float cx, float cy,
        float w, float h, int size, FontStyle style, Color col)
    {
        var go = MakeGO("Txt", parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(cx, cy);
        var t = go.AddComponent<Text>();
        if (_font != null) t.font = _font; t.text = text; t.fontSize = size;
        t.fontStyle = style; t.color = col;
        t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    private void AddBtn(GameObject parent, string label, float cx, float cy,
        float w, float h, Color bg, UnityEngine.Events.UnityAction onClick)
        => AddBtn(parent.transform, label, cx, cy, w, h, bg, onClick);

    private void AddBtn(Transform parent, string label, float cx, float cy,
        float w, float h, Color bg, UnityEngine.Events.UnityAction onClick)
    {
        var go = MakeGO("Btn_" + label.Substring(0, Mathf.Min(8, label.Length)), parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(cx, cy);
        go.AddComponent<Image>().color = bg;
        var btn = go.AddComponent<Button>();
        var cs = btn.colors;
        cs.highlightedColor = Color.Lerp(bg, Color.white, 0.25f);
        cs.pressedColor     = Color.Lerp(bg, Color.black, 0.25f);
        btn.colors = cs;
        btn.onClick.AddListener(onClick);
        MakeText(go.transform, label, 0f, 0f, w - 8f, h - 4f, 15, FontStyle.Bold, Color.white);
    }

    private Slider CreateSlider(Transform parent, float cx, float cy, float w, float h,
        float initVal, UnityEngine.Events.UnityAction<float> onChange)
    {
        var go = MakeGO("Slider", parent);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(cx, cy);

        var slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f;
        slider.value = initVal;

        // Background
        var bg = MakeGO("Background", go.transform);
        var bgRT = bg.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);

        // Fill area
        var fillArea = MakeGO("Fill Area", go.transform);
        var faRT = fillArea.AddComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0f, 0.25f); faRT.anchorMax = new Vector2(1f, 0.75f);
        faRT.offsetMin = new Vector2(5, 0); faRT.offsetMax = new Vector2(-15, 0);
        var fill = MakeGO("Fill", fillArea.transform);
        var fRT = fill.AddComponent<RectTransform>();
        fRT.anchorMin = Vector2.zero; fRT.anchorMax = Vector2.one; fRT.offsetMin = fRT.offsetMax = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.6f, 0.9f);

        // Handle
        var handleArea = MakeGO("Handle Slide Area", go.transform);
        var haRT = handleArea.AddComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one; haRT.offsetMin = haRT.offsetMax = Vector2.zero;
        var handle = MakeGO("Handle", handleArea.transform);
        var hRT = handle.AddComponent<RectTransform>();
        hRT.sizeDelta = new Vector2(20, 0);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect   = fRT;
        slider.handleRect = hRT;
        slider.targetGraphic = handleImg;
        slider.onValueChanged.AddListener(onChange);
        return slider;
    }

    private void AddHudMenuButton(float anchorX)
    {
        var go = MakeGO("HudBtn_Menu", panHUD.transform);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorX - 0.07f, 0.06f);
        rt.anchorMax = new Vector2(anchorX + 0.07f, 0.94f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.55f, 0.12f, 0.12f, 0.92f);
        var btn = go.AddComponent<Button>();
        var cs = btn.colors;
        cs.highlightedColor = new Color(0.75f, 0.22f, 0.22f);
        cs.pressedColor     = new Color(0.35f, 0.08f, 0.08f);
        btn.colors = cs;
        btn.targetGraphic = img;
        btn.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButton();
            if (panRoundEnd  != null) panRoundEnd.SetActive(false);
            if (panTowerShop != null) panTowerShop.SetActive(false);
            if (panHUD       != null) panHUD.SetActive(false);
            GameManager.Instance.GoToMainMenu();
        });
        MakeText(go.transform, "⬅ MENU", 0f, 0f, 130f, 44f, 12, FontStyle.Bold, Color.white);
    }

    private static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }
}
