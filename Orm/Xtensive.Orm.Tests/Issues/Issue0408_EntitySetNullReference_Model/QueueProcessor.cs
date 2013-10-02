using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  public abstract class QueueProcessor : Processor
  {
    [Field]
    public string DocumentTypeToProcess { get; set; }

    [Field]
    public Container InputContainer { get; set; }

    [Field]
    public Container ProcessedContainer { get; set; }

    public static IList<object> GetWork(string key, Domain domain)
    {
      using (var session = domain.OpenSession())
      {
        QueueProcessor queueProcessor = null;
        IList<object> result;
        using (TransactionScope transactionScope = session.OpenTransaction())
        {
          queueProcessor = GetQueueProcessorByKey(key);
          transactionScope.Complete();
          result = queueProcessor.GetWork();
        }
        return result;
      }
    }

    public static void Execute(string key, object workUnit, Domain domain)
    {
      using (var session = domain.OpenSession())
      {
        QueueProcessor queueProcessor = null;
        using (TransactionScope transactionScope = session.OpenTransaction())
        {
          queueProcessor = GetQueueProcessorByKey(key);
          queueProcessor.Execute(workUnit);
          transactionScope.Complete();
        }
      }
    }

    private static QueueProcessor GetQueueProcessorByKey(string key)
    {
      string[] parts = key.Split('~');
      long id = Convert.ToInt64(parts[1]);
      return (from p in Session.Demand().Query.All<QueueProcessor>()
              where p.Id == id
              select p).FirstOrDefault();
    }

    public string SchedulerKey
    {
      get
      {
        return this.GetType().FullName + "~" + this.Id;
      }
    }

    /// <summary>
    /// Process one document: override it with your implementation
    /// </summary>
    public virtual void ProcessDocument(Document inputDocument) 
    { 
      throw new NotImplementedException();
    }

    public virtual IList<object> GetWork()
    {
      if (InputContainer == null)
      {
        throw new InvalidOperationException("A DocumentProcessor should have a not null InputContainer.");
      }

      var work = from d in InputContainer.Documents
                 select (object)d.Id;

      return (IList<object>)work.ToList();
    }

    //-> pour commiter chaque document traité
    //on laisse remonter les exceptions dataobjects
    public virtual void Execute(object workUnit)
    {
      long id = (long)workUnit;
      Document inputDocument = InputContainer.Documents.Where(d => d.Id == id).FirstOrDefault();
      System.Diagnostics.Debug.Assert(inputDocument != null, "QueueProcessor.Execute() called with nothing to do. There must be a document in the InputContainer"); 

      try
      {
        Document documentToProcess = GetDocumentToProcessFromInputDocument(inputDocument);
        if (documentToProcess == null)
        {
          string message = String.Format("No document with DocumentTypeToProcess={0} found in the group of the inputDocument.", DocumentTypeToProcess);
          throw new Exception(message);
        }

        ProcessAndMove(inputDocument, documentToProcess);

      }
      catch (Exception ex) // yes, general exception type
      {
        using (TransactionScope transactionScope = Session.OpenTransaction())
        {
          string message = String.Format("Exception caught in {0}.Execute() : '", this.Name);
          TestLog.Error(message, ex);

          transactionScope.Complete();
        }
      }
    }

    /// <summary>
    /// Allow to override default behaviour that is to use ProcessedContainer property
    /// </summary>
    /// <example>
    /// EdifactoNoOhSwitch has to containers : one for OH, one for NO
    /// </example>
    protected virtual Container GetProcessedContainerForDocument(Document inputDocument)
    {
      return ProcessedContainer;
    }

    private void ProcessAndMove(Document inputDocument, Document documentToProcess)
    {
      // ProcessDocument may disable automatic transactions :
      ProcessDocument(documentToProcess);

      documentToProcess.AddHistoryEntry("Document processed by " + this.Name, HistoryEntryVisibility.AdministratorUser);

      Container processedContainer = GetProcessedContainerForDocument(inputDocument);
      if (processedContainer != null)
      {
        MoveDocumentToContainer(inputDocument, processedContainer);
      }
      else
      {
        TestLog.Warning("No ContainerForDoneInputDocuments is defined for '{0}' DocumentProcessor", this.Name);
      }
    }

    #region Helper functions to be used in sub classes
    /// <summary>
    /// Find a document in the same group of a document, with a specified DocumentType.
    /// Returns :
    ///  * the closest document (the document in the innermost group), if multiple documents are found in the group. 
    ///  * null, if no document with the correct DocumentType is found in the group.
    ///  * the input argument, if this.DocumentTypeToProcess is not set
    /// 
    /// E.g. find the edi document associated with an invoice, find the .ser file associated with an invoice etc. 
    /// </summary>
    protected Document GetDocumentToProcessFromInputDocument(Document inputDocument)
    {
      if (inputDocument == null)
      {
        TestLog.Warning("Cannot find document to process because {0} is not a Document", inputDocument);
        return null;
      }

      string documentTypeToProcess = DocumentTypeToProcess;
      if (String.IsNullOrEmpty(documentTypeToProcess))
      {
        return inputDocument;
      }
      if (inputDocument.DocumentType == documentTypeToProcess)
      {
        return inputDocument;
      }

      List<Document> documentsMatchingDocType = inputDocument.FindDocumentsInMyGroupsByType(documentTypeToProcess, 0);

      // return the document in the innermost group : 
      if (documentsMatchingDocType.Count > 0)
      {
        if (documentsMatchingDocType.Count > 1)
        {
          TestLog.Debug("Found {0} documents matching DocumentType={1} in this group. Returning the document in innermost group : {2}", documentsMatchingDocType.Count, documentTypeToProcess, documentsMatchingDocType[0].Name);
        }
        return documentsMatchingDocType[0];
      }

      // default : 
      return null;
    }

    /// <summary>
    /// Move outputDocument to outputContainer, move inputDocument to doneContainer, and create a link between inputDocument and outputDocument
    /// </summary>
    /// <param name="outputDocument">Document created within an ExecutableObject execution</param>
    /// <param name="inputDocument">Document input of a DocumentProcessor</param>
    /// <param name="outputContainer"></param>
    /// <param name="doneContainer"></param>
    protected void MoveDocumentsToOutputAndDoneContainers(Document outputDocument, Document inputDocument)
    {
      // 1- README : 
      // Commenting out the following call to PropagateAllGroups(inputDocument, outputDocument) gives another exception. 
      PropagateAllGroups(inputDocument, outputDocument);

      // 2- README : 
      // Interchanging the order of the two calls also gives another exception. Please try : 
      //    MoveDocumentToContainer(inputDocument, ProcessedContainer);
      //    MoveDocumentToContainer(outputDocument, OutputContainer);
      // instead of : 
      //    MoveDocumentToContainer(outputDocument, OutputContainer);
      //    MoveDocumentToContainer(inputDocument, ProcessedContainer);
      MoveDocumentToContainer(outputDocument, OutputContainer);
      MoveDocumentToContainer(inputDocument, ProcessedContainer);
    }


    #endregion Helper functions to be used in sub classes

  }
}
