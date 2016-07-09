using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public interface IDbTable : IDbUpdatableView
    {
        IReadOnlyList<DbColumn> Columns { get; }
        IReadOnlyList<IDbIndex> Indexes { get; }
        string Name { get; }
        string[] PrimaryKey { get; }

        IDbQuery AddIndex(IDbIndex index);
        IDbQuery AddColumn(DbColumn field);
        IDbQuery AlterColumn(string fieldName, DbColumn field);
    }
}
