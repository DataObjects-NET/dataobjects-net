using System;

namespace Xtensive.Orm.Tests.Issues.Issue0408_EntitySetNullReference_Model
{
  [Serializable]
  [HierarchyRoot]
  public class DocumentLink : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    /// <summary>
    /// A link between two documents. E.g. corrected invoice with correcting invoice, corrected invoice with credit memo.
    /// </summary>
    [Field]
    public LinkSemantic LinkSemantic { get; set; }

    /// <summary>
    /// In case of a group defining link, this the head of the group.
    /// </summary>
    [Field]
    public Document LinkSource { get; set; }

    /// <summary>
    /// In case of a group defining link, this is the object belonging to the group
    /// </summary>
    [Field]
    public Document LinkDestination { get; set; }
  }

  public enum LinkSemantic
  {
    GroupHeadToDocumentInGroup = 0,
    CorrectedInvoiceToCorrectingInvoice = 1,
    CorrectedInvoiceToCreditMemo = 2,
    InvoiceOnMeasurementToEstimatedInvoice = 3
  }
}
