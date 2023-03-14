using System;
using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    public Player Player;
    public PlayerCameraVisual PlayerCameraVisual;
    public ProjectileVisual ProjectileVisual;
    public PlayerVisual PlayerVisual;
    public EnemyVisual EnemyVisual;
    public float CharacterRadius = 0.5f;
    public float CameraDistance = 10.0f;
    public float CameraAngle = 67.5f;
    public float CameraSmoothTime = 50.0f;
    public float CameraLazyRectWidth = 4.0f;
    public float CameraLazyRectDepth = 2.0f;
    public float ProjectileSpeed = 20.0f;
    public float ProjectileRadius = 0.25f;
    public float ProjectileImpactForce = 1.0f;
    public int ProjectileDamage = 1;
    public float PlayerShootTime = 0.5f;
    public float ProjectileOffsetFromPlayer = 0.5f;
    public float PlayerSnapLookAngleToMoveAngleAfterShootTime = 1.0f;
    public float PlayerShootSpreadAngle = 5.0f;
    public float EnemyFlockRepulseMaxDistance = 3.0f;
    public float EnemyFlockRepulseMaxMagnitude = 3.0f;
    public float EnemyFlockProjectileRepulseMaxDistance = 5.0f;
    public float EnemyFlockProjectileRepulseMaxMagnitude = 20.0f;
    public float EnemyAvoidFoeDistance = 3.0f;
    public float EnemyAvoidFoeMaxMagnitude = 5.0f;
    public CharacterConfig PlayerCharacterConfig;
    public CharacterConfig EnemyCharacterConfig;

    [HideInInspector] public float PlayerSnapLookAngleToMoveAngleAfterShootTicks;
    [HideInInspector] public float PlayerShootSpreadAngleHalf;

    private void OnValidate()
    {
        PlayerSnapLookAngleToMoveAngleAfterShootTicks = (int)(PlayerSnapLookAngleToMoveAngleAfterShootTime * GameConstant.TicksPerSecond);
        PlayerShootSpreadAngleHalf = 0.5f * PlayerShootSpreadAngle;
    }
}
