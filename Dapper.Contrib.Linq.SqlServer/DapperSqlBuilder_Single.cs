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
    public class ProcessSingle : ProcessWhere
    {
      public override void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        state.Top = 2; // to ensure that more than one row is return if the expression isn't actually returning a single
        var whereExpression = expression.Arguments[ 1 ];
        if ( whereExpression is UnaryExpression )
        {
          base.Interpret( expression, state );
        }
        else
          throw new NotSupportedException( "Unable to resolve value for Single()" );
      }
    }
  }
}
