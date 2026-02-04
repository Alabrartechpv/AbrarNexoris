using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Accounts
{
    /// <summary>
    /// Model for Credit Note invoice information (for grid display)
    /// </summary>
    public class CreditNoteInfo
    {
        public Int64 BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double InvoiceAmount { get; set; }
        public double CreditAmount { get; set; }
        public double Balance { get; set; }
    }

    /// <summary>
    /// Credit Note Master model - stores header information
    /// </summary>
    public class CreditNoteMaster
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int VoucherId { get; set; }
        public DateTime VoucherDate { get; set; }
        public int CustomerLedgerId { get; set; }
        public string CustomerName { get; set; }
        public int SReturnNo { get; set; }
        public string InvoiceNo { get; set; }
        public double CreditAmount { get; set; }
        public int PaymentMethodLedgerId { get; set; }
        public string PaymentMethod { get; set; }
        public string Narration { get; set; }
        public int UserId { get; set; }
        public bool CancelFlag { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Credit Note Details model - stores bill-wise credit adjustments
    /// </summary>
    public class CreditNoteDetails
    {
        public int Id { get; set; }
        public int CreditNoteMasterId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public double BillAmount { get; set; }
        public double OldBillAmount { get; set; }
        public double CreditAmount { get; set; }
        public double OldCreditAmount { get; set; }
        public double BalanceAmount { get; set; }
        public bool CancelFlag { get; set; }
    }

    /// <summary>
    /// Grid container for credit note invoices
    /// </summary>
    public class CreditNoteInfoGrid
    {
        public IEnumerable<CreditNoteInfo> InvoiceList { get; set; }
    }
}
