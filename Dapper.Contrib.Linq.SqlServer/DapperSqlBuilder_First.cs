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
    public class ProcessFirst : ProcessWhere
    {
      public override void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        state.Top = 1; // to ensure that just the first row is returned
        var whereExpression = expression.Arguments[ 1 ];
        if ( whereExpression is UnaryExpression )
        {
          base.Interpret( expression, state );
        }
        else
          throw new NotSupportedException( "Unable to resolve value for First()" );
      }
    }
  }
}
