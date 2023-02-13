
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering;
using VRC.SDK3.Image;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Enums;
using VRC.Udon.Common.Interfaces;

public class World : UdonSharpBehaviour
{
    public UdonBehaviour behaviour;
    public VRCUrl URL;

    public GameObject[] Prefabs;
    public GameObject[] EntityModels;

    public Transform PrefabList;
    public Transform PrefabModelList;
    public Chunk chunkPrefab;
    public Entity EntityPrefab;

    private object[] world;


    void Start()
    {
        VRCStringDownloader.LoadUrl(URL, behaviour);

        Prefabs = new GameObject[PrefabList.childCount];

        for (int i = 0; i < PrefabList.childCount; i++)
        {
            Prefabs[i] = PrefabList.GetChild(i).gameObject;
        }

        PrefabList.gameObject.SetActive(false);
        PrefabModelList.gameObject.SetActive(false);
    }

    public override void OnStringLoadSuccess(IVRCStringDownload result)
    {
        world = JsonParser.ImportJson(result.Result).GetJsonObject("levels");

        Debug.Log("Success");
        int count = world.GetJsonCount();
        for (int i = 0; i < count; i++)
        {
            GenerateTerrain(world.GetJsonKey(i));
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log("Failure: " + result.Error + " , " + result.ErrorCode);
    }

    public void GenerateTerrain(string index)
    {
        GameObject gameObject = Instantiate(chunkPrefab.gameObject);
        Chunk chunk = gameObject.GetComponent<Chunk>();
        chunk.json = world.GetJsonObject(index);
        chunk.GenerateChunk();
        chunk.transform.SetParent(transform);
    }

}
