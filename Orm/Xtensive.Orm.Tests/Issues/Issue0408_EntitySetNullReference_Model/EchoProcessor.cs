using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  public class EchoProcessor : QueueProcessor
  {
    public override void ProcessDocument(Document inputDocument)
    {
      TestLog.Info("Entered EchoProcessor.Process() ");

      var outputDocument = OutputContainer.CreateDocument<Document>("output test document");
      MoveDocumentsToOutputAndDoneContainers(outputDocument, inputDocument);
    }
  }
}
