#define Debug
#if UNITY_EDITOR
using UnityEditor;
#endif
#region Using
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
#endregion

//OnEditor
public class ItemJsonEditor : MonoBehaviour
{
    [SerializeField] private ItemJsonEditControl jsonControl;
    [SerializeField] private string path;
    private const string JSONNAME = "item";
    private void Awake()
    {
        path = Application.dataPath;
    }

#if DEBUG
    private void OnGUI()
    {
        if(GUI.Button(new Rect(Screen.currentResolution.width-100, Screen.currentResolution.height-60, 100, 60), "ExampleLoad"))
        {
            DebugLoad();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width - 200, Screen.currentResolution.height-60, 100, 60), "ExampleSave"))
        {
            DebugSave();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width-100, Screen.currentResolution.height-100, 100, 40), "Add"))
        {
            Add();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width-200, Screen.currentResolution.height-100, 100, 40), "Delete"))
        {
            Delete();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width - 100, Screen.currentResolution.height - 140, 100, 40), "Search"))
        {
            Search();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width-200, Screen.currentResolution.height-140, 100, 40), "Apply"))
        {
            Apply();
        }
        if (GUI.Button(new Rect(Screen.currentResolution.width - 100, Screen.currentResolution.height - 180, 100, 40), "Sort"))
        {
            jsonControl.Sort();
        }
    }
#endif

#if UNITY_EDITOR
    #region Control Function
    [SerializeField] private JObjectState objectState;
    [SerializeField] private ItemJsonStruct getted;
    [SerializeField] private ItemJsonStruct jsonStruct;

    private void isExist()
    {
        objectState = jsonControl.Exist(ref jsonStruct);
    }
    private void Search()
    {
        isExist();
        if (objectState == JObjectState.Exist)
        {
            Debug.Log("Exist");
            jsonControl.Search(ref getted);
        }
        else if(objectState == JObjectState.Empty)
        {
            Debug.Log("Not exist");
            getted.uniqueID = -1;            getted.name = "";            getted.description = "";
        }
    }
    private void Add()
    {
        isExist();
        if (objectState != JObjectState.Empty)
            return;
        if (jsonControl.Add(ref jsonStruct) == true)
        {
            getted = jsonStruct;
            Debug.Log("Added : " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
        }

        else
            Debug.Log("Add Failed : " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
    }
    private void Delete()
    {
        isExist();
        if (objectState != JObjectState.Exist)
            return;
        if (jsonControl.Delete(ref jsonStruct) == true)
        {
            getted.uniqueID = -1;            getted.name = "";            getted.description = "";
            Debug.Log("Delete : " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
        }
        else
            Debug.Log("Delete Failed : " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
    }
    private void Apply()
    {
        isExist();
        if (objectState != JObjectState.Exist)
            return;
        if (jsonControl.Apply(ref jsonStruct) == true)
        {
            Debug.Log("Value apply : " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
            Search(); 
        }
        else
            Debug.Log("Failed value apply: " + jsonStruct.uniqueID + jsonStruct.name + jsonStruct.description);
    }
    #endregion
#endif

    private void DebugLoad()
    {
        StartCoroutine(Load());
    }

    private IEnumerator Load()
    {
        string jsonData = "";
        bool loaded = false;
        using (AsyncFileIO io = new AsyncFileIO())
        {
            Task<bool> isExist = Task<bool>.Run(() => io.FileExist(path, JSONNAME + ".json"));
            yield return new WaitUntil(() => isExist.IsCompleted);

            if (isExist.Result == false)
            {
                isExist.Dispose();
#if UNITY_EDITOR
                Debug.Log("Json does not exist ");
#endif
            }

            if (isExist.Result == true)
            {

                Task<string> fileLoad = Task<string>.Run(() => io.FileLoad(path, JSONNAME + ".json"));
                yield return new WaitUntil(() => fileLoad.IsCompleted);

                if (fileLoad.IsFaulted == true)
                {
#if UNITY_EDITOR
                    Debug.Log("File load faulted ");
#endif
                    fileLoad.Dispose();
                    yield break;
                }
                jsonData = fileLoad.Result;
                loaded = true;
                fileLoad.Dispose();

                isExist.Dispose();
            }
            else
            {
                isExist.Dispose();
            }
        }

        ItemJsonJArray jarray = new ItemJsonJArray();
        ItemJsonDic dic;
        if (loaded == true)
        {
            JObject root = JObject.Parse(jsonData);
            if (root.ContainsKey("data") == false)
            {
                yield break;
            }
            JObject data = root["data"] as JObject;
            jarray.Read(ref root, ref data);

            dic = jarray.ToDictionary();
            Debug.Log("Json Loaded");
        }
        else
        {
            Debug.Log("Json Generated");
            jarray.Generate();
            dic = jarray.ToDictionary();
        }

        jsonControl = new ItemJsonEditControl(ref jarray, ref dic);
    }

    private void DebugSave()
    {
        StartCoroutine(Save());
    }

    private IEnumerator Save()
    {
        using (FileIO io = new FileIO())
        {
            Task<bool> save = Task<bool>.Run(() => io.FileSave(path, JSONNAME + ".json", jsonControl.GetJson));

            yield return new WaitUntil(() => save.IsCompleted);

            if (save.IsFaulted)
            {
                save.Dispose();
                Debug.Log("Save Fault");
                yield break;
            }
            else
            {
                Debug.Log("Save Success");
            }
            save.Dispose();
        }
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
        GC.Collect(2, GCCollectionMode.Optimized);
    }


    private void OnDestroy()
    {
        jsonControl.Dispose();
        jsonControl = null;
        path = null;
    }
}
