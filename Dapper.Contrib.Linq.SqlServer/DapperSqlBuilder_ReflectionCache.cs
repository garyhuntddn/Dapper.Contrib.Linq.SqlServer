using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using DDN.SharedCode;

namespace Dapper.Linq
{
  public partial class DapperSqlBuilder
  {
    #region reflection caching
    private static ConcurrentDictionary<System.Reflection.MemberInfo, string> _fieldNameCache = new ConcurrentDictionary<System.Reflection.MemberInfo, string>();
    internal static string GetFieldName( System.Reflection.MemberInfo member )
    {
      string name;
      if ( _fieldNameCache.TryGetValue( member, out name ) ) return name;

      name = member.Name;
      var columnAttribute = member.GetCustomAttribute<System.ComponentModel.DataAnnotations.ColumnAttribute>( false );
      if ( columnAttribute != null ) name = columnAttribute.Name;

      if ( _fieldNameCache.TryAdd( member, name ) ) return name;
      return _fieldNameCache[ member ];
    }

    internal class TableInfo
    {
      public string SchemaName;
      public string TableName;
      public List<string> FieldNames = new List<string>();
      public List<string> PrimaryKeyFieldNames = new List<string>();
    }

    private static ConcurrentDictionary<Type, TableInfo> _tableNameCache = new ConcurrentDictionary<Type, TableInfo>();
    internal static TableInfo GetTableInfo( Type t )
    {
      TableInfo table;
      if ( _tableNameCache.TryGetValue( t, out table ) ) return table;

      table = new TableInfo();
      table.TableName = t.Name;
      table.SchemaName = "dbo";

      // check for an attribute specifying something different
      var tableAttribute = t.GetCustomAttribute<System.ComponentModel.DataAnnotations.TableAttribute>( false );
      if ( tableAttribute != null )
      {
        table.TableName = tableAttribute.Name;
        if ( !string.IsNullOrWhiteSpace( tableAttribute.Schema ) ) table.SchemaName = tableAttribute.Schema;
      }

      // get the property names that can be mapped
      foreach ( var pi in t.GetProperties( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public ).Where( m => m.CanRead && m.CanWrite && !m.HasCustomAttribute<System.ComponentModel.DataAnnotations.NotMappedAttribute>( false ) ) )
        table.FieldNames.Add( GetFieldName( pi ) );

      // get the key property names
      foreach ( var pi in t.GetProperties( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public ).Where( m => m.CanRead && m.CanWrite && m.HasCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>( false )  ) )
        table.PrimaryKeyFieldNames.Add( GetFieldName( pi ) );

      // try to add the newly aquired info
      if ( _tableNameCache.TryAdd( t, table ) ) return table;
      return _tableNameCache[ t ];
    }

    #endregion
  }
}
