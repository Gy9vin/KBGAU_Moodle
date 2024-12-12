using System.Text.RegularExpressions;

namespace KBGAU.Utils
{
    public static class TextUtils
    {
        public static string NormalizeText(string text)
        {
            // Удаление лишних пробелов и приведение текста к нижнему регистру
            return Regex.Replace(text.Trim().ToLower(), @"\s+", " ");
        }
    }
}