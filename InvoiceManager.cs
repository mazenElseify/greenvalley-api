using System.Data;
using Npgsql;
using NpgsqlTypes;

public class InvoiceManager 
{
   public string GenerateInvoiceCode(string direction)
    {
        if (direction == "" || direction == null)
            throw new InvalidOperationException("Invalid direction.");

        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select invoice_code from invoice where direction = @dir order by id desc limit 1";
            cmd.Parameters.Add(new NpgsqlParameter("dir", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = direction.ToUpper() ?? (object)DBNull.Value
            });

            // Format: PREFIX-NUMBER
            string lastInvoiceCode = cmd.ExecuteScalar() as string ?? "0";

            List<char> digits = new List<char>();
            foreach(char c in lastInvoiceCode)
                if(char.IsDigit(c))
                    digits.Add(c);

            string digitsStr = new string(digits.ToArray());

            int number = int.Parse(digitsStr);
            number++;

            string prefix = "I-";
            if (direction == "OUT")
                prefix = "O-";

            // I-00000001
            // I-00000010
            return prefix + number.ToString().PadLeft(7, '0');
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public Invoice GetInvoiceById(int? invoiceId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select 
                i.id,
                c.name as customer_name,
                s.name as supplier_name,
                i.direction as direction,
                i.date as date
            from transaction t
                left outer join invoice i on i.id = t.invoice_id
                left outer join customer c on c.id = i.customer_id
                left outer join supplier s on s.id = i.supplier_id
            where (@id = t.invoice_id)";
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
    public List<Transaction> GetInvoiceLines(string invoiceCode, string direction)
    {
        return new TransactionManager().GetTransactionsList(invoiceCode, direction);
    }

     public void PrintInvoice(string invoiceCode, string direction)
    {
        decimal? InvoiceTotalPrice = 0;
        Console.WriteLine();
        Console.WriteLine("----------------------");
        Console.WriteLine($"Invoice No: {invoiceCode}");
        Console.WriteLine("----------------------");
        List<Transaction> transactions = GetInvoiceLines(invoiceCode, direction);
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
                data[i, 6] = t.Date.Value.ToLocalTime().ToString();
                InvoiceTotalPrice += t.TotalPrice;
            }
           
        }
        Util.PrintTable(headers, data, 1, 16);
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Invoice Total Price: {InvoiceTotalPrice}");
        Console.WriteLine("----------------------------------");
    }

    public int? InsertInvoice(Invoice i)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into invoice (supplier_id,customer_id,direction,date,invoice_code)
                values (@supplier_id,@customer_id,@direction,@date,@invoice_code) returning id";
            cmd.Parameters.Add(new NpgsqlParameter("supplier_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = i.SupplierId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("customer_id",NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = i.CustomerId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("direction", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = i.Direction ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("date" ,NpgsqlTypes.NpgsqlDbType.Timestamp)
            {
                Value = i.Date ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("invoice_code",NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = i.InvoiceCode ?? (object)DBNull.Value
            });
            return cmd.ExecuteScalar() as int?;

            // cmd.Parameters.Clear();
            // cmd.Parameters.Add(new NpgsqlParameter("invoice_code",NpgsqlTypes.NpgsqlDbType.Varchar)
            // {
            //     Value = i.InvoiceCode ?? (object)DBNull.Value
            // });
            // cmd.CommandText = "select id from invoice where invoice_code=@invoice_code";
            // return cmd.ExecuteScalar() as int?;

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
    public void UpdateInvoice(int? transactionId)
    {
        decimal? totalPrice = 0;
        int? invoiceId = null;
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            
            cmd.CommandText = @"select * from transaction 
            where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = transactionId
            });
            NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.HasRows && r.Read())
            {
                invoiceId = r["invoice_id"] as int?;
                decimal? unitPrice = r["unit_price"] as decimal?;
                int? qty = r["qty"] as int?;
                totalPrice += unitPrice * qty;
                
            }

            cmd.Parameters.Clear();


            cmd.CommandText = @"update invoice 
                set total_price = @total_price
            where id = @id";

            cmd.Parameters.Add(new NpgsqlParameter("total_price", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = totalPrice ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = invoiceId ?? (object)DBNull.Value
            });

            cmd.ExecuteNonQuery();
        
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public void CreateInvoice()
    {
        TransactionManager tm = new TransactionManager();
        Invoice i = new Invoice();
        // string input;
        int ask;
        string direct;
        int count = 0;
        bool terminate = false;
        int? directionInput = Util.GetInput<int>("1- Transaction In.\n2- Transaction Out.\nChoose transaction Direction: ","-1",(val) => val > 0 && val <= 2);

        if (directionInput == 1)
        {

            direct = "IN";
            string invoiceCode = GenerateInvoiceCode(direct);
            int? supplierId = null;
            SupplierManager sM = new SupplierManager();
            sM.PrintSupplierList();
            supplierId = Util.GetInput<int>("Select Supplier ID: ", "-1", (val) => sM.CheckIfIdExists(val));            

            i.SupplierId = supplierId;
            i.Direction = direct;
            i.InvoiceCode = invoiceCode;
            i.Date = DateTime.Now;
            int? invoiceId = InsertInvoice(i);

            do
            {
                tm.PromptUserForInsert(invoiceId, direct, supplierId, null);
                Console.WriteLine("Do you want to do another transaction:\n1- 'YES'\n2- 'NO'");
                ask = int.Parse(Console.ReadLine());
                if (ask == 2)
                    terminate = true;
                else
                    terminate = false;
            }
            while (!terminate);
            PrintInvoice(invoiceCode, direct);
        }
        else if (directionInput == 2)
        {
            direct = "OUT";
            string invoiceCode = GenerateInvoiceCode(direct);
            int? customerId = null;
            CustomerManager cM = new CustomerManager();
            
            cM.PrintCustomersList();
            customerId = Util.GetInput<int>("Select Customer ID: ", "-1", (val) => cM.CheckIfIdExists(val));
           
            i.CustomerId = customerId;
            i.Direction = direct;
            i.InvoiceCode = invoiceCode;
            
            i.Date = DateTime.Now;
            int? invoiceId = InsertInvoice(i);

            do
            {
                tm.PromptUserForInsert(invoiceId, direct, null, customerId);
                count++;

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