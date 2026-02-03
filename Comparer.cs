using System;

namespace MyLetterComparer
{
    public class Comparer
{
    /// <summary>
    /// Compares two items (letters or numbers).
    /// </summary>
    /// <param name="item1">First item (letter or number)</param>
    /// <param name="item2">Second item (letter or number)</param>
    /// <returns>
    /// 0 if the items are the same
    /// -1 if the first item is less than the second
    /// 1 if the first item is greater than the second
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when one argument is a letter and the other is a number</exception>
    public int Compare(string item1, string item2)
    {
        bool isLetter1 = char.IsLetter(item1[0]);
        bool isLetter2 = char.IsLetter(item2[0]);
        bool isDigit1 = char.IsDigit(item1[0]);
        bool isDigit2 = char.IsDigit(item2[0]);

        // Check if one is a letter and the other is a number
        if ((isLetter1 && isDigit2) || (isDigit1 && isLetter2))
        {
            throw new ArgumentException("Cannot compare a letter with a number. Both arguments must be either letters or numbers.");
        }

        // Compare letters (case-insensitive)
        if (isLetter1 && isLetter2)
        {
            char char1 = char.ToLower(item1[0]);
            char char2 = char.ToLower(item2[0]);

            if (char1 == char2)
                return 0;
            else if (char1 < char2)
                return -1;
            else
                return 1;
        }

        // Compare numbers
        if (isDigit1 && isDigit2)
        {
            int num1 = int.Parse(item1);
            int num2 = int.Parse(item2);

            if (num1 == num2)
                return 0;
            else if (num1 < num2)
                return -1;
            else
                return 1;
        }

        // If not letter or digit, raise an error
        throw new ArgumentException("Arguments must be letters or numbers.");
    }
    }
}
