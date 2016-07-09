using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public class DbLogicalExpression
    {
        object[] Operands { get; }
        DbLogicalOperator Operator { get; }
        bool NOT { get; }
    }
    public enum DbLogicalOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        Between,
        Like,
        In,
        IsNull,
        And,
        Or
    }
}
