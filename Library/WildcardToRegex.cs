/*

Copyright (c) 2009 Jean-Paul Mikkers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.

*/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace CodePlex.JPMikkers.TFTP
{
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
         
        Characters not in set
         [! ]
         Matches any one character that is not specified in the set.
         
        Escape
         \
         Matches the character that follows the backslash (\) as a literal. This allows you to find the characters used in wildcard notation, such as * and #. 
         
        One or more characters
         *
         Matches zero or more characters. For example, new* matches any text that includes "new", such as newfile.txt. 
         
        Set of characters
         [ ]
         Matches any one of the characters specified in the set.
        */

        private static void FlushEscapeStringToRegex(StringBuilder escapeString, StringBuilder regexString)
        {
            if (escapeString.Length > 0)
            {
                regexString.Append(Regex.Escape(escapeString.ToString()));
                escapeString.Length = 0;
            }
        }

        private static string GetGroupName(string wildcardPattern, ref int index)
        {
            StringBuilder groupName = new StringBuilder();
            if (wildcardPattern[index] == '{')
            {
                index++;
                while (index < wildcardPattern.Length && wildcardPattern[index] != '}')
                {
                    if (Char.IsLetterOrDigit(wildcardPattern[index]))
                    {
                        groupName.Append(wildcardPattern[index]);
                    }
                    index++;
                }
                index++;
            }
            return groupName.ToString();
        }

        public static string Convert(string wildcardPattern)
        {
            StringBuilder sb1 = new StringBuilder();
            StringBuilder regexPattern = new StringBuilder();

            for (int t = 0; t < wildcardPattern.Length; t++)
            {
                char c = wildcardPattern[t];

                switch (c)
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
/*
                    case '\\':
                        t++;
                        if (t < wildcardPattern.Length)
                        {
                            switch (wildcardPattern[t])
                            {
                                case 't':
                                    sb1.Append('\t');
                                    break;

                                case 'n':
                                    sb1.Append('\n');
                                    break;

                                case 'r':
                                    sb1.Append('\r');
                                    break;

                                default:
                                    sb1.Append(wildcardPattern[t]);
                                    break;
                            }
                        }
                        break;

                    case '%':
                        t++;
                        if (t < wildcardPattern.Length)
                        {
                            string groupName = GetGroupName(wildcardPattern, ref t);

                            if (t < wildcardPattern.Length)
                            {
                                switch (wildcardPattern[t])
                                {
                                    case 'd':
                                        FlushEscapeStringToRegex(sb1, regexPattern);
                                        AppendGroupPattern(regexPattern, groupName, @"[0-9]+");
                                        break;

                                    case 'f':
                                        FlushEscapeStringToRegex(sb1, regexPattern);
                                        AppendGroupPattern(regexPattern, groupName, @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?");
                                        break;

                                    case 'w':
                                        FlushEscapeStringToRegex(sb1, regexPattern);
                                        AppendGroupPattern(regexPattern, groupName, @"[_0-9a-zA-Z]+");
                                        break;

                                    default:
                                        sb1.Append(wildcardPattern[t]);
                                        break;
                                }
                            }
                        }
                        break;
*/
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

        private static void AppendGroupPattern(StringBuilder regexPattern, string groupName, string regex)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                regexPattern.Append(regex);
            }
            else
            {
                regexPattern.Append(string.Format("(?<{0}>{1})", groupName, regex));
            }
        }
    }
}
