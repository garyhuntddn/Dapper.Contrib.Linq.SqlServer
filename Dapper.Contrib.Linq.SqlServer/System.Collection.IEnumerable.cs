using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;

namespace System.Collections.Generic
{
  public static class IEnumerableExtensions
  {
    public static TSource RowAt<TSource, TPrimaryKey>( this IQueryable<TSource> source, TPrimaryKey primaryKey )
    {
      if ( source == null ) { throw new ArgumentNullException( "source" ); }

      var mi = MethodBase.GetCurrentMethod() as MethodInfo;
      var gmi = mi.MakeGenericMethod( new Type[] { typeof( TSource ), typeof( TPrimaryKey ) } );
      var mce = Expression.Call( null, gmi, new Expression[] { source.Expression, Expression.Constant( primaryKey ) } );
      return source.Provider.Execute<TSource>( mce );
    }

    public static TSource RowAtOrDefault<TSource, TPrimaryKey>( this IQueryable<TSource> source, TPrimaryKey primaryKey )
    {
      if ( source == null ) { throw new ArgumentNullException( "source" ); }

      var mi = MethodBase.GetCurrentMethod() as MethodInfo;
      var gmi = mi.MakeGenericMethod( new Type[] { typeof( TSource ), typeof( TPrimaryKey ) } );
      var mce = Expression.Call( null, gmi, new Expression[] { source.Expression, Expression.Constant( primaryKey ) } );
      return source.Provider.Execute<TSource>( mce );
    }

    public static IQueryable<TSource> WithNoLock<TSource>( this IQueryable<TSource> source )
    {
      if ( source == null ) { throw new ArgumentNullException( "source" ); }
      return source.Provider.CreateQuery<TSource>( Expression.Call( null, ( (MethodInfo)MethodBase.GetCurrentMethod() ).MakeGenericMethod( new Type[] { typeof( TSource ) } ), new Expression[] { source.Expression } ) );
    }

    public static IQueryable<TSource> WithHoldLock<TSource>( this IQueryable<TSource> source )
    {
      if ( source == null ) { throw new ArgumentNullException( "source" ); }
      return source.Provider.CreateQuery<TSource>( Expression.Call( null, ( (MethodInfo)MethodBase.GetCurrentMethod() ).MakeGenericMethod( new Type[] { typeof( TSource ) } ), new Expression[] { source.Expression } ) );
    }
  }
}
