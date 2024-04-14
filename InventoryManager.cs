using System.Data.SqlTypes;
using Npgsql;

public class InventoryManager
{
    public static int DEFAULT_INVENTORY_ID = 1;

    const string CONN_STRING = "Host=localhost:5432;Username=postgres;Password=2511"; 


    public long GetAvailableStock(string productId, int? inventoryId = null)
    {
        NpgsqlConnection conn = new NpgsqlConnection(CONN_STRING);
        try
        {
            conn.Open();
            NpgsqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"
                select 
                    coalesce((select sum(t.qty)
                        from public.transaction t
                        inner join invoice i on i.id = t.invoice_id
                        where product_id=@product_id and i.direction = 'IN' and inventory_id = @inventory_id), 0) 
                    
                    - 
                    
                    coalesce((select sum(t.qty)
                        from public.transaction t
                        inner join invoice i on i.id = t.invoice_id
                        where product_id=@product_id and i.direction = 'OUT' and inventory_id = @inventory_id), 0)";

            cmd.Parameters.Add(new NpgsqlParameter("product_id",NpgsqlTypes.NpgsqlDbType.Varchar)
            {
                Value = productId
            });

            cmd.Parameters.Add(new NpgsqlParameter("inventory_id", NpgsqlTypes.NpgsqlDbType.Integer)
            {
                Value = inventoryId ?? DEFAULT_INVENTORY_ID
            });

            return (long)cmd.ExecuteScalar();

        }
        finally
        {
        }
    }
}