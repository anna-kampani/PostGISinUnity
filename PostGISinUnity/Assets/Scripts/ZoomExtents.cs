using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;


namespace BrydenWoodUnity
{
    /// <summary>
    /// Class responsible for camera effects.
    /// </summary>
    public class ZoomExtents : MonoBehaviour
    {

        public Camera my_cam;
        MeshRenderer[] renderers;
        Transform[] children;
        Bounds bnds;
        Vector3 centroid;
        Vector3 average;
        int counter;
        public int[] myRange;

        // Use this for initialization
        void Start()
        {
            counter = 0;
            bnds = new Bounds(Vector3.zero, Vector3.one);

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z) && !Input.GetKey(KeyCode.LeftControl))
            {
                Debug.Log("hit bounds");
                CalculateBounds();
            }
        }

        
        public void CalculateBounds()
        {
            {
                GetMaxBounds(gameObject);
                if (my_cam.tag == "MainCamera")
                {
                    FocusCameraOnObject(my_cam);
                }

            }
        }

        /// <summary>
        /// Method that gets the bounding box of an object.
        /// </summary>
        /// <param name="g"></param>
        public void GetMaxBounds(GameObject g)
        {
            counter = 0;
            children = g.GetComponentsInChildren<Transform>();
            foreach (Transform t in children)
            {

                if (t.GetComponent<MeshRenderer>() != null)
                {
                    if (counter == 0)
                    {
                        //Debug.Log(t.gameObject.name);
                        bnds = t.GetComponent<MeshRenderer>().bounds;
                    }
                    else
                    {
                        //Debug.Log(t.gameObject.name);
                        Bounds _b = t.GetComponent<MeshRenderer>().bounds;
                        bnds.Encapsulate(_b);
                    }
                    counter++;
                }

            }

            //return bnds;

        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bnds.center, bnds.size);
        }

        /// <summary>
        /// Method that adjusts camera size and distance to focus on an object.
        /// </summary>
        /// <param name="c"></param>
        public void FocusCameraOnObject(Camera c)
        {
            // as instructed from: https://answers.unity.com/questions/13267/how-can-i-mimic-the-frame-selected-f-camera-move-z.html

            Vector3 max = bnds.size;
            Vector3 cam_pos_now = c.transform.position;
            // Get the radius of a sphere circumscribing the bounds
            float radius = max.magnitude/* / 2f*/;
            // Get the horizontal FOV, since it may be the limiting of the two FOVs to properly encapsulate the objects
            float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(c.fieldOfView * Mathf.Deg2Rad / 2f) * c.aspect) * Mathf.Rad2Deg;
            // Use the smaller FOV as it limits what would get cut off by the frustum        
            float fov = Mathf.Min(c.fieldOfView, horizontalFOV);
            float dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));
            //Debug.Log("Radius = " + radius + " dist = " + dist);
            if (c.GetComponent<maxCamera>() != null)
            {
                c.GetComponent<maxCamera>().desiredDistance = dist;
                //c.GetComponent<maxCamera>().target.position = bnds.center;
            }
            if (c.orthographic)
            {
                c.orthographicSize = 2 * bnds.extents.x;
                c.transform.position = new Vector3(bnds.center.x, cam_pos_now.y, bnds.center.z);
            }

        }
       
        public void IsolateRange(int[] myIndices)
        {
            foreach (Transform c in transform)
            {
                foreach (Transform child in c)
                {
                    if (!myIndices.Contains(child.GetSiblingIndex()))
                    {
                        child.gameObject.SetActive(false);
                    }

                    if (child.childCount > 1)
                    {
                        IsolateRange(myIndices);
                    }

                }

            }
        }
    }

}
