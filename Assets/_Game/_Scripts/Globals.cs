using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Globals
{
    public const int ASTEROID_SEGMENTS = 10;
    public const int ASTEROID_MAX = 200;
    public const int BULLET_MAX = 200;
    public const float ARENA_BUFFER = 5f;

    public struct AsteroidTypeData
    {
        public float Size;
        public int Health;

        public AsteroidTypeData(float size, int health)
        {
            Size = size;
            Health = health;
        }
    }

    public static readonly Dictionary<AsteroidType, AsteroidTypeData> ASTEROID_TYPE_MAP = new()
    {
        {AsteroidType.Small, new AsteroidTypeData(1f, 2) },
        {AsteroidType.Medium, new AsteroidTypeData(2.2f, 10) },
        {AsteroidType.Large, new AsteroidTypeData(4f, 50) }
    };
    public static readonly AsteroidType[] ASTEROID_TYPES = ASTEROID_TYPE_MAP.Keys.ToArray();

}
