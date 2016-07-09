using Emby.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public interface IDb
    {
        IDbView GetView(string name);
        IDbQuery CreateQuery(string commandText, DbQueryType queryType);
        IDbCollectionQuery CreateCollectionQuery(DbQuery query, DbCollectionOperation operation);
        IReadOnlyList<T> Excute<T>(IDbQuery query);
        string GetExpressionString(DbLogicalExpression expression);
    }
}
