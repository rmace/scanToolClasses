using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scanToolClasses
{
    public class xmlIOUtilities
    {
        /// <summary>
        /// Remove illegal XML characters from a string.
        /// </summary>
        public static string SanitizeXmlString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }

            StringBuilder buffer = new StringBuilder(xml.Length);

            foreach (char c in xml)
            {
                if (IsLegalXmlChar(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public static bool IsLegalXmlChar(int character)
        {
            return
            (
                 character == 0x9 /* == '\t' == 9   */          ||
                 character == 0xA /* == '\n' == 10  */          ||
                 character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
            );
        }

        /// <summary>
        /// Whether a given character is a carriage return
        /// </summary>
        public static bool IsCarriageReturn(int character)
        {
            return
            (
                 character == 0xD /* == '\r' == 13  */          
            );
        }

        /// <summary>
        /// Whether a given character is a line feed
        /// </summary>
        public static bool IsNewLine(int character)
        {
            return
            (
                 character == 0xA /* == '\n' == 10  */
            );
        }

        /// <summary>
        ///  Remove carriage returns and/or newlines from a string
        /// </summary>
        public static string removeReturns(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            StringBuilder buffer = new StringBuilder(s.Length);

            foreach (char c in s)
            {
                if (!IsCarriageReturn(c) && !IsNewLine(c))
                {
                    buffer.Append(c);
                }
            }

            return buffer.ToString();

        }
    }
}
