using Emby.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data.Common
{
    public abstract class Operator
    {
        
    }

    public static class DbExtensions
    {
        public static string GetExpessionString(this IDbCollectionQuery collectionQuery, DbLogicalExpression expression)
        {
            return collectionQuery.Db.GetExpressionString(expression);
        }

        public static DbQuery Clone(this IDbQuery query)
        {
            return new DbQuery(query);
        }

        public static IReadOnlyList<T> Execute<T>(this IDbQuery query)
        {
            return query.Db.Excute<T>(query);
        }
    }

}
