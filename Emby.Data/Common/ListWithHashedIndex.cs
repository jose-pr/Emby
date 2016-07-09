using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data.Common
{
    public class ObjectNotUnique : Exception { }
    class ListMultipleIndex<T> : IList<T>
    {
        private List<T> _list;
        PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
        private Dictionary<Object, int> keyIndexMap;
        private string[] _keys;

        public ListMultipleIndex() {
            Init();
        }

        public ListMultipleIndex(string key)
        {
            Init();
            _keys = new string[] { key };
        }
        public ListMultipleIndex(string[] keys)
        {
            Init();
            _keys = keys;
        }
        private void Init()
        {
            keyIndexMap = new Dictionary<object, int>();
        }

        private bool set(T value,int index = -1, string key = null)
        {
            var check = -1;
            string[] keys = key == null ? _keys : new string[] { key };

            for (var l = 0; l < keys.Length; l++)
            {
                var k = keys[l];
                check = findIndex(props[k].GetValue(value),k);
                if(check >= 0) { break; }
            }

            if (check >= 0 && check != index)
            {
                throw new ObjectNotUnique();
            }
            return false;
        }
        private void updateMap(T value, int index)
        {
            for (var l = 0; l < _keys.Length; l++)
            {
                var k = _keys[l];
                keyIndexMap[props[k].GetValue(value)] = index;
            }
        }

        private int findIndex(Object keyVal, string key = null)
        {
            int check = -1;
            if (key == null || _keys.Contains(key))
            {
                if(!keyIndexMap.TryGetValue(keyVal, out check))
                {
                    return -1;
                }
            }else
            {
                for (var l = 0; l < _keys.Length; l++)
                {
                    check = _list.FindIndex(i => props[key].GetValue(i) == keyVal);
                    if (check >= 0) { break; }
                }

            }
            return check;
        }

        public T this[int index]
        {
            get {   return _list[index]; }

            set {
                set(value, index);
                _list[index] = value;
                updateMap(value, index);
            }
        }

        public T this[Object keyVal]
        {
            get
            {
                var check = findIndex(keyVal);
                return check >= 0 ? _list[check] : default(T);
            }
        }

        public T this[Object keyVal, string key]
        {
            get
            {
                var check = findIndex(keyVal, key);
                return check >= 0 ? _list[check] : default(T);
            }
        }

        public int Count    {   get {   return _list.Count; }  }

        public bool IsReadOnly { get { return false; } }

        public void Add(T item)
        {
            set(item);
            _list.Add(item);
            updateMap(item, _list.Count - 1);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public void CleanDefault()
        {
            _list.RemoveAll(i => i.Equals(default(T)));
        }

        public bool Contains(T item)
        {
           return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            set(item);
            _list.Insert(index, item);
            foreach (var key in keyIndexMap.Keys)
            {                
                if (keyIndexMap[key] >= index) { keyIndexMap[key]++; }
            }
            updateMap(item, index);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    public static class IEnumerableExtensions {


    }
}
