using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Sap.Data.Hana;

namespace SAP.QuickCopyUDF
{
    public static class ServiceHelper
    {

        public static void TryAddParameters(this SqlCommand command, params SqlParameter[] parameters)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (parameters == null || parameters.Length == 0) return;
            command.Parameters.AddRange(parameters);
        }

        /// <summary>
        /// hàm add paramater cho hana command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        public static void TryAddParameters(this HanaCommand command, params HanaParameter[] parameters)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (parameters == null || parameters.Length == 0) return;
            command.Parameters.AddRange(parameters);
        }

        public static List<TEntity> Select<TEntity>(this IDataReader reader,
            Func<IDataRecord, TEntity> generator)
        {
            var list = new List<TEntity>();
            while (reader.Read())
                list.Add(generator(reader));
            return list;
        }

        public static IEnumerable<TEntity> GetEnumerator<TEntity>(this IDataReader reader,
            Func<IDataRecord, TEntity> generator)
        {
            while (reader.Read())
                yield return generator(reader);
        }

        internal static double ParseDouble(this object obj)
        {
            try
            {
                double result;
                if (obj == null) return 0;
                if (double.TryParse(obj.ToString(), out result))
                {
                    if (!Double.IsInfinity(result) && !Double.IsNaN(result))
                    {
                        return result;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        internal static int ParseInt(this object obj)
        {
            try
            {
                int result;
                if (obj == null) return 0;
                if (int.TryParse(obj.ToString(), out result))
                    return result;
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        internal static string To_String(this object obj)
        {
            try
            {
                if (obj == null) return string.Empty;
                return obj.ToString().Trim();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        internal static decimal ParseDecimal(this object obj)
        {
            try
            {
                decimal result;
                if (obj == null) return 0;
                var value = obj.ToString();
                if (decimal.TryParse(value, out result))
                    return result;
                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                    return result;
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        internal static DateTime? ParseDateTime(this object obj)
        {
            try
            {
                DateTime result;
                if (obj == null) return null;
                if (DateTime.TryParse(obj.ToString(), out result))
                    return result;
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        internal static double ParseDouble(this object obj, NumberFormatInfo provider)
        {
            try
            {
                double result;
                if (obj == null) return 0;
                if (double.TryParse(obj.ToString(), NumberStyles.Any, provider, out result))
                    if (!Double.IsInfinity(result) && !Double.IsNaN(result))
                    {
                        return result;
                    }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        internal static bool ParseBool(this object obj)
        {
            try
            {
                bool result;
                if (obj == null) return false;
                if (obj.GetType() != typeof(Boolean))
                    return Convert.ToBoolean(obj);
                if (bool.TryParse(obj.ToString(), out result))
                    return result;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
