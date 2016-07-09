using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public class IDbIndex
    {
        public string Name;
        public IEnumerable<IDbColumn> Columns;
        public bool Unique = false;
        public string Where = "";
    }
}
