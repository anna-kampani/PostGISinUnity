using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using UnityNpgsql;


public class DatabaseConnector : MonoBehaviour
{
    public delegate void OnLoadGeometries();
    public static event OnLoadGeometries LoadGeometries;
    
    public delegate void OnImageLoaded(byte[] binary, float lat, float lon);
    public static event OnImageLoaded GenerateTerrain;

    public static DatabaseConnector Instance;

    [Tooltip("Database Connection")]
    public string userID = "unity";
    public string password = "pwd";
    public string database = "nyc";
    public string host = "5432";

    public DataTable dataTable = new DataTable();
    // Reference this in the Inspector
    public RawImage image;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        //Main();     
    }

    public void LoadData()
    {
        string sqlCmd = "SELECT ogc_fid, ST_AsText(ST_Transform(wkb_geometry, 4326)) AS geom, relh2 FROM buildings_tl1000";
        dataTable = GetDataTable(sqlCmd);
        Debug.Log(dataTable.Rows[0].ItemArray[1]);
        //Tigger geometry creation event
        LoadGeometries();
    }

    public void LoadImageTerrain()
    {      
        string sqlCmd = @"SELECT ST_AsPNG(
                             ST_Transform(
                             ST_AddBand(
                                 ST_Union(rast, 1), ARRAY[ST_Union(rast, 2), ST_Union(rast, 3)]), 27700)) As new_rast
                        FROM demelevation

                            WHERE

                                ST_Intersects(rast,
                                    ST_Transform(ST_MakeEnvelope(-0.19118, 51.49065, -0.18913, 51.49245, 4326), 27700))";


        byte[] res = GetImage(sqlCmd);
        Debug.Log(res);

        GenerateImage(res);
       
        GenerateTerrain(res, -0.19118f, 51.49065f);
    }

    /// <summary>
    /// Setup connection to database
    /// </summary>
    static void Main()
    {

        try
        {
            // Specify connection options 
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=unity;" + "Password=pwd;Database=nyc;");
            //Open connection
            conn.Open();

            // Define a query            
            // NpgsqlCommand cmd = new NpgsqlCommand("SELECT name FROM nyc_neighborhoods WHERE boroname = 'Brooklyn'", conn);
            // Bounding box query
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM nyc_streets WHERE nyc_streets.geom && ST_MakeEnvelope( 586785.476789704, 4492901.00145548, 597561.276849023, 4499012.99070226,  26918)", conn);




            // Execute a query
            NpgsqlDataReader dr = cmd.ExecuteReader();


            // Read all rows and output the first column in each row
            while (dr.Read())
            {
                Debug.Log(dr[2]);
                //Debug.Log(dr.GetString(0));
            }

            // Close connection
            conn.Close();
            // Clean up
            conn = null;
            cmd.Dispose();
            cmd = null;
            dr.Close();
            dr = null;
        }
        catch (Exception msg)
        {
            Debug.Log(msg.ToString());

        }


    }

    static byte[] GetImage(string sqlCMD)
    {
        int input_srid = 27700;
        byte[] result = null;       

        DataTable dt = new DataTable();
        try
        {
            // Specify connection options 
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=unity;" + "Password=pwd;Database=nyc;");
            //Open connection
            conn.Open();

            // Define a query          
            NpgsqlCommand cmd = new NpgsqlCommand(sqlCMD, conn);
            cmd.Parameters.Add(new NpgsqlParameter("input_srid", input_srid));
         
            result = (byte[])cmd.ExecuteScalar();
            conn.Close();
        }
        catch (Exception msg)
        {
            Debug.Log(msg.ToString());
            result = null;

        }
        return result;
    }

    /// <summary>
    /// Connect to postgress database and retrieve data as DataTable
    /// </summary>
    /// <returns></returns>
    static DataTable GetDataTable(string sqlCMD)
    {
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        try
        {
            // Specify connection options 
            NpgsqlConnection conn = new NpgsqlConnection("Server=127.0.0.1;User Id=unity;" + "Password=pwd;Database=nyc;");
            //Open connection
            conn.Open();

            // Define a query          
            //NpgsqlCommand cmd = new NpgsqlCommand("SELECT name FROM nyc_neighborhoods WHERE boroname = 'Brooklyn'", conn);
            //NpgsqlDataAdapter da = new NpgsqlDataAdapter("SELECT name FROM nyc_neighborhoods WHERE boroname = 'Brooklyn'", conn);
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(sqlCMD, conn);

            // Close connection
            conn.Close();
            // Clean up
            conn = null;

            ds.Reset();
            da.Fill(ds); // filling DataSet with result from NpgsqlDataAdapter
            dt = ds.Tables[0]; // since it C# DataSet can handle multiple tables, we will select first

            Debug.Log("dt rows " + dt.Rows.Count);

            return dt;

        }
        catch (Exception msg)
        {
            Debug.Log(msg.ToString());
            return dt;
        }
    }


    /// <summary>
    /// Load multiple files from directory
    /// </summary>
    /// <param name="filePaths"></param>
    /// <returns></returns>
    public IEnumerator LoadAll(string[] filePaths)
    {
        foreach (string filePath in filePaths)
        {
            WWW load = new WWW("file:///" + filePath);
            yield return load;
            if (!string.IsNullOrEmpty(load.error))
            {
                Debug.LogWarning(filePath + " error");
            }
            else
            {
                //images.Add(load.texture);
            }
        }
    }

    void GenerateImage(byte[] imgData)
    {        
        Debug.Log(imgData.Length);
        int height = 2000;
        int width = 2000;
        Texture2D target = new Texture2D(height, width);
        
        target.LoadImage(imgData);       

        // In this case we would have to asign the result to a variable in order to use it later        
        byte[] rawJpgBytes = target.EncodeToPNG();
       

        // Assign the texture to the RawImage component
        image.texture = target;

    }
}
