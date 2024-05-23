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

#region Base
[System.Serializable]
public abstract class BaseJDictionary<T> : SerializableDictionary<int, T>
{
    //Base Dictionary for JsonParse
    public abstract void FromJArray(ref JArray jarray);
    public abstract void ToJArray(ref JArray jarray);
}
[System.Serializable]
public abstract class BaseJArray<DICTIONARY, STRUCT, ClASS> where DICTIONARY : BaseJDictionary<ClASS>
{
    protected const string DATA_KEY_STRING = "data";
    protected const string UNIQUEID = "uniqueID";
    protected JObject root;
    protected JObject data;
    protected Action<string> logging;

    protected virtual void Initialize(Action<string> logging = null)
    {
        root = new JObject();
        data = new JObject();
        if (logging != null)
        {
            this.logging = logging;
        }
    }

    //Generate Base
    public virtual void Generate()
    {
        root = new JObject();
        data = new JObject();
        root.Add("data", data);
    }

    //Read JsonString
    public abstract void Read(string jsonData); //Use only one JArray

    //Use multiple JArray from one root + array Check
    public abstract void Read(ref JObject root, ref JObject data);

    //Use multiple JArray from one root
    public abstract void Read(ref JObject root, ref JObject data, ref JArray array);

    #region Object Convert
    public abstract void FromDictionary(ref DICTIONARY dictionary);
    public abstract DICTIONARY ToDictionary();
    #endregion

    #region JArray Function
    public abstract bool Exist(STRUCT block, out int index);
    public abstract void Search(ref STRUCT block);
    public abstract bool Add(STRUCT block);
    public abstract bool Delete(STRUCT block);
    public abstract bool ChangeValue(ref STRUCT block);
    #endregion

    public virtual string GetJson() => root.ToString();
}
#endregion

