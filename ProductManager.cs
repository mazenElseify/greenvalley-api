
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml.XPath;
using Microsoft.VisualBasic;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

public class ProductManager
{
    const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511";


    public string GetProductName(string id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();

            // NpgsqlCommand cmd = new NpgsqlCommand();
            // cmd.Connection = conn;
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select name from Product where id=@pid";

            cmd.Parameters.Add(new NpgsqlParameter("pid", id));

            /*
            //cmd.CommandText = $"insert into product (id, name, ....) values (@id, @name, ";
                cmd.ExecuteNonQuery() // insert or update return int
                cmd.ExecuteScalar() // return single cell
                cmd.ExecuteReader() // return full row
            */

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

    public List<Product> GetAllProducts(int? typeId, string productId = null)
    {
        NpgsqlConnection connect = new NpgsqlConnection(CONN_STRING);
        List<Product> products = new List<Product>();

        try
        {
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connect;
            connect.Open();

            command.CommandText = @"select 
                p.*,
                t.name as type_name,
                st.name as sub_type_name
            from product p
                inner join product_type t on t.id = p.type_id
                inner join product_sub_type st on st.id = p.sub_type_id
            where (@type_id is null or @type_id=p.type_id)
                and (@id is null or upper(p.id) = upper(@id))";

            // object dbTypeId = typeId ?? (object)DBNull.Value;
            // NpgsqlParameter type_param = new NpgsqlParameter("type_id", NpgsqlTypes.NpgsqlDbType.Integer);
            // type_param.Value = dbTypeId;
            // command.Parameters.Add(type_param);

            command.Parameters.Add(new NpgsqlParameter("type_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = typeId ?? (object)DBNull.Value
            });

            // var p2 = new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar);
            // p2.Value = productId ?? (object)DBNull.Value;
            // command.Parameters.Add(p2);
            command.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = productId ?? (object)DBNull.Value
            });

            // coalesce operator
            //string x = null ?? null ?? "22";

            NpgsqlDataReader r = command.ExecuteReader();

            while (r.HasRows && r.Read())
            {
                Product p = new Product();
                p.Id = r["id"] as string;
                p.Name = r["name"] as string;
                p.TypeId = r["type_id"] as int?;
                p.SubTypeId = r["sub_type_id"] as int?;
                p.TypeName = r["type_name"] as string;
                p.SubTypeName = r["sub_type_name"] as string;
                p.BuyPrice = r["buy_price"] as decimal?;
                p.SellPrice = r["sell_price"] as decimal?;
                p.Active = r["active"] as bool?;
                p.Description = r["description"] as string;
                p.Condition = r["condition"] as int?;

                // other methods:
                //p.Id = r.GetString("id");
                //p.Active = r["active"] == DBNull.Value ? null : (bool)r["active"]
                //p.Description = r["description"] == DBNull.Value ? null : r.GetString("description");
                //p.Description = r["description"] == DBNull.Value ? null : (string)r["description"];

                products.Add(p);
            }
        }
        finally
        {
            if (connect.State == ConnectionState.Open)
                connect.Close();
        }

        return products;
    }

    //--------------------------------
    /// Assignment A
    //--------------------------------
    // Get type of database, let it be T
    // Get subtype of database, let it be ST
    // Get count of products where Type = T and Subtype = ST, let it be CNT
    // generate code with the following criterion:
    //  First character of each word of type + first character of each word of subtype + (CNT + 1)
    //
    // Example:
    // T = Cold Line
    // ST = Refrigerator
    // CNT = 5
    // Generated Code = CLR-6


    // HINT: select count(*) from product where type=T and subtype=ST

    // ==========================================================================================
    //functions declation:
    public void PrintTypeList()
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from product_type";

            NpgsqlDataReader r = cmd.ExecuteReader();
            string[] headers = { "ID", "Name" };
            List<string[]> data = new List<string[]>();
            while (r.HasRows && r.Read())
            {
                int? typeId = r["id"] as int?;
                string typeName = r["name"] as string;

                data.Add(new string[]
                {
                        typeId.ToString(),
                        typeName
                });
            }

            string[,] d2 = Util.ToMatrix<string>(data);
            Util.PrintTable(headers, d2, 3);
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

    }

    public bool CheckIfIdExists(string productId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from product where id=@id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = productId ?? (object)DBNull.Value
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
    public bool CheckIfTypeExists(int? pTypeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from product_type where id=@Tid";
            cmd.Parameters.Add(new NpgsqlParameter("Tid", pTypeId));

            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public bool CheckIfSubTypeExists(int? subtypeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = $"select * from product_sub_type where id=@Tid";
            cmd.Parameters.Add(new NpgsqlParameter("Tid", subtypeId));

            NpgsqlDataReader r = cmd.ExecuteReader();

            return r.HasRows;

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
    public void PrintSubTypeList(int? TypeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @$"select * from product_sub_type where type_id=@Tid";
            cmd.Parameters.Add(new NpgsqlParameter("Tid", TypeId));


            string[] headers = { "ID", "Name" };
            NpgsqlDataReader r = cmd.ExecuteReader();
            List<string[]> data = new List<string[]>();
            while (r.HasRows && r.Read())
            {
                int? subTypeId = r["id"] as int?;
                string subTypeName = r["name"] as string;

                data.Add(new string[]
                {
                    subTypeId.ToString(),
                    subTypeName
                });

                string[,] d2 = Util.ToMatrix<string>(data);
                Util.PrintTable(headers, d2, 3);
            }
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
            cmd.CommandText = "select name from product_type where id =@Tid";

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
    public string GetSubTypeName(int? subTypeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);

        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "select name from product_sub_type where id=@StId";

            cmd.Parameters.Add(new NpgsqlParameter("StId", subTypeId));

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
    public int GetProductCount(int? typeId, int? subTypeId)
    {
        int count = 0;
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = "select count(*) from product where type_id=@T and sub_type_id=@ST";

            cmd.Parameters.Add(new NpgsqlParameter("T", typeId));
            cmd.Parameters.Add(new NpgsqlParameter("ST", subTypeId));

            NpgsqlDataReader r = cmd.ExecuteReader();
            while (r.HasRows && r.Read())
            {
                count++;
            }
            return count;
        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }


    }


    public string CreateCode(string typeName) // something wronge in this function
    {
        if (typeName == null || typeName == "")
            return null;

        string[] result = typeName.Split(' ');
        string code = "";
        foreach (string s in result)
        {
            code += s.Substring(0, 1);

        }
        return code;


    }

    public void InsertProduct(Product product)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into product (id,name,type_id,sub_type_id,buy_price,sell_price,active,condition,description) 
                values (@id, @name, @typeId, @subTypeId, @buyPrice, @sellPrice,@productState, @condition, @description)";

            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Name ?? (object)DBNull.Value
            });

            cmd.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.TypeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("subTypeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.SubTypeId ?? (object)DBNull.Value
            });

            cmd.Parameters.Add(new NpgsqlParameter("buyPrice", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = product.BuyPrice ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("sellPrice", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = product.SellPrice ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("productState", NpgsqlTypes.NpgsqlDbType.Boolean)
            {
                Value = product.Active ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("condition", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.Condition ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Description ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Id ?? (object)DBNull.Value
            });

            // Console.WriteLine(product.TypeId + "\t\t" + product.SubTypeId);
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
        Product p = new Product();

        Console.WriteLine(@"Enter product name:
            Press -2 to cancel the process.");

        p.Name = Console.ReadLine();
        if (p.Name == "-2")
            ShowMenu();

        PrintTypeList();
        Console.Write("Select Product Type by ID or enter -2 to return to Menu: ");
        p.TypeId = int.Parse(Console.ReadLine());
        Console.WriteLine();

        if (p.TypeId == -2)
            ShowMenu();

        else if (!CheckIfTypeExists((int)p.TypeId))
        {
            Console.WriteLine("Invalid type.");
            return;
        }
        else
            PrintSubTypeList(p.TypeId);


        Console.Write("Select Product Sub type by ID or enter -2 to return to Menu: ");
        p.SubTypeId = int.Parse(Console.ReadLine());
        Console.WriteLine();

        if (p.SubTypeId == -2)
            ShowMenu();

        else if (!CheckIfSubTypeExists((int)p.SubTypeId))
        {
            Console.WriteLine("Invalid sub type.");
            return;
        }
        else
            Console.Write(@"Set Buy Price or enter -2 to return to Menu: ");
        p.BuyPrice = decimal.Parse(Console.ReadLine());
        Console.WriteLine();
        if (p.BuyPrice == -2)
            ShowMenu();

        Console.Write("Set Sell Price or enter -2 to return to Menu: ");
        p.SellPrice = decimal.Parse(Console.ReadLine());
        Console.WriteLine();
        if (p.SellPrice == -2)
            ShowMenu();


        Console.Write("0. New\n1. Used\n2. Like New\n3.Need repair\nor enter -2 to return to Menu:");
        p.Condition = int.Parse(Console.ReadLine());
        if (p.Condition == -2)
            ShowMenu();

        Console.Write("Describe product condition or enter -2 to return to Menu: ");
        p.Description = Console.ReadLine();
        if (p.Description == "-2")
            ShowMenu();

        p.TypeName = GetTypeName(p.TypeId);
        p.SubTypeName = GetSubTypeName(p.SubTypeId);


        int cnt = GetProductCount(p.TypeId, p.SubTypeId);

        p.Id = CreateCode(p.TypeName) + "-" + CreateCode(p.SubTypeName) + "-" + (cnt + 1);

        p.Active = true;

        InsertProduct(p);

        Console.WriteLine("Product has been inserted successfully");
        return;
    }

    public void UpdateProduct(Product product)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update product
                set name = @name, 
                buy_price= @buyPrice, 
                sell_price= @sellPrice, 
                active= @productState, 
                type_id= @typeId, 
                sub_type_id= @subTypeId, 
                condition= @condition, 
                description= @description 
                where id = @id;";
            // We need change here get the npgsqlTypes
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Name ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("buyPrice", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = product.BuyPrice ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("sellPrice", NpgsqlTypes.NpgsqlDbType.Money)
            {
                Value = product.SellPrice ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("productState", NpgsqlTypes.NpgsqlDbType.Boolean)
            {
                Value = product.Active ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("typeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.TypeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("subTypeId", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.SubTypeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("condition", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = product.Condition ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("description", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Description ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = product.Id ?? (object)DBNull.Value
            });

            cmd.ExecuteNonQuery();
            Console.WriteLine("Update Done Successfully");
        }

        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }

    public void PromptUserForUpdate()
    {

        Console.WriteLine("Update Product: ");

        PrintTypeList();
        Console.Write("Select product type ID: ");
        int typeId = int.Parse(Console.ReadLine());



        List<Product> products = GetAllProducts(typeId);

        string[] headers = { "ID", "Name" };
        List<string[]> data = new List<string[]>();
        for (int i = 0; i < products.Count; i++)
        {
            Product p2 = products[i];
            data.Add(new string[]
            {
                p2.Id,
                p2.Name
            });
            string[,] d2 = Util.ToMatrix<string>(data);
            Util.PrintTable(headers, d2, 3);

        }

        Console.Write("Enter ID of the product you wish to update: ");
        string productId = Console.ReadLine().ToUpper();

        if (!PrintProductById(productId))
        {
            Console.WriteLine($"Product with the specified ID '{productId}' cannot be found.");
            return;
        }

        Product p = null;
        foreach (Product p2 in products)
        {
            if (p2.Id.ToUpper() == productId.ToUpper())
            {
                p = p2;
                break;
            }
        }

        if (p == null)
        {
            Console.WriteLine("Invalid product id.");
            return;
        }
        string input = "";
        Console.Write("Please Enter Product Name or enter NA to keep the original: ");
        input = Console.ReadLine();
        if (input.ToUpper() != "NA")
            p.Name = input;
        // if ((input) == NA)
        // (var) = original

        Console.Write("Enter buy Price or enter NA to keep original: ");
        input = Console.ReadLine();
        if (input.ToUpper() != "NA")
            p.BuyPrice = int.Parse(input);

        Console.Write("Enter sell Price or enter NA to keep original: ");
        input = Console.ReadLine();
        if (input.ToUpper() != "NA")
            p.SellPrice = int.Parse(input);

        Console.Write("0. New\n1. Used\n2. Like New\n3.Need repair\n");
        input = Console.ReadLine();
        if (input.ToUpper() != "NA")
            p.Condition = int.Parse(input);

        Console.Write("Describe product condition or enter NA to keep original: ");
        input = Console.ReadLine();
        if (input != "NA")
            p.Description = input;
        // if ((input) == NA)
        // (var) = original
        // 
        UpdateProduct(p);

    }
    public bool PrintProductById(string productId)
    {
        var result = GetAllProducts(null, productId);

        if (result.Count != 1)
            return false;

        // print p
        Product p = result[0];
        string[] header = { "Product ID:", p.Id.ToString() };
        string[,] data = new string[,] {
            // {"ID:", p.Id.ToString() },
            {"Name:", p.Name.ToString() },
            {"Type:", p.TypeName.ToString() },
            {"Sub Type:", p.SubTypeName.ToString() },
            {"Buy Price:", p.BuyPrice.ToString() },
            {"Sell Price:", p.SellPrice.ToString() },
            {"Condition:", p.ConditionStr.ToString() },
            {"Description:", p.Description.ToString() }
        };

        Util.PrintTable(header, data, 3);
        return true;
    }
    public void DeleteProduct(string id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = @"select count(*) from transaction where product_id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = id ?? (object)DBNull.Value
            });
            if (cmd.ExecuteNonQuery() > 0)
                throw new ApplicationException("Cannot delete product because there is transactions associated with it.");

            cmd.Parameters.Clear();

            cmd.CommandText = @"delete from product
                where id = @PID";

            cmd.Parameters.Add(new NpgsqlParameter("PID", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = id ?? (object)DBNull.Value
            });

            if (cmd.ExecuteNonQuery() == 0)
                throw new ApplicationException("Product was not deleted, perhaps the ID you specified is incorrect.");

            Console.WriteLine($"Product with ID: {id} has been deleted Successfully.");


        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();

        }
    }
    public void PromptUserForDelete()
    {
        PrintProductsInList();
        string id;
        Console.Write("Enter the ID of the product you want to delete: ");
        id = Console.ReadLine().ToUpper();
        DeleteProduct(id);
        Console.WriteLine("Product Deleted Successfully.");



    }
    public void PrintProductsInList()
    {
        int? input;
        List<Product> pList = new List<Product>();
        PrintTypeList();
        Console.Write("Please select the Product Type : ");
        input = int.Parse(Console.ReadLine());
        pList = GetAllProducts(input, null);

        string[,] data = new string[pList.Count, 4];
        for (int i = 0; i < pList.Count; i++)
        {
            data[i, 0] = pList[i].Id;
            data[i, 1] = pList[i].Name;
            data[i, 2] = pList[i].TypeName;
            data[i, 3] = pList[i].SellPrice.ToString();
        }

        // Console.WriteLine("_________________________________________________________________________________________________");
        string[] headers = { "ID", "Name", "Type", "Price" };
        Util.PrintTable(headers, data, 3);

    }
    public void ListProducts()
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        // List<Product> products = new List<Product>();
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"select 
            
                    p.*,
                    t.name as type_name,
                    st.name as sub_type_name
                from product p
                    inner join product_type t on t.id =p.type_id
                    inner join product_sub_type st on st.id =p.sub_type_id
                    ";


            NpgsqlDataReader reader = cmd.ExecuteReader();
            int count = 0;
            // Console.WriteLine("ID\t\tName\t\tType\t\tSubType\t\tPrice\t\tCondition\t\tDescription");
            Console.WriteLine();
            while (reader.HasRows && reader.Read())
            {
                Product p = new Product();
                count++;
                Console.WriteLine($"Product {count}:");
                Console.WriteLine("__________________________");
                p.Id = reader["id"] as string;
                p.Name = reader["name"] as string;
                p.TypeId = reader["type_id"] as int?;
                p.SubTypeId = reader["sub_type_id"] as int?;
                p.TypeName = reader["type_name"] as string;
                p.SubTypeName = reader["sub_type_name"] as string;
                p.BuyPrice = reader["buy_price"] as decimal?;
                p.SellPrice = reader["sell_price"] as decimal?;
                p.Active = reader["active"] as bool?;
                p.Description = reader["description"] as string;
                p.Condition = reader["condition"] as int?;

                string[] header = { "ID:", p.Id };
                string[,] data = new string[,] {

                    {"NAME:",       p.Name},
                    {"TYPE:",       p.TypeName },
                    {"SUB TYPE:",   p.SubTypeName},
                    {"PRICE:", p.SubTypeName},
                    {"CONDITION:",  p.ConditionStr},
                    {"DESCRIPTION:", p.Description}
                };
                Util.PrintTable(header, data, 3);
            }

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }
    }
    public static void ShowMenu()
    {

        string readInput;
        bool terminate = false;

        ProductManager manager = new ProductManager();
        do
        {
            Console.WriteLine("Product Menu:");
            Console.WriteLine("1- List Products.");
            Console.WriteLine("2- Insert New Product.");
            Console.WriteLine("3- Update Product.");
            Console.WriteLine("4- Delete Product.");
            Console.WriteLine("5- Show Type List.");
            Console.WriteLine("6- Show SubType List.");
            Console.WriteLine("7- Insert New Type.");
            Console.WriteLine("8- Update Type.");
            Console.WriteLine("9- Delete Type.");
            Console.WriteLine("10- Insert New SubType.");
            Console.WriteLine("11- Update SubType.");
            Console.WriteLine("12- Delete SubType.");

            Console.Write("Select option or enter -1 to exit: ");
            readInput = Console.ReadLine();
            bool validInt = int.TryParse(readInput, out int selection);
            if (!validInt || selection > 12)
            {
                Console.WriteLine("Invalid selection.");
                ShowMenu();
                return;
            }


            switch (selection)
            {
                case 1:
                    Console.WriteLine("Select option:");
                    Console.WriteLine("________________:");
                    Console.WriteLine("1.Show All products.");
                    Console.WriteLine("2.Select product.");
                    Console.WriteLine("-1. Back to product menu.");
                    selection = int.Parse(Console.ReadLine());
                    switch (selection)
                    {
                        case 1:
                            manager.ListProducts();
                            Console.Write("Press enter to return to menu: ");
                            readInput = Console.ReadLine();
                            if (readInput == "")
                                return;

                            break;
                        case 2:
                            manager.PrintProductsInList();

                            Console.Write("Enter the ID of the product you want to show: ");
                            readInput = Console.ReadLine().ToUpper();
                            // check if id found create function.
                            manager.PrintProductById(readInput);
                            break;

                    }

                    break;
                case 2:
                    manager.PromptUserForInsert();
                    break;
                case 3:
                    manager.PromptUserForUpdate();
                    break;
                case 4:
                    manager.PromptUserForDelete();
                    break;
                case 5:
                    manager.PrintTypeList();
                    break;
                case 6:
                    int input;
                    manager.PrintTypeList();
                    Console.WriteLine("Please specify the type: ");
                    input = int.Parse(Console.ReadLine());
                    manager.PrintSubTypeList(input);
                    break;
                case 7:
                    manager.PromptUserForInsertType();
                    break;
                case 8:
                    manager.PropmptUserForUodateType();
                    break;
                case 9:
                    manager.PromptUserForDeleteType();
                    break;
                case 10:
                    manager.PromptUserForInsertSubType();
                    break;
                case 11:
                    manager.PropmtUserForUpdateSubType();
                    break;
                case 12:
                    manager.PromptUserForDeleteSubType();
                    break;
                case -1:
                    terminate = true;
                    break;

                default:
                    ShowMenu();
                    break;
            }
        }
        while (!terminate);

    }
    public void InsertNewType(string typeName)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into product_type 
                set name = @name";
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = typeName ?? (object)DBNull.Value
            });
            cmd.ExecuteReader();

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

    }
    public void PromptUserForInsertType()
    {
        Console.WriteLine("Insert New Product Type:");
        Console.WriteLine("------------------------");

        Console.WriteLine("Please Enter Type name you want to insert:\n'-1' To cancel.");
        string input = Console.ReadLine();
        if (input == "-1")
        {
            Console.WriteLine("Proccess has been canceled.");

            return;
        }
        else
        {
            InsertNewType(input);
            Console.WriteLine("Product type has been inserted successfully.");
        }

    }

    public void UpdateType(int? typeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update product_type 
                set name= @name
            where id =@id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = typeId ?? (object)DBNull.Value
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
    public void PropmptUserForUodateType()
    {
        Console.WriteLine("Update Product Type:");
        Console.WriteLine("--------------------");
        PrintTypeList();
        string input;
        Console.WriteLine("Enter Type ID you want to update:\n'-1' To cancel.");
        input = Console.ReadLine();
        bool validInt = int.TryParse(input, out int integer);
        if (!validInt)
        {
            Console.WriteLine("Invalid Input, please try again.");
            PropmptUserForUodateType();
            return;
        }
        else if (integer == -1)
        {
            Console.WriteLine("Proccess has been cancel.");
            return;
        }
        else
        {
            UpdateType(integer);
        }

    }
    public void DeleteType(int? typeId)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"delete from product_type where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            { 
                Value = typeId ?? (object)DBNull.Value
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
    public void PromptUserForDeleteType()
    {
        string input;
        Console.WriteLine();
        Console.WriteLine("Delete Product Type:");
        Console.WriteLine("------------------------");

        PrintTypeList();
        Console.WriteLine("Select Product Type ID:\n'-1' To cancel. ");
        input = Console.ReadLine();
        bool validInt = int.TryParse(input, out int typeId);
        if (input == "-1")
        {
            Console.WriteLine("Proccess has been canceled.");
            return;

        }
        else if (!validInt)
        {
            Console.WriteLine("Invalid Type ID, Please try again.");
            PromptUserForDeleteType();
            return;
        }
        else
            DeleteType(typeId);
        Console.WriteLine("Product Type has been deleted successfully.");

    }
    public void InsertNewSubType(int? typeId, string subTypeName)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        

        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into product_sub_type (name) values (@name) where type_id =@tid ";
            cmd.Parameters.Add(new NpgsqlParameter("tid", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = typeId ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name",NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = subTypeName ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Insertion has been done successfully.");



        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
    public void PromptUserForInsertSubType()
    {
        int typeId;
        string subTypeName;
        Console.WriteLine("---------------------------");
        Console.WriteLine("Insert new Product SubType.");
        Console.WriteLine("---------------------------");

        PrintTypeList();
        Console.Write("Please select SubType:\n'-1' to cancel: ");
        typeId = int.Parse(Console.ReadLine());
        if(typeId == -1)
        {
            Console.WriteLine("Proccess has been canceled.");
            return;
        }

        else if (!CheckIfTypeExists(typeId))
        {
            Console.WriteLine("Invalid Input, Subtype id cannot be found.");
            return;
        }
        else
            Console.Write("Please enter the name of Product SubType: ");
        subTypeName = Console.ReadLine();

        InsertNewSubType(typeId, subTypeName);


    }
    public void UpdateSubType(string subTypeName, int? id)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"update product_sub_type set name = @name where id = @id";
            cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = id ?? (object)DBNull.Value
            });
            cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = subTypeName ?? (object)DBNull.Value
            });
            cmd.ExecuteNonQuery();
            Console.WriteLine("Update has been done successfuly.");
            

        }
        finally
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
    }
    public void PropmtUserForUpdateSubType()
    {
        int? input;
        string subTypeName;
        Console.WriteLine("--------------------");
        Console.WriteLine("Update SubType Name:");
        Console.WriteLine("--------------------");
        
        PrintTypeList();
        Console.WriteLine("Please select type ID:\n'-1' to cancel the proccess:");
        input = int.Parse(Console.ReadLine());
        if (input == -1)
        {
            Console.WriteLine("Proccess has been canceled.");
            return;
        }
        else if (!CheckIfTypeExists(input))
        {
            Console.WriteLine("Invalid input, ID cannot be found.");
            return;
        }
        else
            PrintSubTypeList(input);
            Console.Write("Select product SubType you wish to Update:\'-1' to cancel: ");
            input = int.Parse(Console.ReadLine());
            if (input == -1)
            {
                Console.WriteLine("Proccess has been canceled.");
                return;
            }
            else if (!CheckIfSubTypeExists(input))
            {
                Console.WriteLine("Invalid input, SubType ID not found.");
                return;
            }
            else
                Console.WriteLine($"Please enter the name of Subtype {input}");
            subTypeName = Console.ReadLine();

            UpdateSubType(subTypeName,input);    
        
    }

    public void DeleteSubType(int subTypeId)
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
    public void PromptUserForDeleteSubType()
    {

    }
}