using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NFRInvoke
{
    static class StringExtensions
    {
        public static bool ContainsOneOf(this string @this, string[] tokens) { return tokens.Any(@this.Contains); }

        public static string SubstringUpTo(this string @this, int maxLength)
        {
            return @this == null
                        ? null
                        : @this.Length > maxLength
                                    ? @this.Substring(0, maxLength)
                                    : @this;
        }

        public static string ToFirstLineHtml(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this)) { return @this; }
            //
            var iFirstClosingTag = @this.IndexOf("</", StringComparison.Ordinal);
            if (iFirstClosingTag < 5) { return @this; }
            return @this.Substring(0, iFirstClosingTag).WithoutMarkup();
        }

        public static string ToTextAfterHeading(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this)) { return @this; }
            //
            var iFirstClosingH1Tag = @this.IndexOf("</h1", StringComparison.InvariantCultureIgnoreCase);
            var iFirstClosingH2Tag = @this.IndexOf("</h2", StringComparison.InvariantCultureIgnoreCase);
            var iFirstClosingH3Tag = @this.IndexOf("</h3", StringComparison.InvariantCultureIgnoreCase);
            var iFirstClosingH4Tag = @this.IndexOf("</h4", StringComparison.InvariantCultureIgnoreCase);
            var iFirstClosingHeaderTag =
                        new[] { int.MaxValue, iFirstClosingH1Tag, iFirstClosingH2Tag, iFirstClosingH3Tag, iFirstClosingH4Tag }
                                    .Where(i => i > 1).Min();
            if (iFirstClosingHeaderTag == int.MaxValue)
            {
                return @this.WithoutMarkup();

            }
            else
            {
                var afterHeading = @this.Substring(iFirstClosingHeaderTag);
                return afterHeading.WithoutMarkup();
            }
        }

        static readonly Regex brokenEntityStartsAtEnd = new Regex(@"&\w*$");
        static readonly Regex brokenEntityEndsAtStart = new Regex(@"^\w*;");

        public static string ToStringTruncated(this string @this, int maxLength = 200)
        {
            if (@this == null) { return null; }
            if (@this.Length <= maxLength) { return @this; }
            //
            var trunc = SubstringUpTo(@this, maxLength - 1);
            var remainder = @this.Substring(trunc.Length);
            var brokenHalf = brokenEntityEndsAtStart.Match(remainder);
            if (brokenHalf.Success && brokenEntityStartsAtEnd.IsMatch(trunc))
            {
                trunc += brokenHalf.Value;
            }

            return trunc.Length < @this.Length
                        ? trunc.Substring(0, trunc.Length) + "…"
                        : trunc;
        }

        public static string Replace(this string @this, Regex regex, string replacement) { return regex.Replace(@this, replacement); }

        static Regex withoutMarkup1 = new Regex(@"\b(\<[^>]+\>)+\b");
        static Regex withoutMarkup2 = new Regex(@"\<[^>]+\>");

        public static string WithoutMarkup(this string @this)
        {
            return @this == null ? null : @this.Replace(withoutMarkup1, " ").Replace(withoutMarkup2, "");
        }
        public static string WithoutEntities(this string @this) { return @this == null ? null : Regex.Replace(@this, @"&\w+;", " ").Replace("  ", " "); }

        public static string WithoutMarkupOrSqlTerminators(this string @this)
        {
            return @this.WithoutMarkup().Replace("--", "––").Replace(";", ":");
        }

        public static string WithoutHyphens(this string @this) { return Regex.Replace(@this, @"[\-_]", " "); }

        public static string ToTitleCase(this string @this, CultureInfo culture = null)
        {
            var withouthyphens = @this.WithoutHyphens();
            var ti = (culture ?? CultureInfoEnGb).TextInfo;
            return ti.ToTitleCase(withouthyphens);
        }

        static readonly CultureInfo CultureInfoEnGb = new CultureInfo("en-GB");
    }
}