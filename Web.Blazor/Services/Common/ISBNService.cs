using MediaOrganizerApp.Application.Interfaces.Services;
using System;
using System.IO;
using System.Net;
using System.Xml;

namespace Web.Blazor.Services.Common
{
    public class ISBNService : IISBNService
    {
        // ********************************************************************
        // * ISBN Reference:                                                  *
        // * http://en.wikipedia.org/wiki/International_Standard_Book_Number  *
        // ********************************************************************
        /// <summary>

        /// This method will validate a ISBN 10
        /// or ISBN 13 code.
        /// </summary>
        /// <param name="isbn">code to validate</param>
        /// <returns>true, if valid, otherwise false</returns>
        public static bool TryValidate(string isbn)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(isbn))
            {
                if (isbn.Contains("-")) isbn = isbn.Replace("-", "");
                switch (isbn.Length)
                {
                    case 10: result = IsValidIsbn10(isbn); break;
                    case 13: result = IsValidIsbn13(isbn); break;
                }
            }
            return result;
        }

        /// <summary>
        /// Validates ISBN10 codes
        /// </summary>
        /// <param name="isbn10">code to validate</param>
        /// <returns>true if valid</returns>
        private static bool IsValidIsbn10(string isbn10)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(isbn10))
            {
                if (isbn10.Contains("-")) isbn10 = isbn10.Replace("-", "");
                long j;
                // Length must be 10 and only the last character could be a char('X') or a numeric value,
                // otherwise it's not valid.
                if (isbn10.Length != 10 || !long.TryParse(isbn10.Substring(0, isbn10.Length - 1), out j))
                    return false;
                char lastChar = isbn10[isbn10.Length - 1];
                // Using the alternative way of calculation
                int sum = 0;
                for (int i = 0; i < 9; i++)
                    sum += int.Parse(isbn10[i].ToString()) * (i + 1);
                // Getting the remainder or the checkdigit
                int remainder = sum % 11;
                // If the last character is 'X', then we should check if the checkdigit is equal to 10
                if (lastChar == 'X')
                {
                    result = (remainder == 10);
                }

                // Otherwise check if the lastChar is numeric
                // Note: I'm passing sum to the TryParse method to not create a new variable again
                else if (int.TryParse(lastChar.ToString(), out sum))
                {
                    // lastChar is numeric, so let's compare it to remainder
                    result = (remainder == int.Parse(lastChar.ToString()));
                }
            }
            return result;
        }

        /// <summary>
        /// Validates ISBN13 codes
        /// </summary>
        /// <param name="isbn13">code to validate</param>
        /// <returns>true, if valid</returns>
        private static bool IsValidIsbn13(string isbn13)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(isbn13))
            {
                if (isbn13.Contains("-")) isbn13 = isbn13.Replace("-", "");
                // If the length is not 13 or if it contains any non numeric chars, return false
                long temp;
                if (isbn13.Length != 13 || !long.TryParse(isbn13, out temp)) return false;
                // Comment Source: Wikipedia
                // The calculation of an ISBN-13 check digit begins with the first
                // 12 digits of the thirteen-digit ISBN (thus excluding the check digit itself).
                // Each digit, from left to right, is alternately multiplied by 1 or 3,
                // then those products are summed modulo 10 to give a value ranging from 0 to 9.
                // Subtracted from 10, that leaves a result from 1 to 10. A zero (0) replaces a
                // ten (10), so, in all cases, a single check digit results.
                int sum = 0;
                for (int i = 0; i < 12; i++)
                {
                    sum += int.Parse(isbn13[i].ToString()) * (i % 2 == 1 ? 3 : 1);
                }
                int remainder = sum % 10;
                int checkDigit = 10 - remainder;
                if (checkDigit == 10) checkDigit = 0;

                result = (checkDigit == int.Parse(isbn13[12].ToString()));
            }
            return result;
        }

        public String GetXMLData(String chave, String sISBN)
        {
            string accessKey = "5X5U83P4";
            string isbnStr = "9789029084482";

            // para teste --> ISBN = 9789725648162 (2666) -- chave 5X5U83P4
            String results = null;
            XmlTextReader reader = null;

            try
            {
                String ISBNUriStr = "http://isbndb.com//api/books.xml?access_key=" +
                    accessKey + "&results=details&index1=isbn&value1=";

                Uri uri = null;

                if (isbnStr != null)
                    uri = new Uri(ISBNUriStr + isbnStr);

                WebRequest http = HttpWebRequest.Create(uri);
                HttpWebResponse response = (HttpWebResponse)http.GetResponse();
                Stream stream = response.GetResponseStream();

                reader = new XmlTextReader(stream);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        String name = reader.Name;

                        if (name == "BookData")
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }

                            while (reader.MoveToNextAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }
                        }

                        else if (name == "Title")
                            results += "Title: " + reader.ReadString() + "\r\n";

                        else if (name == "TitleLongText")
                            results += "Title Long: " + reader.ReadString() + "\r\n";

                        else if (name == "AuthorsText")
                            results += "Authors: " + reader.ReadString() + "\r\n";

                        else if (name == "PublisherText")
                            results += "Publisher: " + reader.ReadString() + "\r\n";

                        else if (name == "Details")
                        {
                            if (reader.MoveToFirstAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }

                            while (reader.MoveToNextAttribute())
                            {
                                results += reader.Name + ": ";
                                results += reader.Value + "\r\n";
                            }
                        }

                        reader.Read();
                    }

                }

                return results;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
