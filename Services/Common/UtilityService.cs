using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Globalization;
using System.Text;

namespace CommonServices
{
    public class UtilityService
    {

        public static bool CheckInsertUpdateDeleteResponse(DataTable dataTable, out string responseMessage)
        {
            responseMessage = "";

            try
            {
                string Status = "";
                string Message = "";
                //string Error = "";
                try { Status = dataTable.Rows[0]["Status"]?.ToString(); } catch { }
                try { Message = dataTable.Rows[0]["Message"]?.ToString(); } catch { }
                //try { Error = dataTable.Rows[0]["Error_Code"]?.ToString(); } catch { }
                if (!string.IsNullOrWhiteSpace(Status))
                {
                    if (string.Equals(Status, "Fail", StringComparison.OrdinalIgnoreCase))
                    {
                        responseMessage = Message;
                        return false;
                    }
                    else
                    {
                        responseMessage = Message;
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        #region JSON, JObject, JArray --------------------------------------------------------------------

        /// <summary>
        /// This Converts Model To Json String
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ModelToJsonString(object obj)
        {
            if (obj != null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.None);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// This Converts Json String To Model
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T JsonStringToModel<T>(string jsonString)
        {
            if (!string.IsNullOrWhiteSpace(jsonString))
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// This Converts String into Json Object
        /// <para>Parameter : jsonString</para>
        /// <para>Return : JObject</para>
        /// </summary>
        public static JObject StringToJObject(string jsonString)
        {
            JObject jObject = new JObject();
            try
            {
                jObject = JObject.Parse(jsonString);
            }
            catch { }
            return jObject;
        }

        /// <summary>
        /// This Converts String into Json Array
        /// <para>Parameter : jsonString</para>
        /// <para>Return : Jarray</para>
        /// </summary>
        public static JArray StringToJArray(string jsonString)
        {
            JArray jArray = new JArray();
            try
            {
                jArray = JArray.Parse(jsonString);
            }
            catch { }
            return jArray;
        }

        /// <summary>
        /// This Converts Json Object into String
        /// <para>Parameter : JObject</para>
        /// <para>Return : jsonString</para>
        /// </summary>
        public static string JObjectToString(JObject jObject)
        {
            string ret_val = string.Empty;
            try
            {
                ret_val = JsonConvert.SerializeObject(jObject);
            }
            catch { }
            return ret_val;
        }

        /// <summary>
        /// This Converts Json Object into String
        /// <para>Parameter : JArray</para>
        /// <para>Return : jsonString</para>
        /// </summary>
        public static string JArrayToString(JArray jArray)
        {
            string ret_val = string.Empty;
            try
            {
                ret_val = JsonConvert.SerializeObject(jArray);
            }
            catch { }
            return ret_val;
        }

        /// <summary>
        /// This Converts Datatable into Json Object
        /// <para>Parameter : dataTable</para>
        /// <para>Return : JObject</para>
        /// </summary>
        public static JObject DataTableToJObject(DataTable dataTable)
        {
            JObject jObject = new JObject();

            if (dataTable.Rows.Count > 0)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    jObject[col.ColumnName] = dataTable.Rows[0][col].ToString();
                }
            }
            return jObject;
        }

        /// <summary>
        /// This Converts Datatable into Json Array
        /// <para>Parameter : dataTable</para>
        /// <para>Return : JArray</para>
        /// </summary>
        public static JArray DataTableToJArray(DataTable dataTable)
        {
            JArray jArray = new JArray();
            JObject jObject = new JObject();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    jObject = new JObject();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        jObject[col.ColumnName] = row[col].ToString();
                    }
                    jArray.Add(jObject);
                }
            }
            return jArray;
        }

        /// <summary>
        /// This Converts Datatset into Json Object
        /// <para>Parameter : dataSet</para>
        /// <para>Return : JObject{JArray}</para>
        /// </summary>
        public static JObject DataSetToJObject(DataSet dataSet)
        {
            JArray jArray = new JArray();
            JObject jObjectMain = new JObject();
            JObject jObject = new JObject();
            int i = 1;

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    jArray = new JArray();
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            jObject = new JObject();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                jObject[col.ColumnName] = row[col].ToString();
                            }
                            jArray.Add(jObject);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(dataTable.TableName))
                    {
                        jObjectMain.Add(dataTable.TableName, jArray);
                    }
                    else
                    {
                        jObjectMain.Add("Table_" + i.ToString(), jArray);
                    }

                    i++;
                }
            }
            return jObjectMain;
        }

        public static JObject DataSetToJObject(DataSet dataSet, List<string> tablenames)
        {
            JArray jArray = new JArray();
            JObject jObjectMain = new JObject();
            JObject jObject = new JObject();
            int i = 0;

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    jArray = new JArray();
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            jObject = new JObject();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                jObject[col.ColumnName] = row[col].ToString();
                            }
                            jArray.Add(jObject);
                        }
                    }
                    if (i < tablenames.Count)
                    {
                        jObjectMain.Add(tablenames[i].ToString(), jArray);
                    }
                    else
                    {
                        jObjectMain.Add("Table" + i.ToString(), jArray);
                    }
                    i++;
                }
            }
            return jObjectMain;
        }

        public static JObject DataSetToJObject(DataSet dataSet, List<TableName> tablenames)
        {
            JObject jObjectMain = new JObject();
            JArray jArray = new JArray();
            JObject jObject = new JObject();
            int i = 0;

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    jArray = new JArray();
                    if (dataTable.Rows.Count > 0)
                    {
                        if (tablenames[i].Table_Type == true)
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                jObject = new JObject();
                                foreach (DataColumn col in dataTable.Columns)
                                {
                                    jObject[col.ColumnName] = row[col].ToString();
                                }
                                jArray.Add(jObject);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(tablenames[i].Table_Name))
                            {
                                JObject jObj = new JObject();
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    foreach (DataColumn col in dataTable.Columns)
                                    {
                                        jObj[col.ColumnName] = row[col].ToString();
                                    }
                                }
                                jObjectMain.Add(tablenames[i].Table_Name, jObj);
                            }
                            else
                            {
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    //JObject jObject = new JObject();
                                    foreach (DataColumn col in dataTable.Columns)
                                    {
                                        jObjectMain[col.ColumnName] = row[col].ToString();
                                    }
                                    //jObjectMain.Add("1",jObject);
                                }
                            }
                        }
                    }

                    if (tablenames[i].Table_Type == true)
                    {
                        if (i < tablenames.Count)
                        {
                            jObjectMain.Add(tablenames[i].Table_Name.ToString(), jArray);
                        }
                        else
                        {
                            jObjectMain.Add("Table" + i.ToString(), jArray);
                        }
                    }

                    i++;
                }
            }
            return jObjectMain;
        }

        public static JObject DataSetToJObject(DataSet dataSet, string[] tablenames)
        {
            JArray jArray = new JArray();
            JObject jObjectMain = new JObject();
            JObject jObject = new JObject();
            int i = 0;

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    jArray = new JArray();
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            jObject = new JObject();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                jObject[col.ColumnName] = row[col].ToString();
                            }
                            jArray.Add(jObject);
                        }
                    }

                    if (i < tablenames.Length)
                    {
                        jObjectMain.Add(tablenames[i].ToString(), jArray);
                    }
                    else
                    {
                        jObjectMain.Add("Table" + i.ToString(), jArray);
                    }
                    i++;
                }
            }
            return jObjectMain;
        }

        public static JObject DataSetToJObject(DataSet dataSet, string tablenames)
        {
            JArray jArray = new JArray();
            JObject jObjectMain = new JObject();
            JObject jObject = new JObject();
            int i = 0;

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    jArray = new JArray();
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            jObject = new JObject();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                jObject[col.ColumnName] = row[col].ToString();
                            }
                            jArray.Add(jObject);
                        }
                    }

                    jObjectMain.Add(tablenames + i.ToString(), jArray);
                    i++;
                }
            }
            return jObjectMain;
        }

        public static JArray DataSetToJArray(DataSet dataSet)
        {
            JArray jArray = new JArray();

            if (dataSet.Tables.Count > 0)
            {
                foreach (DataTable dataTable in dataSet.Tables)
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        foreach (DataRow row in dataTable.Rows)
                        {
                            JObject jObject = new JObject();
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                jObject[col.ColumnName] = row[col].ToString();
                            }
                            jArray.Add(jObject);
                        }
                    }
                }
            }
            return jArray;
        }

        #endregion JSON, JObject, JArray -----------------------------------------------------------------


        #region Date Compare ----------------------------------------------------------------------------

        /// <summary>
        /// Checks two dates and return.
        /// <para>Parameter 1 string (dd-MM-yyyy) : DateToCompare </para>
        /// <para>Parameter 2 string (dd-MM-yyyy) : Reference_Date (if null, then compared with current date)</para>
        /// <para>Return int : -1 for (Reference_Date less than DateToCompare), 0 for (Reference_Date equal to DateToCompare), 1 for (Reference_Date greater than DateToCompare)</para>
        /// </summary>
        public static int DateCompare(string Date_To_Compare, string Reference_Date)
        {
            int ret_val = 0;

            DateTime DateToCompare;
            DateTime ReferenceDate;
            try
            {
                DateToCompare = DateTime.ParseExact(Date_To_Compare, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                if (Reference_Date == null)
                {
                    ReferenceDate = DateTime.Now; // compare with current date.
                }
                else
                {
                    ReferenceDate = DateTime.ParseExact(Reference_Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                DateToCompare = DateTime.Parse("01-01-2000");
                ReferenceDate = DateTime.Parse("01-01-2000");
            }

            /*
             * DateTime.Compare returns > -1 for dt1 less than dt2
             * DateTime.Compare returns > 0 for dt1 equal to dt2
             * DateTime.Compare returns > +1 for dt1 greater than dt2
            */
            ret_val = DateTime.Compare(ReferenceDate, DateToCompare); // greater than

            return ret_val;
        }

        /// <summary>
        /// Checks current date is between activation date and expiry date.
        /// <para>Parameter 1 string (dd-MM-yyyy) : Activation_Date </para>
        /// <para>Parameter 2 string (dd-MM-yyyy) : Expiry_Date </para>
        /// <para>Return bool : true/false</para>
        /// </summary>
        public static bool ExpiryDateValidate(string Activation_Date, string Expiry_Date)
        {
            bool ret_val = false;

            if (string.IsNullOrWhiteSpace(Activation_Date))
            {
                return ret_val;
            }

            if (string.IsNullOrWhiteSpace(Expiry_Date))
            {
                return ret_val;
            }

            try
            {
                DateTime SoftwareActivationDate = DateTime.ParseExact(Activation_Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                DateTime SoftwareExpiryDate = DateTime.ParseExact(Expiry_Date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                DateTime SoftwareCurrentDate = DateTime.Now;
                //DateTime SoftwareCurrentDate = DateTime.ParseExact(DateTime.Now.ToString("dd-MM-yyyy"), "dd-MM-yyyy", CultureInfo.InvariantCulture);

                /*
                 * DateTime.Compare returns > -1 for dt1 less than dt2
                 * DateTime.Compare returns > 0 for dt1 equal to dt2
                 * DateTime.Compare returns > +1 for dt1 greater than dt2
                */
                int ret1 = DateTime.Compare(SoftwareCurrentDate, SoftwareActivationDate); // greater than
                int ret2 = DateTime.Compare(SoftwareCurrentDate, SoftwareExpiryDate); // lesser than

                if (ret1 >= 0 && ret2 < 0) //  Current Date must be less than Expiry date.
                {
                    ret_val = true;
                }
            }
            catch { }

            return ret_val;
        }

        #endregion Date Compare -------------------------------------------------------------------------


        #region String Split, Last, First Char, String to Paragraph --------------------------------------------

        public static string EscapeSingleQuote(string sourceString)
        {
            string returnValue = null;

            if (!string.IsNullOrWhiteSpace(sourceString))
            {
                sourceString = sourceString.Replace("\\'", "'");
                returnValue = sourceString.Replace("'", "\\'");
            }
            else
            {
                returnValue = sourceString;
            }

            return returnValue;
        }

        public static string[] StringSplit(string source)
        {
            string[] result = source.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            return result;
        }

        public static string StringArrayToString(string[] sourceArray)
        {
            string result = null;

            if (sourceArray != null && sourceArray.Length > 0)
            {
                foreach (string str in sourceArray)
                {
                    result += str.Trim();
                    result += ",";
                }
                result = result.Substring(0, result.Length - 1);
            }

            return result;
        }

        public static string[] StringToStringArray(string sourceString)
        {
            if (!string.IsNullOrWhiteSpace(sourceString))
            {
                return sourceString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Substring of last characters from source string.
        /// <para>Parameter : source string, tail_length</para>
        /// <para>Return : string</para>
        /// </summary>
        public static string GetLastChars(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;

            return source.Substring(source.Length - tail_length);
        }

        /// <summary>
        /// Substring of first characters from source string.
        /// <para>Parameter : source string, head_length</para>
        /// <para>Return : string</para>
        /// </summary>
        public static string GetFirstChars(string source, int head_length)
        {
            if (head_length >= source.Length)
                return source;

            return source.Substring(0, head_length);
        }


        /// <summary>
        /// Convertes Length one line string into multiple line strings.
        /// <para>Parameter : source_string, maxLineLength</para>
        /// <para>Return : List(string)</para>
        /// </summary>
        public static List<string> StringToParagraph(string source_string, int maxLineLength)
        {
            string[] words = source_string.Split(' ');
            List<string> parts = new List<string>();

            string part = string.Empty;

            foreach (var word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    if (part.Length + word.Length < maxLineLength)
                    {
                        part += string.IsNullOrEmpty(part) ? word : " " + word;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(part))
                        {
                            parts.Add(part);
                        }
                        part = word;
                    }
                }
            }
            parts.Add(part);

            // or this following one line also does the same.
            /*
            var charCount = 0;
            var partslist = source_string.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).GroupBy(w => (charCount += w.Length + 1) / maxLineLength).Select(g => string.Join(" ", g.ToArray()));
            */

            return parts;
        }

        #endregion String Last, Forst Char, String to Paragraph -----------------------------------------


        #region Datatable --------------------------------------------------------------------
        public static List<string> GetDistinctValuesFromDataTable(DataTable dataTable, string columnName)
        {
            List<string> distinctValuesList = new List<string>();

            distinctValuesList = dataTable.DefaultView.ToTable(true, columnName).AsEnumerable().Select(r => r.Field<string>(columnName)).ToList();

            if (distinctValuesList.Count > 0)
            {
                // Remove Emptry or white space
                distinctValuesList = distinctValuesList.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            }

            return distinctValuesList;
        }

        #endregion Datatable -----------------------------------------------------------------


        #region Numerical Calculations -----------------------------------------------------------
        public static string GetWholeNumberAsString(decimal value, int decimalPlace)
        {
            string retVal = "0";

            if ((value % 1.0m) > 0)
            {
                if (decimalPlace > 0)
                {
                    string tmp = "";
                    for (int i = 0; i < decimalPlace; i++)
                    {
                        tmp += "0";
                    }
                    retVal = value.ToString("0." + tmp);
                }
                else
                {
                    retVal = value.ToString();
                }
            }
            else
            {
                retVal = ((int)value).ToString();
            }

            return retVal;
        }

        public static decimal Roundoff(decimal value, out decimal roundedValue, int decimals = 2)
        {
            decimal returnRoundoffValue = 0;

            roundedValue = Math.Round(value, decimals, MidpointRounding.AwayFromZero);

            returnRoundoffValue = roundedValue - value;

            return returnRoundoffValue;
        }

        public static decimal Roundedoff(decimal value, int decimals = 2)
        {
            decimal returnRoundoffValue = 0;

            returnRoundoffValue = Math.Round(value, decimals);

            return returnRoundoffValue;
        }

        #endregion Numerical Calculations --------------------------------------------------------

        public static string GenerateRandomPassword(int passwordLength = 8)
        {
            // Define the characters to use in the password
            string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // Create a StringBuilder to store the generated password
            StringBuilder password = new StringBuilder();

            // Initialize a random number generator
            Random random = new Random();

            // Generate the first character (not numeric)
            password.Append(chars[random.Next(10, chars.Length)]);

            // Generate the remaining characters
            for (int i = 1; i < passwordLength; i++)
            {
                // Append a random character from the defined character set
                password.Append(chars[random.Next(chars.Length)]);
            }

            return password.ToString();
        }

    }

    public class TableName
    {
        public string Table_Name { get; set; }
        public bool Table_Type { get; set; } // true for JArray, false for JObject
    }
}
