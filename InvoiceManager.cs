using System.Data;
using Npgsql;

public class InvoiceManager 
{
   const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511"; 
   public int GenerateInvoiceCode(string direction)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select invoice_no from transaction where direction = @dir order by id desc limit 1";
            cmd.Parameters.Add(new NpgsqlParameter("dir", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = direction.ToUpper() ?? (object)DBNull.Value
            });

            // object val = cmd.ExecuteScalar();
            // int lastInvoiceCode = 0;
            // if (val != DBNull.Value)
            //     lastInvoiceCode = (int)val;
            int? lastInvoiceCode = cmd.ExecuteScalar() as int?;

            return (lastInvoiceCode ?? 0) + 1;
            //return (lastInvoiceCode == null ? 0 : 1) + 1;
            // if (lastInvoiceCode == null)
            //     return 1;
            // else
            //     return lastInvoiceCode.Value + 1;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

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
    public void CreateInvoice()
    {
        TransactionManager tm = new TransactionManager();
        string input;
        int ask;
        string direct;
        int count = 0;
        bool terminate = false;
        Console.Write("1- Transaction In.\n2- Transaction Out.\n'-1' cancel.\nPlease Choose transaction direction: ");
        input = Console.ReadLine();
        if (input == "-1")
        {
            Console.WriteLine("Proccess has been canceled.");
            return;
        }
        if (!int.TryParse(input, out int direction))
        {
            Console.WriteLine("Invalid Direction, please try again.");

            return;
        }
        else if (direction == 1)
        {

            direct = "IN";
            int invoiceCode = GenerateInvoiceCode(direct);
            int? supplierId = null;
            SupplierManager sM = new SupplierManager();
            sM.PrintSupplierList();
            Console.Write("Select Supplier ID you want to make transaction with:\n'-1' to cancel: ");
            input = Console.ReadLine();

            if (input == "-1")
            {
                Console.WriteLine("Proccess has been canceled.");
                return;
            }
            else if (!int.TryParse(input, out int sid))
            {
                Console.WriteLine("Invalid Supplier ID.");
                return;
            }
            else if (!sM.CheckIfIdExists(sid))
            {
                Console.WriteLine("No Supplier found with the specified ID.");
                return;
            }
            else
                supplierId = sid;

            // Ask user for supplier

            do
            {
                tm.PromptUserForInsert(invoiceCode, direct, supplierId, null);
                count++;
                Console.WriteLine($"Transaction {count} Done Successfully.");
                Console.WriteLine("Do you want to make another transaction:\n1- 'YES'\n2- 'NO'");
                ask = int.Parse(Console.ReadLine());
                if (ask == 2)
                    terminate = true;
                else
                    terminate = false;
            }
            while (!terminate);
            PrintInvoice(invoiceCode, direct);
        }
        else if (direction == 2)
        {
            direct = "OUT";
            int invoiceCode = GenerateInvoiceCode(direct);
            int? customerId = null;
            CustomerManager cM = new CustomerManager();
            cM.PrintCustomersList();
            Console.Write("Select Customer ID you want to make transaction with:\n'-1' to cancel: ");
            input = Console.ReadLine();

            if (input == "-1")
            {
                Console.WriteLine("Proccess has been canceled.");
                return;
            }
            else if (!int.TryParse(input, out int cid))
            {
                Console.WriteLine("Invalid customer ID.");
                return;
            }
            else if (!cM.CheckIfIdExists(cid))
            {
                Console.WriteLine("No Customer found with the specified ID.");
                return;
            }
            else
                customerId = cid;

            // ask user for customer

            do
            {
                tm.PromptUserForInsert(invoiceCode, direct, null, customerId);
                count++;
                Console.WriteLine($"Transaction {count} Done Successfully.");
                Console.WriteLine("Do you want to make another transaction:\n1- 'YES'\n2- 'NO'");
                ask = int.Parse(Console.ReadLine());
                if (ask == 2)
                    terminate = true;
                else
                    terminate = false;
            }
            while (!terminate);
            PrintInvoice(invoiceCode, direct);
        }

    }
    
}