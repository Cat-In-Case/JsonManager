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
using System.Text;

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

    #region
    /// <summary>
    /// <para> Hashset, HashTable, Hashmap use GetHashCode </para>
    /// <para>When A and B is same Object </para>
    /// <code> 
    /// | if(A.HashCode() == B.HashCode())
    /// |     if(A.Equals(B) == true)
    /// |         Same
    /// |     else
    /// |         Different
    /// | else
    /// |     Different
    /// </code>
    /// <para>Only the fields that enter the equals should be used in GetHashCode</para>
    /// </summary>
    /// <returns></returns>
    #endregion  Summary
    public override int GetHashCode()   
    {
        unchecked    //if Overflow occurs, wrap
        {
            int hash = 5387;
            hash += (hash * 23) + (uniqueID.GetHashCode() * 337);   //if uniqueID is same, two object is same object
            //hash += name.GetHashCode() * 941;     
            //hash += description.GetHashCode() * 281;
            return hash;
        }
    }
    public override bool Equals(object obj)     //List
    {
        if (obj is null)    //if obj is Null, return False
            return false;

        if (object.ReferenceEquals(this, obj) == true)  //Address is same(reference)
            return true;

        if (!(obj is ItemJsonData)) //클래스가 다름
            return false;
        var target = obj as ItemJsonData;

        //about string, compare(==) 
        ///     string A = "Test";      //compile time constant
        ///     string B = "Te";
        ///     B += "st";
        ///     string C = A;      //Compile time constant
        ///     string D = new StringBuilder("Test").ToString();    //result of operation
        ///     object E = C;
        /*  Cautions
         * ReferenceEquals(A, B);   //RefenceEqual == false
         * A==B;        //ValueEquel == true
         * ReferenceEquals(A, C);   //RefenceEqual == true
         * A==C;        //ValueEquel == true
         * ReferenceEquals(A, D);   //RefenceEqual == false
         * A==D;        //ValueEquel == true
         * ReferenceEquals(A, E);   //RefenceEqual == true
         * A==E;        //ValueEquel == false
        */
        //In string, == operator perform value equality instead of reference equality.
        return this.uniqueID == target.uniqueID;
        //&& (ReferenceEquals(target.name, target.name));   //if uniqueID is same, two object is same object
    }
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
    public ItemJsonDic()    {    }
    public ItemJsonDic(IEnumerable<KeyValuePair<int, ItemJsonData>> enumerable)
    {
        foreach(KeyValuePair<int, ItemJsonData> pair in enumerable)
        {
            if(this.TryAdd(pair.Key, pair.Value) == false)
            {
#if UNITY_EDITOR
                Debug.Log("ItemJsonDic Failed");
#endif
            }
        }
    }
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


public class ItemJsonJArray : BaseJArray<ItemJsonDic, ItemJsonStruct, ItemJsonData>, IDisposable
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
        itemData.Add("name", block.name);
        itemData.Add("description", block.description);
        items.Add(itemData);
        itemData = null;    //
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
            queue = null;
            logging?.Invoke("uniqueID is not exist");
            return false;
        }
        else
        {
            foreach (int k in queue)
            {
                items.RemoveAt(k);
            }
            queue.Clear();
            queue = null;
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

        targetObject = null;    //
        return true;
    }

    public void Dispose()
    {
        root = null;
        data = null;
        items = null;
        logging = null;
    }
    #endregion
}


