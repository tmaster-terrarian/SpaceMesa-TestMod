using System.Reflection;
using UnityEngine;

namespace TestPlugin;

public class ExposedPlayerData : MonoBehaviour
{
    private bool _isGrounded;

    public bool IsGrounded => _isGrounded;

    public bool IsJumping {
        get {
            var playerController = gameObject.GetComponent<PlayerController>();
            if(playerController is null) return false;

            return (bool)PlayerHooks.isJumpingField.GetValue(playerController);
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

        bool wasGrounded = component._isGrounded;
        component._isGrounded = orig(self);

        if(component._isGrounded && !wasGrounded)
        {
            PlayerHooks.InvokeLanded(self);
        }

        return component._isGrounded;
    }
}
