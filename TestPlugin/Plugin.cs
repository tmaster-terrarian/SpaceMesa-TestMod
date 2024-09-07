using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestPlugin;

[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "bscit.testplugin";
    public const string PLUGIN_NAME = "Test Plugin";
    public const string PLUGIN_VERSION = "1.0.0";

    private static readonly List<GameObject> cubes = [];

    private void OnEnable()
    {
        On.PlayerController.Start += OnPlayerController_Start;
        On.PlayerController.Jump += OnPlayerController_Jump;

        SceneManager.activeSceneChanged += Init;

        Logger.Log(LogLevel.Info, "Initialized Hooks");
    }

    private void OnDisable()
    {
        On.PlayerController.Start -= OnPlayerController_Start;
        On.PlayerController.Jump -= OnPlayerController_Jump;

        SceneManager.activeSceneChanged -= Init;
    }

    private void Init(Scene oldScene, Scene newScene)
    {
        cubes.Clear();
    }

    private void OnPlayerController_Start(On.PlayerController.orig_Start orig, PlayerController self)
    {
        orig(self);
        Logger.Log(LogLevel.Info, "Player Started!");

        var playerObj = self.gameObject;
        playerObj.AddComponent<ExposedPlayerData>();

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * 0.5f;
        cube.transform.position = playerObj.transform.position + (Vector3.back * 1) + (Vector3.up * 1.35f);
        cube.transform.SetParent(playerObj.transform, true);

        cubes.Add(cube);

        Physics2D.gravity = new(0, -9.8f);
    }

    private void OnPlayerController_Jump(On.PlayerController.orig_Jump orig, PlayerController self)
    {
        orig(self);
        self.rb.velocity = new(self.rb.velocity.x, 40);
    }

    private void Update()
    {
        foreach(var cube in cubes)
        {
            cube?.transform?.Rotate(10 * Time.deltaTime, -3 * Time.deltaTime, 12 * Time.deltaTime);
        }
    }
}
