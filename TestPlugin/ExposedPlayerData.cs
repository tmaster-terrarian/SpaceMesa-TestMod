using System.Reflection;
using UnityEngine;

namespace TestPlugin;

public class ExtraPlayerData : MonoBehaviour
{
    private static readonly MethodInfo isGroundedMethodInfo = typeof(PlayerController).GetMethod("isGrounded", BindingFlags.Instance | BindingFlags.NonPublic);

    public PlayerController PlayerController { get; private set; }

    private bool _isGrounded;

    public Vector2 velocityOverride = Vector2.zero;

    public bool IsGrounded {
        get {
            if(PlayerController is null) return false;

            return (bool)isGroundedMethodInfo.Invoke(PlayerController, []);
        }
    }

    public bool IsJumping {
        get {
            if(PlayerController is null) return false;

            return (bool)PlayerHooks.isJumpingField.GetValue(PlayerController);
        }
    }

    private void OnEnable()
    {
        PlayerController = GetComponent<PlayerController>();

        On.PlayerController.isGrounded += PlayerController_isGrounded;
    }

    private void OnDisable()
    {
        On.PlayerController.isGrounded -= PlayerController_isGrounded;
    }

    // Get isGrounded
    private static bool PlayerController_isGrounded(On.PlayerController.orig_isGrounded orig, PlayerController self)
    {
        var component = self.gameObject.GetComponent<ExtraPlayerData>();

        if(component is null)
            return orig(self);

        bool wasGrounded = component._isGrounded;
        component._isGrounded = orig(self);

        if(component._isGrounded && !wasGrounded && self.rb.velocity.y < -0.1f)
        {
            PlayerHooks.InvokeLanded(self);
        }

        return component._isGrounded;
    }
}
