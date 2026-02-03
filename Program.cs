using System;
using MyLetterComparer;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: MyLetterComparer <item1> <item2>");
            Console.WriteLine("Examples: dotnet run -- a b");
            Console.WriteLine("          dotnet run -- 5 10");
            return;
        }

        try
        {
            Comparer comparer = new Comparer();
            int result = comparer.Compare(args[0], args[1]);
            
            Console.WriteLine($"Comparing '{args[0]}' and '{args[1]}': {result}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
