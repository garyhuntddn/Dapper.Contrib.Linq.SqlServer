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
  public class DapperLinqQueryProvider : IQueryProvider
  {
    private readonly IDbConnection _connection;

    public DapperLinqQueryProvider( IDbConnection connection )
    {
      _connection = connection;
    }

    public IQueryable<TElement> CreateQuery<TElement>( Expression expression )
    {
      return new DapperLinqQuery<TElement>( _connection, expression );
    }

    public IQueryable CreateQuery( Expression expression )
    {
      throw new NotImplementedException();
    }

    public TResult Execute<TResult>( Expression expression )
    {
      List<TResult> results = CreateQuery<TResult>( expression ).ToList();

      switch ( results.Count )
      {
        case 1 : return results[ 0 ];
        case 0: return default(TResult);
        default:
          throw new Exception("Query returned more than one result");
      }
    }

    public object Execute( Expression expression )
    {
      throw new NotImplementedException();
    }
  }
}
