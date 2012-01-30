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
  public class DapperTests
  {
    private static string ConnectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings[ "cs" ].ConnectionString; } }

    [TestMethod]
    public void SimpleSelectStatement()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>( "SELECT * FROM [child]" ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }

    [TestMethod]
    public void SelectStatementWithWhereClause()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child>( "SELECT * FROM [child] WHERE [name] = @p0", new { p0 = "123" } ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void SelectStatementWithJoin()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        cn.Open();
        var results = cn.Query<Example.DAL.Child, Example.DAL.Header, Example.DAL.Child>( "SELECT c.*, h.* FROM [child] AS c INNER JOIN [header] AS h ON c.[headerId] = h.[headerId] WHERE c.[name] = @p0", ( child, header ) => { child.FK_HeaderId = header; return child; }, new { p0 = "123" }, null, true, "headerId" ).ToList();
        Assert.AreEqual( 1, results.Count );
      }
    }

    [TestMethod]
    public void SelectStatementWithJoinAllRows()
    {
      using ( var cn = new SqlConnection( ConnectionString ) )
      {
        // unfortunately it can't determine that two rows use the same joined 
        // row and therefore creates two objects rather than 1
        cn.Open();
        var results = cn.Query<Example.DAL.Child, Example.DAL.Header, Example.DAL.Child>( "SELECT c.*, h.* FROM [child] AS c INNER JOIN [header] AS h ON c.[headerId] = h.[headerId]", ( child, header ) => { child.FK_HeaderId = header; return child; }, null, null, true, "headerId" ).ToList();
        Assert.AreEqual( 1000, results.Count );
      }
    }
  }
}
