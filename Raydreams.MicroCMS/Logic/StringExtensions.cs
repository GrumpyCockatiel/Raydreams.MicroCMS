using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Raydreams.MicroCMS
{
    /// <summary>Tons of string utility functions</summary>
    public static class StringExtensions
	{
        /// <summary>Gets JUST the filename part of a URL</summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFilenameFromUrl( this string url )
        {
            return ( !url.Contains( "." ) ) ? String.Empty : Path.GetFileName( new Uri( url ).AbsolutePath );
        }

        public static string TrimExtension(this string filename)
        {
            if ( String.IsNullOrEmpty(filename) )
                return String.Empty;

            filename = filename.Trim().Trim( new char[] { '.' } );

            if ( !filename.Contains(".") )
                return filename;

            return filename.Substring(0, filename.LastIndexOf(".") );
        }

        /// <summary>Truncates a string to the the specified length or less</summary>
        public static string Truncate(this string str, int length, bool trim = true)
        {
            // if greater than length
            if (str.Length > length)
                return (trim) ? str.Trim().Substring(0, length) : str.Substring(0, length);

            return (trim) ? str.Trim() : str;
        }

        /// <summary>If any string is null or white space, return true.</summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool IsAnyNullOrWhiteSpace( this string[] strs )
        {
            return strs.Count( s => String.IsNullOrWhiteSpace( s ) ) > 0;
        }

        /// <summary>Overlaps the specified end string with the string itself starting from the end. Not an append since the str length remains the same.</summary>
        /// <param name="str"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string ReplaceEndWith( this string str, string suffix )
		{
			if ( String.IsNullOrEmpty( suffix ) )
				return str;

			return String.Format( "{0}{1}", str.Substring( 0, str.Length - suffix.Length ), suffix );
		}

		/// <summary>Returns all of a string after the specified LAST occurance of the specified token</summary>
		/// <returns>The substring</returns>
		public static string GetLastAfter( this string str, char token )
		{
			if ( str.Length < 1 )
				return String.Empty;

			int idx = str.LastIndexOf( token );

			if ( idx < 0 || idx >= str.Length - 1 )
				return String.Empty;

			return str.Substring( idx + 1, str.Length - idx - 1 ).Trim();
		}

		/// <summary>Test and add a backslash to any string without one.</summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string TrailingBackslash(this System.String str)
        {
            str = str.Trim();
            return (str[str.Length - 1] == '\\') ? str : String.Format("{0}\\", str);
        }

        /// <summary>Test a string is nothing but letters and digits</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAlphanumeric(this System.String str)
        {
            return str.All(char.IsLetterOrDigit);
        }

        /// <summary>Returns the enum value by the description string on the enum member</summary>
        public static T EnumByDescription<T>(this string desc) where T : struct, IConvertible
        {
            Type type = typeof(T);

            // is it an enum
            if ( !type.IsEnum )
                throw new System.ArgumentException("Type must be an enum.");

            foreach ( string field in Enum.GetNames(type) )
            {
                MemberInfo[] infos = type.GetMember(field);

                foreach (MemberInfo info in infos)
                {
                    DescriptionAttribute attr = info.GetCustomAttribute<DescriptionAttribute>(false);

                    if ( attr.Description.Equals(desc, StringComparison.InvariantCultureIgnoreCase))
                        return field.GetEnumValue<T>( true );
                }
            }

            return default;
        }
    }
}
