using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace CustomDictionary.SerializableDictionary
{
    [System.Serializable]
    public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {

        [SerializeField]
        private List<K> keys = new List<K>();

        [SerializeField]
        private List<V> values = new List<V>();


        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach(KeyValuePair<K, V> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            this.Clear();
            for(int i = 0,  icount = keys.Count; i < icount; ++i)
            {
                this.Add(keys[i], values[i]);
            }
        }

        public IEnumerable<KeyValuePair<K,V>> GetKeyValuePairs()
        {
            return this.AsEnumerable();
        }

        public void SortByKey()
        {
            var dic = this.OrderBy(key => key.Key);

            this.Clear();

            foreach(KeyValuePair<K,V> pair in dic)
            {
                if (this.TryAdd(pair.Key, pair.Value) == false)
                {
#if UNITY_EDITOR
                    Debug.Log("Sorting Add Error" + pair.Key + " " + pair.Value);
#endif
                }
            }

        }
    }
}