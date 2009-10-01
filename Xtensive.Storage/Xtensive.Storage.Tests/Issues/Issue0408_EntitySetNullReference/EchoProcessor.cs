using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Pilot.Kernel.ProcessingWorkflowModel
{
  public class EchoProcessor : QueueProcessor
  {
    public override void ProcessDocument(Document inputDocument)
    {
      Log.Info("Entered EchoProcessor.Process() ");

      Document outputDocument = OutputContainer.CreateDocument<Document>("output test document");
      MoveDocumentsToOutputAndDoneContainers(outputDocument, inputDocument);
    }
  }
}
