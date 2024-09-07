using System.Reflection;
using UnityEngine;

namespace TestPlugin;

public class ExposedPlayerData : MonoBehaviour
{
    private static readonly FieldInfo isJumpingField = typeof(PlayerController).GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic);

    private bool _isGrounded;

    public bool IsGrounded => _isGrounded;

    public bool IsJumping {
        get {
            var playerController = gameObject.GetComponent<PlayerController>();
            if(playerController is null) return false;

            return (bool)isJumpingField.GetValue(playerController);
        }
    }

    private void OnEnable()
    {
        On.PlayerController.isGrounded += PlayerController_isGrounded;
    }

    private void OnDisable()
    {
        On.PlayerController.isGrounded -= PlayerController_isGrounded;
    }

    // Get isGrounded
    private static bool PlayerController_isGrounded(On.PlayerController.orig_isGrounded orig, PlayerController self)
    {
        var component = self.gameObject.GetComponent<ExposedPlayerData>();

        if(component is null)
            return orig(self);

        return component._isGrounded = orig(self);
    }
}
