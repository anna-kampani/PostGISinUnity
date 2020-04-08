using System.Collections.Generic;
using UnityEngine;



    /// <summary>
    /// A static class with helper functions regarding the translation and IO of geometry
    /// </summary>
    public static class GeometryHelper
    {

    
        /// <summary>
        /// Generates a surface mesh from 4 points
        /// </summary>
        /// <param name="vertices">The list of vertices</param>
        /// <param name="flip">Whether the mesh should face one way or the other</param>
        /// <param name="color">A single color to be used for vertex colours</param>
        /// <returns>The generated mesh</returns>
        public static Mesh GetMeshFrom4Points(Vector3[] vertices, bool flip, Color color)
        {
            Mesh surfaceMesh = new Mesh();

            int[] triangles;
            if (flip)
            {
                triangles = new int[] { 2, 1, 0, 0, 3, 2 };
            }
            else
            {
                triangles = new int[] { 0, 1, 2, 2, 3, 0 };
            }
            surfaceMesh.vertices = vertices;
            surfaceMesh.triangles = triangles;
            surfaceMesh.uv = new Vector2[4]
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(1,1),
                new Vector2(0,1),
            };
            surfaceMesh.colors = new Color[] { color, color, color, color };

            return surfaceMesh;
        }

        /// <summary>
        /// Returns a list of panel meshes from a 4-Point surface
        /// </summary>
        /// <param name="points">The corners of the surface</param>
        /// <param name="size">The size of each panel</param>
        /// <param name="flip">Whether the panels should be flipped</param>
        /// <param name="color">The color of the panels</param>
        /// <returns>List of Unity Meshes</returns>
        public static List<Mesh> GetPanelsFrom4PointSurf(Vector3[] points, float size, bool flip, Color color)
        {
            List<Mesh> panels = new List<Mesh>();

            int uNum = Mathf.FloorToInt(Vector3.Distance(points[0], points[1]) / size);
            int vNum = Mathf.FloorToInt(Vector3.Distance(points[0], points[3]) / size);

            for (int i=0; i<uNum; i++)
            {

                for (int j=0; j< vNum; j++)
                {
                    Mesh panel = new Mesh();

                    Vector3 segU = (points[1] - points[0]) * (i / ((float)uNum - 1));
                    Vector3 segV = (points[3] - points[0]) * (j / ((float)vNum - 1));

                    Vector3[] verts = new Vector3[4];
                    verts[0] = points[0];
                    verts[1] = points[0] + segU;
                    verts[3] = points[0] + segV;
                    verts[2] = points[0] + segU + segV;

                    panel = GetMeshFrom4Points(verts, flip, color);

                    panels.Add(panel);
                }

            }

            return panels;
        }

        /// <summary>
        /// Returns a list of panel meshes from a 4-Point surface
        /// </summary>
        /// <param name="points">The corners of the surface</param>
        /// <param name="size">The size of each panel</param>
        /// <param name="flip">Whether the panels should be flipped</param>
        /// <param name="color">The color of the panels</param>
        /// <param name="ray">The normal of each panel</param>
        /// <returns>List of Unity Meshes</returns>
        public static List<Mesh> GetPanelsFrom4PointSurf(Vector3[] points, float size, bool flip, Color color, out List<Ray> ray)
        {
            List<Mesh> panels = new List<Mesh>();
            ray = new List<Ray>();

            Vector3 centre = new Vector3();
            Vector3 normal = new Vector3();
            int uNum = Mathf.FloorToInt(Vector3.Distance(points[0], points[1]) / size);
            int vNum = Mathf.FloorToInt(Vector3.Distance(points[0], points[3]) / size);
            float stepU = 1.0f / uNum;
            float stepV = 1.0f / vNum;
            Vector3 segU = (points[1] - points[0]) * stepU;
            Vector3 segV = (points[3] - points[0]) * stepV;

            for (int i = 0; i < uNum; i++)
            {
                for (int j = 0; j < vNum; j++)
                {
                    Mesh panel = new Mesh();

                    Vector3[] verts = new Vector3[4];
                    verts[0] = points[0] + segU * i + segV * j;
                    verts[1] = verts[0] + segU;
                    verts[3] = verts[0] + segV;
                    verts[2] = verts[0] + segU + segV;

                    centre = (verts[0] + verts[1] + verts[2] + verts[3]) / 4.0f;

                    panel = GetMeshFrom4Points(verts, flip, color);
                    panel.RecalculateNormals();
                    panel.RecalculateBounds();
                    normal = panel.normals[0];

                    Ray test = new Ray(centre + (normal * -0.1f), normal);
                    RaycastHit hit;
                    if (!Physics.Raycast(test, out hit, 0.15f))
                    {
                        ray.Add(new Ray(centre, normal));
                        panels.Add(panel);
                    }
                }

            }
            if (ray.Count != 0)
            {
                return panels;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generates a surface mesh from 4 points
        /// </summary>
        /// <param name="vertices">The list of vertices</param>
        /// <param name="flip">Whether the mesh should face one way or the other</param>
        /// <param name="colors">A list of colors to be used for vertex colours</param>
        /// <returns>The generated mesh</returns>
        public static Mesh GetMeshFrom4Points(Vector3[] vertices, bool flip, Color[] colors)
        {
            Mesh surfaceMesh = new Mesh();

            int[] triangles;
            if (!flip)
            {
                triangles = new int[] { 0, 1, 2, 2, 3, 0 };
            }
            else
            {
                triangles = new int[] { 2, 1, 0, 0, 3, 2 };
            }
            surfaceMesh.vertices = vertices;
            surfaceMesh.triangles = triangles;
            surfaceMesh.colors = colors;

            return surfaceMesh;
        }


 

    }
