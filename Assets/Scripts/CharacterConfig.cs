using UnityEngine;

[CreateAssetMenu]
public class CharacterConfig : ScriptableObject
{
    public float Acceleration = 100.0f;
    public float Drag = 0.1f;
    public float ShootTime = 1.0f;
    public int Health = 1;

    [HideInInspector] public long ShootTicks;

    private void OnValidate()
    {
        ShootTicks = (int)(ShootTime * GameConstant.TicksPerSecond);
    }
}
