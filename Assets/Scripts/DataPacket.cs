using MessagePack;
using UnityEngine;

[MessagePackObject]
public class DataPacket
{
    [Key(0)] public string id { get; set; } = string.Empty;
    [Key(1)] public MovementData? move { get; set; }
    [Key(2)] public SkillData? skill { get; set; }
}

[MessagePackObject]
public class MovementData
{
    [Key(0)] public Position position { get; set; } = new Position(0, 0, 0);
}

[MessagePackObject]
public class SkillData
{
    [Key(0)] public bool a { get; set; }
    [Key(1)] public bool b { get; set; }
}

[MessagePackObject]
public class Position
{
    [Key(0)]
    public float x { get; set; }
    [Key(1)]
    public float y { get; set; }
    [Key(2)]
    public float z { get; set; }

    public Position() { }

    public Position(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Position operator +(Position a, Position b)
        => new Position(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Position operator -(Position a, Position b)
        => new Position(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Position operator *(Position a, float b)
        => new Position(a.x * b, a.y * b, a.z * b);
    
    public static Position operator *(float a, Position b)
        => new Position(a * b.x, a * b.y, a * b.z);

    public static Position operator /(Position a, float b)
        => new Position(a.x / b, a.y / b, a.z / b);
    
    public static Position operator /(float a, Position b)
        => new Position(a / b.x, a / b.y, a / b.z);
}