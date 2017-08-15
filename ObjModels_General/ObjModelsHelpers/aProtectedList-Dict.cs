using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdConta
{
    public abstract class aProtectedList<T>
    {
        public aProtectedList()
        {
            this._List = new List<T>();
        }
        public aProtectedList(List<T> items)
        {
            this._List = items;
        }

        #region fields
        protected List<T> _List;
        #endregion

        #region properties
        public int Count { get { return this._List.Count; } }
        #endregion

        #region operators
        public T this[int index]
        {
            get
            {
                T i = this._List[index];
                return i;
            }

        }
        #endregion

        #region public methods
        
        public abstract void Add(T item);
        public abstract void RemoveAt(int index);
        public abstract void Clear();
        public virtual void AddRange(IEnumerable<T> collection)
        {
            throw new NotImplementedException();
        }
        public virtual void RemoveRange(int index, int count)
        {
            throw new NotImplementedException();
        }
        public virtual aReadOnlyProtectedList<T> AsReadOnly()
        {
            throw new NotImplementedException();
        }
        public int IndexOf(T item)
        {
            return this._List.IndexOf(item);
        }
        public IEnumerable<T> GetEnumerable()
        {
            return this._List.AsEnumerable<T>();
        }
        #endregion
    }

    public abstract class aReadOnlyProtectedList<T>
    {
        public aReadOnlyProtectedList() { }

        public aReadOnlyProtectedList(List<T> list)
        {
            this._List = list;
        }

        #region fields
        protected List<T> _List;
        #endregion

        #region properties
        public int Count { get { return this._List.Count; } }
        #endregion

        #region operators
        public T this[int index]
        {
            get
            {
                T i = this._List[index];
                return i;
            }

        }
        #endregion

        #region public methods
        public virtual int IndexOf(T item)
        {
            return this._List.IndexOf(item);
        }
        public IEnumerable<T> GetEnumerable()
        {
            return this._List.AsReadOnly().AsEnumerable<T>();
        }
        #endregion
    }


    public abstract class aProtectedDict<R,T>
    {
        public aProtectedDict()
        {
            this._Dict = new Dictionary<R,T>();
        }

        public aProtectedDict(Dictionary<R,T> dict)
        {
            this._Dict = dict;
        }

        #region fields
        /// <summary>
        /// Key = value object's id
        /// </summary>
        protected Dictionary<R,T> _Dict;
        #endregion

        #region properties
        public Dictionary<R, T>.KeyCollection Keys { get { return this._Dict.Keys; } }
        public Dictionary<R, T>.ValueCollection Values { get { return this._Dict.Values; } }
        public int Count { get { return this._Dict.Count; } }
        #endregion

        #region operators
        public T this[R key]
        {
            get
            {
                T i = this._Dict[key];
                return i;
            }

        }
        #endregion

        #region public methods
        public abstract void Add(R key, T item);
        public abstract void Remove(R key);
        public abstract void Clear();
        public IEnumerable<KeyValuePair<R,T>> GetEnumerable()
        {
            return this._Dict.AsEnumerable<KeyValuePair<R, T>>();
        }
        #endregion
    }
}
