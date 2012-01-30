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
    public class ProcessWhere : IProcessExpression
    {
      private static readonly System.Reflection.MethodInfo String_IsNullOrEmpty;
      private static readonly System.Reflection.MethodInfo String_StartsWith;
      private static readonly System.Reflection.MethodInfo String_StartsWithAndComparison;
      private static readonly System.Reflection.MethodInfo String_EndsWith;
      private static readonly System.Reflection.MethodInfo String_EndsWithAndComparison;
      private static readonly System.Reflection.MethodInfo String_Contains;
      private static readonly System.Reflection.MemberInfo Nullable_HasValue;

      private class Operator
      {
        private readonly string _normal;
        private readonly string _reversed;

        public string Value( bool reversed )
        {
          return reversed ? _reversed : _normal;
        }

        public Operator( string normal, string reversed )
        {
          _normal = normal;
          _reversed = reversed;
        }
      }

      private readonly static Dictionary<ExpressionType, Operator> _comparisonOperators = new Dictionary<ExpressionType, Operator>()
      {
        { ExpressionType.Equal, new Operator( "=", "!=") },
        { ExpressionType.NotEqual, new Operator( "!=", "=") },
        { ExpressionType.GreaterThan, new Operator(">", "<=") },
        { ExpressionType.LessThan, new Operator("<", ">=") },
        { ExpressionType.GreaterThanOrEqual, new Operator(">=", "<") },
        { ExpressionType.LessThanOrEqual,  new Operator("<=", ">") }
      };

      static ProcessWhere()
      {
        // know function translations
        String_IsNullOrEmpty = typeof( string ).GetMethod( "IsNullOrEmpty", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public );
        String_StartsWith = typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ) } );
        String_StartsWithAndComparison = typeof( string ).GetMethod( "StartsWith", new Type[] { typeof( string ), typeof( StringComparison ) } );
        String_EndsWith = typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ) } );
        String_EndsWithAndComparison = typeof( string ).GetMethod( "EndsWith", new Type[] { typeof( string ), typeof( StringComparison ) } );
        String_Contains = typeof( string ).GetMethod( "Contains", new Type[] { typeof( string ) } );
        Nullable_HasValue = typeof( Nullable<> ).GetProperty( "HasValue", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty );
      }

      public virtual void Interpret( MethodCallExpression expression, SqlBuilderState state )
      {
        var whereExpression = expression.Arguments[ 1 ];
        if ( whereExpression is UnaryExpression )
        {
          var unaryWhereExpression = whereExpression as UnaryExpression;
          var operandExpression = unaryWhereExpression.Operand;
          ProcessExpression( operandExpression, state );
        }
        else
          throw new NotSupportedException( "Unable to resolve value for Where()" );
      }

      private void ProcessExpression( Expression expression, SqlBuilderState state )
      {
        if ( expression is LambdaExpression )
        {
          var lambdaOperandExpression = expression as LambdaExpression;
          ProcessExpression( lambdaOperandExpression.Body, state );
        }
        else if ( expression is UnaryExpression )
          UnaryExpression( expression as UnaryExpression, state );
        else if ( expression is BinaryExpression )
          BinaryExpression( expression as BinaryExpression, state );
        else if ( expression is MemberExpression )
          MemberExpression( expression as MemberExpression, state );
        else if ( expression is MethodCallExpression )
          MethodCallExpression( expression as MethodCallExpression, state );
        else
          throw new NotSupportedException();
      }

      private void CheckArgumentIsValue<T>( Expression argument, T expectedValue, string exceptionText ) where T : struct
      {
        // ensure that an argument is of the correct type and contains a specific value
        if (
          ( argument is ConstantExpression )
          && ( argument.Type == typeof( T ) )
          && ( EqualityComparer<T>.Default.Equals( (T)( argument as ConstantExpression ).Value, expectedValue ) )
          )
          return;

        throw new NotSupportedException( exceptionText );
      }

      private void UnaryExpression( UnaryExpression body, SqlBuilderState state )
      {
        // simple case of a ! prior to a function call
        if ( body.NodeType == ExpressionType.Not && body.Operand is MethodCallExpression )
        {
          MethodCallExpression( body.Operand as MethodCallExpression, state, true );
          return;
        }

        // simple case of a ! prior to a property call
        if ( body.NodeType == ExpressionType.Not && body.Operand is MemberExpression )
        {
          MemberExpression( body.Operand as MemberExpression, state, true );
          return;
        }

        // simple case of a ! prior to a binary expression
        if ( body.NodeType == ExpressionType.Not && body.Operand is BinaryExpression )
        {
          BinaryExpression( body.Operand as BinaryExpression, state, true );
          return;
        }

        throw new NotSupportedException();
      }

      private void BinaryExpression( BinaryExpression body, SqlBuilderState state, bool inverse = false )
      {
        var left = body.Left;
        var right = body.Right;

        if ( body.NodeType == ExpressionType.AndAlso || body.NodeType == ExpressionType.OrElse )
        {
          if ( inverse ) state.Where.Append( "( NOT " );
          state.Where.Append( "( " );
          ProcessExpression( left, state );
          state.Where.Append( body.NodeType == ExpressionType.AndAlso ? " AND " : " OR " );
          ProcessExpression( right, state );
          state.Where.Append( " )" );
          if ( inverse ) state.Where.Append( " )" );
        }
        else
        {
          // reverse where applicable
          bool reversed = false;
          if ( left is ConstantExpression && right is MemberExpression )
          {
            reversed = true;
            var swap = right;
            right = left;
            left = swap;
          }

          if ( right is ConstantExpression && left is MemberExpression )
          {
            string fieldName = GetFieldName( ( left as MemberExpression ).Member );
            object value = ( right as ConstantExpression ).Value;
            // caching this would lead to caching one or the other depending on whether the original value was NULL or not!
            // could optimise out non-nullable values
            if ( value == null )
            {
              if ( body.NodeType == ExpressionType.Equal || body.NodeType == ExpressionType.NotEqual )
              {
                bool equal = body.NodeType == ExpressionType.Equal;
                if ( inverse ) equal = !equal;
                state.Where.AppendFormat( " ( t.[{0}] {1} NULL ) ", fieldName, equal ? "IS" : "IS NOT" );
                return;
              }
              else
                throw new NotSupportedException();
            }
            else
            {
              Operator op;
              if ( !_comparisonOperators.TryGetValue( body.NodeType, out op ) )
                throw new NotSupportedException();

              string parameterName = state.GetNextParameter();
              if ( !reversed )
                state.Where.AppendFormat( " ( t.[{0}] {1} @{2} ) ", fieldName, op.Value( inverse ), parameterName );
              else
                state.Where.AppendFormat( " ( @{2} {1} t.[{0}] ) ", fieldName, op.Value( inverse ), parameterName );

              state.Parameters.Add( parameterName, value );
            }
          }
          else if ( left is MemberExpression && right is MemberExpression )
          {
            Operator op;
            if ( !_comparisonOperators.TryGetValue( body.NodeType, out op ) )
              throw new NotSupportedException();

            if ( !reversed )
              state.Where.AppendFormat( " ( t.[{0}] {1} t.[{2}] ) ", GetFieldName( ( left as MemberExpression ).Member ), op.Value( inverse ), GetFieldName( ( right as MemberExpression ).Member ) );
            else
              state.Where.AppendFormat( " ( t.[{2}] {1} t.[{0}] ) ", GetFieldName( ( left as MemberExpression ).Member ), op.Value( inverse ), GetFieldName( ( right as MemberExpression ).Member ) );
          }
          else
            throw new NotSupportedException();
        }
      }

      private void MethodCallExpression( MethodCallExpression body, SqlBuilderState state, bool inverse = false )
      {
        string negative = inverse ? "NOT " : string.Empty;

        if ( body.Method == String_IsNullOrEmpty )
        {
          string fieldName = GetFieldName( ( body.Arguments[ 0 ] as MemberExpression ).Member );
          state.Where.AppendFormat( " ( t.[{0}] IS {1}NULL ) ", fieldName, negative );
        }
        else if ( body.Method == String_EndsWith || body.Method == String_EndsWithAndComparison )
        {
          string fieldName = GetFieldName( ( body.Object as MemberExpression ).Member );
          Expression valueExpression = body.Arguments[ 0 ];

          // if comparison is provide ensure it's one that SQL server will respect
          if ( body.Arguments.Count == 2 ) CheckArgumentIsValue<StringComparison>( body.Arguments[ 1 ], StringComparison.OrdinalIgnoreCase, "EndsWith can only be used with StringComparison.OrdinalIgnoreCase" );

          if ( valueExpression is ConstantExpression )
          {
            var constantValueExpression = valueExpression as ConstantExpression;
            var parameter = state.GetNextParameter();
            if ( constantValueExpression.Type == typeof( string ) && constantValueExpression.Value != null )
            {
              string value = (string)constantValueExpression.Value;
              state.Parameters.Add( parameter, "%" + value );
              state.Where.AppendFormat( " ( t.[{0}] {1}LIKE @{2} )", fieldName, negative, parameter );
            }
            else
              throw new NotSupportedException();
          }
          else
            throw new NotSupportedException();
        }
        else if ( body.Method == String_StartsWith || body.Method == String_StartsWithAndComparison )
        {
          string fieldName = GetFieldName( ( body.Object as MemberExpression ).Member );
          Expression valueExpression = body.Arguments[ 0 ];

          // if comparison is provide ensure it's one that SQL server will respect
          if ( body.Arguments.Count == 2 ) CheckArgumentIsValue<StringComparison>( body.Arguments[ 1 ], StringComparison.OrdinalIgnoreCase, "StartsWith can only be used with StringComparison.OrdinalIgnoreCase" );

          if ( valueExpression is ConstantExpression )
          {
            var constantValueExpression = valueExpression as ConstantExpression;
            var parameter = state.GetNextParameter();
            if ( constantValueExpression.Type == typeof( string ) && constantValueExpression.Value != null )
            {
              string value = (string)constantValueExpression.Value;
              state.Parameters.Add( parameter, value + "%" );
              state.Where.AppendFormat( " ( t.[{0}] {1}LIKE @{2} )", fieldName, negative, parameter );
            }
            else
              throw new NotSupportedException();
          }
          else
            throw new NotSupportedException();
        }
        else if ( body.Method == String_Contains )
        {
          string fieldName = GetFieldName( ( body.Object as MemberExpression ).Member );
          Expression valueExpression = body.Arguments[ 0 ];
          if ( valueExpression is ConstantExpression )
          {
            var constantValueExpression = valueExpression as ConstantExpression;
            var parameter = state.GetNextParameter();
            if ( constantValueExpression.Type == typeof( string ) && constantValueExpression.Value != null )
            {
              string value = (string)constantValueExpression.Value;
              state.Parameters.Add( parameter, "%" + value + "%" );
              state.Where.AppendFormat( " ( t.[{0}] {1}LIKE @{2} )", fieldName, negative, parameter );
            }
            else
              throw new NotSupportedException();
          }
          else
            throw new NotSupportedException();
        }
        else
          throw new NotSupportedException();
      }

      private void MemberExpression( MemberExpression body, SqlBuilderState state, bool reversed = false )
      {
        if ( body.Member.Name == Nullable_HasValue.Name )
        {
          string fieldName = GetFieldName( ( body.Expression as MemberExpression ).Member );
          state.Where.AppendFormat( " ( t.[{0}] IS {1}NULL ) ", fieldName, reversed ? string.Empty : "NOT " );
        }
        else
          throw new NotSupportedException();
      }
    }
  }
}
