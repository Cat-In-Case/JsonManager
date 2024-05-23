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

#region ItemData Class & Struct
[System.Serializable]
public class ItemJsonData
{
    #region 생성자 ToClass
    public ItemJsonData(int uniqueID, string name, string description)
    {
        this.uniqueID = uniqueID;
        this.name = name;
        this.description = description;
    }
    public static ItemJsonData ToClass(ref ItemJsonStruct block)
    {
        return new ItemJsonData(block.uniqueID, block.name, block.description);
    }
    #endregion
    public int UniqueID { get { return uniqueID; } }
    public string Name { get { return name; } }
    public string Description { get { return description; } }
    [SerializeField] private int uniqueID;
    [SerializeField] private string name;
    [SerializeField] private string description;
}
[System.Serializable]
public struct ItemJsonStruct
{
    public int uniqueID;
    public string name;
    public string description;
}
#endregion

#region ItemJsonDictionary
[System.Serializable]
public class ItemJsonDic : BaseJDictionary<ItemJsonData>
{
    public override void FromJArray(ref JArray jarray)
    {
        ItemJsonStruct itemJsonBlock;
        foreach (JObject VALUE in jarray)
        {
            itemJsonBlock.uniqueID = VALUE.ContainsKey("uniqueID") == true ? (int)VALUE["uniqueID"] : -1;
            itemJsonBlock.name = VALUE.ContainsKey("name") == true ? (string)VALUE["name"] : "";
            itemJsonBlock.description = VALUE.ContainsKey("description") == true ? (string)VALUE["description"] : "";

            //추가에 성공한 경우 true반환
            if (this.TryAdd(itemJsonBlock.uniqueID, ItemJsonData.ToClass(ref itemJsonBlock)) == false)
            {
                //UniqueID 중복이 원인
#if UNITY_EDITOR
                Debug.Log("Same UniqueID detected in _UIElementData : " + itemJsonBlock.uniqueID + "   /   " + itemJsonBlock.name + "   /   " + itemJsonBlock.description);
#endif
            }
        }
    }
    public override void ToJArray(ref JArray jarray)
    {
        jarray.Clear();
        JObject jobject;
        foreach (KeyValuePair<int, ItemJsonData> data in this)
        {
            jobject = new JObject();
            jobject.Add("uniqueID", data.Value.UniqueID);
            jobject.Add("name", data.Value.Name);
            jobject.Add("description", data.Value.Description);
            jarray.Add(jobject);
        }
    }
}
#endregion


public class ItemJsonJArray : BaseJArray<ItemJsonDic, ItemJsonStruct, ItemJsonData>
{
    protected JArray items;

    protected const string JArray_Name = "Items";
    public override string GetJson()
    {
        return root.ToString();
    }
    public ItemJsonJArray(Action<string> logging = null)
    {
        if(logging == null)
        {
            Initialize();
        }
        else
        {
            Initialize(logging);
        }
    }
    public ItemJsonJArray(ref JObject root, ref JObject data, ref JArray array, Action<string> logging = null)
    {
        if (logging == null)
        {
            Initialize();
            Read(ref root, ref data, ref array);
        }
        else
        {
            Initialize(logging);
            Read(ref root, ref data, ref array);
        }
    }

    protected override void Initialize(Action<string> logging = null)
    {
        base.Initialize(logging);
        items = new JArray();
        data.Add(JArray_Name, items);
    }
    public override void Generate()
    {
        root = new JObject();
        data = new JObject();
        root.Add("data", data);
        items = new JArray();
        data.Add(JArray_Name, items);
    }
    public override void Read(string jsonData)
    {
        root = JObject.Parse(jsonData);
        data = root[DATA_KEY_STRING] as JObject;
        items = data.ContainsKey(JArray_Name) == true ? data[JArray_Name] as JArray : null;
        if(items.Count == 0 || items == null)
        {
            logging?.Invoke("JArray is Null");
        }
    }
    public override void Read(ref JObject root, ref JObject data)
    {
        this.root = root;
        this.data = data;
        items = data.ContainsKey(JArray_Name) == true ? data[JArray_Name] as JArray : null;
        if (items.Count == 0 || items == null)
        {
            logging?.Invoke("JArray is Null");
        }
    }
    public override void Read(ref JObject root, ref JObject data, ref JArray array)
    {
        this.root = root;
        this.data = data;
        items = array;
    }

    //Set JArray from Dictionary
    public override void FromDictionary(ref ItemJsonDic dictionary)
    {
        dictionary.ToJArray(ref this.items);
    }
    //Get Dictionary from JArray
    public override ItemJsonDic ToDictionary()
    {
        ItemJsonDic itemDic = new ItemJsonDic();
        itemDic.FromJArray(ref items);
        return itemDic;
    }
    #region JArray Function
    //Returns if uniqueID is exist
    public override bool Exist(ItemJsonStruct dialogBlock, out int index)   //Exist check 
    {
        int i = 0; index = -1; JObject target;
        foreach (JObject data in items)
        {
            if ((int)data[UNIQUEID] == dialogBlock.uniqueID)
            {
                target = data;
                index = i;
            }
            i++;
        }
        return index == -1 ? false : true;
    }

    //Get ItemJsonStruct Data from JArray (after check Exist)
    public override void Search(ref ItemJsonStruct block)   //Search by uniqueID
    {
        int i = 0;
        foreach (JObject data in items)
        {
            if ((int)data[UNIQUEID] == block.uniqueID)
            {
                block.name = (string)data["name"];
                block.description = (string)data["description"];
            }
            i++;
        }
    }

    //if uniqueID is not exist in JArray. Add ItemJsonStruct Data
    public override bool Add(ItemJsonStruct block)
    {
        int index;
        if (Exist(block, out index) == true)
        {
            logging?.Invoke("Dialogs JArray : can't add / uniqueID already exist");
            return false;
        }
        JObject itemData = new JObject();
        itemData.Add(UNIQUEID, block.uniqueID);
        itemData.Add("nameID", block.name);
        itemData.Add("dialog", block.description);
        items.Add(itemData);
        return true;
    }

    //Delete all data corresponding to uniqueID
    public override bool Delete(ItemJsonStruct block)   
    {
        int i = 0;
        Queue<int> queue = new Queue<int>();
        foreach (JObject data in items)
        {
            if ((int)data[UNIQUEID] == block.uniqueID)
                queue.Enqueue(i);
            i++;
        }
        if (queue.Count == 0)
        {
            logging?.Invoke("uniqueID is not exist");
            return false;
        }
        else
        {
            foreach (int k in queue)
            {
                items.RemoveAt(k);
            }
            return true;
        }
    }

    //Replace values if the same uniqueID is correct
    public override bool ChangeValue(ref ItemJsonStruct block)
    {
        int index = -1;
        if (Exist(block, out index) == false)
        {
            logging?.Invoke("Dialogs : uniqueID is not exist");
            return false;
        }

        if (index == -1)
        {
            Debug.Log("uniqueID is not exist");
            return false;
        }

        JObject targetObject = items[index] as JObject;
        targetObject[UNIQUEID] = block.uniqueID;
        targetObject["name"] = block.name;
        targetObject["description"] = block.description;
        return true;
    }
    #endregion
}


