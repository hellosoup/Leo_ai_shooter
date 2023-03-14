public struct ProjectileId
{
    public int Index;

    public static bool operator ==(ProjectileId a, ProjectileId b) => a.Index == b.Index;
    public static bool operator !=(ProjectileId a, ProjectileId b) => !(a == b);
    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
}

public struct CharacterId
{
    public int Index;

    public static bool operator ==(CharacterId a, CharacterId b) => a.Index == b.Index;
    public static bool operator !=(CharacterId a, CharacterId b) => !(a == b);
    public override bool Equals(object obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
}
