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
    public class SqlBuilderState
    {
      public Type ElementType { get; set; }
      public int? Top { get; set; }
      public bool Distinct { get; set; }
      public List<string> FieldNames { get; private set; }
      public StringBuilder Where { get; private set; }
      public List<string> OrderBy { get; private set;}
      public List<string> Hints { get; private set; }

      private int _nextParameter = 0;
      public string GetNextParameter() { var s = "p" + _nextParameter; _nextParameter++; return s; }
      public bool HasParameters { get { return _nextParameter != 0 || Top.HasValue; } }

      public DynamicParameters Parameters { get; private set; }

      public SqlBuilderState()
      {
        FieldNames = new List<string>();
        Parameters = new DynamicParameters();
        OrderBy = new List<string>();
        Hints = new List<string>();
        Where = new StringBuilder();
        Distinct = false;
      }
    }
  }
}
