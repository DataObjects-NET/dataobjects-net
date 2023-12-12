using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq.SerializableExpressions;

namespace Xtensive.Orm.Tests.Issues
{
  public struct UnifiedCustomerID
  {
    public int Id;
  }

  [TestFixture]
  public class Issue0808_StructSerialization
  {
    public IEnumerable<Type> SerializableExpressionTypes
    {
      get
      {
        yield return typeof (SerializableBinaryExpression);
        yield return typeof (SerializableConditionalExpression);
        yield return typeof (SerializableConstantExpression);
        yield return typeof (SerializableElementInit);
        yield return typeof (SerializableExpression);
        yield return typeof (SerializableInvocationExpression);
        yield return typeof (SerializableLambdaExpression);
        yield return typeof (SerializableListInitExpression);
        yield return typeof (SerializableMemberAssignment);
        yield return typeof (SerializableMemberBinding);
        yield return typeof (SerializableMemberExpression);
        yield return typeof (SerializableMemberInitExpression);
        yield return typeof (SerializableMemberListBinding);
        yield return typeof (SerializableMemberMemberBinding);
        yield return typeof (SerializableMethodCallExpression);
        yield return typeof (SerializableNewArrayExpression);
        yield return typeof (SerializableNewExpression);
        yield return typeof (SerializableParameterExpression);
        yield return typeof (SerializableTypeBinaryExpression);
        yield return typeof (SerializableUnaryExpression);
      }
    }

    [Test]
    public void MainTest()
    {
      var t = typeof(UnifiedCustomerID);
      Expression<Func<int, UnifiedCustomerID>> ex = a => new UnifiedCustomerID { Id = 2 };
      var serializableExpression = ex.ToSerializableExpression();
      using (var memoryStream = new MemoryStream()) {
        var serializer = new DataContractSerializer(typeof(SerializableLambdaExpression),
          SerializableExpressionTypes.Except(Enumerable.Repeat(typeof(SerializableLambdaExpression), 1)));

        serializer.WriteObject(memoryStream, serializableExpression);

        memoryStream.Position = 0;

        var deserializedExpression = (SerializableLambdaExpression) serializer.ReadObject(memoryStream);

        var ex2 = deserializedExpression.ToExpression();
      }
    }
  }
}
