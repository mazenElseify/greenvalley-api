using System.Data;
using Npgsql;

public class InvoiceManager 
{
   const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511"; 
    public static string CreateDisplayInvoiceCode(int? invoiceNo, string direction)
    {
        if (invoiceNo == null)
            return null;

        if (direction == "" || direction == null)
            throw new InvalidOperationException("Invalid direction.");

        string prefix = "I-";
        if (direction == "OUT")
            prefix = "O-";

        // I-00000001
        // I-00000010
        return prefix + invoiceNo.ToString().PadLeft(7, '0');
    }
    public Invoice GetInvoiceById(int? invoiceId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select 
                t.invoice_no as invoice_no,
                c.name as customer_name,
                s.name as supplier_name,
                i.direction as direction,
                i.date as date
            from transaction t
                left outer join invoice i on i.id = t.invoice_no
                left outer join customer c on c.id = i.customer_id
                left outer join supplier s on s.id = i.supplier_id
            where (@id = t.invoice_no)";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = invoiceId ?? (object)DBNull.Value
            });
            NpgsqlDataReader r = cmd.ExecuteReader();
            Invoice invoice = new Invoice();
            while(r.HasRows && r.Read())
            {
                invoice.Id = r["id"] as int?;
                invoice.CustomerId = r["customer_id"] as int?;
                invoice.CustomerName = r["customer_name"] as string;
                invoice.SupplierId =r["supplier_id"] as int?;
                invoice.SupplierName = r["supplier_name"] as string;
                invoice.Direction = r["direction"] as string;

            }
            cmd.ExecuteNonQuery();
            return invoice;
            
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }

     public void PrintInvoice(int? invoiceNo, string direction)
    {
        decimal? InvoiceTotalPrice = 0;
        Console.WriteLine();
        Console.WriteLine("----------------------");
        Console.WriteLine($"Invoice No: {CreateDisplayInvoiceCode(invoiceNo, direction)}");
        Console.WriteLine("----------------------");
        TransactionManager tm = new TransactionManager();
        List<Transaction> transactions = tm.GetTransactionsList(invoiceNo, direction);
        string[] headers = null;
        string[,] data = null;

        if (direction == "OUT")
        {
            
            headers = new string[] { "Transaction ID", "Product Name", "Quantity", "Unit Price", "Customer Name", "T. Total Price", "Date" };
            data = new string[transactions.Count, headers.Length];
            for (int i = 0; i < transactions.Count; i++)
            {
                Transaction t = transactions[i];
                InvoiceTotalPrice += t.TotalPrice;
                data[i, 0] = t.Id.ToString();
                data[i, 1] = t.ProductName;
                data[i, 2] = t.Qty.ToString();
                data[i, 3] = t.Price.ToString();
                data[i, 4] = t.CustomerName;
                data[i, 5] = t.TotalPrice.ToString();
                data[i, 6] = t.Date.Value.ToLocalTime().ToString();
            }

            
        }
        else if (direction == "IN")
        {
            headers = new string[] { "ID", "Product Name", "Quantity", "Unit Price", "Total Price", "Supplier Name", "Date" };
            data = new string[transactions.Count, headers.Length];
            for (int i = 0; i < transactions.Count; i++)
            {
                Transaction t = transactions[i];
                
                data[i, 0] = t.Id.ToString();
                data[i, 1] = t.ProductName;
                data[i, 2] = t.Qty.ToString();
                data[i, 3] = t.Price.ToString();
                data[i, 4] = t.TotalPrice.ToString();
                data[i, 5] = t.SupplierName;
                data[i, 7] = t.Date.Value.ToLocalTime().ToString();
                InvoiceTotalPrice += t.TotalPrice;
            }
           
        }
        Util.PrintTable(headers, data, 1, 16);
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Invoice Total Price: {InvoiceTotalPrice}");
        Console.WriteLine("----------------------------------");
    }
    
}