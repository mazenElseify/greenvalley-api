using System.Data;
using System.Data.Common;
using System.Dynamic;

public class Invoice
{
    public int? Id {get; set;}
    public string InvoiceCode {get; set;}
    public int? SupplierId {get; set;}
    public string SupplierName {get; set;}
    public string CustomerName{set; get;}
    public int? CustomerId {get; set;}
    public DateTime? Date {get; set;}
    public string Direction {get; set;}
}