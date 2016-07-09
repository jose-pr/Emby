using MediaBrowser.Model.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Common.Data
{
    public enum QueryCmd { NonQuery, Reader, Scalar };
    public class Query
    {
        public string Cmd { get; private set; }
        public string ErrorMsg { get; set; }
        public string SuccessMsg { get; set; }
        public List<QueryParams> Parameters { get; protected set; }
        public QueryCmd CmdType { get; set; }
        public CommandBehavior CmdBehavior { get; set; }
        public Type TResult = null; 

        public Query(string cmdTxt, QueryCmd cmdType, CommandBehavior? cmdBehaviour = null)
        {
            Cmd = cmdTxt;
            CmdType = cmdType;
            CmdBehavior = cmdBehaviour ?? CommandBehavior.Default;
            Parameters = new List<QueryParams>();
        }
        public void SetCmd(string cmdTxt, QueryCmd? cmdType = null)
        {
            Cmd = cmdTxt;
        }
        public void FormatCmdTxt(params string[] args)
        {
            Cmd = String.Format(Cmd, args);
        }
        public Query Clone()
        {
            var clone = new Query(Cmd, CmdType) {
                CmdBehavior = CmdBehavior,
                ErrorMsg = ErrorMsg,
                SuccessMsg = SuccessMsg,
                Parameters = Parameters
            };
            return clone;
        }

        public void AddParameter<T>(string id, T value, DbType type)
        {
            var param = Parameters.FirstOrDefault(p => p.Id == id);
            if (param == null)
            {
                param = new QueryParams(id, type);
                Parameters.Add(param);
            }
            
            param.Add(value);
        }
    }

    public class QueryParams : IEnumerable<Object>
    {
        public string Id { get; set; }
        public DbType Type { get; set; }
        private List<Object> _values;
        
        public QueryParams(string id, DbType type)
        {
            Id = id;
            Type = type;
            _values = new List<Object>();            
        }
        public Object this[int index]
        {
            get
            {
                return _values[index];
            }
        }
        public void Clear()
        {
            _values.Clear();
        }
        public void Add(object value)
        {
            _values.Add(value);
         //   Type = Type ;
        }
        public void AddRange<T>(IEnumerable<T> values)
        {
            _values.AddRange(values.Cast<Object>());
          //  Type = Type ?? typeof(T).GetDbType();
        }
        public IEnumerator<object> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
