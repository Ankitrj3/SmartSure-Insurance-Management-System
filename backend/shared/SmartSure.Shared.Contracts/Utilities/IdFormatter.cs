using System;
using System.Text.RegularExpressions;

namespace SmartSure.Shared.Contracts.Utilities
{
    public static class IdFormatter
    {
        public static string FormatApiId(this Guid id, string prefix)
        {
            var cleanStr = Regex.Replace(id.ToString(), "[^a-zA-Z0-9]", "").ToUpper();
            return $"{prefix}{cleanStr.Substring(0, 8)}";
        }
    }
}
