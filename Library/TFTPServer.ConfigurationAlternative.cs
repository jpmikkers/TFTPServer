using System.Text.RegularExpressions;

namespace CodePlex.JPMikkers.TFTP;

public partial class TFTPServer
{
    public class ConfigurationAlternative
    {
        private readonly Regex _regex;
        public ushort WindowSize { get; set; }

        public bool Match(string s)
        {
            return _regex.IsMatch(s);
        }

        private ConfigurationAlternative(Regex regex)
        {
            _regex = regex;
            WindowSize = 1;
        }

        public static ConfigurationAlternative CreateRegex(string regex)
        {
            return new ConfigurationAlternative(new Regex(regex, RegexOptions.IgnoreCase));
        }

        public static ConfigurationAlternative CreateWildcard(string wildcard)
        {
            return CreateRegex(WildcardToRegex.Convert(wildcard));
        }
    }
}
