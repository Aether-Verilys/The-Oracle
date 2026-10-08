using UnityEngine;

namespace Oracle.Prototype
{
    public sealed class PrototypeUnit : MonoBehaviour
    {
        private const float MoveSpeed = 5f;
        private const float AttackRange = 1.5f;
        private const float AttackDamage = 8f;
        private const float AttackInterval = 0.8f;
        private const float DetectionRadius = 6.5f;

        private OraclePrototypeBootstrap _world;
        private Vector3 _destination;
        private Vector3 _patrolA;
        private Vector3 _patrolB;
        private bool _hasDestination;
        private bool _patrolToB;
        private float _nextAttackTime;
        private float _hungerTimer;
        private PrototypeUnit _attackTarget;
        private GUIStyle _unitLabelStyle;

        public string UnitName { get; private set; }
        public bool IsHostile { get; private set; }
        public bool IsDown => Health <= 0f;
        public float Health { get; private set; } = 100f;
        public float Hunger { get; private set; } = 100f;

        public void Initialize(string unitName, bool isHostile, OraclePrototypeBootstrap world)
        {
            UnitName = unitName;
            IsHostile = isHostile;
            _world = world;
            _destination = transform.position;
        }

        public void SetPatrol(Vector3 patrolA, Vector3 patrolB)
        {
            _patrolA = patrolA;
            _patrolB = patrolB;
            _destination = patrolB;
            _hasDestination = true;
            _patrolToB = true;
        }

        public void SetDestination(Vector3 destination)
        {
            _attackTarget = null;
            SetMovementDestination(destination);
        }

        public void SetAttackTarget(PrototypeUnit target)
        {
            if (target == null || !target.IsHostile || IsDown)
            {
                return;
            }

            _attackTarget = target;
            _hasDestination = true;
            _world.SetMessage($"{UnitName} 正在攻击 {target.UnitName}。");
        }

        private void Update()
        {
            if (IsDown)
            {
                return;
            }

            if (IsHostile)
            {
                UpdateHostileBehaviour();
            }
            else
            {
                UpdatePlayerCombat();
                _hungerTimer += Time.deltaTime;
                if (_hungerTimer >= 2.5f)
                {
                    Hunger = Mathf.Max(0f, Hunger - 1f);
                    _hungerTimer = 0f;
                }
            }

            MoveTowardsDestination();
        }

        private void UpdatePlayerCombat()
        {
            if (_attackTarget == null || _attackTarget.IsDown)
            {
                _attackTarget = null;
                return;
            }

            var distance = Vector3.Distance(transform.position, _attackTarget.transform.position);
            SetMovementDestination(_attackTarget.transform.position);
            if (distance <= AttackRange && Time.time >= _nextAttackTime)
            {
                _attackTarget.ReceiveDamage(AttackDamage);
                _nextAttackTime = Time.time + AttackInterval;
            }
        }

        private void SetMovementDestination(Vector3 destination)
        {
            _destination = new Vector3(Mathf.Clamp(destination.x, -28f, 28f), 1f, Mathf.Clamp(destination.z, -28f, 28f));
            _hasDestination = true;
        }

        private void UpdateHostileBehaviour()
        {
            var target = _world.GetClosestActivePartyMember(transform.position);
            if (target == null)
            {
                return;
            }

            var distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < DetectionRadius)
            {
                SetDestination(target.transform.position);
                if (distance <= AttackRange && Time.time >= _nextAttackTime)
                {
                    target.ReceiveDamage(AttackDamage);
                    _nextAttackTime = Time.time + AttackInterval;
                }
            }
            else if (!_hasDestination || Vector3.Distance(transform.position, _destination) < 0.25f)
            {
                _patrolToB = !_patrolToB;
                _destination = _patrolToB ? _patrolB : _patrolA;
                _hasDestination = true;
            }
        }

        private void MoveTowardsDestination()
        {
            if (!_hasDestination)
            {
                return;
            }

            var offset = _destination - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.03f)
            {
                _hasDestination = false;
                return;
            }

            transform.position += offset.normalized * MoveSpeed * Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, offset.normalized, 12f * Time.deltaTime);
        }

        private void ReceiveDamage(float damage)
        {
            Health = Mathf.Max(0f, Health - damage);
            if (IsDown)
            {
                transform.localScale = new Vector3(1f, 0.25f, 1f);
                _world.SetMessage(IsHostile ? $"{UnitName} 已被击退。" : $"{UnitName} 倒下了，切换队员继续行动。");
            }
        }

        private void OnGUI()
        {
            if (Camera.main == null)
            {
                return;
            }

            var screenPoint = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.45f);
            if (screenPoint.z < 0f)
            {
                return;
            }

            var rect = new Rect(screenPoint.x - 45f, Screen.height - screenPoint.y, 90f, 36f);
            GUI.color = IsHostile ? new Color(1f, 0.45f, 0.45f) : new Color(0.55f, 1f, 1f);
            if (_unitLabelStyle == null)
            {
                _unitLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 12 };
            }
            GUI.Label(rect, $"{UnitName}\n{Health:0}", _unitLabelStyle);
            GUI.color = Color.white;
        }
    }
}
