using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public enum DbQueryType { Count, Reader, Scalar };
    public interface IDbQuery
    {
        IDb Db { get; }
        DbQueryType QueryType { get; }
        string CommandText { get; }
    }
}
