using System;

public class GameData : IDisposable
{
    public StageData Stage;

    public GameData(GameSettings settings)
    {
        Stage = new StageData(settings);
    }

    public void Dispose()
    {
        Stage.Dispose();
        GC.Collect();
    }
}
