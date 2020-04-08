using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using System;
using System.Linq;
using System.Text;

using Mapbox.Utils;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Factories;
using Mapbox.Unity.MeshGeneration.Modifiers;
using Mapbox.Unity.Utilities;
using Mapbox.Unity.MeshGeneration.Components;

public class GeometryGenerator : MonoBehaviour
{
    public AbstractMap map;
    public Material mat;

    // Start is called before the first frame update
    private void OnEnable()
    {
        DatabaseConnector.LoadGeometries += LoadGeometries;
    }

    public void LoadGeometries()
    {
        DataTable dataTable = DatabaseConnector.Instance.dataTable;
        //Debug.Log(dataTable.Rows.Count);
        GenerateGeometry(dataTable);
    }

    void GenerateGeometry(DataTable dataTable)
    {
        GameObject parent = new GameObject("Geodata");

        for (int i = 0; i < dataTable.Rows.Count; i++)
        {
            string polygonText = dataTable.Rows[i].ItemArray[1].ToString().Replace("MULTIPOLYGON(((", "");
            polygonText = polygonText.Replace(")))", "");
            //Debug.Log(polygonText);
            string[] coords = polygonText.Split(',');
            //Debug.Log("Coords " + coords.Length);

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> vertices2d = new List<Vector2>();
            for (int j = 0; j < coords.Length - 1; j++)
            {
                coords[j] = coords[j].Replace(".", ",");
                string[] latlonS = coords[j].Split(' ');

                try
                {
                    Vector2d latlon = new Vector2d(double.Parse(latlonS[1]), double.Parse(latlonS[0]));
                    //Debug.Log(latlon.ToString());
                    Vector3 point = map.GeoToWorldPosition(latlon, false);
                    vertices.Add(new Vector3(point.x, point.z, 0));
                    vertices2d.Add(new Vector2(point.x, point.z));
                }
                catch (Exception msg)
                {
                    Debug.Log(msg.ToString());
                }

            }

            float height = 0;
            try
            {
                height = float.Parse(dataTable.Rows[i].ItemArray[2].ToString());
            }
            catch (Exception msg)
            {
                Debug.Log(msg.ToString());
            }

            //------------Generate Mesh------------//
            //vertices2d.Reverse();
            //Mesh mesh = CreateMesh(vertices2d.ToArray(), height);
            TriangulatorSimple triangulator = new TriangulatorSimple(vertices2d.ToArray());
            int[] indices = triangulator.Triangulate();
            //Mesh mesh = new Mesh();
            //mesh.vertices = vertices.ToArray();
            //mesh.triangles = indices;
            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            //mesh.Optimize();

            bool is3D = true;
            GameObject obj = new GameObject(i.ToString());
            PolyExtruder polyExtruder = obj.AddComponent<PolyExtruder>();          
            polyExtruder.createPrism(obj.name, height, vertices2d.ToArray(), Color.white, is3D);
            obj.AddComponent<MeshRenderer>();
            //obj.AddComponent<MeshFilter>();
            //obj.GetComponent<MeshFilter>().mesh = mesh;
            obj.GetComponent<MeshRenderer>().material = mat;
            //obj.transform.localEulerAngles += new Vector3(90, 0, 0);
            obj.transform.parent = parent.transform;
        }

    }

    Mesh CreateMesh(Vector2[] poly, float height)
    {
        // convert polygon to triangles
       
        TriangulatorSimple triangulator = new TriangulatorSimple(poly);
        int[] tris = triangulator.Triangulate();
        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[poly.Length * 2];

        for (int i = 0; i < poly.Length; i++)
        {
            vertices[i].x = poly[i].x;
            vertices[i].y = poly[i].y;
            vertices[i].z = 0;// front vertex
            vertices[i + poly.Length].x = poly[i].x;
            vertices[i + poly.Length].y = poly[i].y;
            vertices[i + poly.Length].z = -height;// back vertex    
        }
        int[] triangles = new int[tris.Length * 2 + poly.Length * 6];
        int count_tris = 0;
        for (int i = 0; i < tris.Length; i += 3)
        {
            triangles[i] = tris[i] + poly.Length;
            triangles[i + 1] = tris[i + 1] + poly.Length;
            triangles[i + 2] = tris[i + 2] + poly.Length;
        } // front vertices
        count_tris += tris.Length;
        for (int i = 0; i < tris.Length; i += 3)
        {
            triangles[count_tris + i] = tris[i + 2];
            triangles[count_tris + i + 1] = tris[i + 1];
            triangles[count_tris + i + 2] = tris[i];
        } // back vertices
        count_tris += tris.Length;
        for (int i = 0; i < poly.Length; i++)
        {
            // triangles around the perimeter of the object
            int n = (i + 1) % poly.Length;
            triangles[count_tris] = i;
            triangles[count_tris + 1] = i + poly.Length;
            triangles[count_tris + 2] = n;
            triangles[count_tris + 3] = i + poly.Length;
            triangles[count_tris + 4] = n + poly.Length;
            triangles[count_tris + 5] = n;
            count_tris += 6;
        }
        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        m.RecalculateBounds();
        m.Optimize();
           
        return m;
    }

   
}
