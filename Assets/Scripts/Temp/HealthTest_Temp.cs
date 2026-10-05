using UnityEngine;

public class HealthTestStub : MonoBehaviour
{
    [SerializeField] private CharacterHealth _health;

    private void Start()
    {
        _health.Damaged += amount => Debug.Log($"[Test] 受伤 {amount}，剩余 {_health.CurrentHealth}");
        _health.Died += () => Debug.Log("[Test] 死亡");
        _health.HealthChanged += hp => Debug.Log($"[Test] 血量变化: {hp}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            bool hit = _health.TakeDamage(1, transform.position);
            Debug.Log($"[Test] 按 K，TakeDamage 返回 {hit}");
        }
    }
}