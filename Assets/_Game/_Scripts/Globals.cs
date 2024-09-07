using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Globals
{
    public static int ASTEROID_SEGMENTS = 10;
    public static int ASTEROID_MAX = 200;
    public static int BULLET_MAX = 200;
    public static float ARENA_BUFFER = 5f;

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

    public static Dictionary<AsteroidType, AsteroidTypeData> ASTEROID_TYPE_MAP = new Dictionary<AsteroidType, AsteroidTypeData>
    {
        {AsteroidType.Small, new AsteroidTypeData(1f, 1) },
        {AsteroidType.Medium, new AsteroidTypeData(2.2f, 5) },
        {AsteroidType.Large, new AsteroidTypeData(4f, 16) }
    };
    public static AsteroidType[] ASTEROID_TYPES = ASTEROID_TYPE_MAP.Keys.ToArray();

}
