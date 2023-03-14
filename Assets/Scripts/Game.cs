using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameSettings m_gameSettings;

    private float m_tickTimer;
    private GameData m_data;

    // Start is called before the first frame update
    void Start()
    {
        m_data = new GameData();

        GameUtil.InitRandom(m_data);
        GameUtil.InitPlayerCameraVisuals(m_gameSettings, m_data);
        GameUtil.InitPlayerVisuals(m_gameSettings, m_data);
        GameUtil.InitEnemyVisuals(m_gameSettings, m_data);
        GameUtil.InitProjectileVisuals(m_gameSettings, m_data);

        GameUtil.CreatePlayer(m_gameSettings, m_data, Vector3.zero);

        for (int i = 0; i < 50; ++i)
            GameUtil.CreateEnemy(m_gameSettings, m_data, new Vector3(3.0f + i * 2.0f, 0.0f, 3.0f));
    }

    // Update is called once per frame
    void Update()
    {
        m_tickTimer += Time.deltaTime;

        if (m_tickTimer >= GameConstant.TickTime)
        {
            m_tickTimer -= GameConstant.TickTime;

            GameUtil.IncrementTicks(m_data);
            GameUtil.TickEnemies(m_gameSettings, m_data);
            GameUtil.TickCharacters(m_gameSettings, m_data);
            GameUtil.TickProjectiles(m_gameSettings, m_data);
        }

        float frameT = (m_tickTimer / GameConstant.TickTime);

        GameUtil.UpdateProjectiles(m_gameSettings, m_data, frameT);
        GameUtil.UpdatePlayers(m_gameSettings, m_data, frameT);
        GameUtil.UpdateEnemies(m_gameSettings, m_data, frameT);
    }
}
