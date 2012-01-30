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

namespace Dapper.Linq
{
  public partial class DapperSqlBuilder
  {
    public class ProcessRowAt : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        var primaryKeyExpression = expression.Arguments[ 1 ];
        if ( primaryKeyExpression is ConstantExpression )
        {
          // create a new expression to do the where clause
          var elementType = expression.Type;
          var tableInfo = GetTableInfo( elementType );
          
          // check that there is a primary key
          if ( tableInfo.PrimaryKeyFieldNames.Count == 0 )
            throw new NotSupportedException( string.Format( "Type {0} does not have any members with the [Key] attribute applied", elementType.Name ) );

          // can only deal with single element primary keys at the moment
          if ( tableInfo.PrimaryKeyFieldNames.Count > 1 )
            throw new NotImplementedException( string.Format( "Type {0} has more than one member with the [Key] attribute applied", elementType.Name ) );

          // add the parameter and where clause
          string parameterName = state.GetNextParameter();
          state.Parameters.Add( parameterName, ( primaryKeyExpression as ConstantExpression ).Value );
          state.Where.AppendFormat( "( [{0}] = @{1} )", tableInfo.PrimaryKeyFieldNames[0], parameterName );
        }
        else
          throw new NotSupportedException( "Unable to resolve value for RowAt()" );
      }
    }
  }
}
