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
    private readonly Expression _expression;

    private bool _evaluated = false;
    private string _sql = null;
    private Dapper.DynamicParameters _parameters = null;

    public DapperSqlBuilder( Expression expression )
    {
      _expression = expression;
    }

    public string Sql { get { EvaluateExpression(); return _sql; } }

    public DynamicParameters Parameters { get { EvaluateExpression(); return _parameters; } }

    private void EvaluateExpression()
    {
      if ( _evaluated ) return;

      var state = new SqlBuilderState();

      var expression = _expression;
      while ( expression != null )
      {
        if ( expression is ConstantExpression )
        {
          var constantExpression = expression as ConstantExpression;
          expression = null; // end of the line

          // should be an IQueryable<T> where T is the resulting table
          state.ElementType = constantExpression.Type.GetGenericArguments()[ 0 ];
        }
        else if ( expression is MethodCallExpression )
        {
          var methodCallExpression = expression as MethodCallExpression;
          if ( methodCallExpression.Arguments.Count == 0 )
            throw new NotSupportedException( "Method call expression must have at least one argument" );

          // up the stack of expressions
          expression = methodCallExpression.Arguments[ 0 ];

          // process each method
          LinqProcessor.Process( methodCallExpression.Method.Name ).Interpret( methodCallExpression, state );
        }
        else
          throw new NotImplementedException( "Expression is not handled" );
      }

      // NICE: put the field list into a concurrent shared dictionary

      _sql = BuildSqlStatement( state );
      if ( state.HasParameters ) _parameters = state.Parameters;
      _evaluated = true;
    }

    private static string BuildSqlStatement( SqlBuilderState state )
    {
      // get the database table name from the type or the table attribute
      TableInfo table = GetTableInfo( state.ElementType );

      // if there were no specific fields then add the field names from the type
      if ( state.FieldNames.Count == 0 ) state.FieldNames.AddRange( table.FieldNames );

      // build up the SQL
      var sb = new StringBuilder( "SELECT " );

      // append the top + fields
      if ( state.Top.HasValue )
      {
        state.Parameters.Add( "top", state.Top.Value, DbType.Int32 );
        sb.Append( "TOP (@top) " );
      }

      // distinct
      if ( state.Distinct ) sb.Append( "DISTINCT " );

      // field list
      sb.Append( string.Join( ", ", state.FieldNames.Select( m => "t.[" + m + "]" ) ) );

      // append the from
      sb.AppendFormat( " FROM [{0}].[{1}] AS t ", table.SchemaName, table.TableName );

      // append the hints
      if ( state.Hints.Count > 0 )
      {
        sb.Append( "WITH (" );
        sb.Append( string.Join( ", ", state.Hints ) );
        sb.Append( ")" );
      }

      // append the where clause
      if ( state.Where.Length != 0 )
      {
        sb.Append( "WHERE " );
        sb.Append( state.Where.ToString() );
      }

      // append the order by
      if ( state.OrderBy.Count > 0 )
      {
        state.OrderBy.Reverse();
        sb.Append( "ORDER BY " );
        sb.Append( string.Join( ", ", state.OrderBy ) );
      }

      return sb.ToString();
    }
  }
}
