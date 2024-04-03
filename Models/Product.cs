public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal? BuyPrice { get; set; }
    public decimal? SellPrice { get; set; }
    public bool? Active { get; set; }
    public int? TypeId { get; set; }
    public string TypeName { get; set; }
    public int? SubTypeId { get; set; }
    public string SubTypeName { get; set; }
    public int? Condition { get; set; }

    public string ConditionStr
    {
        get
        {
            switch(Condition)
            {
                case 0:
                    return "New";

                case 1:
                    return "Used";

                case 2:
                    return "Like New";

                case 3:
                    return "Needs a repair";

                default:
                    return null;
            }
        }
    }

    public string Description { get; set; }
}