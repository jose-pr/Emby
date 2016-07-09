using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public class DbColumn
    {
        public string Name { get; }
        public DbType DbType { get; }
        public bool Nullable { get; }
        public int Length { get; }
        public bool Unique { get; }
        public Tuple<string,string> ForeignKey { get; }
    }
}
