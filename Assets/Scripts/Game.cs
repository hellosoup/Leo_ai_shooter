using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameSettings m_gameSettings;

    private float m_tickTimer;
    private GameData m_data;

    // Start is called before the first frame update
    void Start()
    {
        m_data = new GameData(m_gameSettings);

        GameUtil.TransitionToAwaitGameStart(m_gameSettings, m_data);
    }

    // Update is called once per frame
    void Update()
    {
        m_tickTimer += Time.deltaTime;

        if (m_tickTimer >= GameConstant.TickTime)
        {
            m_tickTimer -= GameConstant.TickTime;

            GameUtil.IncrementTicks(m_data);
            GameUtil.TickGameState(m_gameSettings, m_data);

            if (m_data.Stage.GameState == GameStateType.InGame)
            {
                GameUtil.TickPlayers(m_gameSettings, m_data);
                GameUtil.TickEnemies(m_gameSettings, m_data);
                GameUtil.TickCharacters(m_gameSettings, m_data);
                GameUtil.TickProjectiles(m_gameSettings, m_data);
                GameUtil.TickExplosions(m_gameSettings, m_data);
                GameUtil.TickMessages(m_gameSettings, m_data);
                GameUtil.TickWave(m_gameSettings, m_data);
            }
        }

        float frameT = (m_tickTimer / GameConstant.TickTime);
        GameUtil.UpdateGameState(m_gameSettings, m_data, frameT);
        GameUtil.UpdatePlayerCameras(m_gameSettings, m_data, frameT);

        if (m_data.Stage.GameState == GameStateType.InGame)
        {
            GameUtil.UpdateProjectiles(m_gameSettings, m_data, frameT);
            GameUtil.UpdatePlayers(m_gameSettings, m_data, frameT);
            GameUtil.UpdateEnemies(m_gameSettings, m_data, frameT);
        }
    }
}
