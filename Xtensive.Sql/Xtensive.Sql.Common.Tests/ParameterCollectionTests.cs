using System;
using System.Data;
using NUnit.Framework;

namespace Xtensive.Sql.Common.Tests
{
  [TestFixture]
  public class ParameterCollectionTests
  {
    [Test]
    public void AddMethodsTest()
    {
      ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();
      object objectParameter = new Parameter("parameter0", DbType.Int32);
      Assert.AreEqual(parameters.Add(objectParameter), 0);
      Assert.AreEqual(parameters[0], objectParameter);
      Assert.AreEqual(parameters["parameter0"], objectParameter);
      Parameter parameter = new Parameter("parameter1", DbType.Int32);
      parameters.Add(parameter);
      Assert.AreEqual(parameters[1], parameter);
      Assert.AreEqual(parameters["parameter1"], parameter);
      parameter = parameters.Add("parameter2", 2);
      Assert.AreEqual(parameters[2], parameter);
      Assert.AreEqual(parameters["parameter2"], parameter);
      parameter = parameters.Add("parameter3", DbType.String, 10);
      Assert.AreEqual(parameters[3], parameter);
      Assert.AreEqual(parameters["parameter3"], parameter);
      parameter = parameters.Add("parameter4", DbType.String, 10, "sourceColumn");
      Assert.AreEqual(parameters[4], parameter);
      Assert.AreEqual(parameters["parameter4"], parameter);
      parameter = parameters.Add("parameter5", null);
      Assert.AreEqual(parameters[5], parameter);
      Assert.AreEqual(parameters["parameter5"], parameter);
      Assert.AreEqual(parameters.Count, 6);
    }

    [Test]
    public void RangeMethodsTest()
    {
      ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();
      Parameter[] firstRange = new Parameter[7];
      Object[] secondRange = new Object[5];
      Object[] arrayResult = new Object[12];
      Parameter[] parametersResult = new Parameter[12];

      int i = 0;
      for (; i < 7; i++) {
        firstRange[i] = new Parameter("parameter" + i, DbType.Double);
      }
      parameters.AddRange(firstRange);
      Assert.AreEqual(parameters.Count, 7);

      for (; i < 12; i++) {
        secondRange.SetValue(new Parameter("parameter" + i, DbType.DateTime), i-7);
      }
      parameters.AddRange(secondRange);
      Assert.AreEqual(parameters.Count, 12);

      parameters.CopyTo(arrayResult, 0);
      parameters.CopyTo(parametersResult, 0);

      i = 0;
      for (; i < 7; i++) {
        Assert.AreEqual((arrayResult.GetValue(i) as Parameter).ParameterName, "parameter" + i);
        Assert.AreEqual((arrayResult.GetValue(i) as Parameter).DbType, DbType.Double);
        Assert.AreEqual(parametersResult[i].ParameterName, "parameter" + i);
        Assert.AreEqual(parametersResult[i].DbType, DbType.Double);
      }

      for (; i < 12; i++) {
        Assert.AreEqual((arrayResult.GetValue(i) as Parameter).ParameterName, "parameter" + i);
        Assert.AreEqual((arrayResult.GetValue(i) as Parameter).DbType, DbType.DateTime);
        Assert.AreEqual(parametersResult[i].ParameterName, "parameter" + i);
        Assert.AreEqual(parametersResult[i].DbType, DbType.DateTime);
      }

      parameters.Clear();
      Assert.AreEqual(parameters.Count, 0);
    }

    [Test]
    public void RemoveMethodsTest()
    {
      ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();
      object objectParameter = new Parameter("parameter0", DbType.Int32);
      Assert.AreEqual(parameters.Add(objectParameter), 0);
      Assert.AreEqual(parameters[0], objectParameter);
      Assert.AreEqual(parameters["parameter0"], objectParameter);
      Parameter parameter = new Parameter("parameter1", DbType.Int32);
      parameters.Add(parameter);
      Assert.AreEqual(parameters[1], parameter);
      Assert.AreEqual(parameters["parameter1"], parameter);
      parameter = parameters.Add("parameter2", 2);
      Assert.AreEqual(parameters[2], parameter);
      Assert.AreEqual(parameters["parameter2"], parameter);
      parameter = parameters.Add("parameter3", DbType.String, 10);
      Assert.AreEqual(parameters[3], parameter);
      Assert.AreEqual(parameters["parameter3"], parameter);
      parameter = parameters.Add("parameter4", DbType.String, 10, "sourceColumn");
      Assert.AreEqual(parameters[4], parameter);
      Assert.AreEqual(parameters["parameter4"], parameter);
      parameter = parameters.Add("parameter5", null);
      Assert.AreEqual(parameters[5], parameter);
      Assert.AreEqual(parameters["parameter5"], parameter);
      Assert.AreEqual(parameters.Count, 6);

      Assert.IsTrue(parameters.Contains(objectParameter));
      Assert.IsTrue(parameters.Contains("parameter0"));
      Assert.AreEqual(parameters.IndexOf(objectParameter), 0);
      Assert.AreEqual(parameters.IndexOf("parameter0"), 0);
      parameters.Remove(objectParameter);
      Assert.IsFalse(parameters.Contains(objectParameter));
      Assert.IsFalse(parameters.Contains("parameter0"));
      Assert.AreEqual(parameters.IndexOf(objectParameter), -1);
      Assert.AreEqual(parameters.IndexOf("parameter0"), -1);
      Assert.AreEqual(parameters.Count, 5);

      parameter = parameters["parameter2"];
      Assert.IsTrue(parameters.Contains(parameter));
      Assert.IsTrue(parameters.Contains("parameter2"));
      Assert.AreEqual(parameters.IndexOf(parameter), 1);
      Assert.AreEqual(parameters.IndexOf("parameter2"), 1);
      Assert.IsTrue(parameters.Remove(parameter));
      Assert.IsFalse(parameters.Contains(parameter));
      Assert.IsFalse(parameters.Contains("parameter2"));
      Assert.AreEqual(parameters.IndexOf(parameter), -1);
      Assert.AreEqual(parameters.IndexOf("parameter2"), -1);
      Assert.AreEqual(parameters.Count, 4);

      parameter = parameters["parameter4"];
      Assert.AreEqual(parameters.IndexOf(parameter), 2);
      parameters.RemoveAt(2);
      Assert.IsFalse(parameters.Contains(parameter));
      Assert.AreEqual(parameters.Count, 3);

      Assert.IsTrue(parameters.Contains("parameter3"));
      parameters.RemoveAt("parameter3");
      Assert.IsFalse(parameters.Contains("parameter3"));
      Assert.AreEqual(parameters.Count, 2);
    }

    [Test]
    public void EnumeratorTest()
    {
      ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();
      ParameterCollection<Parameter> newParameters = new ParameterCollection<Parameter>();

      int i = 0;
      for (; i < 7; i++) {
        parameters.Add("parameter" + i, DbType.Double);
      }

      foreach( Parameter parameter in parameters){
        newParameters.Add(parameter);
      }
      Assert.AreEqual(parameters.Count, 7);
      Assert.AreEqual(newParameters.Count, 7);
    }

    [Test]
    public void PropertiesTest()
    {
      ParameterCollection<Parameter> parameters = new ParameterCollection<Parameter>();
      Assert.AreEqual(parameters.Count, 0);
      Assert.AreEqual(parameters.IsFixedSize, false);
      Assert.AreEqual(parameters.IsReadOnly, false);
      Assert.AreEqual(parameters.IsSynchronized, false);
      Assert.IsNotNull(parameters.SyncRoot);
    }
  }
}