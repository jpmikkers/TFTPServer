namespace Baksteen.Net.TFTP.Server;
using System.Text;
using System.Text.RegularExpressions;

public class WildcardToRegex
{
    /*
     * Expression
     Syntax
     Description
     
    Any single character
     ?
     Matches any single character.
     
    Any single digit
     #
     Matches any single digit. For example, 7# matches numbers that include 7 followed by another number, such as 71, but not 17. 
     
    One or more characters
     *
     Matches zero or more characters. For example, new* matches any text that includes "new", such as newfile.txt. 
    */

    private static void FlushEscapeStringToRegex(StringBuilder escapeString, StringBuilder regexString)
    {
        if(escapeString.Length > 0)
        {
            regexString.Append(Regex.Escape(escapeString.ToString()));
            escapeString.Length = 0;
        }
    }

    public static string Convert(string wildcardPattern)
    {
        StringBuilder sb1 = new StringBuilder();
        StringBuilder regexPattern = new StringBuilder();

        for(int t = 0; t < wildcardPattern.Length; t++)
        {
            char c = wildcardPattern[t];

            switch(c)
            {
                case '*':
                    FlushEscapeStringToRegex(sb1, regexPattern);
                    regexPattern.Append(".*");
                    break;

                case '?':
                    FlushEscapeStringToRegex(sb1, regexPattern);
                    regexPattern.Append(".");
                    break;

                case '#':
                    FlushEscapeStringToRegex(sb1, regexPattern);
                    regexPattern.Append("[0-9]");
                    break;

                default:
                    sb1.Append(c);
                    break;
            }
        }
        FlushEscapeStringToRegex(sb1, regexPattern);
        regexPattern.Insert(0, '^');
        regexPattern.Append('$');
        return regexPattern.ToString();
    }
}
