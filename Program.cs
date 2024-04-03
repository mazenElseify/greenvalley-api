
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.XPath;

MainMenu();


// CustomerManager.ShowMenu();
static void MainMenu()
{ 

    bool terminate = false;
    string readInput;
    do
    {
        Console.WriteLine();
        Console.WriteLine("Main Menu:");
        Console.WriteLine("----------");
        Console.WriteLine("1- Product Manager.");
        Console.WriteLine("2- Customer Manager.");
        Console.WriteLine("3- Supplier Manager.");
        Console.WriteLine("4- Transactions.");
        Console.WriteLine("5- Inventory.");
        Console.WriteLine("'-1'- Exit.");
        Console.WriteLine();
        Console.Write("Select Option: ");
        readInput = Console.ReadLine();
        bool validInt = int.TryParse(readInput, out int selection);

        if (!validInt || selection > 5)
        {
            Console.WriteLine("Invalid Input.");
            MainMenu();
            return;
        }

        switch (selection)
        { 
            case 1:
                ProductManager.ShowMenu();
                break;
            case 2:
                CustomerManager.ShowMenu();
                break;
            case 3:
                SupplierManager.ShowMenu();
                break;
            case 4:
                TransactionManager.ShowMenu();
                break;
            case 5:
                break;
            case -1:
                terminate = true;
                break;
        }
    }
    while (!terminate);
}
