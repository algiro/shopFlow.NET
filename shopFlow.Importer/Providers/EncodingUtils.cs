using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shopFlow.Importer.Providers
{
    public static class EncodingUtils
    {
        public static string FixEncodingIssues(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Convert to bytes and back with UTF-8 to fix encoding issues
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            string fixedText = Encoding.UTF8.GetString(bytes);

            // Additional character fixes
            var characterMappings = new Dictionary<string, string>
            {
                { "Â°", "°" },    // Degree symbol
                { "â‚¬", "€" },    // Euro symbol
                { "Âº", "º" },    // Ordinal indicator
                { "Â®", "®" },    // Registered trademark
                { "Â©", "©" },    // Copyright symbol
                { "â„¢", "™" },    // Trademark symbol
            };

            foreach (var mapping in characterMappings)
            {
                fixedText = fixedText.Replace(mapping.Key, mapping.Value);
            }

            return fixedText.Normalize(NormalizationForm.FormC);
        }

    }
}
