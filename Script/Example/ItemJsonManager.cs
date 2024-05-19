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


[Serializable]
public class ItemJsonControl
{
    public ItemJsonControl(ref ItemJsonBlock jsonBlock, ref ItemJsonDic jsonDic)
    {
        this.jsonBlock = jsonBlock;
        jsonDictionary = jsonDic;
    }

    [SerializeField] private ItemJsonBlock jsonBlock;
    [SerializeField] private ItemJsonDic jsonDictionary;

    [SerializeField] private Action<string> log;

    public JObjectState Exist(ItemJsonStruct block)
    {
        int index;
        if (jsonDictionary.ContainsKey(block.uniqueID) == true)
        {
            if (jsonBlock.Exist(block, out index) == true)  //Exist
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
            if (jsonBlock.Exist(block, out index) == true)  //Error
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
        jsonBlock.Search(ref block);
    }

    public bool Add(ref ItemJsonStruct block)
    {
        if (jsonDictionary.ContainsKey(block.uniqueID) == true)
        {
            log?.Invoke("Dialogs Dictionary : can't add / uniqueID already exist");
            return false;
        }
        jsonDictionary.Add(block.uniqueID, ItemJsonData.ToClass(ref block));
        jsonBlock.Add(block);
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
        jsonBlock.Delete(block);
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
        return jsonBlock.ChangeValue(ref block);
    }
}


//OnEditor
public class ItemJsonManager : MonoBehaviour
{


}
