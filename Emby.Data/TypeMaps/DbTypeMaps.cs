using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emby.Data
{
    public static class TypeMaps
    {
        public static readonly IDictionary<Type, DbType> CTypeToDbType = new Dictionary<Type, DbType>();

        static TypeMaps()
        {
            CTypeToDbType[typeof(byte)] = DbType.Byte;
            CTypeToDbType[typeof(sbyte)] = DbType.SByte;
            CTypeToDbType[typeof(short)] = DbType.Int16;
            CTypeToDbType[typeof(ushort)] = DbType.UInt16;
            CTypeToDbType[typeof(int)] = DbType.Int32;
            CTypeToDbType[typeof(uint)] = DbType.UInt32;
            CTypeToDbType[typeof(long)] = DbType.Int64;
            CTypeToDbType[typeof(ulong)] = DbType.UInt64;
            CTypeToDbType[typeof(float)] = DbType.Single;
            CTypeToDbType[typeof(double)] = DbType.Double;
            CTypeToDbType[typeof(decimal)] = DbType.Decimal;
            CTypeToDbType[typeof(bool)] = DbType.Boolean;
            CTypeToDbType[typeof(string)] = DbType.String;
            CTypeToDbType[typeof(char)] = DbType.StringFixedLength;
            CTypeToDbType[typeof(Guid)] = DbType.Guid;
            CTypeToDbType[typeof(DateTime)] = DbType.DateTime;
            CTypeToDbType[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            CTypeToDbType[typeof(byte[])] = DbType.Binary;
            CTypeToDbType[typeof(byte?)] = DbType.Byte;
            CTypeToDbType[typeof(sbyte?)] = DbType.SByte;
            CTypeToDbType[typeof(short?)] = DbType.Int16;
            CTypeToDbType[typeof(ushort?)] = DbType.UInt16;
            CTypeToDbType[typeof(int?)] = DbType.Int32;
            CTypeToDbType[typeof(uint?)] = DbType.UInt32;
            CTypeToDbType[typeof(long?)] = DbType.Int64;
            CTypeToDbType[typeof(ulong?)] = DbType.UInt64;
            CTypeToDbType[typeof(float?)] = DbType.Single;
            CTypeToDbType[typeof(double?)] = DbType.Double;
            CTypeToDbType[typeof(decimal?)] = DbType.Decimal;
            CTypeToDbType[typeof(bool?)] = DbType.Boolean;
            CTypeToDbType[typeof(char?)] = DbType.StringFixedLength;
            CTypeToDbType[typeof(Guid?)] = DbType.Guid;
            CTypeToDbType[typeof(DateTime?)] = DbType.DateTime;
            CTypeToDbType[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
          //  CTypeToDbType[typeof(System.Data.Linq.Binary)] = DbType.Binary;
        }



    }
}
