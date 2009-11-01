using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using NUnit.Framework;

namespace Xtensive.Sql.Common.Tests
{
  [TestFixture]
  public class ParameterTests
  {
    [Test]
    public void ConstructorTest0()
    {
      Parameter parameter = new Parameter();
      Assert.AreEqual(parameter.DbType, DbType.String);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, String.Empty);
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, String.Empty);
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, null);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);
    }

    [Test]
    public void ConstructorTest1()
    {
      Parameter parameter = new Parameter("ParameterName", DbType.Currency);
      Assert.AreEqual(parameter.DbType, DbType.Currency);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, String.Empty);
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, null);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);
    }

    [Test]
    public void ConstructorTest2()
    {
      DateTime now = DateTime.Now;
      Parameter parameter = new Parameter("ParameterName", now);
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, String.Empty);
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, now);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);
    }

    [Test]
    public void ConstructorTest3()
    {
      Parameter parameter = new Parameter("ParameterName", DbType.Binary, 99);
      Assert.AreEqual(parameter.DbType, DbType.Binary);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 99);
      Assert.AreEqual(parameter.SourceColumn, String.Empty);
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, null);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);
    }

    [Test]
    public void ConstructorTest4()
    {
      Parameter parameter = new Parameter("ParameterName", DbType.Binary, 99, "SourceColumnName");
      Assert.AreEqual(parameter.DbType, DbType.Binary);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 99);
      Assert.AreEqual(parameter.SourceColumn, "SourceColumnName");
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, null);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);
    }

    [Test]
    public void ConstructorTest5()
    {
      Parameter parameter = new Parameter(
        "ParameterName", DbType.Double, 0,
        ParameterDirection.InputOutput, true, 2, 3, "SourceColumnName", DataRowVersion.Default, 17.22);
      Assert.AreEqual(parameter.DbType, DbType.Double);
      Assert.AreEqual(parameter.Direction, ParameterDirection.InputOutput);
      Assert.AreEqual(parameter.IsNullable, true);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, "SourceColumnName");
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Default);
      Assert.AreEqual(parameter.Value, 17.22);
      Assert.AreEqual(parameter.Precision, 2);
      Assert.AreEqual(parameter.Scale, 3);
    }

    [Test]
    public void ResetDbTypeTest()
    {
      DateTime now = DateTime.Now;
      Parameter parameter = new Parameter("ParameterName", now);
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
      Assert.AreEqual(parameter.Direction, ParameterDirection.Input);
      Assert.AreEqual(parameter.IsNullable, false);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, String.Empty);
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Current);
      Assert.AreEqual(parameter.Value, now);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Scale, 0);

      parameter.DbType = DbType.Currency;
      Assert.AreEqual(parameter.DbType, DbType.Currency);
      parameter.ResetDbType();
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
    }

    [Test]
    public void NullValueTest()
    {
      Parameter parameter = new Parameter();
      parameter.Value = null;
    }

    [Test]
    public void ParameterBehaviourTest()
    {
      ParameterBehaviourTest1(new SqlParameter("parameter", SqlDbType.DateTime));
      ParameterBehaviourTest1(new Parameter("parameter", DbType.DateTime));

      ParameterBehaviourTest2(new SqlParameter("parameter1", SqlDbType.Float, 1, ParameterDirection.Input, true, 2, 3, "param1", DataRowVersion.Original, null));
      ParameterBehaviourTest2(new Parameter("parameter1", DbType.Double, 1, ParameterDirection.Input, true, 2, 3, "param1", DataRowVersion.Original, null));
    }

    private void ParameterBehaviourTest1(IDbDataParameter parameter)
    {
      parameter.Value = "14.05.05 17:55";
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
      Assert.AreEqual(parameter.Value, "14.05.05 17:55");
      Assert.AreEqual(parameter.Scale, 0);
      Assert.AreEqual(parameter.Precision, 0);
      Assert.AreEqual(parameter.Size, "14.05.05 17:55".Length);
    }

    private void ParameterBehaviourTest2(IDbDataParameter parameter)
    {
      int intValue = 1;
      parameter.Value = intValue;
      Assert.AreEqual(parameter.DbType, DbType.Double);
      Assert.AreEqual(parameter.Value, 1);
      Assert.AreEqual(parameter.Scale, 3);
      Assert.AreEqual(parameter.Precision, 2);
      Assert.AreEqual(parameter.Size, 1);
      parameter.DbType = DbType.Int32;
      Assert.AreEqual(parameter.DbType, DbType.Int32);
      ((DbParameter)parameter).ResetDbType();
      Assert.AreEqual(parameter.DbType, DbType.Int32);
      Assert.AreEqual(parameter.Scale, 3);
      Assert.AreEqual(parameter.Precision, 2);
      parameter.Value = 1.35;
      Assert.AreEqual(parameter.DbType, DbType.Double);
      Assert.AreEqual(parameter.Scale, 3);
      Assert.AreEqual(parameter.Precision, 2);
    }

    [Test]
    public void DefineTypeCode()
    {
      TypeCode typeCode = Type.GetTypeCode(typeof(byte[]));
      Assert.AreEqual(typeCode, TypeCode.Object);
      typeCode = Type.GetTypeCode(typeof(Guid));
      Assert.AreEqual(typeCode, TypeCode.Object);
      typeCode = Type.GetTypeCode(typeof(TimeSpan));
      Assert.AreEqual(typeCode, TypeCode.Object);

      typeCode = Type.GetTypeCode(typeof(string));
      Assert.AreEqual(typeCode, TypeCode.String);
      
    }

    [Test]
    public void DetectDbTypeTest()
    {
      Parameter parameter = new Parameter();
      Assert.AreEqual(parameter.DbType, DbType.String);
      parameter.Value = new char();
      Assert.AreEqual(parameter.DbType, DbType.Int16);
      parameter.Value = new bool();
      Assert.AreEqual(parameter.DbType, DbType.Boolean);
      parameter.Value = new byte();
      Assert.AreEqual(parameter.DbType, DbType.Byte);
      parameter.Value = new DateTime();
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
      parameter.Value = new Decimal();
      Assert.AreEqual(parameter.DbType, DbType.Decimal);
      parameter.Value = new double();
      Assert.AreEqual(parameter.DbType, DbType.Double);
      parameter.Value = new short();
      Assert.AreEqual(parameter.DbType, DbType.Int16);
      parameter.Value = new int();
      Assert.AreEqual(parameter.DbType, DbType.Int32);
      parameter.Value = new long();
      Assert.AreEqual(parameter.DbType, DbType.Int64);
      parameter.Value = new byte[]{new byte()};
      Assert.AreEqual(parameter.DbType, DbType.Binary);
      parameter.Value = new Guid();
      Assert.AreEqual(parameter.DbType, DbType.Guid);
      parameter.Value = new TimeSpan();
      Assert.AreEqual(parameter.DbType, DbType.DateTime);
      parameter.Value = new object();
      Assert.AreEqual(parameter.DbType, DbType.Object);
      parameter.Value = new Single();
      Assert.AreEqual(parameter.DbType, DbType.Single);
      parameter.Value = "";
      Assert.AreEqual(parameter.DbType, DbType.String);
      parameter.Value = new UInt16();
      Assert.AreEqual(parameter.DbType, DbType.UInt16);
      parameter.Value = new uint();
      Assert.AreEqual(parameter.DbType, DbType.UInt32);
      parameter.Value = new ulong();
      Assert.AreEqual(parameter.DbType, DbType.UInt64);
    }

    [Test]
    public void CloneTest()
    {
      Parameter sourceParameter = new Parameter(
        "ParameterName", DbType.Double, 0,
        ParameterDirection.InputOutput, true, 2, 3, "SourceColumnName", DataRowVersion.Default, 17.22);
      Parameter parameter = (Parameter)sourceParameter.Clone();

      Assert.AreEqual(parameter.DbType, DbType.Double);
      Assert.AreEqual(parameter.Direction, ParameterDirection.InputOutput);
      Assert.AreEqual(parameter.IsNullable, true);
      Assert.AreEqual(parameter.ParameterName, "ParameterName");
      Assert.AreEqual(parameter.Size, 0);
      Assert.AreEqual(parameter.SourceColumn, "SourceColumnName");
      Assert.AreEqual(parameter.SourceColumnNullMapping, false);
      Assert.AreEqual(parameter.SourceVersion, DataRowVersion.Default);
      Assert.AreEqual(parameter.Value, 17.22);
      Assert.AreEqual(parameter.Precision, 2);
      Assert.AreEqual(parameter.Scale, 3);
    }
  }
}