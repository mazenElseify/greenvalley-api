using System.Data;
using System.Security.Cryptography;
using Npgsql;

public class TransactionManager
{
    const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511";
    public List<Transaction> GetTransactionsList(int? invoiceNo, string direction)
    {

        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        List<Transaction> transactions = new List<Transaction>();
        try
        {


            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select 
                t.*,
                p.name as product_name,
                c.name as customer_name,
                s.name as supplier_name
            from transaction t
                inner join product p on p.id = t.product_id
                left outer join customer c on c.id = t.customer_id
                left outer join supplier s on s.id = t.supplier_id
            where (@invoice_no is null or @invoice_no = t.invoice_no)
                and (@direction is null or direction = @direction)";

            cmd.Parameters.Add(new NpgsqlParameter("direction", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = direction ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("invoice_no", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = invoiceNo ?? (object)DBNull.Value
            });
            NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.HasRows && r.Read())
            {
                Transaction transaction = new Transaction();
                transaction.Id = r["id"] as int?;
                transaction.Date = r["date"] as DateTime?;
                transaction.ProductId = r["product_id"] as string;
                transaction.ProductName = r["product_name"] as string;
                transaction.Qty = r["qty"] as int?;
                transaction.Price = r["unit_price"] as decimal?;
                transaction.Direction = r["direction"] as string;
                transaction.InvoiceNo = r["invoice_no"] as int?;
                transaction.CustomerId = r["customer_id"] as int?;
                transaction.CustomerName = r["customer_name"] as string;
                transaction.SupplierId = r["supplier_id"] as int?;
                transaction.SupplierName = r["supplier_name"] as string;
                transaction.InventoryId = r["inventory_id"] as int?;
                transactions.Add(transaction);
            }
            return transactions;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

    }
    public bool CheckIfTransactionExisits(int? transId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from transaction where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = transId ?? (object)DBNull.Value
            });
            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

    }
    // public int GetTransactionCount()
    // {
    //     NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
    //     try
    //     {
    //         conn.Open();
    //         NpgsqlCommand cmd = conn.CreateCommand();
    //         cmd.CommandText = @"";
    //     }
    //     finally
    //     {
    //         if (conn.State == ConnectionState.Open)
    //         {
    //             conn.Close();
    //         }
    //     }

    // }
    public void PrintAllTransactions()
    {
        string[] headers = null;
        string[,] data = null;
        string direction = null;
        int? input;
        Console.Write("0- Show All Transactions.\n1- Show Out Transactions.\n2- Show In Transactions.\nSelect Transaction Direction: ");
        input = int.Parse(Console.ReadLine());
        if (input == 0)
            direction = null;
        else if (input == 1)
            direction = "OUT";
        else if (input == 2)
            direction = "IN";
        else
        {
            Console.WriteLine("Invalid Input, Please specify Transaction direction.");
            PrintAllTransactions();
            return;
        }

        List<Transaction> transactions = GetTransactionsList(null, direction);
        // string[] headers = {"ID","Product Name","Quantity","Unit Price","Direction","Customer Name","Supplier Name","Invoice No.","Date" };
        // string[,] data = new string[transactions.Count, headers.Length];
        if (direction == "OUT")
        {
            Console.WriteLine();
            Console.WriteLine("-----------------");
            Console.WriteLine("OUT Transactions:");
            Console.WriteLine("-----------------");
            headers = new string[] { "ID", "Product Name", "Quantity", "Unit Price","Total Price", "Customer Name","Invoice No.", "Date" };
            data = new string[transactions.Count, headers.Length];
            for (int i = 0; i < transactions.Count; i++)
            {
                Transaction t = transactions[i];
                data[i, 0] = t.Id.ToString();
                data[i, 1] = t.ProductName;
                data[i, 2] = t.Qty.ToString();
                data[i, 3] = t.Price.ToString();
                data[i, 4] = t.TotalPrice.ToString();
                data[i, 5] = t.CustomerName;
                data[i, 6] = t.InvoiceNoStr;
                data[i, 7] = t.Date.Value.ToLocalTime().ToString();
            }
            Util.PrintTable(headers, data, 1, 16);
        }
        else if (direction == "IN")
        {
            Console.WriteLine();
            Console.WriteLine("-----------------");
            Console.WriteLine("IN Transactions:");
            Console.WriteLine("-----------------");
            // List<Transaction> transactions = GetTransactionsList(direction);
            headers = new string[] { "ID", "Product Name", "Quantity", "Unit Price", "Total Price", "Supplier Name", "Invoice No.", "Date" };
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
                data[i, 6] = t.InvoiceNoStr;
                data[i, 7] = t.Date.Value.ToLocalTime().ToString();

            }
            Util.PrintTable(headers, data, 1, 16);

        }
        else
        {
            Console.WriteLine("-----------------");
            Console.WriteLine("ALL Transactions:");
            Console.WriteLine("-----------------");

             headers = new string[] { "ID", "Product Name", "Quantity", "Unit Price", "Direction", "Customer Name", "Supplier Name", "Invoice No.", "Date" };
             data = new string[transactions.Count, headers.Length];
            for (int i = 0; i < transactions.Count; i++)
            {
                Transaction t = transactions[i];
                data[i, 0] = t.Id.ToString();
                data[i, 1] = t.ProductName;
                data[i, 2] = t.Qty.ToString();
                data[i, 3] = t.Price.ToString();
                data[i, 4] = t.Direction;
                data[i, 5] = t.CustomerName;
                data[i, 6] = t.SupplierName;
                data[i, 7] = t.InvoiceNoStr;
                data[i, 8] = t.Date.Value.ToLocalTime().ToString();
                // data[i,9] = t.InventoryId.ToString();
            }
            Util.PrintTable(headers, data, 1, 16);
        }

    }
    public void PrintInvoice(int? invoiceNo, string direction)
    {
        decimal? InvoiceTotalPrice = 0;
        Console.WriteLine();
        Console.WriteLine("----------------------");
        Console.WriteLine($"Invoice No: {CreateDisplayInvoiceCode(invoiceNo, direction)}");
        Console.WriteLine("----------------------");

        List<Transaction> transactions = GetTransactionsList(invoiceNo, direction);
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
    public void PrintTransactionById(int? id)
    {

    }
    public void InsertTransaction(Transaction t)
    {
        // Transaction transaction = new Transaction();
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into transaction (product_id,qty,unit_price,direction,supplier_id, customer_id,invoice_no,date,total_price)
                values (@product_id,@qty,@price,@direction,@supplier_id, @customer_id,@invoice_no,@date,@total_price)";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("product_id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = t.ProductId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("qty", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.Qty ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("price", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = t.Price ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("direction", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = t.Direction ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("supplier_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.SupplierId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("customer_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.CustomerId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("invoice_no", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.InvoiceNo ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("date", NpgsqlTypes.NpgsqlDbType.Timestamp)
            {
                Value = t.Date ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("total_price", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = t.TotalPrice ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
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

    public void PromptUserForInsert(int invoiceNo, string direction, int? supplierId, int? customerId)
    {
        string input;
        Transaction t = new Transaction();
        ProductManager pM = new ProductManager();
        CustomerManager cM = new CustomerManager();
        SupplierManager sM = new SupplierManager();

        Console.WriteLine("Create Transaction:");
        Console.WriteLine("------------------");
        t.Direction = direction;
        t.SupplierId = supplierId;
        t.CustomerId = customerId;

        if (t.Direction == "OUT")
        {
            pM.PrintProductsInList();
            Console.WriteLine("Select Product ID you want to make transaction on:\n'-1' to cancel. ");
            input = Console.ReadLine();
            if (input == "-1")
            {
                Console.WriteLine("Operation has been canceled.");
                return;
            }
            else if (!pM.CheckIfIdExists(input.ToUpper()))
            {
                Console.WriteLine("Invalid ID, Please choose a valid ID:\n'-1' to cancel.");
                return;
            }
            else
                t.ProductId = input.ToUpper();

            Console.Write("Select transaction Quantity: ");
            input = Console.ReadLine();
            if (!int.TryParse(input, out int qty))
            {
                Console.WriteLine("Invalid Input, you should enter Number.");
            }
            else
                t.Qty = qty;

            Console.WriteLine("Enter Product Unit Price:\n'-1' to cancel");
            input = Console.ReadLine();
            if (input == "-1")
            {
                Console.WriteLine("Proccess has been canceled.");
                return;
            }
            else if (!int.TryParse(input, out int price))
            {
                Console.WriteLine("Invalid Price.");
                return;
            }
            else
                t.Price = price;

            

            t.InvoiceNo = invoiceNo;

            t.Date = DateTime.Now;
        }
        else
        {
            pM.PrintProductsInList();
            Console.WriteLine("Select Product ID you want to make transaction on:\n'-1' to cancel. ");
            input = Console.ReadLine();
            if (input == "-1")
            {
                Console.WriteLine("Operation has been canceled.");
                return;
            }
            else if (!pM.CheckIfIdExists(input.ToUpper()))
            {
                Console.WriteLine("Invalid ID, Please choose a valid ID:\n'-1' to cancel.");
                return;
            }
            else
                t.ProductId = input.ToUpper();

            Console.Write("Select transaction Quantity: ");
            input = Console.ReadLine();
            if (!int.TryParse(input, out int qty))
            {
                Console.WriteLine("Invalid Input, you should enter Number.");
            }
            else
                t.Qty = qty;

            Console.WriteLine("Enter Transaction Price:\n'-1' to cancel");
            input = Console.ReadLine();
            if (input == "-1")
            {
                Console.WriteLine("Proccess has been canceled.");
                return;
            }
            else if (!int.TryParse(input, out int price))
            {
                Console.WriteLine("Invalid Price.");
                return;
            }
            else
                t.Price = price;

            

            t.InvoiceNo = invoiceNo;
            t.Date = DateTime.Now;
        }
        InsertTransaction(t);



    }

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

    public void UpdateTransaction(Transaction t)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update transaction
                set unit_price = @price,
                qty=@qty
            where id=@id ";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("price", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.Price ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("qty", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.Qty ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Update has been Done Successfully.");


        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
    public void PromptUserForUpdate()
    {
        Transaction t = new Transaction();
        Console.WriteLine("Update Transaction:");
        Console.WriteLine("-------------------");
        int input;
        PrintAllTransactions();
        Console.Write("Select Transaction ID you want to Update:\n'-1' To Cancel: ");
        input = int.Parse(Console.ReadLine());
        if (input == -1)
        {
            return;
        }
        else if (!CheckIfTransactionExisits(input))
        {
            Console.WriteLine("Invalid Input, The ID you specified is not found, Please Try Again.");
            PromptUserForUpdate();
            return;
        }
        else
            t.Id = input;

        Console.Write("Enter The Updated Price:\n'-1' To Cancel: ");
        input = int.Parse(Console.ReadLine());
        if (input == -1)
        {
            Console.WriteLine("Proccess has been canceled.");
            return;
        }
        else
            t.Price = input;

        Console.Write("Enter Quantity:\n'-1' To Cancel: ");
        input = int.Parse(Console.ReadLine());
        if (input == -1)
        {
            Console.WriteLine("Proccess has been canceled.");
            return;
        }
        else
            t.Qty = input;

        UpdateTransaction(t);

    }
    public void DeleteTransaction(int? id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"";
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
    public void PromptUserForDelete()
    {

    }
    public void CreateInvoice()
    {
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
                PromptUserForInsert(invoiceCode, direct, supplierId, null);
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
                PromptUserForInsert(invoiceCode, direct, null, customerId);
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
    public static void ShowMenu()
    {
        TransactionManager tm = new TransactionManager();
        bool terminate = false;
        string readInput;

        do
        {
            Console.WriteLine("Transaction Menu:");
            Console.WriteLine("==============");

            Console.WriteLine("1- Show Transaction List.");
            Console.WriteLine("2- Create New Invoice.");
            Console.WriteLine("3- Update Transaction.");
            // Console.WriteLine("4- Delete Transaction.");
            Console.WriteLine("'-1'-Back to Main menu.");
            readInput = Console.ReadLine();

            bool validInt = int.TryParse(readInput, out int selection);
            if ((!validInt || selection < -1 || selection > 3) && selection != 0)
            {
                Console.WriteLine("Invalid selection.");
                Console.WriteLine();
                // ShowMenu();
                return;
            }
            else if (selection == -1)
            {
                return;
            }

            switch (selection)
            {
                case 1:
                    tm.PrintAllTransactions();
                    break;
                case 2:
                    tm.CreateInvoice();
                    break;
                case 3:
                    tm.PromptUserForUpdate();
                    break;
                // case 4:
                //     tm.PromptUserForDelete();
                //     break;
                case -1:
                    terminate = true;
                    break;
            }
        }
        while (!terminate);
    }

}