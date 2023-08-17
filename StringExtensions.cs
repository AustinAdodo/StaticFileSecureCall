using System.Runtime.CompilerServices;

namespace StaticFileSecureCall
{
    /// <summary>
    /// Extension method to ensure connection string is read correctly everytime.
    /// </summary>
    public static class StringExtensions
    {
        public const string store = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static string EscapeSpecialCharacters(this string unchecked_string)
        {
            var escaped_string = "";
            for (int i = 0; i < unchecked_string.Length; i++)
            {
                char character = unchecked_string[i];
                if (!store.Contains(character))
                {
                    escaped_string += "\\" + character;
                }
                else
                {
                    escaped_string += character;
                }
            }
            return escaped_string;
        }
    }
}
