using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public interface IDbView
    {
        IDbCollectionQuery Join(string view, DbLogicalExpression on);
        IDbCollectionQuery Join(string[] fields);
        IDbCollectionQuery Select(string[] fields);
        IDbCollectionQuery Distinct(string[] fields);
        IDbCollectionQuery Where(DbLogicalExpression where);
        IDbCollectionQuery GroupBy(string[] groupBy, DbLogicalExpression having);
    }

}
