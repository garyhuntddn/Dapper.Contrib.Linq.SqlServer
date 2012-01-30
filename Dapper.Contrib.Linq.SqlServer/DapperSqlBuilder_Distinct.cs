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
    public class ProcessDistinct : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        state.Distinct = true;
      }
    }
  }
}
