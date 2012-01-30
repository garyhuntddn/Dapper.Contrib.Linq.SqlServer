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
    public static class LinqProcessor
    {
      public static IProcessExpression Process( string methodName ) { return _functionMapping[ methodName ]; }

      private static readonly Dictionary<string, IProcessExpression> _functionMapping = new Dictionary<string, IProcessExpression>();
      static LinqProcessor()
      {
        var _take = new ProcessTake();
        var _where = new ProcessWhere();
        var _single = new ProcessSingle();
        var _first = new ProcessFirst();
        var _distinct = new ProcessDistinct();
        var _holdLock = new ProcessWithHoldLock();
        var _noLock = new ProcessWithNoLock();
        var _rowAt = new ProcessRowAt();
        var _orderByAscending = new ProcessOrderBy( true );
        var _orderByDescending = new ProcessOrderBy( false );
        var _notImplemented = new ProcessNotImplemented();

        _functionMapping.Add( "Aggregate", _notImplemented ); // aggregate
        _functionMapping.Add( "All", _notImplemented ); // can't see how this can be done
        _functionMapping.Add( "Any", _notImplemented ); // can't see how this can be done
        _functionMapping.Add( "AsQueryable", _notImplemented ); // can't see how this is necessary
        _functionMapping.Add( "Average", _notImplemented ); // aggregate
        _functionMapping.Add( "Cast", _notImplemented ); // can't see how this is necessary
        _functionMapping.Add( "Concat", _notImplemented ); // can't see how this is necessary
        _functionMapping.Add( "Contains", _notImplemented ); // can't see how this is necessary
        _functionMapping.Add( "Count", _notImplemented ); // aggregate
        _functionMapping.Add( "DefaultIfEmpty", _notImplemented );// not even sure what this does
        _functionMapping.Add( "Distinct", _distinct );
        _functionMapping.Add( "ElementAt", _notImplemented ); // can't see how this is necessary, unless do a TOP N and return the last item
        _functionMapping.Add( "ElementAtOrDefault", _notImplemented ); // can't see how this is necessary, unless do a TOP N and return the last item
        _functionMapping.Add( "Except", _notImplemented );
        _functionMapping.Add( "First", _first );
        _functionMapping.Add( "FirstOrDefault", _first );
        _functionMapping.Add( "GroupBy", _notImplemented ); // aggregate
        _functionMapping.Add( "GroupJoin", _notImplemented ); // aggregate
        _functionMapping.Add( "Intersect", _notImplemented ); // can't see how this can be done
        _functionMapping.Add( "Join", _notImplemented ); // can't see how this can be done
        _functionMapping.Add( "Last", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "LastOrDefault", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "LongCount", _notImplemented ); // aggregate
        _functionMapping.Add( "Max", _notImplemented ); // aggregate
        _functionMapping.Add( "Min", _notImplemented ); // aggregate
        _functionMapping.Add( "OfType", _notImplemented ); // can't see how this is necessary
        _functionMapping.Add( "OrderBy", _orderByAscending );
        _functionMapping.Add( "OrderByDescending", _orderByDescending );
        _functionMapping.Add( "Reverse", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "RowAt", _rowAt );
        _functionMapping.Add( "RowAtOrDefault", _rowAt );
        _functionMapping.Add( "Select", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "SelectMany", _notImplemented );
        _functionMapping.Add( "SequenceEqual", _notImplemented ); // not even sure what this does
        _functionMapping.Add( "Single", _single );
        _functionMapping.Add( "SingleOrDefault", _single );
        _functionMapping.Add( "Skip", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "SkipWhile", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "Sum", _notImplemented ); // aggregate
        _functionMapping.Add( "Take", _take ); // not even sure what this does
        _functionMapping.Add( "TakeWhile", _notImplemented ); // not even sure what this does
        _functionMapping.Add( "ThenBy", _orderByAscending );
        _functionMapping.Add( "ThenByDescending", _orderByDescending );
        _functionMapping.Add( "Union", _notImplemented ); // rely on the user to do this manually
        _functionMapping.Add( "Where", _where );
        _functionMapping.Add( "WithNoLock", _noLock );
        _functionMapping.Add( "WithHoldLock", _holdLock );
        _functionMapping.Add( "Zip", _notImplemented ); // rely on the user to do this manually
      }
    }

    public interface IProcessExpression
    {
      void Interpret( MethodCallExpression expression, SqlBuilderState state );
    }

    public class ProcessNotImplemented : IProcessExpression
    {
      public void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        throw new NotImplementedException( string.Format( "DapperLinq does not support the {0}() method", ( expression as MethodCallExpression ).Method.Name ) );
      }
    }
  }
}
