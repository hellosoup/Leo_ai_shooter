using UnityEngine;

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
    public float MoveAngle;
    public float LookAngle;
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

public struct Player
{
    public CharacterId CharacterId;
    public Vector3 CameraTargetPosition;
    public Vector3 CameraVelocity;
    public PlayerVisual PlayerVisual;
    public PlayerCameraVisual CameraVisual;
}

public struct Enemy
{
    public CharacterId CharacterId;
    public EnemyVisual Visual;
    public bool Remove;
}