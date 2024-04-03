using System.ComponentModel.Design;
using System.Data;
using System.Formats.Asn1;
using Npgsql;

public class CustomerManager
{
    const string CONN_STRING = "Host=localhost:5432;Username=postgres;password=2511";

    public Customer GetCustomerById(int id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();

            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select c.name,
                c.type_id,
                c.phone,
                c.email,
                t.name as type_name
            from customer c
                left outer join customer_type t on c.type_id = t.id
            where c.id=@id";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id
            });

            NpgsqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows && r.Read())
            {
                return new Customer()
                {
                    Id = id,
                    Name = r["name"] as string,
                    Type = r["type_name"] as string,
                    TypeId = r["type_id"] as int?,
                    Phone = r["phone"] as string,
                    Email = r["email"] as string
                };
            }

            return null;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public List<Customer> GetCustomerList()
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        List<Customer> customers = new List<Customer>();
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select 
                c.id,
                c.name,
                c.phone,
                c.type_id,
                c.email,
                t.name as type_name
            from customer c
                left outer join customer_type t on c.type_id = t.id";
            NpgsqlDataReader r = cmd.ExecuteReader();

            while (r.HasRows && r.Read())
            {
                Customer customer = new Customer();

                customer.Id = r["id"] as int?;
                customer.Name = r["name"] as string;
                customer.TypeId = r["type_id"] as int?;
                customer.Type = r["type_name"] as string;
                customer.Phone = r["phone"] as string;
                customer.Email = r["email"] as string;

                customers.Add(customer);
            }
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
        return customers;
    }

    public void PrintCustomersList()
    {
        string[] headers = { "ID", "Name", "Type", "Phone","Email" };
        List<Customer> customers = GetCustomerList();


        string[,] data = new string[customers.Count, 5];
        for (int i = 0; i < customers.Count; i++)
        {
            data[i, 0] = customers[i].Id.ToString();
            data[i, 1] = customers[i].Name;
            data[i, 2] = customers[i].Type;
            data[i, 3] = customers[i].Phone;
            data[i, 4] = customers[i].Email;
        }
        Util.PrintTable(headers, data, 3);
    }
    public void InsertNewCustomer(Customer customer)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into customer (name,phone,type_id,email) 
            values (@name,@phone,@typeId,@email)";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = customer.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Name ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("phone", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Phone ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = customer.TypeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Email ?? (object)DBNull.Value
            });

            // cmd.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Varchar)
            // { 
            //     Value = customer.Type ?? (object)DBNull.Value
            // });

            cmd.ExecuteNonQuery();
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
    public void PromptUserForInsert()
    {
        string input;
        Customer c = new Customer();

        Console.WriteLine(@"Enter Customer name:
            Press -2 to return to menu.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else
            c.Name = input;

        PrintTypeList();
        Console.WriteLine(@"Choose Customer type:
            Press -2 to return to menu.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else if (!CheckIfTypeExists(int.Parse(input)))
        {
            Console.WriteLine("Invalid type, Please Try again.");
            PromptUserForInsert();
            return;
        }
        else
            c.TypeId = int.Parse(input);

        Console.WriteLine(@"Enter Customer Phone number:
            Press -2 to return to menu.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else
            c.Phone = input;

        Console.WriteLine(@"Enter Customer Email:
            Press -2 to return to menu.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else
            c.Email = input;

        InsertNewCustomer(c);

        Console.WriteLine("Product has been inserted successfully");

    }
    public void UpdateCustomer(Customer customer)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update customer
                set name = @name,
                type_id = @typeId,
                phone = @phone,
                email = @email
                where id = @id";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = customer.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Name ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = customer.TypeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("phone", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Phone ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = customer.Email ?? (object)DBNull.Value
            });

            cmd.ExecuteNonQuery();
            Console.WriteLine("Update done successfully.");
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();

        }

    }
    public void PromptUserForUpdate()
    {
        // Customer c = new Customer();
        string input;

        Console.WriteLine("Update Customer: ");

        PrintCustomersList();
        Console.Write("Select Customer ID to update: ");
        input = Console.ReadLine();
        if (!CheckIfIdExists(int.Parse(input)))
        {
            Console.WriteLine("Invalid Input, please try again.");
            PromptUserForUpdate();
            return;
        }
        Customer c = GetCustomerById(int.Parse(input));
        c.Id = int.Parse(input);

        Console.WriteLine(@"Enter the name of customer or 'NA' To keep the original:
             or Press '-2' to cancel.");
        input = Console.ReadLine();

        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else if (input.ToUpper() == "NA")
        {
            c.Name = c.Name;
        }
        else
            c.Name = input;

        PrintTypeList();
        Console.WriteLine(@"Select type ID or 'NA' To keep the original:
        input = Console.ReadLine();
            Press '-2' to cancel");
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else if (input.ToUpper() == "NA")
            c.TypeId = c.TypeId;
        else if (!CheckIfTypeExists(int.Parse(input)) || int.Parse(input) < 1)
        {
            Console.WriteLine("Invalid input");
            PromptUserForUpdate();
            return;
        }

        else
            c.TypeId = int.Parse(input);

        Console.WriteLine(@"Enter Customer phone number or 'NA' To keep the original:
            or press '-2' to cancel.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else if (input.ToUpper() == "NA")
            c.Phone = c.Phone;

        else
            c.Phone = input;

        Console.WriteLine(@"Enter Customer Email or 'NA' To keep the original:
            or press '-2' to cancel.");
        input = Console.ReadLine();
        if (input == "-2")
        {
            ShowMenu();
            return;
        }
        else if (input.ToUpper() == "NA")
            c.Email = c.Email;

        else
            c.Email = input;

        UpdateCustomer(c);



    }
    public void DeleteCustomer(int? Id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = @"select count(*) from transaction where customer_id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = Id ?? (object)DBNull.Value
            });

            if (cmd.ExecuteNonQuery() > 0)
                throw new ApplicationException("Cannot delete customer because there is transaction associated with it.");
            

            cmd.Parameters.Clear();

            cmd.CommandText = @"delete from customer where id=@id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = Id ?? (object)DBNull.Value
            });
            if (cmd.ExecuteNonQuery() == 0)
                throw new ApplicationException("Customer was not deleted, perhaps the ID you specified is incorrect,");

            Console.WriteLine($"Customer with ID: {Id} has been deleted successfully.");

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();

        }

    }
    public void PromptUserForDelete()
    {
        PrintCustomersList();
        int id;
        Console.Write("Enter the ID of the Customer you wish to delete: ");
        id = int.Parse(Console.ReadLine());
        if (id == -2)
        {
            ShowMenu();
            return;
        }
        else if (!CheckIfIdExists(id))
        {
            Console.WriteLine("Invalid ID, ID can not be found, please try again.");
            PromptUserForDelete();
            return;
        }
        else
        { 
            DeleteCustomer(id);
            Console.WriteLine("Customer has been Deleted successfully.");
        }
    }

    public void PrintTypeList()
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();

            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from customer_type";

            NpgsqlDataReader r = cmd.ExecuteReader();

            string[] headers = { "ID", "Name" };
            List<string[]> data = new List<string[]>();

            while (r.HasRows && r.Read())
            {
                int? typeId = r["id"] as int?;
                string name = r["name"] as string;

                data.Add(new string[]
                {
                    typeId.ToString(),
                    name
                });

            }
            string[,] d = Util.ToMatrix<string>(data);
            Util.PrintTable(headers, d, 3);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public bool CheckIfIdExists(int Id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select * from customer where id=@Id";
            cmd.Parameters.Add(new NpgsqlParameter("Id", Id));
            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
    public bool CheckIfTypeExists(int typeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select * from customer_type where id=@Tid";

            cmd.Parameters.Add(new NpgsqlParameter("Tid", typeId));
            // cmd.Parameters.Add(new NpgsqlParameter("Pid", NpgsqlTypes.NpgsqlDbType.Integer)
            // { 
            //     Value = typeId ?? (object)DBNull.Value
            // });
            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public string GetTypeName(int? typeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();

            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select name from customer_type where id= @Tid";
            cmd.Parameters.Add(new NpgsqlParameter("Tid", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = typeId ?? (object)DBNull.Value
            });
            object result = cmd.ExecuteScalar();
            if (result == DBNull.Value)
                return null;
            else
                return (string)result;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }


    public void InsertNewType()
    {
        string input;
        CustomerType cT = new CustomerType();
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            Console.WriteLine("Insert new Customer type:");
            Console.WriteLine("-------------------------");
            Console.Write("Enter type name you wnat to insert or enter '-2' to cancel: ");
            input = Console.ReadLine();
            if (input == "-2")
            {
                Console.WriteLine("Proccess has been cancelled.");
                ShowMenu();
                return;
            }
            else
                cT.Name = input;

            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into customer_type (name) values (@name)";
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = cT.Name ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Customer type inserted successfully.");
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
    public void UpdateType(CustomerType ct)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            Customer c = new Customer();
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update customer_type set name=@name where id=@id ";
            
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            { 
                Value = ct.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            { 
                Value = ct.Name ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Update Done Successfully.");
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

    }
    public void PromptUserForUpdateType()
    {
        CustomerType ct = new CustomerType();
        int input;
        string strInput;
        Console.WriteLine("Update Customer Type:");
        Console.WriteLine("---------------------");
        PrintTypeList();
        Console.WriteLine("Choose the id of product you wish to update:\n'-1' to cancel.");
        input = int.Parse(Console.ReadLine());
        if (input == -1)
        {
            ShowMenu();
            return;
        }
        else if (!CheckIfTypeExists(input))
        {
            Console.WriteLine("Invald ID, Please try again.");
            PromptUserForUpdateType();
        }
        else
        {
            ct.Id = input;
        }
        Console.WriteLine($"Enter the name of type with ID: '{ct.Id}':");
        strInput = Console.ReadLine();
        ct.Name = strInput;
            UpdateType(ct);
    }

    public void DeleteType(int? typeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {




            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"delete from customer_type  where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            { 
                Value = typeId ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Deletion has been Done Successfully.");
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

    }
    public void PromptUserForDeleteType()
    {
        string input;
        PrintTypeList();
        Console.WriteLine("Choose the ID of type you wish to delete or enter '-2' to cancel: ");
        input = Console.ReadLine();
        bool validInt = int.TryParse(input, out int typeId);
        if (!validInt)
        {
            Console.WriteLine("Invalid input, Please try again.");
            PromptUserForDeleteType();
            return;
        }
        else if (typeId == -2)
        {
            Console.WriteLine("The proccess has been cancelled.");
            ShowMenu();
            return;

        }
        else if (!CheckIfTypeExists(typeId))
        {
            Console.WriteLine("Invalid ID, ID can not be found, please try again.");
            PromptUserForDeleteType();
            return;
        }
        else
        {
            DeleteType(typeId);
        }
    }

    public static void ShowMenu()
    {
        CustomerManager c = new CustomerManager();
        bool terminate = false;
        string readInput;

        do
        {
            Console.WriteLine("Customer Menu:");
            Console.WriteLine("==============");

            Console.WriteLine("1- Show Customer List.");
            Console.WriteLine("2- Insert New Customer.");
            Console.WriteLine("3- Update Customer.");
            Console.WriteLine("4- Delete Customer.");
            Console.WriteLine("5- Show Customer Types.");
            Console.WriteLine("6- Insert New Customer Type.");
            Console.WriteLine("7- Update Customer Type.");
            Console.WriteLine("8- Delete Customer Type.");
            Console.WriteLine("'-1'- Back");
            Console.WriteLine();
            Console.Write("Select Option: ");
            readInput = Console.ReadLine();

            bool validInt = int.TryParse(readInput, out int selection);
            if ((!validInt || selection < -1 || selection > 8) && selection != 0)
            {
                Console.WriteLine("Invalid selection.");
                Console.WriteLine();
                ShowMenu();
                return;
            }
            else if (selection == -1)
            {
                return;
            }

            switch (selection)
            {
                case 1:
                    c.PrintCustomersList();
                    break;
                case 2:
                    c.PromptUserForInsert();
                    break;
                case 3:
                    c.PromptUserForUpdate();
                    break;
                case 4:
                    c.PromptUserForDelete();
                    break;
                case 5:
                    c.PrintTypeList();
                    break;
                case 6:
                    c.InsertNewType();
                    break;
                case 7:
                    c.PromptUserForUpdateType();
                    break;
                case 8:
                    c.PromptUserForDeleteType();
                    break;
                case -1:
                    terminate = true;
                    break;
            }
        }
        while (!terminate);
    }
}