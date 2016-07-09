using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public interface IDbUpdatableView : IDbView
    {
        IDbQuery Insert(string[] column);
        IDbQuery Update(string[] column);
        IDbQuery Delete();
    }
}
