using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DDN.SharedCode;
using System.Data.SqlClient;
using Dapper;
using Dapper.Linq;

namespace DDN.SharedCode.Tests.Dapper
{
  [TestClass]
  public class DapperLinqTests
  {
    private static string ConnectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings[ "cs" ].ConnectionString; } }

    [TestMethod]
    public void SelectStatement()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void Top10a()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Take( 10 ).ToList();
        Assert.AreEqual( 10, results.Count );
      }
    }

    [TestMethod]
    public void Top10b()
    {
      const int topCount = 10;
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Take( topCount ).ToList();
        Assert.AreEqual( 10, results.Count );
      }
    }

    [TestMethod]
    public void Top10c()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        for ( int topCount = 1; topCount < 10; topCount++ )
        {
          var results = cn.Query<Example.DAL.Child>().Take( topCount ).ToList();
          Assert.AreEqual( topCount, results.Count );
        }
      }
    }

    [TestMethod]
    public void Distinct()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Distinct().ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void OrderBy()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().OrderBy( m => m.Name ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void OrderByAndThenBy()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().OrderBy( m => m.Name ).ThenBy( m => m.HeaderId ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void OrderByWithTop()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().OrderBy( m => m.Name ).ThenBy( m => m.HeaderId ).Take( 5 ).ToList();
        Assert.AreEqual( 5, results.Count );
      }
    }

    [TestMethod]
    public void WhereSimpleEqual()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name == "123" ).ToList();
        Assert.AreEqual( 1, results.Count );
        Assert.AreEqual( "123", results[ 0 ].Name );
      }
    }

    [TestMethod]
    public void WhereSimpleEqualWithoutParameter()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.ChildId == m.HeaderId ).ToList();
        Assert.AreEqual( 0, results.Count );
      }
    }

    [TestMethod]
    public void WhereIsNullOrEmpty()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => String.IsNullOrEmpty( m.Name ) ).ToList();
        Assert.AreEqual( 0, results.Count );
      }
    }

    [TestMethod]
    public void WhereNotIsNullOrEmpty()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !String.IsNullOrEmpty( m.Name ) ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void WhereHasValue()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Created.HasValue ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void WhereNotHasValue()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !m.Created.HasValue ).ToList();
        Assert.AreEqual( 0, results.Count );
      }
    }

    [TestMethod]
    public void WhereLike()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name.Contains( "12" ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void WhereEndsWith()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name.StartsWith( "12" ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void WhereStartsWith()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name.EndsWith( "23" ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void WhereEndsWithAndComparison()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name.StartsWith( "12", StringComparison.OrdinalIgnoreCase ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void WhereStartsWithAndComparison()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name.EndsWith( "23", StringComparison.OrdinalIgnoreCase ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void WhereNotLike()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !m.Name.Contains( "12" ) ).ToList();
        Assert.AreEqual( 999, results.Count );
      }
    }

    [TestMethod]
    public void WhereNotEndsWith()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !m.Name.StartsWith( "12" ) ).ToList();
        Assert.AreEqual( 999, results.Count );
      }
    }

    [TestMethod]
    public void WhereNotStartsWith()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !m.Name.EndsWith( "23" ) ).ToList();
        Assert.AreEqual( 999, results.Count );
      }
    }

    [TestMethod]
    public void TwoPartWhereAnd()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name == "123" && m.Created.HasValue ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void TwoPartWhereOr()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name == "123" || m.Name == "456" ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void MultiPartWhereAndOr()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => m.Name == "123" && ( m.Name == "456" || m.Created.HasValue ) ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void MultiPartWhereAndOr2()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>().Where( m => !( m.Name == "123" ) && ( m.Name == "456" || m.Created.HasValue ) ).ToList();
        Assert.AreEqual( 999, results.Count );
      }
    }

    [TestMethod]
    public void Single()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var result = cn.Query<Example.DAL.Child>().Single( m => m.Name == "123" );
        Assert.AreEqual( "123", result.Name );
      }
    }

    [TestMethod]
    public void HoldLock()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var result = cn.Query<Example.DAL.Child>().WithHoldLock().ToList();
      }
    }

    [TestMethod]
    public void NoLock()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var result = cn.Query<Example.DAL.Child>().WithNoLock().ToList();
      }
    }

    [TestMethod]
    public void RowAtOrDefaultForMissingItem()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var result = cn.Query<Example.DAL.Child>().RowAtOrDefault( Guid.Empty );
        Assert.IsNull( result );
      }
    }
  }
}
