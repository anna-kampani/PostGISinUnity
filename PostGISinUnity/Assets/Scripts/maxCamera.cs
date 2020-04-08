//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;



namespace BrydenWoodUnity
{
    public enum NavigationMode
    {
        Select = 0,
        Pan = 1,
        Zoom = 2,
        Rotate = 3,
        Full = 4,
    }
    /// <summary>
    /// Class responsible for camera effects.
    /// </summary>
    [AddComponentMenu("Camera-Control/3dsMax Camera Style")]
    public class maxCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 targetOffset;
        public float distance = 5.0f;
        public float maxDistance = 20;
        public float minDistance = .6f;
        public float xSpeed = 200.0f;
        public float ySpeed = 200.0f;
        public int yMinLimit = -80;
        public int yMaxLimit = 80;
        public float yOrbitLimit = 0;
        public int zoomRate = 40;
        public float panSpeed = 0.3f;
        public float zoomDampening = 5.0f;

        public NavigationMode navigationMode;

        public List<Texture2D> cursorTextures;

        public float xDeg = 88.0f;
        public float yDeg = 38.0f;
        public float currentDistance;
        public float desiredDistance;
        private Quaternion currentRotation;
        public Quaternion desiredRotation;
        public Quaternion rotation;
        public Vector3 position;

        float clamped_y;

        
        public GameObject carModel;



        void Start()
        {

            Init();

        }
        void OnEnable()
        {
            Init();
        }

        /// <summary>
        /// Method that positions the camera target.
        /// </summary>
        public void Init()
        {


            //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
            if (!target)
            {
                GameObject go = new GameObject("Cam Target");
                go.transform.position = transform.position + (transform.forward * distance);
                target = go.transform;
            }

            //target.position = target.GetComponent<MoveAlongPath>().points[0];

            distance = Vector3.Distance(transform.position, target.position);
            currentDistance = distance;
            desiredDistance = distance;

            //be sure to grab the current rotations as starting points.
            position = transform.position;
            rotation = transform.rotation;
            currentRotation = transform.rotation;
            desiredRotation = transform.rotation;

            xDeg = Vector3.Angle(Vector3.right, transform.right);
            yDeg = Vector3.Angle(Vector3.up, transform.up);
        }

        /*
         * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
         */
        void LateUpdate()
        {

            switch (navigationMode)
            {
                case NavigationMode.Full:
                    FullNavigation();
                    break;
                case NavigationMode.Pan:
                    PanNavigation();
                    break;
                case NavigationMode.Rotate:
                    RotateNavigation();
                    break;
                case NavigationMode.Select:
                    SelectNavigation();
                    break;
                case NavigationMode.Zoom:
                    ZoomNavigation();
                    break;
            }
            //if (ZoomExtents.NearlyEqual(desiredDistance, currentDistance))
            //{
            //    Debug.Log("distances equalized");
            //}
            //Debug.Log(navigationMode.ToString());
        }

        private void Update()
        {
            position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
            clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
            //transform.position = position;
            transform.position = new Vector3(position.x, clamped_y, position.z);

        }
        public void SetNavigationMode(int index)
        {
            navigationMode = (NavigationMode)index;            
        }

        /// <summary>
        /// Method that controls the pan interaction.
        /// </summary>
        void PanNavigation()
        {
            if (!EventSystem.current.IsPointerOverGameObject(-1))
            {
                if (Input.GetMouseButton(0))
                {
                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    target.rotation = transform.rotation;
                    target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
                    target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
                    //Snap(target.GetComponent<MoveAlongPath>().points);
                }

                ////////Orbit Position

                // affect the desired Zoom distance if we roll the scrollwheel
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                //clamp the zoom min/max
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

                // calculate position based on the new currentDistance + clamp camera position.y according to the Y_OrbitLimit
                position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
                clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
                //transform.position = position;
                transform.position = new Vector3(position.x, clamped_y, position.z);
            }
        }

        /// <summary>
        /// Method that controls rotation.
        /// </summary>
        void RotateNavigation()
        {
            if (!EventSystem.current.IsPointerOverGameObject(-1))
            {
                if (Input.GetMouseButton(1))
                {
                    xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                    ////////OrbitAngle

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
                    // set camera rotation 
                    desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                    currentRotation = transform.rotation;

                    rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
                    transform.rotation = rotation;
                }

                ////////Orbit Position

                // affect the desired Zoom distance if we roll the scrollwheel
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                //clamp the zoom min/max
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

                // calculate position based on the new currentDistance + clamp camera position.y according to the Y_OrbitLimit
                position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
                clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
                //transform.position = position;
                transform.position = new Vector3(position.x, clamped_y, position.z);
            }
        }

        /// <summary>
        /// Method that controls zoom in/out.
        /// </summary>
        void ZoomNavigation()
        {
            if (!EventSystem.current.IsPointerOverGameObject(-1))
            {
                // If Control and Alt and Middle button? ZOOM!
                if (Input.GetMouseButton(0))
                {
                    desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
                }

                ////////Orbit Position

                // affect the desired Zoom distance if we roll the scrollwheel
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                //clamp the zoom min/max
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

                // calculate position based on the new currentDistance + clamp camera position.y according to the Y_OrbitLimit
                position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
                clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
                //transform.position = position;
                transform.position = new Vector3(position.x, clamped_y, position.z);
            }
        }

        /// <summary>
        /// Method that controls orbit, zoom and pan.
        /// </summary>
        void FullNavigation()
        {
            if (!EventSystem.current.IsPointerOverGameObject(-1))
            {
                //// If Control and Alt and Middle button? ZOOM!
                ////if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
                //if (Input.GetMouseButton(2))
                //{
                //    desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
                //    //desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate *100* Mathf.Abs(desiredDistance);
                //}
                //// If middle mouse and left alt are selected? ORBIT
                ////else if (Input.GetMouseButton(1))
                if (Input.GetMouseButton(1))
                {
                    xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                    ////////OrbitAngle

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
                    // set camera rotation 
                    desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                    currentRotation = transform.rotation;

                    rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
                    transform.rotation = rotation;
                }
                // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
                //else if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
                else if (Input.GetMouseButton(0))
                {
                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    target.rotation = transform.rotation;
                    target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
                    target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
                    //Snap(target.GetComponent<MoveAlongPath>().points);
                }

                ////////Orbit Position

                // affect the desired Zoom distance if we roll the scrollwheel
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                //clamp the zoom min/max
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

                // calculate position based on the new currentDistance + clamp camera position.y according to the Y_OrbitLimit
                position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
                clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
                //transform.position = position;
                transform.position = new Vector3(position.x, clamped_y, position.z);

            }
        }

        /// <summary>
        /// Method that controls orbit, zoom and pan.
        /// </summary>
        void SelectNavigation()
        {
            if (!EventSystem.current.IsPointerOverGameObject(-1))
            {
                // If Control and Alt and Middle button? ZOOM!
                if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
                {
                    desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * zoomRate * 0.125f * Mathf.Abs(desiredDistance);
                }
                // If middle mouse and left alt are selected? ORBIT
                else if (Input.GetMouseButton(1))
                {
                    xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                    ////////OrbitAngle

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
                    // set camera rotation 
                    desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                    currentRotation = transform.rotation;

                    rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
                    transform.rotation = rotation;
                }
                // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
                else if (Input.GetMouseButton(2))
                {
                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    target.rotation = transform.rotation;
                    target.Translate(Vector3.right * -Input.GetAxis("Mouse X") * panSpeed);
                    //Debug.Log(-Input.GetAxis("Mouse Y"));
                    target.Translate(transform.up * -Input.GetAxis("Mouse Y") * panSpeed, Space.World);
                    //Snap(target.GetComponent<MoveAlongPath>().points);
                }

                ////////Orbit Position

                // affect the desired Zoom distance if we roll the scrollwheel
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                //clamp the zoom min/max
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

                // calculate position based on the new currentDistance + clamp camera position.y according to the Y_OrbitLimit
                position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
                clamped_y = Mathf.Clamp(position.y, yOrbitLimit, 1000f);
                //transform.position = position;
                transform.position = new Vector3(position.x, clamped_y, position.z);
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
        /// <summary>
        /// Snaps the gameobject to the projected point between the closest 2 point from a list
        /// </summary>
        /// <param name="positions"></param>List of objects to find the 2 closest
        void Snap(List<Vector3> positions)
        {


            Vector3 close = new Vector3();
            float[] distances = new float[positions.Count];
            float distance = float.MaxValue;
            int index = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                distances[i] = Vector3.Distance(positions[i], target.position);
                if (Vector3.Distance(positions[i], target.position) < distance)
                {
                    distance = Vector3.Distance(positions[i], target.position);
                    close = positions[i];
                    index = i;
                }
            }

            var next = positions[(index + 1) % positions.Count];
            var prevIndex = (index - 1 > 0) ? index - 1 : positions.Count - 1;
            var previous = positions[prevIndex];

            Vector3 close2 = (Vector3.Distance(next, target.position) < Vector3.Distance(previous, target.position)) ? next : previous;



            Vector3 start = close;
            Vector3 end = close2;


            Vector3 snapedPos = Vector3.Project(start - target.position, start - end) + start;
            if ((end - start).magnitude > snapedPos.magnitude)
            {
                target.position = start;
            }
            else
            {
                target.position = snapedPos;
            }
            //var snapVectors = target.GetComponent<MoveAlongPath>().points;
           


        }



    }
}
