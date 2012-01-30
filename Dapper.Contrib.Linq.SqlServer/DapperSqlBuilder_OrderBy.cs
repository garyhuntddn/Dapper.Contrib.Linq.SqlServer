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
    private class ProcessOrderBy  : IProcessExpression
    {
      public bool Ascending { get; private set; }

      public ProcessOrderBy( bool ascending )
      {
        Ascending = ascending;
      }

      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        var orderByExpression = expression.Arguments[ 1 ];
        if ( orderByExpression is UnaryExpression )
        {
          var unaryOrderByExpression = orderByExpression as UnaryExpression;
          var operandExpression = unaryOrderByExpression.Operand as Expression;
          if ( operandExpression is LambdaExpression )
          {
            var lamdaOperandExpression = operandExpression as LambdaExpression;
            var bodyExpression = lamdaOperandExpression.Body;
            if ( bodyExpression is MemberExpression )
            {
              var memberBodyExpression = bodyExpression as MemberExpression;
              var mi = memberBodyExpression.Member;
              var fieldName = GetFieldName( mi );
              state.OrderBy.Add( string.Format( "t.[{0}]{1}", fieldName, Ascending ? string.Empty : " DESC" ) );
            }
            else
              throw new NotImplementedException( "Unable to resolve value for OrderBy" );
          }
          else
            throw new NotImplementedException( "Unable to resolve value for OrderBy" );
        }
        else
          throw new NotImplementedException( "Unable to resolve value for OrderBy" );
      }
    }
  }
}
