using UnityEditor;
using UnityEngine;
using System.IO;

public class CharacterCreatorWindow : EditorWindow
{
    // ---- 基本信息 ----
    private string _characterName = "NewCharacter";
    private string _prefabPath = "Assets/Prefabs/Characters/";
    private string _configPath = "Assets/GameData/Characters/";
    private string _layer = "Enemy";
    private string _tag = "Untagged";
    private Vector3 _scale = Vector3.one;

    // ---- 模块开关 ----
    private bool _useMotor = true;
    private bool _useJumper = true;
    private bool _useGroundCheck = true;
    private bool _useHealth = false;
    private bool _useAttacker = false;
    private bool _useDash = false;
    private bool _useHitbox = false;
    private bool _useAnimation = true;

    // ---- 输入 ----
    private InputSourceType _inputSource = InputSourceType.None;

    // ---- 每个模块的 Config 设置 ----
    private ConfigMode _motorMode = ConfigMode.Independent;
    private MovementConfig _motorShared;
    private ConfigMode _jumpMode = ConfigMode.Independent;
    private JumpConfig _jumpShared;
    private ConfigMode _healthMode = ConfigMode.Independent;
    private HealthConfig _healthShared;
    private ConfigMode _attackMode = ConfigMode.Independent;
    private AttackConfig _attackShared;
    private ConfigMode _dashMode = ConfigMode.Independent;
    private DashConfig _dashShared;

    // ---- 物理 ----
    private float _gravityScale = 4f;
    private Vector2 _colliderSize = new Vector2(0.6f, 1.8f);

    private Vector2 _scroll;

    [MenuItem("Tools/Character/Creator")]
    public static void ShowWindow()
    {
        var win = GetWindow<CharacterCreatorWindow>("Character Creator");
        win.minSize = new Vector2(520, 720);
    }

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.LabelField("Character Creator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "填写参数 → 点生成 → 得到独立 Prefab 和 Config SO。\n" +
            "每个模块可选：共享 Config 或生成独立 Config。",
            MessageType.Info);

        DrawBasicSection();
        DrawPresetSection();
        DrawModulesSection();
        DrawPhysicsSection();
        DrawSummarySection();

        EditorGUILayout.Space();
        if (GUILayout.Button("生成 Prefab 及 Config", GUILayout.Height(40)))
            CreateCharacter();

        EditorGUILayout.EndScrollView();
    }

    private void DrawBasicSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);

        _characterName = EditorGUILayout.TextField("名称", _characterName);
        _prefabPath = EditorGUILayout.TextField("Prefab 路径", _prefabPath);
        _configPath = EditorGUILayout.TextField("Config 路径", _configPath);

        int layerId = LayerMask.NameToLayer(_layer);
        int newLayer = EditorGUILayout.LayerField("Layer", layerId >= 0 ? layerId : 0);
        _layer = LayerMask.LayerToName(newLayer);
    }

    private void DrawPresetSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("快速预设", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Player")) ApplyPreset(CharacterPreset.Player);
        if (GUILayout.Button("Enemy Melee")) ApplyPreset(CharacterPreset.EnemyMelee);
        if (GUILayout.Button("Enemy Ranged")) ApplyPreset(CharacterPreset.EnemyRanged);
        if (GUILayout.Button("NPC")) ApplyPreset(CharacterPreset.NPC);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawModulesSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("模块", EditorStyles.boldLabel);

        DrawModuleRow("Motor", ref _useMotor, ref _motorMode, ref _motorShared);
        DrawModuleRow("Jumper", ref _useJumper, ref _jumpMode, ref _jumpShared);
        DrawToggleRow("GroundCheck", ref _useGroundCheck);
        DrawModuleRow("Health", ref _useHealth, ref _healthMode, ref _healthShared);
        DrawModuleRow("Attacker", ref _useAttacker, ref _attackMode, ref _attackShared);
        DrawModuleRow("Dash", ref _useDash, ref _dashMode, ref _dashShared);
        DrawToggleRow("Hitbox", ref _useHitbox);
        DrawToggleRow("Animation", ref _useAnimation);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("输入源", EditorStyles.boldLabel);
        _inputSource = (InputSourceType)EditorGUILayout.EnumPopup("Input", _inputSource);
    }

    private void DrawModuleRow<T>(string label, ref bool enabled, ref ConfigMode mode, ref T shared)
        where T : ScriptableObject
    {
        EditorGUILayout.BeginHorizontal();
        enabled = EditorGUILayout.ToggleLeft(label, enabled, GUILayout.Width(110));

        using (new EditorGUI.DisabledScope(!enabled))
        {
            mode = (ConfigMode)EditorGUILayout.EnumPopup(mode, GUILayout.Width(80));

            if (mode == ConfigMode.Shared)
            {
                shared = (T)EditorGUILayout.ObjectField(shared, typeof(T), false);
            }
            else
            {
                EditorGUILayout.LabelField("生成独立", EditorStyles.miniLabel);
                shared = (T)EditorGUILayout.ObjectField(shared, typeof(T), false);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawToggleRow(string label, ref bool enabled)
    {
        EditorGUILayout.BeginHorizontal();
        enabled = EditorGUILayout.ToggleLeft(label, enabled, GUILayout.Width(110));
        EditorGUILayout.LabelField("", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPhysicsSection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("物理", EditorStyles.boldLabel);
        _gravityScale = EditorGUILayout.FloatField("Gravity Scale", _gravityScale);
        _colliderSize = EditorGUILayout.Vector2Field("Collider Size", _colliderSize);
        _scale = EditorGUILayout.Vector3Field("Root Scale", _scale);
    }

    private void DrawSummarySection()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("将生成的资源", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.LabelField($"Prefab: {_prefabPath}{_characterName}.prefab");

        if (_useMotor && _motorMode == ConfigMode.Independent)
            EditorGUILayout.LabelField($"Config: {_characterName}_MovementConfig.asset");
        if (_useJumper && _jumpMode == ConfigMode.Independent)
            EditorGUILayout.LabelField($"Config: {_characterName}_JumpConfig.asset");
        if (_useHealth && _healthMode == ConfigMode.Independent)
            EditorGUILayout.LabelField($"Config: {_characterName}_HealthConfig.asset");
        if (_useAttacker && _attackMode == ConfigMode.Independent)
            EditorGUILayout.LabelField($"Config: {_characterName}_AttackConfig.asset");
        if (_useDash && _dashMode == ConfigMode.Independent)
            EditorGUILayout.LabelField($"Config: {_characterName}_DashConfig.asset");

        EditorGUI.indentLevel--;
    }

    // ---- 预设 ----
    private void ApplyPreset(CharacterPreset preset)
    {
        switch (preset)
        {
            case CharacterPreset.Player:
                _useMotor = true; _useJumper = true; _useGroundCheck = true;
                _useHealth = true; _useAttacker = true; _useDash = true;
                _useHitbox = true; _useAnimation = true;
                _inputSource = InputSourceType.Player;
                _layer = "Player";
                _motorMode = _jumpMode = _healthMode = _attackMode = _dashMode = ConfigMode.Shared;
                break;

            case CharacterPreset.EnemyMelee:
                _useMotor = true; _useJumper = true; _useGroundCheck = true;
                _useHealth = true; _useAttacker = true; _useDash = false;
                _useHitbox = true; _useAnimation = true;
                _inputSource = InputSourceType.AI;
                _layer = "Enemy";
                _motorMode = _jumpMode = _healthMode = _attackMode = ConfigMode.Independent;
                break;

            case CharacterPreset.EnemyRanged:
                _useMotor = true; _useJumper = false; _useGroundCheck = false;
                _useHealth = true; _useAttacker = true; _useDash = false;
                _useHitbox = false; _useAnimation = true;
                _inputSource = InputSourceType.AI;
                _layer = "Enemy";
                _motorMode = _healthMode = _attackMode = ConfigMode.Independent;
                break;

            case CharacterPreset.NPC:
                _useMotor = false; _useJumper = false; _useGroundCheck = false;
                _useHealth = false; _useAttacker = false; _useDash = false;
                _useHitbox = false; _useAnimation = true;
                _inputSource = InputSourceType.None;
                _layer = "Default";
                break;
        }
    }

    // ---- 生成 ----
    private void CreateCharacter()
    {
        if (string.IsNullOrWhiteSpace(_characterName)) { Warn("名称不能为空"); return; }
        if (!_prefabPath.StartsWith("Assets/")) { Warn("Prefab 路径必须以 Assets/ 开头"); return; }
        if (!_configPath.StartsWith("Assets/")) { Warn("Config 路径必须以 Assets/ 开头"); return; }

        EnsureDirectory(_prefabPath);
        EnsureDirectory(_configPath);

        // 1. 解析每个模块的 Config（共享直接用，独立则复制一份）
        MovementConfig motorCfg = _useMotor ? ResolveConfig<MovementConfig>(_motorMode, _motorShared, "Movement") : null;
        JumpConfig jumpCfg = _useJumper ? ResolveConfig<JumpConfig>(_jumpMode, _jumpShared, "Jump") : null;
        HealthConfig healthCfg = _useHealth ? ResolveConfig<HealthConfig>(_healthMode, _healthShared, "Health") : null;
        AttackConfig attackCfg = _useAttacker ? ResolveConfig<AttackConfig>(_attackMode, _attackShared, "Attack") : null;
        DashConfig dashCfg = _useDash ? ResolveConfig<DashConfig>(_dashMode, _dashShared, "Dash") : null;

        // 2. 创建 CharacterConfig 聚合
        CharacterConfig aggregated = ScriptableObject.CreateInstance<CharacterConfig>();
        aggregated.name = $"{_characterName}_CharacterConfig";
        SetPrivateField(aggregated, "_movement", motorCfg);
        SetPrivateField(aggregated, "_jump", jumpCfg);
        SetPrivateField(aggregated, "_attack", attackCfg);
        SetPrivateField(aggregated, "_health", healthCfg);
        SetPrivateField(aggregated, "_dash", dashCfg);
        AssetDatabase.CreateAsset(aggregated, AssetDatabase.GenerateUniqueAssetPath(
            $"{_configPath.TrimEnd('/')}/{_characterName}_CharacterConfig.asset"));

        // 3. 创建 Prefab
        var root = BuildGameObject(aggregated, attackCfg);

        string prefabFullPath = AssetDatabase.GenerateUniqueAssetPath(
            $"{_prefabPath.TrimEnd('/')}/{_characterName}.prefab");
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabFullPath);
        DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[CharacterCreator] 生成成功: {prefabFullPath}");
    }

    /// <summary>解析 Config：Shared 直接用，Independent 复制一份新的</summary>
    private T ResolveConfig<T>(ConfigMode mode, T shared, string moduleName) where T : ScriptableObject
    {
        if (mode == ConfigMode.Shared)
        {
            if (shared == null)
                Debug.LogWarning($"[CharacterCreator] {moduleName} 使用 Shared 模式但未指定共享 SO，将不生成 Config。");
            return shared;
        }

        // Independent
        T source = shared;
        T instance;
        if (source != null)
            instance = Instantiate(source);
        else
            instance = ScriptableObject.CreateInstance<T>();

        instance.name = $"{_characterName}_{moduleName}Config";
        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{_configPath.TrimEnd('/')}/{_characterName}_{moduleName}Config.asset");
        AssetDatabase.CreateAsset(instance, path);
        return instance;
    }

    private GameObject BuildGameObject(CharacterConfig aggregated, AttackConfig attackCfg)
    {
        var root = new GameObject(_characterName);
        int layerId = LayerMask.NameToLayer(_layer);
        root.layer = layerId >= 0 ? layerId : 0;
        try { root.tag = _tag; } catch { }
        root.transform.localScale = _scale;

        // 物理
        var rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = _gravityScale;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var capsule = root.AddComponent<CapsuleCollider2D>();
        capsule.size = _colliderSize;
        capsule.direction = CapsuleDirection2D.Vertical;

        // Character
        var character = root.AddComponent<Character>();
        SetPrivateField(character, "config", aggregated);

        // 模块
        if (_useMotor) root.AddComponent<CharacterMotor2D>();
        if (_useJumper) root.AddComponent<CharacterJumper2D>();
        if (_useHealth) root.AddComponent<CharacterHealth>();
        if (_useAttacker) root.AddComponent<CharacterAttacker>();
        if (_useDash) root.AddComponent<CharacterDash>();
        if (_useAnimation) root.AddComponent<CharacterAnimationBridge>();
        if (_inputSource == InputSourceType.Player) root.AddComponent<PlayerInput>();

        // Visual
        if (_useAnimation)
        {
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            var anim = visual.AddComponent<Animator>();

            var bridge = root.GetComponent<CharacterAnimationBridge>();
            if (bridge != null)
            {
                SetPrivateField(bridge, "_animator", anim);
                SetPrivateField(bridge, "_spriteRenderer", sr);
            }
        }

        // GroundCheck
        if (_useGroundCheck)
        {
            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(root.transform, false);
            gc.transform.localPosition = new Vector3(0f, -_colliderSize.y * 0.5f, 0f);

            var col = gc.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(_colliderSize.x, 0.1f);

            var checker = gc.AddComponent<GroundChecker>();
            SetPrivateField(checker, "sensorCollider", col);
        }

        // AttackHitbox
        if (_useHitbox)
        {
            var hb = new GameObject("AttackHitbox");
            hb.transform.SetParent(root.transform, false);
            hb.AddComponent<HitboxController>();
        }

        return root;
    }

    // ---- 工具方法 ----
    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static void SetPrivateField(Object target, string fieldName, Object value)
    {
        if (target == null) return;
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }

    private static void Warn(string msg)
    {
        EditorUtility.DisplayDialog("提示", msg, "OK");
    }
}

public enum ConfigMode { Shared, Independent }

public enum InputSourceType { None, Player, AI }

public enum CharacterPreset { Player, EnemyMelee, EnemyRanged, NPC }