using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Oracle.Prototype
{
    /// <summary>
    /// Builds a self-contained playable prototype at runtime so SampleScene stays easy to open and run.
    /// </summary>
    public sealed class OraclePrototypeBootstrap : MonoBehaviour
    {
        private readonly List<PrototypeUnit> _party = new();
        private PrototypeUnit _selectedUnit;
        private Camera _camera;
        private Material _groundMaterial;
        private Material _roadMaterial;
        private Material _friendlyMaterial;
        private Material _enemyMaterial;
        private Material _supplyMaterial;
        private int _supplies;
        private bool _missionComplete;
        private float _messageUntil;
        private readonly Vector3 _serviceStationEntrance = new(-8f, 1f, 6f);
        private string _message = "抵达北环服务站。搜集补给，避开桥下人。";
        private GUIStyle _headerStyle;
        private GUIStyle _textStyle;
        private GUIStyle _messageStyle;

        private void Start()
        {
            Application.targetFrameRate = 60;
            SetupCamera();
            CreateMaterials();
            BuildDistrict();
            CreateParty();
            CreateEnemyPatrol();
            _selectedUnit = _party[0];
        }

        private void Update()
        {
            HandlePointerInput();
            HandleCameraInput();
            UpdateSupplies();
            UpdateMissionState();
            UpdateCameraFollow();
        }

        private void SetupCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var cameraObject = new GameObject("Prototype Camera");
                cameraObject.tag = "MainCamera";
                _camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            _camera.transform.SetPositionAndRotation(new Vector3(0f, 26f, -19f), Quaternion.Euler(54f, 0f, 0f));
            _camera.orthographic = true;
            _camera.orthographicSize = 18f;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.045f, 0.066f, 0.085f);
        }

        private void CreateMaterials()
        {
            _groundMaterial = CreateMaterial(new Color(0.24f, 0.27f, 0.25f));
            _roadMaterial = CreateMaterial(new Color(0.14f, 0.17f, 0.18f));
            _friendlyMaterial = CreateMaterial(new Color(0.18f, 0.75f, 0.76f));
            _enemyMaterial = CreateMaterial(new Color(0.78f, 0.22f, 0.21f));
            _supplyMaterial = CreateMaterial(new Color(0.95f, 0.67f, 0.18f));
        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            return material;
        }

        private void BuildDistrict()
        {
            CreatePrimitive(PrimitiveType.Plane, "North Ring Ground", Vector3.zero, new Vector3(20f, 1f, 20f), _groundMaterial);
            CreatePrimitive(PrimitiveType.Cube, "Service Road", new Vector3(0f, 0.02f, 0f), new Vector3(48f, 0.08f, 6f), _roadMaterial);
            var roadMarkingMaterial = CreateMaterial(new Color(0.72f, 0.68f, 0.48f));
            for (var x = -21; x <= 21; x += 6)
            {
                CreatePrimitive(PrimitiveType.Cube, "Road Marking", new Vector3(x, 0.071f, 0f), new Vector3(2.2f, 0.012f, 0.12f), roadMarkingMaterial);
            }
            CreatePrimitive(PrimitiveType.Cube, "Service Station", new Vector3(-13f, 1.8f, 9f), new Vector3(9f, 3.6f, 5f), CreateMaterial(new Color(0.12f, 0.31f, 0.37f)));
            CreatePrimitive(PrimitiveType.Cylinder, "Service Station Entrance", _serviceStationEntrance, new Vector3(1.4f, 0.08f, 1.4f), CreateMaterial(new Color(0.20f, 0.85f, 0.78f)));
            CreatePrimitive(PrimitiveType.Cube, "Logistics Warehouse", new Vector3(15f, 2.3f, -9f), new Vector3(10f, 4.6f, 7f), CreateMaterial(new Color(0.27f, 0.25f, 0.21f)));
            CreatePrimitive(PrimitiveType.Cube, "Overpass", new Vector3(8f, 5.2f, 10f), new Vector3(39f, 0.8f, 4f), CreateMaterial(new Color(0.29f, 0.30f, 0.30f)));

            for (var x = -16; x <= 16; x += 8)
            {
                CreatePrimitive(PrimitiveType.Cylinder, "Street Light", new Vector3(x, 2.3f, -3.8f), new Vector3(0.15f, 2.3f, 0.15f), CreateMaterial(new Color(0.35f, 0.35f, 0.31f)));
            }

            CreateSupply(new Vector3(-4f, 0.45f, 8f), "罐头箱");
            CreateSupply(new Vector3(5f, 0.45f, -6f), "医疗包");
            CreateSupply(new Vector3(16f, 0.45f, 4f), "物流补给");
            CreateSupply(new Vector3(-18f, 0.45f, -8f), "废弃背包");
        }

        private void CreateParty()
        {
            _party.Add(CreateUnit("黎明", new Vector3(-10f, 1f, 0f), _friendlyMaterial, false));
            _party.Add(CreateUnit("阿简", new Vector3(-12f, 1f, -2f), CreateMaterial(new Color(0.32f, 0.55f, 0.94f)), false));
        }

        private void CreateEnemyPatrol()
        {
            var patrol = CreateUnit("桥下人", new Vector3(11f, 1f, 2f), _enemyMaterial, true);
            patrol.SetPatrol(new Vector3(-4f, 0.95f, 2f), new Vector3(19f, 0.95f, 2f));
        }

        private PrototypeUnit CreateUnit(string unitName, Vector3 position, Material material, bool hostile)
        {
            var unitObject = CreatePrimitive(PrimitiveType.Capsule, unitName, position, Vector3.one, material);
            var unit = unitObject.AddComponent<PrototypeUnit>();
            unit.Initialize(unitName, hostile, this);
            return unit;
        }

        private void CreateSupply(Vector3 position, string supplyName)
        {
            var crate = CreatePrimitive(PrimitiveType.Cube, supplyName, position, new Vector3(0.85f, 0.85f, 0.85f), _supplyMaterial);
            crate.AddComponent<PrototypeSupply>();
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Material material)
        {
            var created = GameObject.CreatePrimitive(type);
            created.name = objectName;
            created.transform.SetPositionAndRotation(position, Quaternion.identity);
            created.transform.localScale = scale;
            created.GetComponent<Renderer>().sharedMaterial = material;
            return created;
        }

        private void HandlePointerInput()
        {
            var mouse = Mouse.current;
            if (mouse == null || _camera == null)
            {
                return;
            }

            var isMoveCommand = mouse.leftButton.wasPressedThisFrame;
            var isAttackCommand = mouse.rightButton.wasPressedThisFrame;
            if (!isMoveCommand && !isAttackCommand)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 100f))
            {
                return;
            }

            if (hit.collider.TryGetComponent<PrototypeUnit>(out var clickedUnit) && !clickedUnit.IsHostile)
            {
                if (isMoveCommand)
                {
                    _selectedUnit = clickedUnit;
                    SetMessage($"已选择：{clickedUnit.UnitName}");
                }
                return;
            }

            if (isAttackCommand && hit.collider.TryGetComponent<PrototypeUnit>(out var hostile) && hostile.IsHostile)
            {
                _selectedUnit?.SetAttackTarget(hostile);
                return;
            }

            if (isMoveCommand && _selectedUnit != null && !_selectedUnit.IsDown)
            {
                _selectedUnit.SetDestination(hit.point);
            }
        }

        private void HandleCameraInput()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            var scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize - scroll * 0.012f, 11f, 25f);
            }
        }

        private void UpdateMissionState()
        {
            if (_missionComplete || _supplies < 3)
            {
                return;
            }

            var activeMember = GetClosestActivePartyMember(_serviceStationEntrance);
            if (activeMember != null && Vector3.Distance(activeMember.transform.position, _serviceStationEntrance) < 2.5f)
            {
                _missionComplete = true;
                SetMessage("任务完成：补给已送回北环服务站。你们暂时安全了。");
            }
        }

        private void UpdateCameraFollow()
        {
            if (_camera == null || _party.Count == 0)
            {
                return;
            }

            var target = GetClosestActivePartyMember(_camera.transform.position);
            if (target == null)
            {
                return;
            }

            var desired = target.transform.position + new Vector3(0f, 25f, -19f);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, desired, 2.5f * Time.deltaTime);
        }

        private void UpdateSupplies()
        {
            foreach (var supply in FindObjectsByType<PrototypeSupply>(FindObjectsSortMode.None))
            {
                if (supply.IsCollected)
                {
                    continue;
                }

                foreach (var member in _party)
                {
                    if (!member.IsDown && Vector3.Distance(member.transform.position, supply.transform.position) < 1.35f)
                    {
                        supply.Collect();
                        _supplies++;
                        SetMessage($"获得补给：{supply.name}（{_supplies}/3）");
                        break;
                    }
                }
            }
        }

        public PrototypeUnit GetClosestActivePartyMember(Vector3 position)
        {
            PrototypeUnit closest = null;
            var closestDistance = float.MaxValue;
            foreach (var member in _party)
            {
                if (member.IsDown)
                {
                    continue;
                }

                var distance = Vector3.SqrMagnitude(member.transform.position - position);
                if (distance < closestDistance)
                {
                    closest = member;
                    closestDistance = distance;
                }
            }
            return closest;
        }

        public void SetMessage(string message)
        {
            _message = message;
            _messageUntil = Time.time + 4f;
        }

        private void OnGUI()
        {
            EnsureStyles();
            var previousColor = GUI.color;
            var scale = Mathf.Clamp(Screen.width / 960f, 0.65f, 1f);
            var panelWidth = Mathf.Min(340f * scale, Screen.width - 28f);
            var panelHeight = 138f * scale;
            GUI.color = new Color(0.03f, 0.05f, 0.07f, 0.92f);
            GUI.Box(new Rect(14f * scale, 14f * scale, panelWidth, panelHeight), string.Empty);
            GUI.color = Color.white;
            var left = 28f * scale;
            var lineHeight = 25f * scale;
            GUI.Label(new Rect(left, 22f * scale, panelWidth - 28f * scale, lineHeight), "神谕  /  北环封控区", _headerStyle);
            GUI.Label(new Rect(left, 51f * scale, panelWidth - 28f * scale, lineHeight), _missionComplete ? "任务完成：补给线已恢复" : (_supplies >= 3 ? "带补给返回服务站" : "搜集补给：" + _supplies + "/3"), _textStyle);
            GUI.Label(new Rect(left, 78f * scale, panelWidth - 28f * scale, lineHeight), _selectedUnit == null ? "暂无可行动队员" : $"{_selectedUnit.UnitName}  生命 {_selectedUnit.Health:0}  饥饿 {_selectedUnit.Hunger:0}", _textStyle);
            GUI.Label(new Rect(left, 105f * scale, panelWidth - 28f * scale, lineHeight), "左键移动/选人   右键攻击", _textStyle);
            if (_selectedUnit != null)
            {
                GUI.Label(new Rect(Screen.width - 110f * scale, 18f * scale, 96f * scale, 24f * scale), $"队伍 {_party.Count} 人", _textStyle);
            }

            if (Time.time < _messageUntil)
            {
                var messageWidth = Mathf.Min(600f * scale, Screen.width - 28f * scale);
                GUI.color = new Color(0.03f, 0.05f, 0.07f, 0.92f);
                GUI.Box(new Rect(14f * scale, Screen.height - 48f * scale, messageWidth, 34f * scale), string.Empty);
                GUI.color = new Color(0.9f, 0.94f, 0.95f);
                GUI.Label(new Rect(24f * scale, Screen.height - 43f * scale, messageWidth - 20f * scale, 24f * scale), _message, _messageStyle);
            }
            GUI.color = previousColor;
        }

        private void EnsureStyles()
        {
            if (_headerStyle != null)
            {
                return;
            }

            _headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.35f, 0.91f, 0.91f) } };
            _textStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(0.88f, 0.92f, 0.94f) } };
            _messageStyle = new GUIStyle(_textStyle) { fontSize = 13 };
        }
    }
}
