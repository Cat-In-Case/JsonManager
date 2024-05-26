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

//Read Json on Runtime
public abstract class BaseJsonRuntimeParser<DICTIONARY, CLASS> : IDisposable where DICTIONARY : BaseJDictionary<CLASS>,  new()
{
    protected const string DATA_KEY_STRING = "data";
    protected BaseJsonRuntimeParser()    {    }
    protected JObject root; protected JObject data;
    [SerializeField] protected string json { get; set; }
    public virtual async Task<DICTIONARY> Read()
    {
        await Task.Delay(500);
        return  null;
    }
    public abstract Task<DICTIONARY> ToDictionary(JArray array);
    public virtual void Dispose()    {    }
}


#region File
public class AsyncFileIO : IDisposable
{
    public async Task<bool> FileSave(string path, string fileName, string jsonData)
    {

        using (FileStream fs = new FileStream(string.Format("{0}/{1}", path, fileName), FileMode.Create))
        {
            var t = Task<string>.Run(async () =>
            {
                byte[] data = Encoding.UTF8.GetBytes(jsonData);
                fs.Write(data, 0, data.Length);
            });

            await t;
#if UNITY_EDITOR
            Debug.Log(Thread.CurrentThread.ManagedThreadId);
#endif
            if (t.IsFaulted == true)
            {
                return false;
            }
            else
            {
                if (t.IsCompletedSuccessfully == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    public async Task<string> FileLoad(string path, string fileName)
    {
        string jsonText = "";
        using (FileStream fs = new FileStream(string.Format("{0}/{1}", path, fileName), FileMode.Open))
        {
            var t = Task<string>.Run(async () =>
            {
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                jsonText = Encoding.UTF8.GetString(data);
            });
            await t;

            if (t.IsFaulted == true)
            {
                Console.WriteLine(t.Exception.Message);
                jsonText = "Error";
            }
        }
#if UNITY_EDITOR
        Debug.Log(Thread.CurrentThread.ManagedThreadId);
#endif
        return jsonText;
    }

    public Task<bool> FileExist(string path, string fileName)
    {
        if (File.Exists(string.Format("{0}/{1}", path, fileName)) == true)
        {
#if UNITY_EDITOR
            Debug.Log(Thread.CurrentThread.ManagedThreadId + "   true");
#endif
            return Task.FromResult(true);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log(Thread.CurrentThread.ManagedThreadId +  "   false");
#endif
            return Task.FromResult(false);
        }
    }



    public void Dispose()
    {

    }
}
#endregion
