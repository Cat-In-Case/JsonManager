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


//OnRuntime

//Inferace inherited from Class with JsonText
public interface IJsonText
{
    public void Refresh();
}

/// <summary>
/// ItemJsonDic, ItemParser
/// </summary>
public class ItemJsonManager : MonoBehaviour
{

    [SerializeField] private ItemJsonDic dicionary;
    public bool GetItemData(int uniqueID, ref ItemJsonData data)
    {
        if(dicionary.ContainsKey(uniqueID) == true)
        {
            data = dicionary[uniqueID]; 
            return true;
        }
        else
            return false;
    }
    [SerializeField] private string path;
    private const string JSONNAME = "item";

    //
    private List<IJsonText> iJsonTexts;

    private void Awake()
    {
        path = Application.dataPath;
    }

    public void Load()
    {
        StartCoroutine(_Load());
    }

    private IEnumerator _Load()
    {
        string jsonData;
        using (AsyncFileIO io = new AsyncFileIO())
        {
            Task<bool> isExist = Task<bool>.Run(() => io.FileExist(path, JSONNAME));
            yield return new WaitUntil(() => isExist.IsCompleted);

            if(isExist.Result == false)
            {
                isExist.Dispose();
#if UNITY_EDITOR
                Debug.Log("Json does not exist ");
#endif
                yield break;
            }
            isExist.Dispose();

            Task<string> fileLoad = Task<string>.Run(() => io.FileLoad(path, JSONNAME));
            yield return new WaitUntil(() => fileLoad.IsCompleted);

            if(fileLoad.IsFaulted == true)
            {
#if UNITY_EDITOR
                Debug.Log("File load faulted ");
#endif
                fileLoad.Dispose();
                yield break;
            }
            jsonData = fileLoad.Result;
            fileLoad.Dispose();
        }

        using(ItemRuntimeParser parser = new ItemRuntimeParser(jsonData))
        {
            Task<ItemJsonDic> parse = Task<ItemJsonDic>.Run(() => parser.Read());
            yield return new WaitUntil(() => parse.IsCompleted);

            if(parse.IsFaulted == true || parse.Result == null)
            {
#if UNITY_EDITOR
                Debug.Log("Parsing is faulted. check json file. ");
#endif
                parse.Dispose();
                yield break;
            }

            dicionary = parse.Result;
            parse.Dispose();
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach(IJsonText ui in iJsonTexts)
        {
            ui.Refresh();
        }
    }
}
