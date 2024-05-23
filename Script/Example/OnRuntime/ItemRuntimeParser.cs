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
/// <para>On Runtime. Not provide manipulation function</para>
/// <para>런타임용. 조작어 제공 안함</para>
/// </summary>
public class ItemRuntimeParser : BaseJsonRuntimeParser<ItemJsonDic, ItemJsonData>
{
    private const string ITEM_KEY_STRING = "items";
    private JArray items;
    [SerializeField] private bool rootLoaded = false;
    public ItemRuntimeParser(string jsonData)
    {
        json = jsonData;
        rootLoaded = false;
    }
    public ItemRuntimeParser(ref JObject root, ref JObject data)
    {
        this.root = root;
        this.data = data;
        rootLoaded = true;
    }
    public override async Task<ItemJsonDic> Read()
    {
        if(rootLoaded == false)
        {
            root = JObject.Parse(json);

            if (data.ContainsKey(DATA_KEY_STRING) == false)
                return await Task.FromException<ItemJsonDic>(new Exception("root/data is broken"));
            data = root[DATA_KEY_STRING] as JObject;
        }

        if (data.ContainsKey(ITEM_KEY_STRING) == false)
            return await Task.FromException<ItemJsonDic>(new Exception("root/data/items is broken"));
        items = data[ITEM_KEY_STRING] as JArray;

        ItemJsonDic block;
        Task<ItemJsonDic> itemTask = Task<ItemJsonDic>.Run(() => ToDictionary(items));
        await itemTask;

        if (itemTask.IsFaulted == false)
            return block = itemTask.Result;
        else
            return await Task.FromException<ItemJsonDic>(new Exception("Dictionary convert is faulted"));
    }
    public override Task<ItemJsonDic> ToDictionary(JArray array)
    {
        ItemJsonDic block = new ItemJsonDic();
        block.FromJArray(ref array);
        return Task.FromResult(block);
    }
    public override void Dispose()
    {
        root.RemoveAll();
        data.RemoveAll();
        items.Clear();
        root = null;
        data = null;
        items = null;
        json = "";
        json = null;
    }
}
