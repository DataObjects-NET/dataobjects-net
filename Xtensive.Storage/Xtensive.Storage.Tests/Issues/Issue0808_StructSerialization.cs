using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Serialization.Binary;

namespace Xtensive.Storage.Tests.Issues
{
  public struct MyStruct
  {
    public string StringField { get; set; }
    public int IntField { get; set; }
  }

  [TestFixture]
  public class Issue0808_StructSerialization
  {
    [Test]
    public void MainTest()
    {
      Expression<Func<int, MyStruct>> ex = a => new MyStruct {StringField = "string value", IntField = a};
      var serializableExpression = ex.ToSerializableExpression();
      var serializer = new BinaryFormatter() ;
      var memoryStream = new MemoryStream();
      serializer.Serialize(memoryStream, serializableExpression);
      memoryStream.Position = 0;
      var deserializedExpression = serializer.Deserialize(memoryStream);
    }
  }
}
