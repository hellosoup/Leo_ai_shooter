using RTLOL;
using System.Collections.Generic;

public class GameData
{
    public long Ticks;
    public System.Random Random;
    public ProjectileId NextFreeProjectileId = new ProjectileId { Index = 1 };
    public CharacterId NextFreeCharacterId = new CharacterId { Index = 1 };
    public Queue<PlayerCameraVisual> PlayerCameraVisualPool = new Queue<PlayerCameraVisual>(GameConstant.MaxPlayers);
    public Queue<PlayerVisual> PlayerVisualPool = new Queue<PlayerVisual>(GameConstant.MaxPlayers);
    public Queue<EnemyVisual> EnemyVisualPool = new Queue<EnemyVisual>(GameConstant.MaxEnemies);
    public Queue<ProjectileVisual> ProjectileVisualPool = new Queue<ProjectileVisual>(GameConstant.MaxProjectiles);
    public FixedList<Character> Characters = new FixedList<Character>(GameConstant.MaxCharacters);
    public FixedList<Player> Players = new FixedList<Player>(GameConstant.MaxPlayers);
    public FixedList<Enemy> Enemies = new FixedList<Enemy>(GameConstant.MaxEnemies);
    public FixedList<Projectile> Projectiles = new FixedList<Projectile>(GameConstant.MaxProjectiles);

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
