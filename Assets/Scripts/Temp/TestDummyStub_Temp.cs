using UnityEngine;

public class TestDummyStub : MonoBehaviour
{
    [SerializeField] private CharacterHealth _health;

    private void Awake()
    {
        _health.Initialize(_healthConfig, GetComponent<Rigidbody2D>());
        _health.Damaged += amount => Debug.Log($"[Dummy] 被打 {amount}，剩余 {_health.CurrentHealth}");
        _health.Died += () => Debug.Log("[Dummy] 死亡");
    }

    [SerializeField] private HealthConfig _healthConfig;
}