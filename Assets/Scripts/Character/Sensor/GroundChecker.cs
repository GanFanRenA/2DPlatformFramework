using UnityEngine;
using Service.Diagnostics;

/// <summary>
/// 该代码为角色的地面检测组件，负责判断角色是否接触地面
/// </summary>
public class GroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Collider2D sensorCollider;

    public bool IsGrounded { get; private set; }

    [SerializeField] private int contactCount;

    private readonly Collider2D[] _results = new Collider2D[8];

    private void Reset()
    {
        // 第一次挂到物体上时自动抓取本物体的 Collider2D
        sensorCollider = GetComponent<Collider2D>();
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    private void Awake()
    {
        if (sensorCollider == null) sensorCollider = GetComponent<Collider2D>();
        if (sensorCollider == null)
        {
            DebugOutputService.RunNullOptional(gameObject, "Collider2D", "GroundChecker 已禁用");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        contactCount = 0;
        IsGrounded = false;
        RefreshContacts();
    }

    private void OnDisable()
    {
        contactCount = 0;
        IsGrounded = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, groundLayer)) return;
        contactCount++;
        IsGrounded = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsInLayerMask(other.gameObject.layer, groundLayer)) return;
        contactCount = Mathf.Max(0, contactCount - 1);
        IsGrounded = contactCount > 0;
    }

    /// <summary>
    /// 立即检测一次当前重叠的地面碰撞体，修复对象池复用后 IsGrounded 残留为 false 的问题
    /// </summary>
    private void RefreshContacts()
    {
        if (sensorCollider == null) return;

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = groundLayer,
        };

        int count = Physics2D.OverlapCollider(sensorCollider, filter, _results);
        contactCount = count;
        IsGrounded = count > 0;
    }

    private static bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}