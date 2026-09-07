using System.Reflection;
using System.Text;

namespace WIN_BOT
{
    public class SQLHelper
    {
        public static string BuildInsertQuery<T>(T model, string tableName)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"INSERT INTO {tableName} (");

            PropertyInfo[] properties = typeof(T).GetProperties();
            int fieldcount = 0, valuecount = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                //if (i > 0)
                if (fieldcount > 0)
                {
                    queryBuilder.Append(", ");
                    queryBuilder.Append(property.Name);
                    Console.WriteLine(queryBuilder);
                }
                else
                {
                    fieldcount += 1;
                    queryBuilder.Append(property.Name);
                    Console.WriteLine(queryBuilder);
                }

            }

            queryBuilder.Append(") VALUES (");

            for (int i = 0; i < properties.Length; i++)
            {
                object value = properties[i].GetValue(model);
                if (valuecount > 0)
                {
                    queryBuilder.Append(", ");
                    queryBuilder.Append($"{FormatValueForQuery(value)}");
                    Console.WriteLine(queryBuilder);
                }
                else
                {
                    valuecount += 1;
                    queryBuilder.Append($"{FormatValueForQuery(value)}");
                    Console.WriteLine(queryBuilder);
                }
            }

            queryBuilder.Append(");");

            return queryBuilder.ToString();
        }

        public static string BuildInsertQuery<T>(T model, string tableName, List<string> ignoreColumns)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"INSERT INTO {tableName} (");

            int fieldcount = 0, valuecount = 0;
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (!ignoreColumns.Contains(property.Name))
                {
                    //if (i > 0)
                    if (fieldcount > 0)
                    {
                        queryBuilder.Append(", ");
                        queryBuilder.Append(property.Name);
                        Console.WriteLine(queryBuilder);
                    }
                    else
                    {
                        fieldcount += 1;
                        queryBuilder.Append(property.Name);
                        Console.WriteLine(queryBuilder);
                    }
                }
            }

            queryBuilder.Append(") VALUES (");

            for (int i = 0; i < properties.Length; i++)
            {
                 if (!ignoreColumns.Contains(properties[i].Name))
                {
                    object value = properties[i].GetValue(model);
                    if (valuecount > 0)
                    {
                        queryBuilder.Append(", ");
                        queryBuilder.Append($"{FormatValueForQuery(value)}");
                        Console.WriteLine(queryBuilder);
                        Console.WriteLine("A");
                        Console.WriteLine(properties[i].Name, FormatValueForQuery(value), "value");
                    }
                    else
                    {
                        valuecount += 1;
                        queryBuilder.Append($"{FormatValueForQuery(value)}");
                        Console.WriteLine(queryBuilder);
                        Console.WriteLine("B");
                    }

                }
            }

            queryBuilder.Append(");");

            return queryBuilder.ToString();
        }

        public static string BuildUpdateQuery<T>(T model, string tableName, string keyPropertyName, object keyValue)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE {tableName} SET ");

            int fieldcount = 0;
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.Name == keyPropertyName)
                    continue;

                object value = property.GetValue(model);
                if (fieldcount > 0)
                {
                    queryBuilder.Append(", ");
                    queryBuilder.Append($"{property.Name} =  {FormatValueForQuery(value)}");
                }
                else
                {
                    fieldcount += 1;
                    queryBuilder.Append($"{property.Name} =  {FormatValueForQuery(value)}");
                }
            }

            //queryBuilder.Append($" WHERE {keyPropertyName} = @{keyPropertyName};");
            queryBuilder.Append($" WHERE {keyPropertyName} = {FormatValueForQuery(keyValue)};");

            return queryBuilder.ToString();
        }

        public static string BuildUpdateQuery<T>(T model, string tableName, string keyPropertyName, object keyValue, List<string> ignoreColumns)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE {tableName} SET ");

            int fieldcount = 0;
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.Name == keyPropertyName)
                    continue;

                if (!ignoreColumns.Contains(property.Name))
                {
                    object value = property.GetValue(model);
                    if (fieldcount > 0)
                    {
                        queryBuilder.Append(", ");
                        queryBuilder.Append($"{property.Name} =  {FormatValueForQuery(value)}");
                    }
                    else
                    {
                        fieldcount += 1;
                        queryBuilder.Append($"{property.Name} =  {FormatValueForQuery(value)}");
                    }
                }
            }

            //queryBuilder.Append($" WHERE {keyPropertyName} = @{keyPropertyName};");
            queryBuilder.Append($" WHERE {keyPropertyName} = {FormatValueForQuery(keyValue)};");

            return queryBuilder.ToString();
        }


        //Multiple Where Conditions
        public static string BuildUpdateQuery<T>(T model, string tableName, Dictionary<string, object> whereConditions, List<string> ignoreColumns)
        {
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append($"UPDATE {tableName} SET ");

            int fieldcount = 0;
            PropertyInfo[] properties = typeof(T).GetProperties();

            // Loop through properties to build the SET clause
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                // Ignore columns from the ignoreColumns list and the WHERE keys
                if (whereConditions.ContainsKey(property.Name) || ignoreColumns.Contains(property.Name))
                    continue;

                object value = property.GetValue(model);
                if (fieldcount > 0)
                {
                    queryBuilder.Append(", ");
                }

                queryBuilder.Append($"{property.Name} = {FormatValueForQuery(value)}");
                fieldcount++;
            }

            // Build the WHERE clause with multiple conditions
            queryBuilder.Append(" WHERE ");
            int conditionCount = 0;
            foreach (var condition in whereConditions)
            {
                if (conditionCount > 0)
                {
                    queryBuilder.Append(" AND ");
                }
                queryBuilder.Append($"{condition.Key} = {FormatValueForQuery(condition.Value)}");
                conditionCount++;
            }

            return queryBuilder.ToString();
        }

        // Helper method to format values for the query (e.g., adding quotes for strings)
        private static string FormatValueForQuery(object value)
        {
            if (value == null)
                return "NULL";

            if (value is string || value is Guid)
                return $"'{value}'";

            if (value is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";

            return value.ToString();
        }


        public static string BuildDeleteQuery(string tableName, string keyPropertyName, object keyValue)
        {
            //return $"DELETE FROM {tableName} WHERE {keyPropertyName} = @{keyPropertyName};";
            return $"DELETE FROM {tableName} WHERE {keyPropertyName} = {FormatValueForQuery(keyValue)};";
        }


    }
}
