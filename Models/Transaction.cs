using System.ComponentModel.DataAnnotations;
using System.Dynamic;

public class Transaction
{
    public int? Id { get; set; }
    public DateTime? Date { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int? Qty { get; set; }
    public decimal? Price { get; set; }
    public string Direction { get; set; }
    public int? InvoiceId { get; set; }

    public string InvoiceCode{get; set;}

    public int? CustomerId { get; set; }
    public string CustomerName { get; set; }
    public int? SupplierId { get; set; }
    public string SupplierName { get; set; }
    public int? InventoryId { get; set; }
    public string InventoryName { get; set; }
    public decimal? TotalPrice {
        get
        {
            return Qty * Price;
        }
        
    }
}
