using System.Reflection;

namespace TestPlugin;

public static class PlayerHooks
{
    public delegate void hook_Landed(PlayerController self);

    public static event hook_Landed Landed;

    internal static void InvokeLanded(PlayerController self) => Landed?.Invoke(self);

    internal static readonly FieldInfo isJumpingField = typeof(PlayerController).GetField("isJumping", BindingFlags.Instance | BindingFlags.NonPublic);
}
