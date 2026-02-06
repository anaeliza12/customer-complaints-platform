using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace ApiChamados.Domain.Extensions
{
    public static class StringExtensions
    {
        private static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            return new string(
                texto.Normalize(NormalizationForm.FormD)
                     .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                     .ToArray()
            ).Normalize(NormalizationForm.FormC);
        }

        public static string ToNormalizedSearchText(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var texto = RemoverAcentos(input).ToLowerInvariant();
            var semEspeciais = Regex.Replace(texto, @"[^a-z0-9\s]", "");

            return Regex.Replace(semEspeciais, @"\s+", " ").Trim();
        }

        public static string ToCorrelationId(this string input)
        {
            var texto = WebUtility.UrlDecode(input).Replace("\"", "");
            texto = RemoverAcentos(texto).ToLowerInvariant();

            var limpo = Regex.Replace(texto, @"[^a-z0-9]", "_");

            return Regex.Replace(limpo, @"_+", "_").Trim('_');
        }
    }
}