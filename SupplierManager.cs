using System.Data;
using Npgsql;

public class SupplierManager
{
    public Supplier GetSupplierById(int? id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from supplier where id = @id";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id ?? (object)DBNull.Value
            });

            NpgsqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows && r.Read())
            {
                return new Supplier()
                {
                    Id = id,
                    Name = r["name"] as string,
                    Phone = r["phone"] as string,
                    International = r["international"] as bool?,
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
    public List<Supplier> GetSupplierList()
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        List<Supplier> suppliers = new List<Supplier>();
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from supplier";

            NpgsqlDataReader r = cmd.ExecuteReader();

            while (r.HasRows && r.Read())
            {
                Supplier supp = new Supplier();

                supp.Id = r["id"] as int?;
                supp.Name = r["name"] as string;
                supp.International = r["international"] as bool?;
                supp.Phone = r["phone"] as string;
                supp.Email = r["email"] as string;

                suppliers.Add(supp);

            }
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
        return suppliers;

    }
    public void PrintSupplierById(int? id)
    {

        List<Supplier> supp = new List<Supplier>();

        Supplier s = GetSupplierById(id);
        supp.Add(s);

        string[,] data = new string[,]
        {
            {"ID", s.Id?.ToString()},
            {"Name", s.Name },
            {"International", s.InternationalStr },
            {"Phone", s.Phone },
            {"Email", s.Email }
        };

        Util.PrintTable(null, data, 3);
    }

    // select * from transaction where supplier_id=@id
    // select * from transaction where customer_id=@id
    // select * from transaction where customer_id=@id
    public void PrintSupplierList()
    {
        string[] headers = { "ID", "Name", "State", "Phone", "Email" };

        List<Supplier> supplliers = GetSupplierList();

        string[,] data = new string[supplliers.Count, headers.Length];
        for (int i = 0; i < supplliers.Count; i++)
        {
            data[i, 0] = supplliers[i].Id.ToString();
            data[i, 1] = supplliers[i].Name;
            data[i, 2] = supplliers[i].InternationalStr;
            data[i, 3] = supplliers[i].Phone;
            data[i, 4] = supplliers[i].Email;
        }
        Util.PrintTable(headers, data, 3);

    }
    public void InsertNewSupplier(Supplier supplier)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into supplier (name,phone,international,email)
            values (@name,@phone,@international,@email)";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = supplier.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Name ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("international", NpgsqlTypes.NpgsqlDbType.Boolean)
            {
                Value = supplier.International ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("phone", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Phone ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Email ?? (object)DBNull.Value
            });

            NpgsqlDataReader r = cmd.ExecuteReader();

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }
    public void PromptUserForInsert()
    {
        Supplier supplier = new Supplier();

        int? input;
        // string inputStr;

        Console.WriteLine("--------------------");
        Console.WriteLine("Insert new Supplier:");
        Console.WriteLine("--------------------");

        // Input Name:
        supplier.Name = Util.GetInput("Enter Supplier Name: ", "-1", (val) => val != null);
        if (supplier.Name == null)
            return;

        input = Util.GetInput<int>("0. International\n1. Local\n Select Supplier Nationallity: ", "-1", (val) => val < 2 && val >= 0);
        if (input == null)
            return;
        
        if (input == 1)
        {
            supplier.International = true;
        }
        else if (input == 0)
        {
            supplier.International = false;
        }
        
        supplier.Phone = Util.GetInput("Enter Supplier Phone number: ", "-1", (val) => val.Length <= 15);
        if (supplier.Phone == null)
            return;
        
        supplier.Email = Util.GetInput("Enter Supplier Email: ", "-1", null);

        InsertNewSupplier(supplier);
        Console.WriteLine("Supplier has been inserted successfully.");


    }
    public void UpdateSupplier(Supplier supplier)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update supplier
            set name = @name,
            international = @international,
            phone = @phone,
            email = @email
            where id = @id";

            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = supplier.Id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Name ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("international", NpgsqlTypes.NpgsqlDbType.Boolean)
            {
                Value = supplier.International ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("phone", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Phone ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("email", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = supplier.Email ?? (object)DBNull.Value
            });

            cmd.ExecuteNonQuery();
            Console.WriteLine("Supplier has been Updated successfully.");

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }
    public void PromptUserForUpdate()
    {
        string input;
        Supplier supplier = new Supplier();

        Console.WriteLine("Update Supplier:");
        Console.WriteLine("-----------------");

        PrintSupplierList();

        Console.WriteLine();
        Console.Write("Enter ID of Supplier you wish to Update: ");
        input = Console.ReadLine();
        if (!CheckIfIdExists(int.Parse(input)))
        {
            Console.WriteLine($"Invalid ID, ID:'${input}' not found.");
            ShowMenu();
            return;
        }
        else
            supplier.Id = int.Parse(input);

        Supplier s = GetSupplierById(supplier.Id);
        PrintSupplierById(supplier.Id);
        Console.WriteLine("Enter new name:\nEnter 'NA' to keep the original:\n -1 to cancel.");
        input = Console.ReadLine();
        
        if (input == "-1")
        {
            return;
        }
        else if (input.ToUpper() == "NA")
            supplier.Name = s.Name;
        else
            supplier.Name = input;

        Console.WriteLine("Enter Supplier Phone no.:\n'NA' To keep the original:\n-1 To cancel.");
        input = Console.ReadLine();
        if (input == "-1")
        {
            return;
        }
        else if (input.ToUpper() == "NA")
        {
            supplier.Phone = s.Phone;
        }
        else
            supplier.Phone = input;


        Console.WriteLine("Enter Email:\n'NA' To keep the original:\n-1 to cancel.");
        input = Console.ReadLine();
        if (input == "-1")
        {
            return;
        }
        else if (input.ToUpper() == "NA")
        {
            supplier.Email = s.Email;
        }
        else
            supplier.Email = input;

        UpdateSupplier(supplier);
        Console.WriteLine("Supplier Updated Successfully.");
    }
    public void DeleteSupplier(int? id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select count(*) from transaction where supplier_id=@id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id ?? (object)DBNull.Value
            });

            if ((int)cmd.ExecuteScalar() > 0)
                throw new ApplicationException("Cannot delete supplier because there are transaction associated with it.");

            cmd.Parameters.Clear();
            cmd.CommandText = @"delete from supplier where id=@id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id ?? (object)DBNull.Value
            });

            if (cmd.ExecuteNonQuery() == 0)
                throw new ApplicationException("Supplier was not deleted, perhaps the ID you have specified is not correct.");

            Console.WriteLine($"Supplier with ID: {id} has been deleted successfully.");

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }
    public void PromptUserForDelete()
    {
        string input;
        // Supplier s = new Supplier();
        PrintSupplierList();
        Console.WriteLine("Select the ID of supplier you wish to delete:\n-1 to cancel: ");
        input = Console.ReadLine();
        bool validInt = int.TryParse(input, out int id);

        if (!validInt || !CheckIfIdExists(id))
        {
            Console.WriteLine("Invalid ID, Please try again.");
            PromptUserForDelete();
        }
        else if (id == -1)
        {
            ShowMenu();
            return;
        }
        else
        {
            try
            {
                DeleteSupplier(id);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
    public bool CheckIfIdExists(int? id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select * from supplier where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id ?? (object)DBNull.Value
            });
            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }
    // public string GetSupplierName(int? id)
    // {
    //     NpgsqlConnection conn = new NpgsqlConnection(Constants.CONN_STRING);
    //     try
    //     {
    //         conn.Open();
    //         NpgsqlCommand cmd = conn.CreateCommand();
    //         cmd.CommandText = @"";

    //     }
    //     finally
    //     {
    //         if (conn.State == ConnectionState.Open)
    //             conn.Close();
    //     }

    // }
    public static void ShowMenu()
    {
        SupplierManager sm = new SupplierManager();
        bool terminate = false;
        string readInput;

        do
        {
            Console.WriteLine("Supplier Menu:");
            Console.WriteLine("==============");

            Console.WriteLine("1- Show Supplier List.");
            Console.WriteLine("2- Insert New Supplier.");
            Console.WriteLine("3- Update Supplier.");
            Console.WriteLine("4- Delete Supplier.");
            Console.WriteLine("'-1'-Back to Main menu.");
            readInput = Console.ReadLine();

            bool validInt = int.TryParse(readInput, out int selection);
            if ((!validInt || selection < -1 || selection > 4) && selection != 0)
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
                    sm.PrintSupplierList();
                    break;
                case 2:
                    sm.PromptUserForInsert();
                    break;
                case 3:
                    sm.PromptUserForUpdate();
                    break;
                case 4:
                    sm.PromptUserForDelete();
                    break;
                case -1:
                    terminate = true;
                    break;
            }
        }
        while (!terminate);
    }

}
