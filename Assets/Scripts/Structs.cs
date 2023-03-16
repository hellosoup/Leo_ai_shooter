using RTLOL;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StageData : IDisposable
{
    public Transform Root;
    public ArenaVisual ArenaVisual;
    public HudVisual Hud;
    public string[] WaveTexts = new string[GameConstant.MaxWaveCount];
    public System.Random Random;
    public ProjectileId NextFreeProjectileId = new ProjectileId { Index = 1 };
    public CharacterId NextFreeCharacterId = new CharacterId { Index = 1 };
    public Queue<PlayerCameraVisual> PlayerCameraVisualPool = new Queue<PlayerCameraVisual>(GameConstant.MaxPlayers);
    public Queue<PlayerVisual> PlayerVisualPool = new Queue<PlayerVisual>(GameConstant.MaxPlayers);
    public Queue<EnemyVisual> EnemyVisualPool = new Queue<EnemyVisual>(GameConstant.MaxEnemies);
    public Queue<ProjectileVisual> ProjectileVisualPool = new Queue<ProjectileVisual>(GameConstant.MaxProjectiles);
    public Queue<ExplosionVisual> ExplosionVisualPool = new Queue<ExplosionVisual>(GameConstant.MaxExplosions);
    public FixedList<PlayerCamera> PlayerCameras = new FixedList<PlayerCamera>(GameConstant.MaxPlayers);
    public FixedList<Character> Characters = new FixedList<Character>(GameConstant.MaxCharacters);
    public FixedList<Player> Players = new FixedList<Player>(GameConstant.MaxPlayers);
    public FixedList<Enemy> Enemies = new FixedList<Enemy>(GameConstant.MaxEnemies);
    public FixedList<Projectile> Projectiles = new FixedList<Projectile>(GameConstant.MaxProjectiles);
    public FixedList<Explosion> Explosions = new FixedList<Explosion>(GameConstant.MaxExplosions);
    public Queue<string> Messages = new Queue<string>(GameConstant.MessagesCapacity);
    public FixedList<Wall> Walls = new FixedList<Wall>(GameConstant.MaxWalls);

    public GameStateType GameState;
    public long Ticks;
    public long NextMessageTicks;
    public long RestCompleteTicks;
    public bool InWave;
    public int Wave;
    public int EnemySpawnsQueued;
    public long NextEnemySpawnTicks;

    public StageData(GameSettings settings)
    {
        Random = new System.Random();

        Root = new GameObject("Stage").transform;

        ArenaVisual = UnityEngine.Object.Instantiate(settings.ArenaVisual, Root);
        Hud = UnityEngine.Object.Instantiate(settings.HudVisual, Root);

        for (int i = 0; i < GameConstant.MaxWaveCount - 1; ++i)
            WaveTexts[i] = $"Wave {i + 1}";
        WaveTexts[GameConstant.MaxWaveCount - 1] = "Final Wave";

        while (PlayerCameraVisualPool.Count < GameConstant.MaxPlayers)
        {
            var visual = UnityEngine.Object.Instantiate(settings.PlayerCameraVisual, Root);
            visual.gameObject.SetActive(false);
            PlayerCameraVisualPool.Enqueue(visual);
        }

        while (PlayerVisualPool.Count < GameConstant.MaxPlayers)
        {
            var visual = UnityEngine.Object.Instantiate(settings.PlayerVisual, Root);
            visual.gameObject.SetActive(false);
            PlayerVisualPool.Enqueue(visual);
        }

        while (EnemyVisualPool.Count < GameConstant.MaxEnemies)
        {
            var visual = UnityEngine.Object.Instantiate(settings.EnemyVisual, Root);
            visual.gameObject.SetActive(false);
            EnemyVisualPool.Enqueue(visual);
        }

        while (ProjectileVisualPool.Count < GameConstant.MaxProjectiles)
        {
            var visual = UnityEngine.Object.Instantiate(settings.ProjectileVisual, Root);
            visual.gameObject.SetActive(false);
            ProjectileVisualPool.Enqueue(visual);
        }

        while (ExplosionVisualPool.Count < GameConstant.MaxExplosions)
        {
            var visual = UnityEngine.Object.Instantiate(settings.ExplosionVisual, Root);
            visual.gameObject.SetActive(false);
            ExplosionVisualPool.Enqueue(visual);
        }

        foreach (ref ArenaVisual.Obstacle obstacle in ArenaVisual.Obstacles.AsSpan())
        {
            for (int i = 0; i < obstacle.Points.Length; ++i)
            {
                Walls.Add(new Wall
                {
                    A = obstacle.Origin + obstacle.Points[i],
                    B = obstacle.Origin + obstacle.Points[(i + 1) % obstacle.Points.Length],
                });
            }
        }
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(Root.gameObject);
        GC.Collect();
    }

    public bool FindCharacter(CharacterId id, out FixedIterator<Character> found)
    {
        foreach (FixedIterator<Character> iterator in Characters)
        {
            if (iterator.Value.Id == id)
            {
                found = iterator;
                return true;
            }
        }
        found = default;
        return false;
    }
}

public struct Character
{
    public CharacterConfig Config;
    public CharacterId Id;
    public Vector2 InputMove;
    public Vector2 InputLook;
    public bool InputShoot;
    public Vector3 PrevPosition;
    public Vector3 CurrPosition;
    public Vector3 Velocity;
    public long LastShootTicks;
    public float PrevLookAngle;
    public float CurrLookAngle;
    public float MoveAngleVelocity;
    public TeamType Team;
    public int Health;
    public bool Remove;
}

public struct Projectile
{
    public Vector3 PrevPosition;
    public Vector3 CurrPosition;
    public float Angle;
    public long StartTicks;
    public ProjectileVisual Visual;
    public TeamType Team;
    public bool Remove;
}

public struct Explosion
{
    public ExplosionVisual Visual;
}

public struct Player
{
    public CharacterId CharacterId;
    public PlayerVisual Visual;
    public bool Remove;
}

public struct PlayerCamera
{
    public CharacterId CharacterId;
    public Vector3 TargetPosition;
    public Vector3 Velocity;
    public PlayerCameraVisual Visual;
}

public struct Enemy
{
    public CharacterId CharacterId;
    public EnemyVisual Visual;
    public bool Remove;
}

public struct Message
{
    public int Start;
    public int Count;
}

public struct Wall
{
    public Vector2 A;
    public Vector2 B;
}