using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [HierarchyRoot]
  public partial class Document : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public string DocumentType { get; set; }

    [Field]
    public Container Container { get; set; }

    /// <summary>
    /// Automatically filled creation date of Document. (UTC Time)
    /// </summary>
    [Field]
    protected DateTime CreationDate { get; set; }

    [Field, Association(PairTo = "LinkSource", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<DocumentLink> LinksFromThis { get; private set; }

    [Field, Association(PairTo = "LinkDestination", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<DocumentLink> LinksToThis { get; private set; }

    public Document()
    {
      CreationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Create a new group whose head will be this document
    /// </summary>
    public void CreateNewGroupAndAddThis()
    {
      CreateNewLinkfromMe(this, LinkSemantic.GroupHeadToDocumentInGroup);
      Log.Debug("Starting a new group : head document is " + this);
    }

    /// <summary>
    /// Create a new DocumentLink with this document as source.
    /// Possible semantics: "GroupHeadToDocumentInGroup", ...
    /// </summary>
    // DOFIXME : remove this function (one line ... )
    public void CreateNewLinkfromMe(Document destination, LinkSemantic semantic)
    {
      if (destination == null)
      {
        throw new ArgumentNullException("destination");
      }

      DocumentLink newLink = new DocumentLink { LinkSemantic = semantic, LinkDestination=destination, LinkSource=this};
    }

    /// <summary>
    /// Returns the heads of all the groups that this document belongs to. 
    /// The heads are returnd in descending order of creation date, i.e. from innermost to the outermost group
    /// 
    /// <remarks>
    /// The 'head' of a group is the Document that is the 'LinkSource' for all DocumentLinks in the group.
    /// Implementation note : do not sort with 
    ///   'Order By {LinkSource.CreationDate} Desc'
    /// instead of 
    ///   'Order By {LinkSource.ID} Desc'
    /// Confirmed risk of error if the server date is changed (NTP synchronization)  while the agent is running. 
    /// </remarks>
    /// </summary>
    public List<Document> FindHeadsFromGroupsIBelongTo()
    {
      var heads = from link in Session.Demand().Query.All<DocumentLink>()
                  where link.LinkSemantic == LinkSemantic.GroupHeadToDocumentInGroup
                  where link.LinkDestination == this
                  orderby link.LinkSource.Id descending
                  select link.LinkSource ;
      return heads.ToList<Document>(); 
    }

    /// <summary>
    /// Find all documents in the group whose head is this document
    /// </summary>
    /// <returns>empty list if none</returns>
    public List<Document> FindDocumentsInGroup()
    {
      var documents = from link in Session.Demand().Query.All<DocumentLink>()
                 where link.LinkSemantic == LinkSemantic.GroupHeadToDocumentInGroup
                 where link.LinkSource == this
                 select link.LinkDestination;

      return documents.ToList<Document>(); 
    }

    public override string ToString()
    {
      return
        "[Document]<" + Id + ">" +
        "Name='" + Name + "'::" +
        (String.IsNullOrEmpty(DocumentType) ? null : "::DocType='" + DocumentType + "'::") + 
        "Container=[" + Container  + "]::" ; 
    }


    #region FindDocumentsInMyGroupsByType (based on DocumentType)

    /// <summary>
    /// Find a document with the given DocumentType in the same group as this document. 
    /// If this document belongs to multiple groups, return the document of the nearest (innermost) group first.
    /// 
    /// 'maxDepth' is the number of heads to consider for the search in the group :   
    ///  * maxDepth = 1 : search only in nearest group (i.e. keep only 1 head (the most recent) in the group)
    ///  * maxDepth = 0 : search in all groups (no depth limit) 
    /// <remarks>
    /// Please note that we suppose that the head of the inner group belongs
    /// to the outer group : e.g. the outer group contains the inner group :
    /// No orthogonal group allowed.
    /// </remarks>
    /// </summary>
    public List<Document> FindDocumentsInMyGroupsByType(string documentType, int maxDepth)
    {
      List<Document> result = new List<Document>();
      List<Document> heads = FindHeadsFromGroupsIBelongTo();

      // if there is a depth limit, remove all unnecessary heads (0 = no depth limit, 1 = innermost group only)
      if (maxDepth != 0 && heads.Count > maxDepth)
      {
        //log.DebugFormat("Found too many heads ({0} > {1}), won't follow group with head {2}", heads.Count, maxDepth, heads[heads.Count - 1]);
        while (heads.Count > maxDepth)
          heads.RemoveAt(heads.Count - 1);
      }

      // Heads are sorted from innermost group to outermost group
      foreach (Document head in heads)
      {
        // Select DocumentLinks from head with the desired DocumentType :

        var qr = from link in Session.Demand().Query.All<DocumentLink>()
                 where link.LinkSemantic == LinkSemantic.GroupHeadToDocumentInGroup
                 where link.LinkSource == head
                 where link.LinkDestination.DocumentType == documentType
                 select link.LinkDestination;

        //QueryResult qr;
        //using (Query q = Session.CreateQuery("Select DocumentLink instances where {LinkSemantic}=@semantic and LinkSource=@head and {LinkDestination.DocumentType}=@docType"))
        //{
        //  q.Parameters.Add("@semantic", LinkSemantic.GroupHeadToDocumentInGroup);
        //  q.Parameters.Add("@head", head);
        //  q.Parameters.Add("@docType", documentType);
        //  qr = q.Execute();
        //}

        result.AddRange(qr);
      }
      return result;
    }

    #endregion
  }
}
