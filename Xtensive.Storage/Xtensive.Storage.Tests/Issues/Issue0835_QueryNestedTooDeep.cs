using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues_Issue0835_QueryNestedTooDeep;

namespace Xtensive.Storage.Tests.Issues_Issue0835_QueryNestedTooDeep
{
  [HierarchyRoot]
  public class BaseEntity3 : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
 
  public class BaseEntity2 : BaseEntity3
  {
  }

  public class BaseEntity1 : BaseEntity2
  {
  }

  public class BaseEntity : BaseEntity1
  {
  }

  public class EconomicOperation : BaseEntity
  {
  }
  public class Region : BaseEntity
  {
  }
  public class Nomenclature : BaseEntity
  {
  }
  public class TaxRates : BaseEntity
  {

  }

  public class Customer : BaseEntity
  {
  }

  public class ConsumptionCategory : BaseEntity
  {
  }

  public class PrivilegesLaw : BaseEntity
  {
  }

  public class Contract : BaseEntity
  {
  }

  public class EntryCategory : BaseEntity
  {
  }

  public class SaleCategory : BaseEntity
  {
  }

  public class NamedEntity : BaseEntity
  {
  }

  [HierarchyRoot]
  public class PaymentTransfer : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public long Number { get; set; }

    [Field]
    public DateTime Date { get; set; }

    [Field]
    public PaymentTransferOperationType? OperationType { get; set; }

    [Field]
    public GasDeliveryTypes? DeliveryTypeTo { get; set; }

    [Field]
    public GasDeliveryTypes? DeliveryTypeFrom { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public EntryCategory EntryCategory { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public EntryCategory EntryCategoryFrom { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public SaleCategory SaleCategoryTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public SaleCategory SaleCategoryFrom { get; set; }

    [Field]
    public DateTime? ClosedDebtCorrectionsDate { get; set; }

    [Field]
    public DateTime? AdvanceCorrectionsDate { get; set; }

    [Field]
    public DateTime? ReversalAdvanceCorrectionsDate { get; set; }

    [Field]
    public DateTime? ReversalDebtCorrectionsDate { get; set; }

    [Field]
    public DateTime? PaymentDocumentDate { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public Contract ContractTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public Contract ContractFrom { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field]
    public Contract PayerContract { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Contract PayerContractTo { get; set; }

    [Field()]
    public bool? BuyerDocument { get; set; }

    [Field()]
    public bool? SupplierDocument { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public PrivilegesLaw PrivilegesLaw { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public PrivilegesLaw PrivilegesLawFrom { get; set; }

    [Field()]
    public bool? CloseRecoveredDebts { get; set; }

    [Field()]
    public bool? CloseDebtsUnderMoratorium { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public ConsumptionCategory ConsumptionCategoryTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public ConsumptionCategory ConsumptionCategoryFrom { get; set; }

    [Field()]
    public DateTime? PaymentPeriodEnd { get; set; }

    [Field()]
    public DateTime? PaymentToSupplierPeriodEnd { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer CustomerTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer CustomerFrom { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public TaxRates TaxRatesTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public TaxRates TaxRatesFrom { get; set; }

    [Field()]
    public DateTime? PaymentPeriodBegin { get; set; }

    [Field()]
    public DateTime? PaymentToSupplierPeriodBegin { get; set; }

    [Field()]
    public bool? NotReflectReversalOfClosedAdvances { get; set; }

    [Field()]
    public bool? DoNotCreateNewAdvancesInvoices { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Nomenclature Nomenclature { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Nomenclature NomenclatureFrom { get; set; }

    [Field(Length = 50)]
    public string PaymentDocumentNumber { get; set; }

    [Field()]
    public bool? ReflectPaymentReversalAsCreditAdvance { get; set; }

    [Field()]
    public bool? RecalculateTaxesOnTheOldAdvanceDocument { get; set; }

    [Field()]
    public DateTime? CorrectionsPeriod { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer Payer { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer PayerTo { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer PrivilegeRecipient { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer PrivilegeRecipientFrom { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer Consumer { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Customer ConsumerFrom { get; set; }

    [Field(Length = 100)]
    public string DocumentPresentation { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Region Region { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public Region RegionFrom { get; set; }

    [Field()]
    public bool? SaveAdvancesOnTheOldPaymentDocument { get; set; }

    [Field()]
    public bool? SaveAdvancesSecurities { get; set; }

    [Field()]
    public bool? ReverseAdvancesOnFIFO { get; set; }

    [Field()]
    public bool? ReverseOnlyIndicatedDocument { get; set; }

    [Field(Precision = 15, Scale = 2)]
    public decimal? Sum { get; set; }

    [Field(Precision = 15, Scale = 2)]
    public decimal? VatTo { get; set; }

    [Field(Precision = 15, Scale = 2)]
    public decimal? VatFrom { get; set; }

    [Field()]
    public bool? SpecifySalesBookCorrectionsPeriodManually { get; set; }

    [Field()]
    public bool? ComposeAdvanceForFullDocumentSum { get; set; }

    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    [Field()]
    public EconomicOperation EconomicOperation { get; set; }

    [Field()]
    public NamedEntity Entity1{ get; set; }

    [Field()]
    public NamedEntity Entity2 { get; set; }

    [Field()]
    public NamedEntity Entity3 { get; set; }

    [Field()]
    public NamedEntity Entity4 { get; set; }

    [Field()]
    public NamedEntity Entity5 { get; set; }

    [Field()]
    public NamedEntity Entity6 { get; set; }

    [Field()]
    public NamedEntity Entity7 { get; set; }

    [Field()]
    public NamedEntity Entity8 { get; set; }
  }

  public enum GasDeliveryTypes
  {
    C,
    D
  }

  public enum PaymentTransferOperationType
  {
    A,
    B
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0835_QueryNestedTooDeep : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (PaymentTransfer).Assembly, typeof (PaymentTransfer).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Session.Open(Domain)) {
        using (var transaction = Transaction.Open(session)) {
          var query = Query.All<PaymentTransfer>().Select(c => new PaymentTransferDto {
            ID = c.Id,
            Number = c.Number,
            Date = c.Date,
            OperationType = (PaymentTransferOperationType?) (int?) c.OperationType,
            DeliveryTypeTo = (GasDeliveryTypes?) (int?) c.DeliveryTypeTo,
            DeliveryTypeFrom = (GasDeliveryTypes?) (int?) c.DeliveryTypeFrom,
            EntryCategoryId = c.EntryCategory!=null ? (int?) c.EntryCategory.Id : null,
            EntryCategoryName = c.EntryCategory!=null ? c.EntryCategory.Name : null,
            EntryCategoryFromId = c.EntryCategoryFrom!=null ? (int?) c.EntryCategoryFrom.Id : null,
            EntryCategoryFromName = c.EntryCategoryFrom!=null ? c.EntryCategoryFrom.Name : null,
            SaleCategoryToId = c.SaleCategoryTo!=null ? (int?) c.SaleCategoryTo.Id : null,
            SaleCategoryToName = c.SaleCategoryTo!=null ? c.SaleCategoryTo.Name : null,
            SaleCategoryFromId = c.SaleCategoryFrom!=null ? (int?) c.SaleCategoryFrom.Id : null,
            SaleCategoryFromName = c.SaleCategoryFrom!=null ? c.SaleCategoryFrom.Name : null,
            ClosedDebtCorrectionsDate = c.ClosedDebtCorrectionsDate,
            AdvanceCorrectionsDate = c.AdvanceCorrectionsDate,
            ReversalAdvanceCorrectionsDate = c.ReversalAdvanceCorrectionsDate,
            ReversalDebtCorrectionsDate = c.ReversalDebtCorrectionsDate,
            PaymentDocumentDate = c.PaymentDocumentDate,
            ContractToId = c.ContractTo!=null ? (int?) c.ContractTo.Id : null,
            ContractToName = c.ContractTo!=null ? c.ContractTo.Name : null,
            ContractFromId = c.ContractFrom!=null ? (int?) c.ContractFrom.Id : null,
            ContractFromName = c.ContractFrom!=null ? c.ContractFrom.Name : null,
            PayerContractId = c.PayerContract!=null ? (int?) c.PayerContract.Id : null,
            PayerContractName = c.PayerContract!=null ? c.PayerContract.Name : null,
            PayerContractToId = c.PayerContractTo!=null ? (int?) c.PayerContractTo.Id : null,
            PayerContractToName = c.PayerContractTo!=null ? c.PayerContractTo.Name : null,
            BuyerDocument = c.BuyerDocument,
            SupplierDocument = c.SupplierDocument,
            PrivilegesLawId = c.PrivilegesLaw!=null ? (int?) c.PrivilegesLaw.Id : null,
            PrivilegesLawName = c.PrivilegesLaw!=null ? c.PrivilegesLaw.Name : null,
            PrivilegesLawFromId = c.PrivilegesLawFrom!=null ? (int?) c.PrivilegesLawFrom.Id : null,
            PrivilegesLawFromName = c.PrivilegesLawFrom!=null ? c.PrivilegesLawFrom.Name : null,
            CloseRecoveredDebts = c.CloseRecoveredDebts,
            CloseDebtsUnderMoratorium = c.CloseDebtsUnderMoratorium,
            ConsumptionCategoryToId = c.ConsumptionCategoryTo!=null ? (int?) c.ConsumptionCategoryTo.Id : null,
            ConsumptionCategoryToName = c.ConsumptionCategoryTo!=null ? c.ConsumptionCategoryTo.Name : null,
            ConsumptionCategoryFromId = c.ConsumptionCategoryFrom!=null ? (int?) c.ConsumptionCategoryFrom.Id : null,
            ConsumptionCategoryFromName = c.ConsumptionCategoryFrom!=null ? c.ConsumptionCategoryFrom.Name : null,
            PaymentPeriodEnd = c.PaymentPeriodEnd,
            PaymentToSupplierPeriodEnd = c.PaymentToSupplierPeriodEnd,
            CustomerToId = c.CustomerTo!=null ? (int?) c.CustomerTo.Id : null,
            CustomerToName = c.CustomerTo!=null ? c.CustomerTo.Name : null,
            CustomerFromId = c.CustomerFrom!=null ? (int?) c.CustomerFrom.Id : null,
            CustomerFromName = c.CustomerFrom!=null ? c.CustomerFrom.Name : null,
            TaxRatesToId = c.TaxRatesTo!=null ? (int?) c.TaxRatesTo.Id : null,
            TaxRatesToName = c.TaxRatesTo!=null ? c.TaxRatesTo.Name : null,
            TaxRatesFromId = c.TaxRatesFrom!=null ? (int?) c.TaxRatesFrom.Id : null,
            TaxRatesFromName = c.TaxRatesFrom!=null ? c.TaxRatesFrom.Name : null,
            PaymentPeriodBegin = c.PaymentPeriodBegin,
            PaymentToSupplierPeriodBegin = c.PaymentToSupplierPeriodBegin,
            NotReflectReversalOfClosedAdvances = c.NotReflectReversalOfClosedAdvances,
            DoNotCreateNewAdvancesInvoices = c.DoNotCreateNewAdvancesInvoices,
            NomenclatureId = c.Nomenclature!=null ? (int?) c.Nomenclature.Id : null,
            NomenclatureName = c.Nomenclature!=null ? c.Nomenclature.Name : null,
            NomenclatureFromId = c.NomenclatureFrom!=null ? (int?) c.NomenclatureFrom.Id : null,
            NomenclatureFromName = c.NomenclatureFrom!=null ? c.NomenclatureFrom.Name : null,
            PaymentDocumentNumber = c.PaymentDocumentNumber,
            ReflectPaymentReversalAsCreditAdvance = c.ReflectPaymentReversalAsCreditAdvance,
            RecalculateTaxesOnTheOldAdvanceDocument = c.RecalculateTaxesOnTheOldAdvanceDocument,
            CorrectionsPeriod = c.CorrectionsPeriod,
            PayerId = c.Payer!=null ? (int?) c.Payer.Id : null,
            PayerName = c.Payer!=null ? c.Payer.Name : null,
            PayerToId = c.PayerTo!=null ? (int?) c.PayerTo.Id : null,
            PayerToName = c.PayerTo!=null ? c.PayerTo.Name : null,
            PrivilegeRecipientId = c.PrivilegeRecipient!=null ? (int?) c.PrivilegeRecipient.Id : null,
            PrivilegeRecipientName = c.PrivilegeRecipient!=null ? c.PrivilegeRecipient.Name : null,
            PrivilegeRecipientFromId = c.PrivilegeRecipientFrom!=null ? (int?) c.PrivilegeRecipientFrom.Id : null,
            PrivilegeRecipientFromName = c.PrivilegeRecipientFrom!=null ? c.PrivilegeRecipientFrom.Name : null,
            ConsumerId = c.Consumer!=null ? (int?) c.Consumer.Id : null,
            ConsumerName = c.Consumer!=null ? c.Consumer.Name : null,
            ConsumerFromId = c.ConsumerFrom!=null ? (int?) c.ConsumerFrom.Id : null,
            ConsumerFromName = c.ConsumerFrom!=null ? c.ConsumerFrom.Name : null,
            DocumentPresentation = c.DocumentPresentation,
            RegionId = c.Region!=null ? (int?) c.Region.Id : null,
            RegionName = c.Region!=null ? c.Region.Name : null,
            RegionFromId = c.RegionFrom!=null ? (int?) c.RegionFrom.Id : null,
            RegionFromName = c.RegionFrom!=null ? c.RegionFrom.Name : null,
            SaveAdvancesOnTheOldPaymentDocument = c.SaveAdvancesOnTheOldPaymentDocument,
            SaveAdvancesSecurities = c.SaveAdvancesSecurities,
            ReverseAdvancesOnFIFO = c.ReverseAdvancesOnFIFO,
            ReverseOnlyIndicatedDocument = c.ReverseOnlyIndicatedDocument,
            Sum = c.Sum,
            VatTo = c.VatTo,
            VatFrom = c.VatFrom,
            SpecifySalesBookCorrectionsPeriodManually = c.SpecifySalesBookCorrectionsPeriodManually,
            ComposeAdvanceForFullDocumentSum = c.ComposeAdvanceForFullDocumentSum,
            EconomicOperationId = c.EconomicOperation!=null ? (int?) c.EconomicOperation.Id : null,
            EconomicOperationName = c.EconomicOperation!=null ? c.EconomicOperation.Name : null,
          });
          var result = query.Count();
        }
      }
    }
  }

  public class PaymentTransferDto
  {
    public Guid ID { get; set; }

    public long Number { get; set; }

    public DateTime Date { get; set; }

    public PaymentTransferOperationType? OperationType { get; set; }

    public GasDeliveryTypes? DeliveryTypeTo { get; set; }

    public GasDeliveryTypes? DeliveryTypeFrom { get; set; }

    public int? EntryCategoryId { get; set; }

    public string EntryCategoryName { get; set; }

    public int? EntryCategoryFromId { get; set; }

    public string EntryCategoryFromName { get; set; }

    public int? SaleCategoryToId{ get; set; }

    public string SaleCategoryToName { get; set; }

    public int? SaleCategoryFromId { get; set; }

    public string SaleCategoryFromName { get; set; }

    public DateTime? ClosedDebtCorrectionsDate { get; set; }

    public DateTime? AdvanceCorrectionsDate { get; set; }

    public DateTime? ReversalAdvanceCorrectionsDate { get; set; }

    public DateTime? ReversalDebtCorrectionsDate { get; set; }

    public DateTime? PaymentDocumentDate { get; set; }

    public int? ContractToId { get; set; }

    public string ContractToName { get; set; }

    public int? ContractFromId { get; set; }

    public string ContractFromName { get; set; }

    public int? PayerContractId { get; set; }

    public string PayerContractName { get; set; }

    public int? PayerContractToId { get; set; }

    public string PayerContractToName { get; set; }

    public bool? BuyerDocument { get; set; }

    public bool? SupplierDocument { get; set; }

    public int? PrivilegesLawId { get; set; }

    public string PrivilegesLawName { get; set; }

    public int? PrivilegesLawFromId { get; set; }

    public string PrivilegesLawFromName { get; set; }

    public bool? CloseRecoveredDebts { get; set; }

    public bool? CloseDebtsUnderMoratorium { get; set; }

    public int? ConsumptionCategoryToId { get; set; }

    public string ConsumptionCategoryToName { get; set; }

    public int? ConsumptionCategoryFromId { get; set; }

    public string ConsumptionCategoryFromName { get; set; }

    public DateTime? PaymentPeriodEnd { get; set; }

    public DateTime? PaymentToSupplierPeriodEnd { get; set; }

    public int? CustomerToId { get; set; }

    public string CustomerToName { get; set; }

    public int? CustomerFromId { get; set; }

    public string CustomerFromName { get; set; }

    public int? TaxRatesToId { get; set; }

    public string TaxRatesToName { get; set; }

    public int? TaxRatesFromId { get; set; }

    public string TaxRatesFromName { get; set; }

    public DateTime? PaymentPeriodBegin { get; set; }

    public DateTime? PaymentToSupplierPeriodBegin { get; set; }

    public bool? NotReflectReversalOfClosedAdvances { get; set; }

    public bool? DoNotCreateNewAdvancesInvoices { get; set; }

    public int? NomenclatureId { get; set; }

    public string NomenclatureName { get; set; }

    public int? NomenclatureFromId { get; set; }

    public string NomenclatureFromName { get; set; }

    public string PaymentDocumentNumber { get; set; }

    public bool? ReflectPaymentReversalAsCreditAdvance { get; set; }

    public bool? RecalculateTaxesOnTheOldAdvanceDocument { get; set; }

    public DateTime? CorrectionsPeriod { get; set; }

    public int? PayerId { get; set; }

    public string PayerName { get; set; }

    public int? PayerToId { get; set; }

    public string PayerToName { get; set; }

    public int? PrivilegeRecipientId { get; set; }

    public string PrivilegeRecipientName { get; set; }

    public int? PrivilegeRecipientFromId { get; set; }

    public string PrivilegeRecipientFromName { get; set; }

    public int? ConsumerId { get; set; }

    public string ConsumerName { get; set; }

    public int? ConsumerFromId { get; set; }

    public string ConsumerFromName { get; set; }

    public string DocumentPresentation { get; set; }

    public int? RegionId { get; set; }

    public string RegionName { get; set; }

    public int? RegionFromId { get; set; }

    public string RegionFromName { get; set; }

    public bool? SaveAdvancesOnTheOldPaymentDocument { get; set; }

    public bool? SaveAdvancesSecurities { get; set; }

    public bool? ReverseAdvancesOnFIFO { get; set; }

    public bool? ReverseOnlyIndicatedDocument { get; set; }

    public decimal? Sum { get; set; }

    public decimal? VatTo { get; set; }

    public decimal? VatFrom { get; set; }

    public bool? SpecifySalesBookCorrectionsPeriodManually { get; set; }

    public bool? ComposeAdvanceForFullDocumentSum { get; set; }

    public int? EconomicOperationId { get; set; }

    public string EconomicOperationName { get; set; }

    public string Name1 { get; set; }

    public string Name2 { get; set; }

    public string Name3 { get; set; }

    public string Name4 { get; set; }

    public string Name5 { get; set; }

    public string Name6 { get; set; }

    public string Name7 { get; set; }

    public string Name8 { get; set; }
  }
}
 