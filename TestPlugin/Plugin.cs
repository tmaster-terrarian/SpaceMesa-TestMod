using System.Collections.Generic;
using System.Reflection;
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
        On.PlayerController.FixedUpdate += OnPlayerController_FixedUpdate;
        On.PlayerController.Update += OnPlayerController_Update;
        On.PlayerController.StopWallJumping += OnPlayerController_StopWallJumping;

        SceneManager.activeSceneChanged += OnSceneChange;

        Logger.Log(LogLevel.Info, "Initialized Hooks");
    }

    private void OnDisable()
    {
        On.PlayerController.Start -= OnPlayerController_Start;
        On.PlayerController.Jump -= OnPlayerController_Jump;
        On.PlayerController.FixedUpdate -= OnPlayerController_FixedUpdate;
        On.PlayerController.Update -= OnPlayerController_Update;
        On.PlayerController.StopWallJumping -= OnPlayerController_StopWallJumping;

        SceneManager.activeSceneChanged -= OnSceneChange;
    }

    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        cubes.Clear();
        Physics2D.gravity = new(0, -9.8f);
    }

    private void OnPlayerController_Start(On.PlayerController.orig_Start orig, PlayerController self)
    {
        orig(self);
        Logger.Log(LogLevel.Info, "Player Started!");

        var playerObj = self.gameObject;
        playerObj.AddComponent<ExtraPlayerData>();

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale = Vector3.one * 0.5f;
        cube.transform.position = playerObj.transform.position + (Vector3.back * 1) + (Vector3.up * 1.35f);
        cube.transform.SetParent(playerObj.transform, true);

        cubes.Add(cube);
    }

    private void OnPlayerController_Jump(On.PlayerController.orig_Jump orig, PlayerController self)
    {
        orig(self);
        self.rb.velocity = new(self.rb.velocity.x, 28);

        var data = self.GetComponent<ExtraPlayerData>();
        data.velocityOverride = Vector2.zero;
    }

    private static readonly FieldInfo player_isWallJumpingField =
        typeof(PlayerController).GetField("isWallJumping", BindingFlags.Instance | BindingFlags.NonPublic);

    private void OnPlayerController_StopWallJumping(On.PlayerController.orig_StopWallJumping orig, PlayerController self)
    {
        // if((bool)player_isWallJumpingField.GetValue(self) == true)
        // {
        //     self.GetComponent<ExtraPlayerData>().velocityOverride = Vector2.zero;
        // }
        orig(self);
    }

    private void OnPlayerController_Update(On.PlayerController.orig_Update orig, PlayerController self)
    {
        var data = self.GetComponent<ExtraPlayerData>();

        var wasSliding = self.isWallSliding;

        if((bool)player_isWallJumpingField.GetValue(self) == false && self.isWallSliding && UserInput.instance.JumpJustPressed)
        {
            data.velocityOverride = Vector2.zero;
        }

        orig(self);

        if(wasSliding != self.isWallSliding && data.velocityOverride != Vector2.zero)
        {
            data.velocityOverride = Vector2.zero;
        }

        if((bool)player_isWallJumpingField.GetValue(self) == true && UserInput.instance.JumpJustPressed)
        {
            self.rb.velocity *= Vector2.right * 2.4f + Vector2.up * 0.35f;
        }
    }

    private void OnPlayerController_FixedUpdate(On.PlayerController.orig_FixedUpdate orig, PlayerController self)
    {
        orig(self);

        self.rb.gravityScale = 0;

        float maxSpeed = 18;

        if(self.isGroundSliding)
            maxSpeed = 30;

        if(!self.isDashing)
            self.rb.velocity = new(Mathf.Clamp(self.rb.velocity.x, -maxSpeed, maxSpeed), self.rb.velocity.y);

        var data = self.GetComponent<ExtraPlayerData>();

        if(self.isDashing)
        {
            data.velocityOverride = Vector2.zero;
        }
        else if(self.isWallSliding)
        {
            data.velocityOverride += Vector2.down * (6f * Time.fixedDeltaTime);
            data.velocityOverride = Vector2.ClampMagnitude(data.velocityOverride, 10f);

            self.rb.velocity += data.velocityOverride;
        }
        else if(!data.IsGrounded)
        {
            data.velocityOverride += Vector2.down * (5f * Time.fixedDeltaTime);
            data.velocityOverride = Vector2.ClampMagnitude(data.velocityOverride, 20f);

            self.rb.velocity += data.velocityOverride;
        }

        var oldPos = self.transform.position;
        self.transform.position += (Vector3)self.rb.velocity;

        if(data.IsGrounded && !data.IsJumping)
        {
            data.velocityOverride = Vector2.zero;
        }

        self.transform.position = oldPos;
    }

    private void Update()
    {
        foreach(var cube in cubes)
        {
            cube?.transform?.Rotate(10 * Time.deltaTime, -3 * Time.deltaTime, 12 * Time.deltaTime);
        }
    }
}
