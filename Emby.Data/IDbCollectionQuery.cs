using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public enum DbCollectionOperation { None, Select, Join, Where, GroupBy, Union, Distinct }

    public interface IDbCollectionQuery : IDbQuery, IDbView
    {
        DbCollectionOperation CurrentOperation { get; }
    }
}
