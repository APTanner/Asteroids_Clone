using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static bool OutOfBounds(Vector3 position, float size, bool useBuffer = false)
    {
        float x = position.x;
        float y = position.y;

        float buffer = useBuffer ? Globals.ARENA_BUFFER * 2 : 0;
        Vector2 camBounds = GameManager.Instance.CameraBounds;

        float xDist = camBounds.x + size / 2 + buffer;
        float yDist = camBounds.y + size / 2 + buffer;

        return x < -xDist || x > xDist || y < -yDist || y > yDist;
    }
}
