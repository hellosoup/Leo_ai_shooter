using UnityEngine;

public static class GameConstant
{
    public const int TicksPerSecond = 30;
    public const float TickTime = (1.0f / TicksPerSecond);
    public const int MaxProjectiles = 256;
    public const int MaxExplosions = 32;
    public const float ProjectileLifetime = 5.0f;
    public const long ProjectileLifeticks = (int)(ProjectileLifetime * TicksPerSecond);
    public const int MaxPlayers = 1;
    public const int MaxEnemies = 64;
    public const int MaxCharacters = (MaxPlayers + MaxEnemies);
    public const int MessagesCapacity = 16;
    public const int MaxWaveCount = 10;
}
