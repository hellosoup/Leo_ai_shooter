using UnityEngine;

[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    public ArenaVisual ArenaVisual;
    public PlayerCameraVisual PlayerCameraVisual;
    public ProjectileVisual ProjectileVisual;
    public PlayerVisual PlayerVisual;
    public EnemyVisual EnemyVisual;
    public HudVisual HudVisual;
    public ExplosionVisual ExplosionVisual;
    public CharacterConfig PlayerCharacterConfig;
    public CharacterConfig EnemyCharacterConfig;
    public float CharacterRadius = 0.5f;
    public float CharacterRotateTime = 0.25f;
    public float CameraDistance = 10.0f;
    public float CameraAngle = 67.5f;
    public float CameraSmoothTime = 50.0f;
    public float CameraLazyRectWidth = 4.0f;
    public float CameraLazyRectDepth = 2.0f;
    public float ProjectileSpeed = 20.0f;
    public float ProjectileRadius = 0.25f;
    public float ProjectileImpactForce = 1.0f;
    public int ProjectileDamage = 1;
    public Vector3 ProjectileOffsetFromPlayer = Vector3.zero;
    public float PlayerSnapLookAngleToMoveAngleAfterShootTime = 1.0f;
    public float PlayerShootSpreadAngle = 5.0f;
    public float PlayerRunAnimationSpeed = 0.1f;
    public float PlayerRunAnimationAmplitude = 1.0f;
    public float PlayeRunAnimationDampTime = 0.125f;
    public float EnemyFlockRepulseMaxDistance = 3.0f;
    public float EnemyFlockRepulseMaxMagnitude = 3.0f;
    public float EnemyFlockProjectileRepulseMaxDistance = 5.0f;
    public float EnemyFlockProjectileRepulseMaxMagnitude = 20.0f;
    public float EnemyAvoidFoeDistance = 3.0f;
    public float EnemyAvoidFoeMaxMagnitude = 5.0f;
    public Material[] EnemyVariantMaterials;
    public int InitialEnemyCount = 1;
    public int EnemyCountIncreasePerWave = 1;
    public float WaveCompleteRestTime = 2.0f;
    public float EnemySpawnWaitTime = 1.0f;
    public float MessageTime = 2.0f;
    public float SpawnClearOfGoodyRadius = 15.0f;
    public float SpawnClearOfBaddyRadius = 5.0f;
    public float GameOverTime = 4.0f;
    public float TransitionTime = 1.0f;
    public float EnemyRunAnimationSpeed = 0.1f;
    public float EnemyDeathExplosionOffsetY = 0.0f;

    [HideInInspector] public long PlayerSnapLookAngleToMoveAngleAfterShootTicks;
    [HideInInspector] public float PlayerShootSpreadAngleHalf;
    [HideInInspector] public long MessageTicks;
    [HideInInspector] public long WaveCompleteRestTicks;
    [HideInInspector] public long EnemySpawnWaitTicks;
    [HideInInspector] public float SpawnClearOfGoodyRadiusSquared;
    [HideInInspector] public float SpawnClearOfBaddyRadiusSquared;
    [HideInInspector] public long GameOverTicks;
    [HideInInspector] public long TransitionTicks;

    private void OnValidate()
    {
        PlayerSnapLookAngleToMoveAngleAfterShootTicks = (int)(PlayerSnapLookAngleToMoveAngleAfterShootTime * GameConstant.TicksPerSecond);
        PlayerShootSpreadAngleHalf = 0.5f * PlayerShootSpreadAngle;
        MessageTicks = (int)(MessageTime * GameConstant.TicksPerSecond);
        WaveCompleteRestTicks = (int)(WaveCompleteRestTime * GameConstant.TicksPerSecond);
        EnemySpawnWaitTicks = (int)(EnemySpawnWaitTime * GameConstant.TicksPerSecond);
        SpawnClearOfGoodyRadiusSquared = (SpawnClearOfGoodyRadius * SpawnClearOfGoodyRadius);
        SpawnClearOfBaddyRadiusSquared = (SpawnClearOfBaddyRadius * SpawnClearOfBaddyRadius);
        GameOverTicks = (int)(GameOverTime * GameConstant.TicksPerSecond);
        TransitionTicks = (int)(TransitionTime * GameConstant.TicksPerSecond);
    }
}
