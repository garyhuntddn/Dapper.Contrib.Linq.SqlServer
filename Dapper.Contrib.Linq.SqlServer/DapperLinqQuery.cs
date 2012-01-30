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
  public class DapperLinqQuery<T> : IQueryable<T>, IOrderedQueryable<T>
  {
    private readonly IDbConnection _connection;
    private readonly DapperLinqQueryProvider _provider;
    private readonly Expression _expression;

    private IEnumerable<T> _result;

    public DapperLinqQuery( IDbConnection connection, Expression expression = null )
    {
      _connection = connection;
      _provider = new DapperLinqQueryProvider( _connection );
      _expression = expression ?? Expression.Constant( this );
    }

    private IEnumerable<T> GetResult()
    {
      if ( _result == null )
      {
        var dsb = new DapperSqlBuilder( _expression );
        _result = _connection.Query<T>( dsb.Sql, dsb.Parameters );
      }
      return _result;
    }

    public IEnumerator<T> GetEnumerator() { return GetResult().GetEnumerator(); }

    IEnumerator IEnumerable.GetEnumerator() { return GetResult().GetEnumerator(); }

    public Type ElementType { get { return typeof( T ); } }

    public Expression Expression { get { return _expression; } }

    public IQueryProvider Provider { get { return _provider; } }
  }
}
