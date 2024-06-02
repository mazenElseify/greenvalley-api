using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;

public static class Util
{
    static readonly string[] DELIMITERS = { " " };

    private static string PadCell(string value, int noOfHeaders, int charsPerCell, char c = ' ')
    {
        string result = value ?? "";

        while (result.Length < charsPerCell)
            result += c;

        return result;
    }

    public static T[,] ToMatrix<T>(T[][] arr)
    {
        int width = 0;
        if (arr.Length > 0)
            width = arr[0].Length;

        var data = new T[arr.GetLength(0), width];
        for (int i = 0; i < arr.Length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                data[i, j] = arr[i][j];
            }
        }
        return data;

    }

    public static T[,] ToMatrix<T>(List<T[]> list)
    {
        var data = list.ToArray();
        return ToMatrix<T>(data);
    }

    public static void PrintTable(string[] headers, string[,] data, int wordsPerLine, int charsPerCell = 28, string[] wordDelimiters = null)
    {
        string[] delimiters = wordDelimiters;
        if (delimiters == null)
            delimiters = DELIMITERS;

        int headerLength = headers == null ? 0 : headers.Length;

        if (headers != null && headers.Length > 0)
        {
            LineSeparator(headers.Length, charsPerCell, '=');

            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i];
                string paddedHeader = PadCell(header, headers.Length, charsPerCell);
                Console.Write($"{paddedHeader}| ");
            }

            LineSeparator(headers.Length, charsPerCell, '=');
        }

        // Data Printer:
        for (int i = 0; i < data.GetLength(0); i++) // Rows
        {
            int w = data.GetLength(1);
            int h = 0;

            // perform initial loop on all cells to determine maximum number of lines
            // based on which the cell value will be split
            for (int j = 0; j < data.GetLength(1); j++)
            {
                string s = data[i, j];
                if (s == null || s == "")
                    continue;
                string[] words = s.Split(delimiters, StringSplitOptions.None);
                if (words.Length > h)
                    h = (int)Math.Ceiling((double)words.Length / wordsPerLine);
            }

            string[,] expandedRow = new string[h, w];

            for (int j = 0; j < data.GetLength(1); j++) // Columns/cells per row
            {
                string s = data[i, j];
                if (s == null || s == "")
                    continue;
                string[] words = s.Split(delimiters, StringSplitOptions.None);

                for (int k = 0; k < h; k++) // each expanded row
                {
                    string nextWord = "";
                    for (int l = 0; l < wordsPerLine; l++)
                    {
                        // formula to determine what words we will put
                        // in the current row based on wordsPerLine
                        //
                        // Example (wordsPerLine = 2)
                        // 0: w[0] + w[1]
                        // 1: w[2] + w[3]
                        // 2: w[4] + w[5]
                        // 3: w[6] + w[7]

                        int nextWordIdx = k * wordsPerLine + l;
                        if (nextWordIdx < words.Length)
                        {
                            if (nextWord != "")
                                nextWord += " ";

                            nextWord += words[nextWordIdx];

                            //nextWord = string.Join(" ", new string[] { nextWord, words[nextWordIdx] });
                        }
                        else
                            break;
                    }

                    expandedRow[k, j] = nextWord;
                }
            }

            // Print row after exapnding it based on wordsPerLine
            for (int x = 0; x < expandedRow.GetLength(0); x++)
            {
                for (int y = 0; y < expandedRow.GetLength(1); y++)
                {
                    string cell = expandedRow[x, y];
                    string paddedCell = PadCell(cell, w, charsPerCell);
                    Console.Write($"{paddedCell}| ");
                }

                // Do not print new line if not the last expanded row
                if (x != expandedRow.GetLength(0) - 1)
                    Console.WriteLine();
            } // expanded row

            LineSeparator(headerLength, charsPerCell);
        } // row

        Console.WriteLine("");
    }

    public static void LineSeparator(int headerLength, int charsPerCell, char c = '-')
    {
        Console.WriteLine();
        for (int i = 0; i < headerLength; i++)
        {
            Console.Write(PadCell("", headerLength, charsPerCell, c));
            Console.Write("|" + c);
        }

        Console.WriteLine();
    }

    internal static T[,] ToMatrix<T>(List<Customer> customers)
    {
        throw new NotImplementedException();
    }

    public static T? GetInput<T>(string message, string terminator, Func<T, bool> validate) where T: struct
    {
        string input = null;

        Type typeInfo = typeof(T);
        var converter = TypeDescriptor.GetConverter(typeInfo);

        int tryCount = 0;

        while(true)
        {
            if(tryCount > 0)
                Console.WriteLine("Invalid Input.\n");

            tryCount++;

            Console.Write(message);
            if(terminator != null)
                Console.Write($" (or enter {terminator} to cancel)\n\t");

            input = Console.ReadLine();

            if(input == terminator)
                break;

            if(converter.IsValid(input))
            {
                T val = (T)converter.ConvertFromString(input);
                if(validate(val))
                    return val;
            }
        }

        return null;
    }

    public static string GetInput(string message, string terminator, Func<string, bool> validate)
    {
        string input = null;
        int tryCount = 0;

        while(true)
        {
            if(tryCount > 0)
                Console.WriteLine("Invalid value.\n");

            tryCount++;

            Console.Write(message);
            if(terminator != null)
                Console.Write($" (or enter '{terminator}' to cancel)\n\t ");

            input = Console.ReadLine();

            if(input == terminator)
                break;

            if(validate(input))
                return input;
        }

        return null;
    }
}