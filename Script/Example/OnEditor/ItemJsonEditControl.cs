#region Using
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomDictionary.SerializableDictionary;
#endregion



/// <summary>
/// <para>ItemJsonBlock : JArray Function</para>
/// <para>ItemJsonDic : Dictionary Function</para>
/// <para>Only on editor. Provide manipulation function</para>
/// </summary>
[Serializable]
public class ItemJsonEditControl
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
}