using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidBuilder    
{
    public Mesh CreateAsteroidMesh()
    {
        Mesh res = new Mesh();
        int asteroidSegments = Globals.ASTEROID_SEGMENTS;

        Vector3[] vertices = new Vector3[asteroidSegments];
        for (int i = 0; i < asteroidSegments; ++i)
        {
            float dist = Random.Range(.5f, 1);
            float theta = (2 * Mathf.PI) / asteroidSegments * i;
            vertices[i] = new Vector3(dist * Mathf.Cos(theta), dist * Mathf.Sin(theta), 0);
        }

        Color[] colors = new Color[asteroidSegments];
        for (int i = 0; i < asteroidSegments; ++i)
        {
            colors[i] = new Color();
        }

        int[] indicies = new int[asteroidSegments * 2];
        for (int i = 0; i < asteroidSegments; ++i)
        {
            int idxIdx = i * 2;
            indicies[idxIdx] = i;
            indicies[idxIdx + 1] = (i + 1) % asteroidSegments;
        }

        res.vertices = vertices;
        res.colors = colors;
        res.SetIndices(indicies, MeshTopology.Lines, 0);

        return res;
    }
}
