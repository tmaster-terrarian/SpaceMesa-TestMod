using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using UnityEngine;

namespace TestPlugin;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "bscit.testplugin";
    public const string PLUGIN_NAME = "Test Plugin";
    public const string PLUGIN_VERSION = "1.0.0";

    private static GameObject playerObj;
    private static PlayerController PlayerController => playerObj.GetComponent<PlayerController>();

    private static GameObject cube;

    private void OnEnable()
    {
        Logger.Log(LogLevel.Info, "Hello World!");

        On.PlayerController.isGrounded += OnPlayerController_isGrounded;
        On.PlayerController.Start += OnPlayerController_Start;
        On.PlayerController.Jump += OnPlayerController_Jump;
    }

    private static bool playerIsGrounded = false;

    private bool OnPlayerController_isGrounded(On.PlayerController.orig_isGrounded orig, PlayerController self)
    {
        playerIsGrounded = orig(self);
        return playerIsGrounded;
    }

    private void OnPlayerController_Start(On.PlayerController.orig_Start orig, PlayerController self)
    {
        orig(self);
        Logger.Log(LogLevel.Info, "Player Started!");

        playerObj = self.gameObject;

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * 0.25f;
        cube.transform.position = playerObj.transform.position + (Vector3.back * 1) + (Vector3.up * 1.35f);
        cube.transform.SetParent(playerObj.transform, true);

        Physics2D.gravity = new(0, -9.8f);
    }

    private void OnPlayerController_Jump(On.PlayerController.orig_Jump orig, PlayerController self)
    {
        orig(self);
        self.rb.velocity = new(self.rb.velocity.x, 40);
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        cube?.transform.Rotate(10 * Time.deltaTime, -3 * Time.deltaTime, 12 * Time.deltaTime);
    }
}
