using System.Data;
using System.Security.Cryptography;
using Npgsql;

public class TransactionManager
{
    const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511";
    public List<Transaction> GetTransactionsList(string invoiceCode, string direction)
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
                s.name as supplier_name,
                i.direction as direction,
                i.customer_id,
                i.supplier_id,
                i.invoice_code
            from transaction t
                left outer join invoice i on i.id = t.invoice_id
                inner join product p on p.id = t.product_id
                left outer join customer c on c.id = i.customer_id
                left outer join supplier s on s.id = i.supplier_id
            where (@invoice_code is null or @invoice_code = i.invoice_code)
                and (@direction is null or direction = @direction)";

            cmd.Parameters.Add(new NpgsqlParameter("direction", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = direction ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("invoice_code", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = invoiceCode ?? (object)DBNull.Value
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
                transaction.InvoiceId = r["invoice_id"] as int?;
                transaction.CustomerId = r["customer_id"] as int?;
                transaction.CustomerName = r["customer_name"] as string;
                transaction.SupplierId = r["supplier_id"] as int?;
                transaction.SupplierName = r["supplier_name"] as string;
                transaction.InventoryId = r["inventory_id"] as int?;
                transaction.InvoiceCode = r["invoice_code"] as string;
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
        InvoiceManager im = new InvoiceManager();
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
                data[i, 6] = t.InvoiceCode ;
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
                data[i, 6] = t.InvoiceCode;
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
                data[i, 7] = t.InvoiceCode;
                data[i, 8] = t.Date.Value.ToLocalTime().ToString();
                // data[i,9] = t.InventoryId.ToString();
            }
            Util.PrintTable(headers, data, 1, 16);
        }

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
            cmd.CommandText = @"insert into transaction (invoice_id,product_id,qty,unit_price,date,inventory_id)
                values (@invoice_id,@product_id,@qty,@price,@date,@inventory_id)";
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
            cmd.Parameters.Add(new NpgsqlParameter("invoice_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.InvoiceId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("inventory_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = t.InventoryId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("date", NpgsqlTypes.NpgsqlDbType.Timestamp)
            {
                Value = t.Date ?? (object)DBNull.Value
            });
            // cmd.Parameters.Add(new NpgsqlParameter("total_price", NpgsqlTypes.NpgsqlDbType.Money)
            // {
            //     Value = t.TotalPrice ?? (object)DBNull.Value
            // });
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

      public void PromptUserForInsert(int? invoiceId, string direction, int? supplierId, int? customerId)
    {
        // string input;
        Transaction t = new Transaction();
        ProductManager pM = new ProductManager();
        CustomerManager cM = new CustomerManager();
        SupplierManager sM = new SupplierManager();

        Console.WriteLine("Create Transaction:");
        Console.WriteLine("------------------");
        t.Direction = direction;
        t.SupplierId = supplierId;
        t.CustomerId = customerId;
        t.InventoryId = InventoryManager.DEFAULT_INVENTORY_ID;

        if (t.Direction == "OUT")
        {
            Console.WriteLine("----------------");
            Console.WriteLine("Out Transaction:");
            Console.WriteLine("----------------");
            
            pM.PrintProductsInList();
            t.ProductId = Util.GetInput("Select Product ID: ", "-1",(val) => pM.CheckIfIdExists(val.ToUpper())).ToUpper();
            if (t.ProductId == null)
                return;            

            var mgr = new InventoryManager();
            var availableStock = mgr.GetAvailableStock(t.ProductId);

            Console.WriteLine("Available stock: " + availableStock);
            t.Qty = Util.GetInput<int>("Enter quantity: ", "-1", (val) => val > 0 && val <= availableStock);
            if(t.Qty == null)
                return;
            t.Price = Util.GetInput<decimal>("Enter Product Price: ", "-1", (val) => val > 0);
            if (t.Price == null)
                return;
                        

            t.InvoiceId = invoiceId;

            t.Date = DateTime.Now;
        }
        else
        {
            Console.WriteLine("----------------");
            Console.WriteLine("In Transaction:");
            Console.WriteLine("----------------");
            pM.PrintProductsInList();
            t.ProductId = Util.GetInput("Select Product ID: " , "-1", (val) => pM.CheckIfIdExists(val.ToUpper())).ToUpper();
            if (t.ProductId == null)
                return;

            t.Qty = Util.GetInput<int>("Enter quantity: ", "-1", (val) => val > 0);
            if(t.Qty == null)
                return;
            
            t.Price = Util.GetInput<decimal>("Enter Product Price: ", "-1", (val) => val > 0);
            if (t.Price == null)
                return;

            t.InvoiceId = invoiceId;
            t.Date = DateTime.Now;
        }
        InsertTransaction(t);

        Console.WriteLine($"Transaction Done Successfully.");



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
        Console.WriteLine("-------------------");
        Console.WriteLine("Update Transaction:");
        Console.WriteLine("-------------------");
 

        PrintAllTransactions();
        t.Id = Util.GetInput<int>("Select Transaction ID: ", "-1", (val) => CheckIfTransactionExisits(val));
        if (t.Id == null)
            return;
        
        t.Price = Util.GetInput<decimal>("Enter the new Price: ", "-1", (val) => val > 0);
        if (t.Price == null)
            return;

        t.Qty = Util.GetInput<int>("Enter Quantity: ", "-1", (val) => val > 0);
        if (t.Qty == null)
            return;
        
        
        UpdateTransaction(t);
        InvoiceManager im = new InvoiceManager();
        im.UpdateInvoice(t.Id);

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
    
    public static void ShowMenu()
    {
        TransactionManager tm = new TransactionManager();
        InvoiceManager im = new InvoiceManager();
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
                    im.CreateInvoice();
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