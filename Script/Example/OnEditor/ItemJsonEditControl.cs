#region Using
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomDictionary.SerializableDictionary;
#endregion
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// <para>ItemJsonBlock : JArray Function</para>
/// <para>ItemJsonDic : Dictionary Function</para>
/// <para>Only on editor. Provide manipulation function</para>
/// </summary>
[Serializable]
public class ItemJsonEditControl : IDisposable
{
    public ItemJsonEditControl(ref ItemJsonJArray jsonArray, ref ItemJsonDic jsonDic)
    {
        this.jsonArray = jsonArray;
        jsonDictionary = jsonDic;
    }

    [SerializeField] private ItemJsonJArray jsonArray;
    [SerializeField] private ItemJsonDic jsonDictionary;
    public string GetJson => jsonArray.GetJson();
    [SerializeField] private Action<string> log;

    public JObjectState Exist(ref ItemJsonStruct block)
    {
        int index;
        if (jsonDictionary.ContainsKey(block.uniqueID) == true)
        {
            if (jsonArray.Exist(block, out index) == true)  //Exist
            {
                log?.Invoke(block.uniqueID + " is Exist"); return JObjectState.Exist;
            }
            else                                             //Error
            {
                log?.Invoke(block.uniqueID + " : Exist on Dictionary. but not JArray"); return JObjectState.Error;
            }
        }
        else
        {
            if (jsonArray.Exist(block, out index) == true)  //Error
            {
                log?.Invoke(block.uniqueID + " : Not exist on Dictionary. exist on JArray"); return JObjectState.Error;
            }
            else                                            //Empty
            {
                log?.Invoke(block.uniqueID + "  is Not Exists"); return JObjectState.Empty;
            }
        }
    }
    public void Search(ref ItemJsonStruct block)
    {
        jsonArray.Search(ref block);
    }

    public bool Add(ref ItemJsonStruct block)
    {
        if (jsonDictionary.ContainsKey(block.uniqueID) == true)
        {
            log?.Invoke("Dialogs Dictionary : can't add / uniqueID already exist");
            return false;
        }
        jsonDictionary.Add(block.uniqueID, ItemJsonData.ToClass(ref block));
        jsonArray.Add(block);
        return true;
    }
    public bool Delete(ref ItemJsonStruct block)
    {
        if (jsonDictionary.ContainsKey(block.uniqueID) == false)
        {
            log?.Invoke("Dialogs Dictionary : can't delete / uniqueID is not exist");
            return false;
        }
        jsonDictionary.Remove(block.uniqueID);
        jsonArray.Delete(block);
        return true;
    }
    public bool Apply(ref ItemJsonStruct block)
    {
        if (jsonDictionary.ContainsKey(block.uniqueID) == false)
        {
            log?.Invoke("Dialogs Dictionary : can't change / uniqueID is not exist");
            return false;
        }
        jsonDictionary[block.uniqueID] = ItemJsonData.ToClass(ref block);
        return jsonArray.ChangeValue(ref block);
    }

    public void Sort()
    {
#if UNITY_EDITOR
        Debug.Log("When sort start : " + GC.GetTotalMemory(false));
#endif
        ClearDictionary(ref jsonDictionary);
        GC.Collect(2, GCCollectionMode.Optimized);
#if UNITY_EDITOR
        Debug.Log("After clear origin Dictionary : " + GC.GetTotalMemory(false));
#endif

        jsonDictionary = jsonArray.ToDictionary();  //재생성 및 할당
        jsonDictionary.SortByKey();
        jsonArray.FromDictionary(ref jsonDictionary);
        GC.Collect(0, GCCollectionMode.Forced);
#if UNITY_EDITOR
        Debug.Log("After new allocate Dictionary : " + GC.GetTotalMemory(false));
#endif
    }

    public void Compare(ref ItemJsonJArray target, out IEnumerable<int> O_Differ_T, out IEnumerable<int> T_Differ_O)
    {
#if UNITY_EDITOR
        Debug.Log("Before Compare GC Collect : " + GC.GetTotalMemory(false));
        AssetDatabase.Refresh();
#endif
        ItemJsonDic targetDic = target.ToDictionary();
        targetDic.SortByKey();
        var targetKeys = targetDic.Keys.ToList();    //Key is int. Compare keys is faster

        var originDic = new ItemJsonDic(jsonDictionary.GetSortByKey());
        var originKeys = originDic.Select(x => x.Key).ToList();

        HashSet<int> ids = new HashSet<int>(targetKeys);
        O_Differ_T = originKeys.Where(x => !ids.Contains(x));

        ids = new HashSet<int>(originKeys);
        T_Differ_O = targetKeys.Where(x => !ids.Contains(x));

        ClearDictionary(ref targetDic);
        ClearDictionary(ref originDic);
        ids.Clear();        ids = null;
        targetDic.Clear();        targetDic = null;

        GC.Collect(2, GCCollectionMode.Optimized);
#if UNITY_EDITOR
        Debug.Log("After Compare GC Collect : " + GC.GetTotalMemory(false));
#endif
    }

    public void ClearDictionary(ref ItemJsonDic target)
    {
        Type type = typeof(ItemJsonData);
        FieldInfo nameInfo = type.GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo descriptionInfo = type.GetField("description", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (KeyValuePair<int, ItemJsonData> pair in target)
        {
            nameInfo.SetValue(pair.Value, null);
            descriptionInfo.SetValue(pair.Value, null);
        }
        target.Clear();        target = null;
        nameInfo = null;        descriptionInfo = null;        type = null;
    }

    public void Dispose()
    {
        jsonDictionary.Clear();
        jsonDictionary = null;
        log = null;
        jsonArray.Dispose();
    }
}