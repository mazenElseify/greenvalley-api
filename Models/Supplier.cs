using System.Reflection.Metadata.Ecma335;

public class Supplier
{
    public int? Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public bool? International { get; set; }
    public string InternationalStr
    {
        get
        {
            return (International ?? false) ? "Local" : "International";
        }
    }

    public string Email { get; set; }
}