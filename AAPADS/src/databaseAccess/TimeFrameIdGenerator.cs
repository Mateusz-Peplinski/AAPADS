using System.Linq;

public class TimeFrameIdGenerator
{
    private int number_ID;
    private string letter_ID;

    public TimeFrameIdGenerator(string initialValue = "A0")
    {
        letter_ID = new string(initialValue.Where(char.IsLetter).ToArray());
        number_ID = int.Parse(new string(initialValue.Where(char.IsDigit).ToArray()));
    }

    public string GenerateNextId()
    {
        number_ID++;

        if (number_ID > 999)
        {
            number_ID = 1;
            IncrementLetterPart();
        }

        return $"#{letter_ID}{number_ID:D3}"; // :D3 ensures the number is always 3 digits.
    }

    private void IncrementLetterPart()
    {
        var lastCharIndex = letter_ID.Length - 1;
        var lastChar = letter_ID[lastCharIndex];

        // If it's not 'Z', just increment the letter.
        if (lastChar != 'Z')
        {
            char nextChar = (char)(lastChar + 1);
            letter_ID = letter_ID.Substring(0, lastCharIndex) + nextChar;
        }
        else
        {
            // Find the position of the first non-'Z' from the end.
            int nonZPosition = letter_ID.TakeWhile(ch => ch == 'Z').Count();
            if (nonZPosition == letter_ID.Length) // all characters are 'Z'.
            {
                letter_ID = new string('A', letter_ID.Length + 1); // e.g., ZZ -> AAA
            }
            else
            {
                char nextChar = (char)(letter_ID[letter_ID.Length - nonZPosition - 1] + 1);
                letter_ID = letter_ID.Substring(0, letter_ID.Length - nonZPosition - 1) + nextChar + new string('A', nonZPosition);
            }
        }
    }
}
