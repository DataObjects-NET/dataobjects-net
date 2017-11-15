using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  public class EchoProcessor : QueueProcessor
  {
    public override void ProcessDocument(Document inputDocument)
    {
      TestLog.Info("Entered EchoProcessor.Process() ");

      Document outputDocument = OutputContainer.CreateDocument<Document>("output test document");
      MoveDocumentsToOutputAndDoneContainers(outputDocument, inputDocument);
    }
  }
}
