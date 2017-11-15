using System;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  [HierarchyRoot] 
  public abstract class Processor : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public bool Enabled { get; set; }

    [Field]
    public Container OutputContainer { get; set; }

    [Field]
    public string DocumentTypeToProduce { get; set; }

    protected Processor()
    {
      Enabled = true;
    }


    #region Helper functions to be used in sub classes

    /// <summary>
    /// Propagate all the links that point to sourceDocument to destinationDocument
    /// </summary>
    /// <param name="sourceDocument"></param>
    /// <param name="destinationDocument"></param>
    protected void PropagateAllGroups(Document sourceDocument, Document destinationDocument)
    {
      TestLog.Debug("Propagating to document {0} all groups of document {1}.", destinationDocument, sourceDocument);

      //// Does the source document belong to any group(s) ?
      //QueryResult qr;
      //using (Query q = Session.CreateQuery("Select DocumentLink instances where {LinkSemantic}=@semantic and LinkDestination=@ourSource"))
      //{
      //  q.Parameters.Add("@semantic", LinkSemantic.GroupHeadToDocumentInGroup);
      //  q.Parameters.Add("@ourSource", sourceDocument);
      //  qr = q.Execute();
      //}

      var qr = from link in Session.Demand().Query.All<DocumentLink>()
               where link.LinkSemantic == LinkSemantic.GroupHeadToDocumentInGroup
               where link.LinkDestination == sourceDocument
               select link;

      if (qr.Count() > 0)
      {
        foreach (DocumentLink link in qr)
        {
          if (destinationDocument == link.LinkDestination)
          {
            // The link we are about to create is already exactly the one which is already present.
            // This happens when we move a document which just received a link from some other source.
            // E.g. this document was just promoted head of a group by e.g. UnBundleEDIFACTOxl.
            continue;
          }

          // propagate all groups of sourceDocument to destinationDocument
          link.LinkSource.CreateNewLinkfromMe(destinationDocument, link.LinkSemantic);
          TestLog.Debug("Document/ID={0} inherits a link from Document/ID={1}. Head of the group is Document/ID={2}. (LinkSemantic={3})", destinationDocument.Id, sourceDocument.Id, link.LinkSource.Id, link.LinkSemantic);
        }
      }
      else
      {
        TestLog.Warning("Creating a new group from this document because no links point to it. (Document is {0})", sourceDocument); 
        sourceDocument.CreateNewGroupAndAddThis(); 
      }
    }

    /// <summary>
    /// Link sourceDocument and outputDocument together, and then move the first argument to outputContainer.
    /// </summary>
    /// <param name="outputDocument"></param>
    /// <param name="sourceDocument"></param>
    /// <param name="outputContainer"></param>
    protected void LinkToSourceDocumentAndMoveToContainer(Document outputDocument, Document sourceDocument, Container outputContainer)
    {
      System.Diagnostics.Debug.Assert(Transaction.Current != null, "Cannot execute out of a transaction");
      
      PropagateAllGroups(sourceDocument, outputDocument);
      MoveDocumentToContainer(outputDocument, outputContainer);
    }

    /// <summary>
    /// Move document from outputContainer to doneContainer.
    /// </summary>
    /// <param name="portfolio"></param>
    /// <param name="outputContainer"></param>
    /// <param name="doneContainer"></param>
    protected void MoveDocumentToContainer(Document document, Container container)
    {
      System.Diagnostics.Debug.Assert(Transaction.Current != null, "Cannot execute out of a transaction"); 

      if (container == null)
      {
        throw new ArgumentNullException("container", "The container must be non-null.");
      }
      TestLog.Debug("Moving " + document + " from " + document.Container + " to  " + container);
      document.Container = container;
    }


    #endregion
  }
}
