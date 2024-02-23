using System.Globalization;
using System.Text.RegularExpressions;

namespace Application.Models.Extensions
{
	public static class StringExtension
	{
		#region Fields

		public const char DefaultWildcardCharacter = '*';

		#endregion

		#region Methods

		public static bool Like(this string value, string pattern)
		{
			return value.Like(pattern, DefaultWildcardCharacter);
		}

		public static bool Like(this string value, string pattern, char wildcardCharacter)
		{
			return value.Like(pattern, wildcardCharacter, true);
		}

		public static bool Like(this string value, string pattern, bool caseInsensitive)
		{
			return value.Like(pattern, DefaultWildcardCharacter, caseInsensitive);
		}

		public static bool Like(this string value, string pattern, char wildcardCharacter, bool caseInsensitive)
		{
			ArgumentNullException.ThrowIfNull(value);

			ArgumentNullException.ThrowIfNull(pattern);

			var regexOptions = RegexOptions.Compiled;

			if(caseInsensitive)
				regexOptions |= RegexOptions.IgnoreCase;

			var regexPattern = pattern.Replace(wildcardCharacter.ToString(CultureInfo.InvariantCulture), "*");
			regexPattern = "^" + Regex.Escape(regexPattern).Replace("\\*", ".*") + "$";

			return Regex.IsMatch(value, regexPattern, regexOptions);
		}

		#endregion
	}
}