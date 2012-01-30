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
    public class ProcessWithNoLock : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        state.Hints.Add( "nolock" );
      }
    }

    public class ProcessWithHoldLock : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        state.Hints.Add( "holdlock" );
      }
    }
  }
}
