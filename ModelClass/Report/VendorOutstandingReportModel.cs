using System;

namespace ModelClass.Report
{
    public class VendorOutstandingReportRow
    {
        public int LedgerID { get; set; }
        public string LedgerName { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
    }

    public class VendorOutstandingReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int FinYearId { get; set; }
        public int LedgerId { get; set; }
    }
}
