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
    public class ProcessTake : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        var topExpression = expression.Arguments[ 1 ];
        if ( topExpression is ConstantExpression )
        {
          var constantTopExpression = topExpression as ConstantExpression;
          if ( constantTopExpression.Type == typeof( Int32 ) )
            state.Top = (int)constantTopExpression.Value;
          else
            state.Top = Convert.ToInt32( constantTopExpression.Value );
        }
        else
          throw new NotSupportedException( "Unable to resolve value for Take()" );
      }
    }
  }
}
