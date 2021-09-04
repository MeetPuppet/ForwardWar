//
//POINT CACHE READER
//
//Created By: Dejan Omasta
//email: list3ner@gmail.com
//
//=================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
 
public class PointCacheReader : MonoBehaviour 
{
    //
    //POINT CACHE Vs
    //
    public string pointCacheFilePath;
    
    struct PointCacheFile
    {
        public char[] signature;
        public int fileVersion;
        public int numPoints;
        public float startFrame;
        public float sampleRate;
        public int numSamples;
        public List<Vector3> vertexCoords;
    }
    
    PointCacheFile pcFile;
    bool fileParsed = false;
    //
    //VERTEX POS Vs
    //
    public float fps = 24.0f;
    public MeshFilter mf;
    Mesh mesh;
    Vector3[] vertices;
    int counter = 0;
    int curFrame = 0;
    public int startFrame = 0;
    float timeMeasure = 0.0f;
    
    // Use this for initialization
    void Start () 
    {
        pcFile = new PointCacheFile();
        ParsePCFile();
        timeMeasure = 1 / fps;
        mesh = mf.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        curFrame = startFrame;
        counter = vertices.Length * curFrame;
    }
    
    void OnBecameVisible()
    {
        if(!IsInvoking("PlayLoop") && fileParsed)
        {
            InvokeRepeating("PlayLoop", 0.01f, timeMeasure);
        }
    }
    
    void OnBecameInvisible()
    {
        if(IsInvoking("PlayLoop"))
        {
            CancelInvoke("PlayLoop");
        }
    }

    void PlayLoop()
    {
        for (int x = 0; x < vertices.Length; x++)
        {
            vertices[x] = pcFile.vertexCoords[counter];
            counter++;
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        curFrame++;
        if (curFrame == pcFile.numSamples)
        {
            curFrame = 0;
            counter = 0;
        }        
    }
 
    void ParsePCFile()
    {
        FileStream fs = new FileStream(pointCacheFilePath, FileMode.Open);
        BinaryReader binReader = new BinaryReader(fs);
        //
        //SIGNATURE
        //
        pcFile.signature = new char[12];
        pcFile.signature = binReader.ReadChars(12);
        //
        //FILE VERSION
        //
        pcFile.fileVersion = binReader.ReadInt32();
        //
        //NUMBER OF POINTS
        //
        pcFile.numPoints = binReader.ReadInt32();
        //
        //START FRAME
        //
        pcFile.startFrame = binReader.ReadSingle();
        //
        //SAMPLE RATE
        //
        pcFile.sampleRate = binReader.ReadSingle();
        //
        //NUMBER OF SAMPLES
        //
        pcFile.numSamples = binReader.ReadInt32();
        //
        //GET VERTEX COORDS
        //
        pcFile.vertexCoords = new List<Vector3>();
        for (int i = 0; i < pcFile.numSamples; i++)
        {
            for (int x = 0; x < pcFile.numPoints; x++)
            {
                Vector3 vPos = new Vector3();
                vPos.x = binReader.ReadSingle();
                vPos.y = binReader.ReadSingle();
                vPos.z = binReader.ReadSingle();
                pcFile.vertexCoords.Add(vPos);
            }
        }
        
        fileParsed = true;
        fs.Close();
    }
}