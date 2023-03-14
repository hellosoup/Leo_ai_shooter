using RTLOL;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtil
{
    public static void IncrementTicks(GameData data)
    {
        ++data.Ticks;
    }

    public static void InitRandom(GameData data)
    {
        data.Random = new System.Random();
    }

    public static void InitPlayerCameraVisuals(GameSettings settings, GameData data)
    {
        while (data.PlayerCameraVisualPool.Count < GameConstant.MaxPlayers)
        {
            var visual = Object.Instantiate(settings.PlayerCameraVisual);
            visual.gameObject.SetActive(false);
            data.PlayerCameraVisualPool.Enqueue(visual);
        }
    }

    public static void InitPlayerVisuals(GameSettings settings, GameData data)
    {
        while (data.PlayerVisualPool.Count < GameConstant.MaxPlayers)
        {
            var visual = Object.Instantiate(settings.PlayerVisual);
            visual.gameObject.SetActive(false);
            data.PlayerVisualPool.Enqueue(visual);
        }
    }

    public static void InitEnemyVisuals(GameSettings settings, GameData data)
    {
        while (data.EnemyVisualPool.Count < GameConstant.MaxEnemies)
        {
            var visual = Object.Instantiate(settings.EnemyVisual);
            visual.gameObject.SetActive(false);
            data.EnemyVisualPool.Enqueue(visual);
        }
    }

    public static void InitProjectileVisuals(GameSettings settings, GameData data)
    {
        while (data.ProjectileVisualPool.Count < GameConstant.MaxProjectiles)
        {
            var visual = Object.Instantiate(settings.ProjectileVisual);
            visual.gameObject.SetActive(false);
            data.ProjectileVisualPool.Enqueue(visual);
        }
    }

    public static CharacterId CreateCharacter(GameSettings settings, GameData data, CharacterConfig config, TeamType team, in Vector3 position)
    {
        data.Characters.Add(new Character());
        ref Character character = ref data.Characters[data.Characters.Count - 1];
        character.Config = config;
        character.Id = new CharacterId { Index = data.NextFreeCharacterId.Index++ };
        character.Team = team;
        character.Health = config.Health;
        character.PrevPosition = position;
        character.CurrPosition = position;
        return character.Id;
    }

    public static void CreatePlayer(GameSettings settings, GameData data, in Vector3 position)
    {
        if (data.PlayerVisualPool.Count == 0 || data.PlayerCameraVisualPool.Count == 0 || data.Players.Count >= data.Players.Capacity)
            return;

        data.Players.Add(new Player());
        ref Player player = ref data.Players[data.Players.Count - 1];
        player.PlayerVisual = data.PlayerVisualPool.Dequeue();
        player.CameraVisual = data.PlayerCameraVisualPool.Dequeue();
        player.PlayerVisual.gameObject.SetActive(true);
        player.CameraVisual.gameObject.SetActive(true);
        player.CharacterId = CreateCharacter(settings, data, settings.PlayerCharacterConfig, TeamType.Goodies, position);
    }

    public static void CreateEnemy(GameSettings settings, GameData data, in Vector3 position)
    {
        if (data.EnemyVisualPool.Count == 0 || data.Enemies.Count >= data.Enemies.Capacity)
            return;

        data.Enemies.Add(new Enemy());
        ref Enemy enemy = ref data.Enemies[data.Enemies.Count - 1];
        enemy.Visual = data.EnemyVisualPool.Dequeue();
        enemy.Visual.gameObject.SetActive(true);
        enemy.CharacterId = CreateCharacter(settings, data, settings.EnemyCharacterConfig, TeamType.Baddies, position);
    }

    public static void CreateProjectile(GameData data, TeamType team, in Vector3 position, float angle)
    {
        if (data.ProjectileVisualPool.Count == 0 || data.Projectiles.Count >= data.Projectiles.Capacity)
            return;

        data.Projectiles.Add(new Projectile());
        ref Projectile projectile = ref data.Projectiles[data.Projectiles.Count - 1];
        projectile.PrevPosition = position;
        projectile.CurrPosition = position;
        projectile.Angle = angle;
        projectile.Team = team;
        projectile.StartTicks = data.Ticks;

        projectile.Visual = data.ProjectileVisualPool.Dequeue();
        projectile.Visual.gameObject.SetActive(true);
    }

    public static void TickCharacters(GameSettings settings, GameData data)
    {
        // Accept input
        foreach (ref Character character in data.Characters.AsSpan())
        {
            character.PrevPosition = character.CurrPosition;

            if (character.InputMove.sqrMagnitude >= 0.005f)
            {
                Vector3 move = new Vector3(character.InputMove.x, 0.0f, character.InputMove.y).normalized;
                character.Velocity += move * GameConstant.TickTime * character.Config.Acceleration;
            }

            character.CurrPosition += character.Velocity * GameConstant.TickTime;
            character.Velocity *= character.Config.Drag;

            if (character.InputShoot &&
                (data.Ticks - character.LastShootTicks) >= character.Config.ShootTicks)
            {
                character.LastShootTicks = data.Ticks;

                Vector3 look = Vector3.right;
                if (character.InputLook.sqrMagnitude >= 0.005f)
                    look = new Vector3(character.InputLook.x, 0.0f, character.InputLook.y).normalized;

                Vector3 projectilePosition = character.CurrPosition + look * settings.ProjectileOffsetFromPlayer;
                float lookAngle = Mathf.Atan2(look.z, look.x);

                lookAngle += Mathf.Deg2Rad * Mathf.Lerp(-settings.PlayerShootSpreadAngleHalf, +settings.PlayerShootSpreadAngleHalf, (float)data.Random.NextDouble());
                CreateProjectile(data, character.Team, projectilePosition, lookAngle);
            }
        }

        // Run physics
        {
            var span = data.Characters.AsSpan();
            for (int a = 0; a < span.Length; ++a)
            {
                for (int b = (a + 1); b < span.Length; ++b)
                {
                    ref Character characterA = ref span[a];
                    ref Character characterB = ref span[b];

                    float deltaX = (characterB.CurrPosition.x - characterA.CurrPosition.x);
                    float deltaZ = (characterB.CurrPosition.z - characterA.CurrPosition.z);
                    float dist = Mathf.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
                    float radii = (settings.CharacterRadius + settings.CharacterRadius);
                    float dirX = 1.0f;
                    float dirZ = 0.0f;
                    if (dist >= 0.005f)
                    {
                        dirX = (deltaX / dist);
                        dirZ = (deltaZ / dist);
                    }

                    float pen = Mathf.Max(0.0f, radii - dist);
                    float resX = (0.5f * dirX * pen);
                    float resZ = (0.5f * dirZ * pen);
                    characterA.CurrPosition.x -= resX;
                    characterA.CurrPosition.z -= resZ;
                    characterB.CurrPosition.x += resX;
                    characterB.CurrPosition.z += resZ;

                }
            }
        }

        // Remove
        data.Characters.RemoveAll(character => character.Remove);
    }

    public static void TickEnemies(GameSettings settings, GameData data)
    {
        foreach (ref Enemy enemy in data.Enemies.AsSpan())
        {
            if (data.FindCharacter(enemy.CharacterId, out FixedIterator<Character> found))
            {
                ref Character enemyCharacter = ref found.Value;

                enemyCharacter.InputMove = Vector2.zero;
                enemyCharacter.InputLook = Vector2.zero;
                enemyCharacter.InputShoot = false;

                foreach (ref Character otherCharacter in data.Characters.AsSpan())
                {
                    // Ignore self
                    if (otherCharacter.Id == enemyCharacter.Id)
                        continue;

                    // Target
                    if (otherCharacter.Team != enemyCharacter.Team)
                    {
                        enemyCharacter.InputMove = new Vector2(otherCharacter.CurrPosition.x - enemyCharacter.CurrPosition.x, otherCharacter.CurrPosition.z - enemyCharacter.CurrPosition.z).normalized;
                        enemyCharacter.InputLook = enemyCharacter.InputMove;
                        enemyCharacter.InputShoot = true;
                    }

                    // Avoid friends
                    if (otherCharacter.Team == enemyCharacter.Team)
                    {
                        Vector2 delta = new Vector2(otherCharacter.CurrPosition.x - enemyCharacter.CurrPosition.x, otherCharacter.CurrPosition.z - enemyCharacter.CurrPosition.z);
                        float distance = delta.magnitude;
                        float weight = Mathf.Max(0.0f, (settings.EnemyFlockRepulseMaxDistance - distance) / settings.EnemyFlockRepulseMaxDistance);
                        float mag = weight * settings.EnemyFlockRepulseMaxMagnitude;
                        Vector2 dir = delta.normalized;
                        enemyCharacter.InputMove -= (dir * mag);
                    }

                    // Avoid enemies
                    if (otherCharacter.Team != enemyCharacter.Team)
                    {
                        Vector2 delta = new Vector2(otherCharacter.CurrPosition.x - enemyCharacter.CurrPosition.x, otherCharacter.CurrPosition.z - enemyCharacter.CurrPosition.z);
                        float distance = delta.magnitude;
                        float weight = Mathf.Max(0.0f, (settings.EnemyAvoidFoeDistance - distance) / settings.EnemyAvoidFoeDistance);
                        float mag = weight * settings.EnemyAvoidFoeMaxMagnitude;
                        Vector2 dir = delta.normalized;
                        enemyCharacter.InputMove -= (dir * mag);
                    }
                }

                // Avoid projectiles
                foreach (ref Projectile projectile in data.Projectiles.AsSpan())
                {
                    if (projectile.Team != enemyCharacter.Team)
                    {
                        Vector2 delta = new Vector2(projectile.CurrPosition.x - enemyCharacter.CurrPosition.x, projectile.CurrPosition.z - enemyCharacter.CurrPosition.z);
                        Vector2 projectileDir = new Vector2(Mathf.Cos(projectile.Angle), Mathf.Sin(projectile.Angle));
                        float distance = delta.magnitude;
                        float weight = Mathf.Max(0.0f, (settings.EnemyFlockProjectileRepulseMaxDistance - distance) / settings.EnemyFlockProjectileRepulseMaxDistance);
                        float mag = weight * settings.EnemyFlockProjectileRepulseMaxMagnitude;
                        Vector2 para = new Vector2(projectileDir.y, -projectileDir.x);
                        float dot = Vector2.Dot(para, delta);
                        float sign = Mathf.Sign(dot);
                        Vector2 evade = para * sign;
                        enemyCharacter.InputMove -= (evade * mag);
                    }
                }

                // Die
                if (enemyCharacter.Health <= 0)
                {
                    enemyCharacter.Remove = true;
                    enemy.Remove = true;
                }
            }
        }

        // Die
        data.Enemies.RemoveAll(data, static (GameData data, ref Enemy enemy) =>
        {
            if (enemy.Remove)
            {
                enemy.Visual.gameObject.SetActive(false);
                data.EnemyVisualPool.Enqueue(enemy.Visual);
                return true;
            }
            return false;
        });
    }

    public static void TickProjectiles(GameSettings settings, GameData data)
    {
        // Move projectiles
        foreach (ref Projectile projectile in data.Projectiles.AsSpan())
        {
            projectile.PrevPosition = projectile.CurrPosition;
            Vector3 direction = new Vector3(Mathf.Cos(projectile.Angle), 0.0f, Mathf.Sin(projectile.Angle));
            projectile.CurrPosition += direction * (settings.ProjectileSpeed * GameConstant.TickTime);
        }

        // Collide projectiles with characters
        foreach (ref Character character in data.Characters.AsSpan())
        {
            foreach (ref Projectile projectile in data.Projectiles.AsSpan())
            {
                float deltaX = (character.CurrPosition.x - projectile.CurrPosition.x);
                float deltaZ = (character.CurrPosition.z - projectile.CurrPosition.z);
                float sqDist = (deltaX * deltaX) + (deltaZ * deltaZ);
                float sqRadii = (settings.ProjectileRadius * settings.ProjectileRadius);
                if (sqDist < sqRadii && character.Team != projectile.Team)
                {
                    float dirX = Mathf.Cos(projectile.Angle);
                    float dirZ = Mathf.Sin(projectile.Angle);
                    character.Velocity.x += dirX * settings.ProjectileImpactForce;
                    character.Velocity.z += dirZ * settings.ProjectileImpactForce;
                    character.Health -= settings.ProjectileDamage;
                    projectile.Remove = true;
                }
            }
        }

        // Age out projectiles
        foreach (ref Projectile projectile in data.Projectiles.AsSpan())
        {
            if ((data.Ticks - projectile.StartTicks) > GameConstant.ProjectileLifeticks)
                projectile.Remove = true;
        }

        // Remove projectiles
        data.Projectiles.RemoveAll(data, static (GameData data, ref Projectile projectile) =>
        {
            if (projectile.Remove)
            {
                projectile.Visual.gameObject.SetActive(false);
                data.ProjectileVisualPool.Enqueue(projectile.Visual);
                return true;
            }
            return false;
        });
    }

    public static void UpdatePlayers(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Player player in data.Players.AsSpan())
        {
            if (data.FindCharacter(player.CharacterId, out FixedIterator<Character> iterator))
            {
                ref Character character = ref iterator.Value;

                // Input
                {
                    character.InputMove = Vector3.zero;

                    if (Input.GetKey(KeyCode.A))
                        character.InputMove.x -= 1.0f;
                    if (Input.GetKey(KeyCode.D))
                        character.InputMove.x += 1.0f;
                    if (Input.GetKey(KeyCode.S))
                        character.InputMove.y -= 1.0f;
                    if (Input.GetKey(KeyCode.W))
                        character.InputMove.y += 1.0f;

                    character.InputShoot = Input.GetMouseButton(0);

                    Ray cursorRay = player.CameraVisual.Camera.ScreenPointToRay(Input.mousePosition);
                    Plane cursorPlane = new Plane(Vector3.up, 0.0f);
                    if (cursorPlane.Raycast(cursorRay, out float enter))
                    {
                        Vector3 cursorPosition = cursorRay.origin + (cursorRay.direction * enter);
                        Vector3 playerToCursor = (cursorPosition - character.CurrPosition);
                        character.InputLook = new Vector2(playerToCursor.x, playerToCursor.z);
                    }
                }

                // Player visual
                {
                    player.PlayerVisual.transform.position = Vector3.Lerp(character.PrevPosition, character.CurrPosition, frameT);
                }

                // Camera visual
                {
                    player.CameraVisual.transform.rotation = Quaternion.AngleAxis(settings.CameraAngle, Vector3.right);

                    float xDiff = (player.PlayerVisual.transform.position.x - player.CameraTargetPosition.x);
                    float zDiff = (player.PlayerVisual.transform.position.z - player.CameraTargetPosition.z);
                    float xDist = Mathf.Abs(xDiff);
                    float zDist = Mathf.Abs(zDiff);
                    float xTravel = xDist - settings.CameraLazyRectWidth;
                    float zTravel = zDist - settings.CameraLazyRectDepth;
                    if (xDist > settings.CameraLazyRectWidth)
                        player.CameraTargetPosition.x += xTravel * Mathf.Sign(xDiff);
                    if (zDist > settings.CameraLazyRectDepth)
                        player.CameraTargetPosition.z += zTravel * Mathf.Sign(zDiff);

                    Vector3 moveTo = player.CameraTargetPosition - player.CameraVisual.transform.rotation * Vector3.forward * settings.CameraDistance;
                    player.CameraVisual.transform.position = Vector3.SmoothDamp(player.CameraVisual.transform.position, moveTo, ref player.CameraVelocity, Time.deltaTime * settings.CameraSmoothTime);

                    Debug.DrawLine(player.CameraTargetPosition, player.CameraTargetPosition + Vector3.up, Color.red);
                }
            }
        }
    }

    public static void UpdateEnemies(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Enemy enemy in data.Enemies.AsSpan())
        {
            if (data.FindCharacter(enemy.CharacterId, out FixedIterator<Character> iterator))
            {
                ref Character character = ref iterator.Value;
                enemy.Visual.transform.position = Vector3.Lerp(character.PrevPosition, character.CurrPosition, frameT);
            }
        }
    }

    public static void UpdateProjectiles(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Projectile projectile in data.Projectiles.AsSpan())
        {
            projectile.Visual.transform.position = Vector3.Lerp(projectile.PrevPosition, projectile.CurrPosition, frameT);
            projectile.Visual.transform.rotation = Quaternion.AngleAxis(projectile.Angle * Mathf.Rad2Deg, Vector3.down);
        }
    }

    public static bool TryGetCursorPosition(PlayerCameraVisual playerCameraVisual, out Vector3 cursorPosition)
    {
        Ray ray = playerCameraVisual.Camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, 0.0f);
        bool success = plane.Raycast(ray, out float enter);
        cursorPosition = ray.origin + (ray.direction * enter);
        return success;
    }
}
