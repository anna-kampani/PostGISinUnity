using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.MeshGeneration.Components;
using System.Data;
using Mapbox.Examples;

public class TerrainGenerator : MonoBehaviour
{
    public Terrain terrain; // terrain to modify
    public AbstractMap map;

    private void OnEnable()
    {
        DatabaseConnector.GenerateTerrain += GenerateHeightmap;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   

    void GenerateHeightmap(byte[] binaryData, float lat, float lon)
    {
        terrain = Terrain.activeTerrain;
        //int heightmapWidth = terrain.terrainData.heightmapWidth;
        //int heightmapHeight = terrain.terrainData.heightmapHeight;
        int heightmapWidth = 240;
        int heightmapHeight = 250;
      
        ToHeightmap(binaryData, heightmapWidth, heightmapHeight);        

        //geo-locate
        Vector2d latlon = new Vector2d(lon, lat);
        terrain.transform.parent.position = map.GeoToWorldPosition(latlon);
    }   

    void ToHeightmap(byte[] binarydata, int heightmapWidth, int heightmapHeight)
    {
        int height = 250;
        int width = 250;      

      
        Texture2D heightmap = new Texture2D(height, width);
        heightmap.LoadImage(binarydata);

        var terrain = Terrain.activeTerrain.terrainData;
        int w = heightmap.width;
        int h = heightmap.height;
        int w2 = terrain.heightmapWidth;
        float[,] heightmapData = terrain.GetHeights(0, 0, w2, w2);
        Color[] mapColors = heightmap.GetPixels();
        Color[] map = new Color[w2 * w2];
        if (w2 != w || h != w)
        {
            // Resize using nearest-neighbor scaling if texture has no filtering
            if (heightmap.filterMode == FilterMode.Point)
            {
                float dx = (float)w / (float)w2;
                float dy = (float)h / (float)w2;
                for (int y = 0; y < w2; y++)
                {                   
                    int thisY = Mathf.FloorToInt(dy * y) * w;
                    int yw = y * w2;
                    for (int x = 0; x < w2; x++)
                    {
                        map[yw + x] = mapColors[Mathf.FloorToInt(thisY + dx * x)];
                    }
                }
            }
            // Otherwise resize using bilinear filtering
            else
            {
                float ratioX = (1.0f / ((float)w2 / (w - 1)));
                float ratioY = (1.0f / ((float)w2 / (h - 1)));
                for (int y = 0; y < w2; y++)
                {                    
                    int yy = Mathf.FloorToInt(y * ratioY);
                    int y1 = yy * w;
                    int y2 = (yy + 1) * w;
                    int yw = y * w2;
                    for (int x = 0; x < w2; x++)
                    {
                        int xx = Mathf.FloorToInt(x * ratioX);
                        Color bl = mapColors[y1 + xx];
                        Color br = mapColors[y1 + xx + 1];
                        Color tl = mapColors[y2 + xx];
                        Color tr = mapColors[y2 + xx + 1];
                        float xLerp = x * ratioX - xx;
                        map[yw + x] = Color.Lerp(Color.Lerp(bl, br, xLerp), Color.Lerp(tl, tr, xLerp), y * ratioY - (float)yy);
                    }
                }
            }           
        }
        else
        {
            // Use original if no resize is needed
            map = mapColors;
        }
        // Assign texture data to heightmap
        for (int y = 0; y < w2; y++)
        {
            for (int x = 0; x < w2; x++)
            {
                heightmapData[y, x] = map[y * w2 + x].grayscale;
            }
        }
        terrain.SetHeights(0, 0, heightmapData);
    }
}


