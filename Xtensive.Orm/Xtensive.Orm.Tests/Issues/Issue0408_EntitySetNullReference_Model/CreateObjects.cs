using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  public static class CreateObjects
  {
    public static string CreateTestEchoQueueProcessor(Domain domain)
    {
      Container inputContainer = null;
      Container outputContainer = null;
      Container processedInputContainer = null;

      string key = null;

      using (var session = domain.OpenSession())
      {
        using (var transactionScope = session.OpenTransaction())
        {
          inputContainer = new Container { Name = "DebugInputContainer" };
          inputContainer.CreateDocument<Document>("FirstDocument");

          outputContainer = new Container { Name = "DebugOutputContainer" };
          processedInputContainer = new Container { Name = "DebugProcessedContainer" };
          transactionScope.Complete();
        }

        // 
        using (var transactionScope = session.OpenTransaction())
        {
          EchoProcessor echoProcessor = new EchoProcessor
          {
            Name = "testEchoProcessor",
            InputContainer = inputContainer,
            OutputContainer = outputContainer,
            ProcessedContainer = processedInputContainer
          };
          key = echoProcessor.SchedulerKey;
          transactionScope.Complete();
        }
      }

      return key;
    }
  }
}
