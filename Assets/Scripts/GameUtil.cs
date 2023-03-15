using RTLOL;
using UnityEngine;

public static class GameUtil
{
    public static void IncrementTicks(GameData data)
    {
        ++data.Stage.Ticks;
    }

    public static CharacterId CreateCharacter(GameSettings settings, GameData data, CharacterConfig config, TeamType team, in Vector3 position)
    {
        data.Stage.Characters.Add(new Character());
        ref Character character = ref data.Stage.Characters[data.Stage.Characters.Count - 1];
        character.Config = config;
        character.Id = new CharacterId { Index = data.Stage.NextFreeCharacterId.Index++ };
        character.Team = team;
        character.Health = config.Health;
        character.PrevPosition = position;
        character.CurrPosition = position;
        return character.Id;
    }

    public static bool TryCreatePlayer(GameSettings settings, GameData data, in Vector3 position, out CharacterId characterId)
    {
        if (data.Stage.PlayerVisualPool.Count == 0 || data.Stage.Players.Count >= data.Stage.Players.Capacity)
        {
            characterId = default;
            return false;
        }

        data.Stage.Players.Add(new Player());
        ref Player player = ref data.Stage.Players[data.Stage.Players.Count - 1];
        player.Visual = data.Stage.PlayerVisualPool.Dequeue();
        player.Visual.gameObject.SetActive(true);
        player.CharacterId = CreateCharacter(settings, data, settings.PlayerCharacterConfig, TeamType.Goodies, position);
        characterId = player.CharacterId;
        return true;
    }

    public static void CreatePlayerCamera(GameSettings settings, GameData data, in Vector3 position)
    {
        if (data.Stage.PlayerCameraVisualPool.Count == 0 || data.Stage.PlayerCameras.Count >= data.Stage.PlayerCameras.Capacity)
            return;

        data.Stage.PlayerCameras.Add(new PlayerCamera());
        ref PlayerCamera camera = ref data.Stage.PlayerCameras[data.Stage.PlayerCameras.Count - 1];
        camera.Visual = data.Stage.PlayerCameraVisualPool.Dequeue();
        camera.Visual.gameObject.SetActive(true);
        camera.Visual.transform.position = position;
    }

    public static void CreateEnemy(GameSettings settings, GameData data, in Vector3 position)
    {
        if (data.Stage.EnemyVisualPool.Count == 0 || data.Stage.Enemies.Count >= data.Stage.Enemies.Capacity)
            return;

        data.Stage.Enemies.Add(new Enemy());
        ref Enemy enemy = ref data.Stage.Enemies[data.Stage.Enemies.Count - 1];
        enemy.Visual = data.Stage.EnemyVisualPool.Dequeue();
        enemy.Visual.gameObject.SetActive(true);
        enemy.CharacterId = CreateCharacter(settings, data, settings.EnemyCharacterConfig, TeamType.Baddies, position);
    }

    public static void CreateProjectile(GameData data, TeamType team, in Vector3 position, float angle)
    {
        if (data.Stage.ProjectileVisualPool.Count == 0 || data.Stage.Projectiles.Count >= data.Stage.Projectiles.Capacity)
            return;

        data.Stage.Projectiles.Add(new Projectile());
        ref Projectile projectile = ref data.Stage.Projectiles[data.Stage.Projectiles.Count - 1];
        projectile.PrevPosition = position;
        projectile.CurrPosition = position;
        projectile.Angle = angle;
        projectile.Team = team;
        projectile.StartTicks = data.Stage.Ticks;

        projectile.Visual = data.Stage.ProjectileVisualPool.Dequeue();
        projectile.Visual.gameObject.SetActive(true);
    }

    public static void TickCharacters(GameSettings settings, GameData data)
    {
        // Accept input
        foreach (ref Character character in data.Stage.Characters.AsSpan())
        {
            character.PrevPosition = character.CurrPosition;
            character.PrevMoveAngle = character.CurrMoveAngle;

            if (character.InputMove.sqrMagnitude >= 0.005f)
            {
                Vector3 move = new Vector3(character.InputMove.x, 0.0f, character.InputMove.y).normalized;
                character.Velocity += move * GameConstant.TickTime * character.Config.Acceleration;
            }

            character.CurrPosition += character.Velocity * GameConstant.TickTime;
            character.Velocity *= character.Config.Drag;

            if (character.InputLook.sqrMagnitude >= 0.005f)
                character.LookAngle = Mathf.Atan2(character.InputLook.y, character.InputLook.x);

            if (character.InputMove.sqrMagnitude >= 0.005f)
            {
                float targetAngle = Mathf.Atan2(character.InputMove.y, character.InputMove.x) * Mathf.Rad2Deg;
                character.CurrMoveAngle = Mathf.Deg2Rad * Mathf.SmoothDampAngle(character.CurrMoveAngle * Mathf.Rad2Deg, targetAngle, ref character.MoveAngleVelocity, settings.CharacterRotateTime, Mathf.Infinity, GameConstant.TickTime);
            }

            if (character.InputShoot &&
                (data.Stage.Ticks - character.LastShootTicks) >= character.Config.ShootTicks)
            {
                character.LastShootTicks = data.Stage.Ticks;

                Vector3 look = new Vector3(Mathf.Cos(character.LookAngle), 0.0f, Mathf.Sin(character.LookAngle));
                Vector3 projectilePosition = character.CurrPosition + look * settings.ProjectileOffsetFromPlayer;
                float lookAngle = Mathf.Atan2(look.z, look.x);

                lookAngle += Mathf.Deg2Rad * Mathf.Lerp(-settings.PlayerShootSpreadAngleHalf, +settings.PlayerShootSpreadAngleHalf, (float)data.Stage.Random.NextDouble());
                CreateProjectile(data, character.Team, projectilePosition, lookAngle);
            }
        }

        // Run physics
        {
            var span = data.Stage.Characters.AsSpan();
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
        data.Stage.Characters.RemoveAll(character => character.Remove);
    }

    public static void TickPlayers(GameSettings settings, GameData data)
    {
        foreach (ref Player player in data.Stage.Players.AsSpan())
        {
            if (data.Stage.FindCharacter(player.CharacterId, out FixedIterator<Character> found))
            {
                ref Character playerCharacter = ref found.Value;
                if (playerCharacter.Health <= 0)
                {
                    player.Remove = true;
                    playerCharacter.Remove = true;
                }
            }
        }

        data.Stage.Players.RemoveAll(data, static (GameData data, ref Player player) =>
        {
            if (player.Remove)
            {
                player.Visual.gameObject.SetActive(false);
                data.Stage.PlayerVisualPool.Enqueue(player.Visual);
                return true;
            }
            return false;
        });
    }

    public static void TickEnemies(GameSettings settings, GameData data)
    {
        foreach (ref Enemy enemy in data.Stage.Enemies.AsSpan())
        {
            if (data.Stage.FindCharacter(enemy.CharacterId, out FixedIterator<Character> found))
            {
                ref Character enemyCharacter = ref found.Value;

                enemyCharacter.InputMove = Vector2.zero;
                enemyCharacter.InputLook = Vector2.zero;
                enemyCharacter.InputShoot = false;

                foreach (ref Character otherCharacter in data.Stage.Characters.AsSpan())
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
                foreach (ref Projectile projectile in data.Stage.Projectiles.AsSpan())
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

        // Remove
        data.Stage.Enemies.RemoveAll(data, static (GameData data, ref Enemy enemy) =>
        {
            if (enemy.Remove)
            {
                enemy.Visual.gameObject.SetActive(false);
                data.Stage.EnemyVisualPool.Enqueue(enemy.Visual);
                return true;
            }
            return false;
        });
    }

    public static void TickProjectiles(GameSettings settings, GameData data)
    {
        // Collide projectiles with characters
        foreach (ref Character character in data.Stage.Characters.AsSpan())
        {
            foreach (ref Projectile projectile in data.Stage.Projectiles.AsSpan())
            {
                if (character.Team != projectile.Team &&
                    DoesCapsuleIntersectSphere(projectile.PrevPosition.GetV2(), projectile.CurrPosition.GetV2(), settings.ProjectileRadius, character.CurrPosition.GetV2(), settings.CharacterRadius))
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

        // Move projectiles
        foreach (ref Projectile projectile in data.Stage.Projectiles.AsSpan())
        {
            projectile.PrevPosition = projectile.CurrPosition;
            Vector3 direction = new Vector3(Mathf.Cos(projectile.Angle), 0.0f, Mathf.Sin(projectile.Angle));
            projectile.CurrPosition += direction * (settings.ProjectileSpeed * GameConstant.TickTime);
        }

        // Age out projectiles
        foreach (ref Projectile projectile in data.Stage.Projectiles.AsSpan())
        {
            if ((data.Stage.Ticks - projectile.StartTicks) > GameConstant.ProjectileLifeticks)
                projectile.Remove = true;
        }

        // Remove projectiles
        data.Stage.Projectiles.RemoveAll(data, static (GameData data, ref Projectile projectile) =>
        {
            if (projectile.Remove)
            {
                projectile.Visual.gameObject.SetActive(false);
                data.Stage.ProjectileVisualPool.Enqueue(projectile.Visual);
                return true;
            }
            return false;
        });
    }

    public static void TickWave(GameSettings settings, GameData data)
    {
        int baddyCount = 0;
        foreach (ref Character character in data.Stage.Characters.AsSpan())
        {
            if (character.Team == TeamType.Baddies)
                ++baddyCount;
        }

        if (!data.Stage.InWave && data.Stage.Ticks >= data.Stage.RestCompleteTicks && data.Stage.Wave < GameConstant.MaxWaveCount)
        {
            data.Stage.InWave = true;
            data.Stage.EnemySpawnsQueued += (settings.InitialEnemyCount + data.Stage.Wave * settings.EnemyCountIncreasePerWave);
            data.Stage.NextEnemySpawnTicks = (data.Stage.Ticks + settings.WaveCompleteRestTicks);
            data.Stage.Messages.Enqueue(data.Stage.WaveTexts[data.Stage.Wave]);
        }

        if (data.Stage.InWave && baddyCount == 0 && data.Stage.EnemySpawnsQueued == 0)
        {
            data.Stage.InWave = false;

            ++data.Stage.Wave;
            data.Stage.RestCompleteTicks = (data.Stage.Ticks + settings.WaveCompleteRestTicks);

            if (data.Stage.Wave < GameConstant.MaxWaveCount)
                data.Stage.Messages.Enqueue("Wave Clear");
            else
                data.Stage.Messages.Enqueue("You Win");
        }

        if (data.Stage.InWave && data.Stage.EnemySpawnsQueued > 0 && data.Stage.Ticks >= data.Stage.NextEnemySpawnTicks)
        {
            Transform spawnPoint = data.Stage.ArenaVisual.EnemySpawnPoints[data.Stage.Random.Next() % data.Stage.ArenaVisual.EnemySpawnPoints.Length];

            bool isSpawnPointFree = true;
            foreach (ref Character character in data.Stage.Characters.AsSpan())
            {
                float clearRadiusSquared = 0.0f;

                if (character.Team == TeamType.Goodies)
                    clearRadiusSquared = settings.SpawnClearOfGoodyRadiusSquared;

                if (character.Team == TeamType.Baddies)
                    clearRadiusSquared = settings.SpawnClearOfBaddyRadius;

                Vector2 delta = new Vector2
                {
                    x = character.CurrPosition.x - spawnPoint.transform.position.x,
                    y = character.CurrPosition.z - spawnPoint.transform.position.z,
                };

                if (delta.sqrMagnitude < clearRadiusSquared)
                    isSpawnPointFree = false;
            }

            if (isSpawnPointFree)
            {
                data.Stage.NextEnemySpawnTicks = (data.Stage.Ticks + settings.EnemySpawnWaitTicks);
                --data.Stage.EnemySpawnsQueued;
                CreateEnemy(settings, data, spawnPoint.transform.position);
            }
        }
    }

    public static void TickMessages(GameSettings settings, GameData data)
    {
        if (data.Stage.Messages.Count > 0 && data.Stage.Ticks >= data.Stage.NextMessageTicks)
        {
            data.Stage.NextMessageTicks = data.Stage.Ticks + settings.MessageTicks;
            data.Stage.Hud.MessageTextMesh.text = data.Stage.Messages.Dequeue();
        }

        data.Stage.Hud.MessageTextMesh.enabled = (data.Stage.Ticks < data.Stage.NextMessageTicks);
    }

    public static void TickGameState(GameSettings settings, GameData data)
    {
        switch (data.Stage.GameState)
        {
            case GameStateType.AwaitGameStart:
                TickGameState_AwaitGameStart(settings, data);
                break;

            case GameStateType.InGame:
                TickGameState_InGame(settings, data);
                break;

            case GameStateType.GameOver:
                TickGameState_GameOver(settings, data);
                break;

            case GameStateType.TransitionOutOfGameOver:
                TickGameState_TransitionFromGameOverToAwaitGameStart_Out(settings, data);
                break;

            case GameStateType.TransitionInToAwaitGameStart:
                TickGameState_TransitionFromGameOverToAwaitGameStart_In(settings, data);
                break;

            default:
                Debug.Log($"Unhandlded {nameof(GameStateType)} {data.Stage.GameState}");
                break;
        }
    }

    public static void TickGameState_AwaitGameStart(GameSettings settings, GameData data)
    {
        if (Input.GetKey(KeyCode.Space) &&
                TryCreatePlayer(settings, data, data.Stage.ArenaVisual.PlayerSpawnPoint.position, out CharacterId characterId))
        {
            SetGameState(data, GameStateType.InGame);
            data.Stage.Hud.GameStart.SetActive(false);
            data.Stage.PlayerCameras[0].CharacterId = characterId;
        }
    }

    public static void TickGameState_InGame(GameSettings settings, GameData data)
    {
        if (data.Stage.Players.Count == 0)
        {
            SetGameState(data, GameStateType.GameOver);
            data.Stage.Hud.GameOver.SetActive(true);
        }
    }

    public static void TickGameState_GameOver(GameSettings settings, GameData data)
    {
        if (data.Stage.Ticks >= settings.GameOverTicks)
        {
            SetGameState(data, GameStateType.TransitionOutOfGameOver);
            data.Stage.Hud.TransitionImage.enabled = true;
        }
    }

    public static void TickGameState_TransitionFromGameOverToAwaitGameStart_Out(GameSettings settings, GameData data)
    {
        if (data.Stage.Ticks >= settings.TransitionTicks / 2)
        {
            data.Stage.Hud.GameOver.SetActive(false);
            data.Stage.Dispose();
            data.Stage = new StageData(settings);
            TransitionToAwaitGameStart(settings, data);
        }
    }

    public static void TickGameState_TransitionFromGameOverToAwaitGameStart_In(GameSettings settings, GameData data)
    {
        if (data.Stage.Ticks >= settings.TransitionTicks / 2)
        {
            SetGameState(data, GameStateType.AwaitGameStart);
            data.Stage.Hud.TransitionImage.enabled = false;
            data.Stage.Hud.GameStart.SetActive(true);
        }
    }

    public static void UpdatePlayers(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Player player in data.Stage.Players.AsSpan())
        {
            if (data.Stage.FindCharacter(player.CharacterId, out FixedIterator<Character> iterator))
            {
                ref Character character = ref iterator.Value;
                player.Visual.transform.position = Vector3.Lerp(character.PrevPosition, character.CurrPosition, frameT);
                RotateTransformUsingCharacter(player.Visual.transform, character, frameT);
            }
        }
    }

    public static void SetGameState(GameData data, GameStateType gameState)
    {
        data.Stage.GameState = gameState;
        data.Stage.Ticks = 0;
    }

    public static void UpdatePlayerCameras(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref PlayerCamera camera in data.Stage.PlayerCameras.AsSpan())
        {
            camera.Visual.transform.rotation = Quaternion.AngleAxis(settings.CameraAngle, Vector3.right);

            if (data.Stage.FindCharacter(camera.CharacterId, out FixedIterator<Character> iterator))
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

                    Ray cursorRay = camera.Visual.Camera.ScreenPointToRay(Input.mousePosition);
                    Plane cursorPlane = new Plane(Vector3.up, 0.0f);
                    if (cursorPlane.Raycast(cursorRay, out float enter))
                    {
                        Vector3 cursorPosition = cursorRay.origin + (cursorRay.direction * enter);
                        Vector3 playerToCursor = (cursorPosition - character.CurrPosition);
                        character.InputLook = new Vector2(playerToCursor.x, playerToCursor.z);
                    }
                }

                // Transform camera
                {
                    float xDiff = (character.CurrPosition.x - camera.TargetPosition.x);
                    float zDiff = (character.CurrPosition.z - camera.TargetPosition.z);
                    float xDist = Mathf.Abs(xDiff);
                    float zDist = Mathf.Abs(zDiff);
                    float xTravel = xDist - settings.CameraLazyRectWidth;
                    float zTravel = zDist - settings.CameraLazyRectDepth;
                    if (xDist > settings.CameraLazyRectWidth)
                        camera.TargetPosition.x += xTravel * Mathf.Sign(xDiff);
                    if (zDist > settings.CameraLazyRectDepth)
                        camera.TargetPosition.z += zTravel * Mathf.Sign(zDiff);

                    Vector3 moveTo = camera.TargetPosition - camera.Visual.transform.rotation * Vector3.forward * settings.CameraDistance;
                    camera.Visual.transform.position = Vector3.SmoothDamp(camera.Visual.transform.position, moveTo, ref camera.Velocity, Time.deltaTime * settings.CameraSmoothTime);
                }
            }
        }
    }

    public static void UpdateEnemies(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Enemy enemy in data.Stage.Enemies.AsSpan())
        {
            if (data.Stage.FindCharacter(enemy.CharacterId, out FixedIterator<Character> iterator))
            {
                ref Character character = ref iterator.Value;
                enemy.Visual.transform.position = Vector3.Lerp(character.PrevPosition, character.CurrPosition, frameT);
                enemy.Visual.Animator.speed = character.Velocity.magnitude * settings.EnemyRunAnimationSpeed;
                RotateTransformUsingCharacter(enemy.Visual.transform, character, frameT);
            }
        }
    }

    public static void UpdateProjectiles(GameSettings settings, GameData data, float frameT)
    {
        foreach (ref Projectile projectile in data.Stage.Projectiles.AsSpan())
        {
            projectile.Visual.transform.position = Vector3.Lerp(projectile.PrevPosition, projectile.CurrPosition, frameT);
            projectile.Visual.transform.rotation = Quaternion.AngleAxis(projectile.Angle * Mathf.Rad2Deg, Vector3.down);
        }
    }

    public static void UpdateGameState_AwaitGameStart(GameSettings settings, GameData data, float frameT)
    {

    }

    public static void UpdateGameState_InGame(GameSettings settings, GameData data, float frameT)
    {
    }

    public static void UpdateGameState_GameOver(GameSettings settings, GameData data, float frameT)
    {
    }

    public static void UpdateGameState_TransitionOutOfGameOver(GameSettings settings, GameData data, float frameT)
    {
        Color color = data.Stage.Hud.TransitionImage.color;
        float t = (float)data.Stage.Ticks / (settings.TransitionTicks * 0.5f);
        color.a = Mathf.Sin(t * Mathf.PI * 0.5f);
        data.Stage.Hud.TransitionImage.color = color;
    }

    public static void UpdateGameState_TransitionInToAwaitGameStart(GameSettings settings, GameData data, float frameT)
    {
        Color color = data.Stage.Hud.TransitionImage.color;
        float t = (float)data.Stage.Ticks / (settings.TransitionTicks * 0.5f);
        color.a = Mathf.Sin(Mathf.PI * 0.5f + t * Mathf.PI * 0.5f);
        data.Stage.Hud.TransitionImage.color = color;
    }

    public static void UpdateGameState(GameSettings settings, GameData data, float frameT)
    {
        switch (data.Stage.GameState)
        {
            case GameStateType.AwaitGameStart:
                UpdateGameState_AwaitGameStart(settings, data, frameT);
                break;

            case GameStateType.InGame:
                UpdateGameState_InGame(settings, data, frameT);
                break;

            case GameStateType.GameOver:
                UpdateGameState_GameOver(settings, data, frameT);
                break;

            case GameStateType.TransitionOutOfGameOver:
                UpdateGameState_TransitionOutOfGameOver(settings, data, frameT);
                break;

            case GameStateType.TransitionInToAwaitGameStart:
                UpdateGameState_TransitionInToAwaitGameStart(settings, data, frameT);
                break;

            default:
                Debug.Log($"Unhandlded {nameof(GameStateType)} {data.Stage.GameState}");
                break;
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

    public static void TransitionToAwaitGameStart(GameSettings settings, GameData data)
    {
        SetGameState(data, GameStateType.TransitionInToAwaitGameStart);
        data.Stage.Hud.GameStart.SetActive(true);
        CreatePlayerCamera(settings, data, data.Stage.ArenaVisual.CameraSpawnPoint.transform.position);
    }

    public static bool DoesCapsuleIntersectSphere(in Vector2 capsuleA, in Vector2 capsuleB, float capsuleRadius, in Vector2 spherePoint, float sphereRadius)
    {
        Vector2 s = spherePoint - capsuleA;
        Vector2 a = capsuleB - capsuleA;
        Vector2 b = capsuleB - capsuleA;

        Vector2 dir = b.normalized;
        float dot = Vector2.Dot(dir, s);
        float dotClamped = Mathf.Clamp(dot, 0.0f, b.magnitude);
        Vector2 proj = dotClamped * dir;
        float distSq = Vector2.SqrMagnitude(proj - s);

        float radii = capsuleRadius + sphereRadius;
        float radiiSq = radii * radii;

        return (distSq <= radiiSq);
    }

    public static Vector2 GetV2(in this Vector3 v) => new Vector2(v.x, v.z);
    public static Vector3 GetV3(in this Vector2 v) => new Vector3(v.x, 0.0f, v.y);

    public static void DrawDebugCircle(in Vector2 position, float radius, in Color color)
    {
        Vector2 a = position + new Vector2(radius, 0.0f);

        const int count = 32;
        const int max = count - 1;

        for (int i = 0; i < count; ++i)
        {
            float t = (float)i / max * 2.0f * Mathf.PI;
            Vector2 b = position + radius * new Vector2(Mathf.Cos(t), Mathf.Sin(t));
            Debug.DrawLine(a.GetV3(), b.GetV3(), color);
            a = b;
        }
    }

    public static void RotateTransformUsingCharacter(Transform transform, in Character character, float frameT)
    {
        float angle = Mathf.LerpAngle(character.PrevMoveAngle, character.CurrMoveAngle, frameT);
        transform.rotation = Quaternion.AngleAxis(270.0f + angle * Mathf.Rad2Deg, Vector3.down);
    }
}
