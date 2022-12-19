
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

public class World : UdonSharpBehaviour
{

    private VRCImageDownloader Downloader;
    public UdonBehaviour behaviour;
    public VRCUrl URL;

    public MeshRenderer Renderer;
    public MeshFilter filter;
    public MeshCollider collider;

    private Vector3[][] HeightLookup = new Vector3[][] 
    {
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1)},
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1)},
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 1, 1)},
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 1, 1)},
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1)},
        new Vector3[]{ new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1)},
        new Vector3[]{ new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1)},
        new Vector3[]{ new Vector3(0, 1, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1), new Vector3(1, 0, 1)},
        new Vector3[]{ new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 1), new Vector3(1, 0, 1)},
    };

    private Vector2[][] UVLookup = new Vector2[][]
    {
        new Vector2[]{ new Vector2(0.4f, 0.6f), new Vector2(0.6f, 0.6f), new Vector2(0.4f, 0.4f), new Vector2(0.6f, 0.4f) },
    };

    private int[][] TriLookup = new int[][]
    {
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 2, 1, 1, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 2, 1, 1, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3},
        new int[]{0, 3, 1, 0, 2, 3}
    };

    public Texture2D DownloadedTexture;

    /// <summary>
    /// Section
    /// </summary>
    private Color32[] pixels;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] tris;
    private int indexVert = 0;
    private int indextri = 0;
    private int indexUV = 0;
    private int width;
    private int height;
    private Vector3 offset;
    private int y;

    void Start()
    {
        Downloader = new VRCImageDownloader();

        Downloader.DownloadImage(URL, null, behaviour);
    }

    public void OnImageLoadSuccess(IVRCImageDownload result)
    {
        DownloadedTexture = result.Result;
        pixels = DownloadedTexture.GetPixels32();
        Debug.Log("Success");
        GenerateTerrain();
    }

    public void OnImageLoadError(IVRCImageDownload result)
    {
        Debug.Log("Failiure: " + result.Error + " , " + result.ErrorMessage);
    }

    public void GenerateTerrain()
    {
        width = DownloadedTexture.width;
        height = DownloadedTexture.height;
        offset = new Vector3(width, 0, height) / 2;

        int size = 4 * width * height; 

        vertices = new Vector3[size];
        uvs = new Vector2[size];
        tris = new int[6 * width * height];

        indexVert = 0;
        indextri = 0;
        indexUV = 0;

        y = 0;

        SendCustomEventDelayedFrames("ProcessWidthDeffered", 1, EventTiming.Update);
    }

    public void ProcessWidthDeffered()
    {
        for (int x = 0; x < width; x++)
        {
            int index = x + y * width;
            int green = pixels[index].g;
            int value = (green & 0b00001111);
            int color = green >> 4;
            int heightCel = pixels[index].r;
            Vector2 colorOffset = new Vector2(color & 0b0011, (color & 0b1100) >> 2) / 4;

            Vector3[] vertToAdd = HeightLookup[value];
            Vector2[] UVToAdd = UVLookup[0];
            int[] TriToAdd = TriLookup[value];

            for (int j = 0; j < TriToAdd.Length; j++)
            {
                tris[indextri] = indexVert + TriToAdd[j];
                indextri++;
            }

            for (int j = 0; j < vertToAdd.Length; j++)
            {
                vertices[indexVert] = vertToAdd[j] + new Vector3(x, heightCel, y) - offset;
                indexVert++;
            }

            for (int j = 0; j < UVToAdd.Length; j++)
            {
                uvs[indexUV] = (UVToAdd[j] / 4f) + colorOffset;
                indexUV++;
            }
        }

        y++;

        if(y < height)
        {
            SendCustomEventDelayedFrames("ProcessWidthDeffered", 1);
        }
        else
        {
            Mesh mesh = filter.sharedMesh;

            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
            }

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            filter.sharedMesh = mesh;
            collider.sharedMesh = mesh;
        }
    }

    public void OnDestroy()
    {
        Downloader.Dispose();
    }
}
