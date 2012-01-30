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
  public static class DapperLinqExtensions
  {
    public static DapperLinqQuery<T> Query<T>( this IDbConnection cn )
    {
      return new DapperLinqQuery<T>( cn );
    }
  }
}
