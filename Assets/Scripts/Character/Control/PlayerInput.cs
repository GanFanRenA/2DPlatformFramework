using UnityEngine;
using UnityEngine.InputSystem;
using Service.Diagnostics;

/// <summary>
/// 该代码只将玩家输入映射到角色控制组件，不进行任何实际操作
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private InputControl inputControl;
    private Character player;
    private bool _ready;

    private void Awake()
    {
        player = GetComponent<Character>();
        inputControl = new InputControl();

        if (player == null) { DebugOutputService.RunNullFatal(gameObject, nameof(player)); enabled = false; return; }
        if (player.Motor == null) { DebugOutputService.RunNullFatal(gameObject, "Character.Motor"); enabled = false; return; }
        if (player.Jumper == null) { DebugOutputService.RunNullFatal(gameObject, "Character.Jumper"); enabled = false; return; }
        if (inputControl == null) { DebugOutputService.RunNullFatal(gameObject, nameof(inputControl)); enabled = false; return; }

        _ready = true;
    }

    private void OnEnable()
    {
        if (!_ready) return;

        inputControl.Player.Enable();
        inputControl.Player.Move.performed += OnMove;
        inputControl.Player.Move.canceled += OnMove;
        inputControl.Player.Jump.performed += OnJump;
        inputControl.Player.Crouch.performed += OnCrouch;
        inputControl.Player.Crouch.canceled += OnCrouch;
        inputControl.Player.Dash.performed += OnDash;
        inputControl.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        if (!_ready) return;

        inputControl.Player.Move.performed -= OnMove;
        inputControl.Player.Move.canceled -= OnMove;
        inputControl.Player.Jump.performed -= OnJump;
        inputControl.Player.Crouch.performed -= OnCrouch;
        inputControl.Player.Crouch.canceled -= OnCrouch;
        inputControl.Player.Dash.performed -= OnDash;
        inputControl.Player.Attack.performed -= OnAttack;
        inputControl.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (player == null || player.Motor == null) return;
        player.Motor.SetMoveInput(ctx.ReadValue<Vector2>().x);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (player == null || player.Jumper == null) return;
        player.Jumper.RequestJump();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (player == null || player.Attacker == null) return;
        player.Attacker.RequestAttack();
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        // TODO: 蹲下未实现
    }
    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (player == null || player.Dash == null || player.Motor == null) return;
        player.Dash.RequestDash(player.Motor.Facing);
    }
}