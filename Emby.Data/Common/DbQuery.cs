using Emby.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data.Common
{
    public class DbQuery : IDbQuery
    {
        public DbQuery(string commandText, DbQueryType queryType, IDb db)
        {
            CommandText = commandText;
            QueryType = queryType;
            Db = db;
        }
        public DbQuery(IDbQuery query)
        {
            if (query == null)
            {
                CommandText = "";
                QueryType = DbQueryType.Reader;
                Db = null;
            }
            else
            {
                CommandText = query.CommandText;
                QueryType = query.QueryType;
                Db = query.Db;
            }
        }
        public string CommandText
        {
            get;  set;
        }

        public IDb Db
        {
            get; set;
        }

        public DbQueryType QueryType
        {
            get; set;
        }
        public DbQuery FormatCmdTxt(params string[] args)
        {
            CommandText = String.Format(CommandText, args);
            return this;
        }
    }
}
