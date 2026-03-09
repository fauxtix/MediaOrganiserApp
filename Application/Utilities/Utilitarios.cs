using MediaOrganizerApp.Application.Configurations;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace MediaOrganizerApp.Application.Utilities
{
    public class Utilitarios
    {
        public static int GetMonthsBetweenDates(DateTime dInicio, DateTime dFim)
        {
            var iMeses = dFim.Subtract(dInicio).Days / (365.2425 / 12);
            return Convert.ToInt32(iMeses);
        }

        /// <summary>
        /// Verifica se email é valido
        /// </summary>
        /// <param name="strEmail"></param>
        /// <returns></returns>
        public static bool IsEmailValid(string strEmail)
        {
            string s1;
            string s2;
            long i;
            bool blnIsItValid;
            blnIsItValid = true;
            string dummy;
            dummy = strEmail.Replace("@", "");
            i = strEmail.Length - dummy.Length;
            if (i != 1)
            {
                return false;
            }

            s1 = strEmail[(strEmail.IndexOf("@") + 1)..];
            s2 = strEmail.Substring(0, strEmail.IndexOf("@"));
            if (s1.Length == 0 || s2.Length == 0)
            {
                return false;
            }

            for (int iLoop = 0; iLoop < s1.Length - 1; iLoop++)
            {
                dummy = s1.Substring(iLoop, 0);
                dummy = dummy.ToLower();
                int xxx = dummy.IndexOf("abcdefghijklmnopqrstuvwxyz_-.");
                if (xxx <= 0)
                    blnIsItValid = false;

                dummy = s2.Substring(iLoop, 0);
                dummy = dummy.ToLower();
                xxx = dummy.IndexOf("abcdefghijklmnopqrstuvwxyz_-.");
                if (xxx <= 0)
                    blnIsItValid = false;
                if (s1.Substring(0, 1) == "." || s2.Substring(0, 1) == ".")
                    blnIsItValid = false;
                if (s1[^1..] == "" || s2.Substring(s2.Length - 1, 1) == ".")
                    blnIsItValid = false;
            }


            return blnIsItValid;

        }

        /// <summary>
        /// Valida e-mail
        /// </summary>
        /// <param name="Email"></param>
        /// <returns></returns>
        public bool ValidateEmail(string Email)
        {
            try
            {
                if ((Email.IndexOf('@') != Email.LastIndexOf('@')) || !Email.Contains('@')) return false;

                if (Email[Email.IndexOf('@') + 1] == '.') return false;

                if ((Email.LastIndexOf(".") == Email.Length - 1) || Email.LastIndexOf(".") == -1) return false;

                if (Email.Contains(" ") || Email.Contains("\\") || Email.Contains("/") || Email.Contains("*") || Email.Contains(";")) return false;

                return true;
            }
            catch //(System.Exception ex)
            {
                return false;
                //                throw new ApplicationException("E-Mail inválido");
                //throw;
            }

        }

        /// <summary>
        /// Calcula idade através da data de nascimento passada
        /// </summary>
        /// <param name="DataNascimento"></param>
        /// <returns></returns>
        public static int CalculaIdade(DateTime DataNascimento)
        {
            int anos = DateTime.Now.Year - DataNascimento.Year;
            if (DateTime.Now.Month < DataNascimento.Month ||
                (DateTime.Now.Month == DataNascimento.Month && DateTime.Now.Day < DataNascimento.Day))
                anos--;
            return anos;
        }


        public static string GetLanguage()
        {
            string ExeName = System.Reflection.Assembly.GetEntryAssembly().Location;
            var configFile = ConfigurationManager.OpenExeConfiguration(ExeName);
            return configFile.AppSettings.Settings["Language"].Value;

        }

        public static bool IsUrlValid(string sUrlName)
        {
            try
            {
                Uri uriNew;
                uriNew = GetUri(sUrlName);
                string sUrl = uriNew.AbsoluteUri;

                bool result = Uri.TryCreate(sUrl, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                return result;

            }
            catch 
            {
                ////Logger.WriteLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Justificação de texto
        /// </summary>
        /// <param name="s"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static String Justify(String s, Int32 count)
        {
            if (count <= 0)
                return s;

            Int32 middle = s.Length / 2;
            IDictionary<Int32, Int32> spaceOffsetsToParts = new Dictionary<Int32, Int32>();
            String[] parts = s.Split(' ');
            for (Int32 partIndex = 0, offset = 0; partIndex < parts.Length; partIndex++)
            {
                spaceOffsetsToParts.Add(offset, partIndex);
                offset += parts[partIndex].Length + 1; // +1 to count space that was removed by Split 
            }
            foreach (var pair in spaceOffsetsToParts.OrderBy(entry => Math.Abs(middle - entry.Key)))
            {
                count--;
                if (count < 0)
                    break;
                parts[pair.Value] += ' ';
            }
            return String.Join(" ", parts);
        }

        /// <summary>
        /// Confirm that an email address is valid
        /// in format
        /// </summary>
        /// <param name="emailAddress">Full email address to validate</param>
        /// <returns>True if email address is valid</returns>
        public static bool ValidateEmailAddress(string emailAddress)
        {
            try
            {
                string TextToValidate = emailAddress;
                Regex expression = new(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");

                // test email address with expression
                if (expression.IsMatch(TextToValidate))
                {
                    // is valid email address
                    return true;
                }
                else
                {
                    // is not valid email address
                    return false;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }



        //Começam aqui os Métodos de Compatibilazação com VB 6 .........Left() Right() Mid()
        public static string Left(string param, int length)
        {
            //we start at 0 since we want to get the characters starting from the
            //left and with the specified lenght and assign it to a variable
            if (param == "")
                return "";
            string result = param.Substring(0, length);
            //return the result of the operation
            return result;
        }

        public static string Right(string param, int length)
        {
            //start at the index based on the lenght of the sting minus
            //the specified lenght and assign it a variable
            if (param == "")
                return "";
            string result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }

        public static string Mid(string param, int startIndex, int length)
        {
            //start at the specified index in the string ang get N number of
            //characters depending on the lenght and assign it to a variable
            string result = param.Substring(startIndex, length);
            //return the result of the operation
            return result;
        }

        public static string Mid(string param, int startIndex)
        {
            //start at the specified index and return all characters after it
            //and assign it to a variable
            string result = param[startIndex..];
            //return the result of the operation
            return result;
        }

        /// <summary>
        /// Verifica se sistema operativo é de 64 bits
        /// </summary>
        /// <param name="hProcess"></param>
        /// <param name="lpSystemInfo"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        public static bool Is64Bit()
        {

            IsWow64Process(Process.GetCurrentProcess().Handle, out bool retVal);

            return retVal;
        }

        public static bool Is64Bits()
        {
            if (IntPtr.Size == 8)
            {
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Verifica se motor de dados usado no programa é o SQL Server
        /// </summary>
        /// <returns></returns>
        public static bool IsSQLServer()
        {
            string connectionString = ApplicationConfiguration.DefaultConnection.ToString();

            if (connectionString.ToLower().Contains("sqlserver"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Verifica se motor de dados usado no programa é o SQL Server
        /// </summary>
        /// <returns></returns>
        public static bool IsAccess()
        {
            string connectionString = ApplicationConfiguration.DefaultConnection.ToString();

            if (connectionString.ToLower().StartsWith("msaccess"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Verifica se motor de dados usado no programa é o MySQL
        /// </summary>
        /// <returns></returns>
        public static bool IsMySQL()
        {
            string connectionString = ApplicationConfiguration.DefaultConnection.ToString();

            if (connectionString.ToLower().StartsWith("mysql"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Verifica se motor de dados usado no programa é o MySQL
        /// </summary>
        /// <returns></returns>
        public static bool IsSQLite()
        {
            string connectionString = ApplicationConfiguration.DefaultConnection.ToString();

            if (connectionString.ToLower().StartsWith("sqlite"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Verifica se o sqlserver está instalado
        /// </summary>
        /// <returns>sucesso?</returns>
        public static bool IsSqlServerInstalled()
        {
            bool blnInstalled = false;

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    blnInstalled = true;
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        Console.WriteLine(Environment.MachineName + @"\" + instanceName);
                    }
                }
                return blnInstalled;
            }
        }

        /// <summary>
        /// Obtém info sobre o sistema operativo
        /// </summary>
        /// <returns></returns>
        public string GetOSInfo()
        {
            //Get Operating system information.
            OperatingSystem os = Environment.OSVersion;
            //Get version information about the os.
            System.Version vs = os.Version;

            //Variable to hold our return value
            string operatingSystem = "";

            if (os.Platform == PlatformID.Win32Windows)
            {
                //This is a pre-NT version of Windows
                switch (vs.Minor)
                {
                    case 0:
                        operatingSystem = "95";
                        break;
                    case 10:
                        if (vs.Revision.ToString() == "2222A")
                            operatingSystem = "98SE";
                        else
                            operatingSystem = "98";
                        break;
                    case 90:
                        operatingSystem = "Me";
                        break;
                    default:
                        break;
                }
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                switch (vs.Major)
                {
                    case 3:
                        operatingSystem = "NT 3.51";
                        break;
                    case 4:
                        operatingSystem = "NT 4.0";
                        break;
                    case 5:
                        if (vs.Minor == 0)
                            operatingSystem = "2000";
                        else
                            operatingSystem = "XP";
                        break;
                    case 6:
                        if (vs.Minor == 0)
                            operatingSystem = "Vista";
                        else
                            operatingSystem = "7";
                        break;
                    default:
                        break;
                }
            }
            //Make sure we actually got something in our OS check
            //We don't want to just return " Service Pack 2" or " 32-bit"
            //That information is useless without the OS version.
            if (operatingSystem != "")
            {
                //Got something.  Let's prepend "Windows" and get more info.
                operatingSystem = "Windows " + operatingSystem;
                //See if there's a service pack installed.
                if (os.ServicePack != "")
                {
                    //Append it to the OS name.  i.e. "Windows XP Service Pack 3"
                    operatingSystem += " " + os.ServicePack;
                }
                //Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
                operatingSystem += " " + GetOSArchitecture().ToString() + "-bit";
            }
            //Return the information we've gathered.
            return operatingSystem;
        }

        private static int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        public bool IsAccessDatabaseEngineInstalled()
        {
            bool llretval = false;
            RegistryKey rkACDBKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\Installer\Products");
            if (rkACDBKey != null)
            {
                //int lnSubKeyCount = 0;
                //lnSubKeyCount =rkACDBKey.SubKeyCount; 
                foreach (string subKeyName in rkACDBKey.GetSubKeyNames())
                {
                    using (RegistryKey RegSubKey = rkACDBKey.OpenSubKey(subKeyName))
                    {
                        foreach (string valueName in RegSubKey.GetValueNames())
                        {
                            if (valueName.ToUpper() == "PRODUCTNAME")
                            {
                                string AccessDBAsValue = (string)RegSubKey.GetValue(valueName.ToUpper());
                                if (AccessDBAsValue.Contains("Access database engine"))
                                {
                                    llretval = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (llretval)
                    {
                        break;
                    }
                }
            }
            return llretval;
        }


        public bool IsAccessInstalled()
        {
            // Verifica se o Access está instalado neste PC,
            // ao verificar a chave HKEY_CLASSES_ROOT\Access.Application.
            using (var regAccess = Registry.ClassesRoot.OpenSubKey("Access.Application"))
            {
                if (regAccess == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

        public bool IsWordFileOpened()
        {
            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("WINWORD");
            if (processes != null)
            {
                if (processes.Length > 0)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }
        public bool IsWordInstalled()
        {
            // Verifica se o Winword está instalado neste PC,
            // ao verificar a chave HKEY_CLASSES_ROOT\Word.Application.
            using (var regWord = Registry.ClassesRoot.OpenSubKey("Word.Application"))
            {
                if (regWord == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

        public bool IsExcelInstalled()
        {
            // Verifica se o Excel está instalado neste PC,
            // ao verificar a chave HKEY_CLASSES_ROOT\Excel.Application.
            using (var regWord = Registry.ClassesRoot.OpenSubKey("Excel.Application"))
            {
                if (regWord == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }


        public static bool IsSQLServerRunning()
        {
            var factory = SqlClientFactory.Instance;
            //DbProviderFactory factory;
            var ret = false;
            if (factory.CanCreateDataSourceEnumerator)
            {
                DbDataSourceEnumerator instance =
                    factory.CreateDataSourceEnumerator();
                DataTable table = instance.GetDataSources();
                if (table.Rows.Count > 0)
                    ret = true;
            }

            return ret;
        }


        //public void DisableUnwantedExportFormat(ReportViewer ReportViewerID, string strFormatName)
        //{
        //    FieldInfo info;
        //    foreach (RenderingExtension extension in ReportViewerID.LocalReport.ListRenderingExtensions())
        //    {
        //        if (extension.Name == strFormatName)
        //        {
        //            info = extension.GetType().GetField("m_isVisible", BindingFlags.Instance | BindingFlags.NonPublic);
        //            info.SetValue(extension, false);
        //        }
        //    }
        //}

        public static bool UsaNotifier()
        {
            string sUsaNotifier = ApplicationConfiguration.UsaNotifier;
            if (sUsaNotifier.ToLower() == "sim")
                return true;
            else
                return false;
        }

        public static int Tempo_Notifier()
        {
            int sUsaNotifier = ApplicationConfiguration.Tempo_Notifier;
            return sUsaNotifier;
        }

        public static string PastaListagens()
        {
            string sPastaImagens = ApplicationConfiguration.PastaListagens;
            return sPastaImagens;
        }

        public static string DefaultConnection()
        {
            string sConnection = ApplicationConfiguration.DefaultConnection;
            return sConnection;

        }

        public static void UpdateConfig(string key, string value, string fileName)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(fileName);
            configFile.AppSettings.Settings[key].Value = value;
            try
            {
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings/" + key);
                ConfigurationManager.OpenExeConfiguration(fileName);

                //configFile.Save();

            }
            catch
            {
                throw;
            }
        }

        public static bool IsSQLServerExecutando()
        {

            var factory = SqlClientFactory.Instance;
            //DbProviderFactory factory;
            var ret = false;
            if (factory.CanCreateDataSourceEnumerator)
            {
                DbDataSourceEnumerator instance =
                    factory.CreateDataSourceEnumerator();
                DataTable table = instance.GetDataSources();
                if (table.Rows.Count > 0)
                    ret = true;
            }

            return ret;
        }



        /// <summary>
        /// Verifica se base de dados (SQL Server) existe
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        public bool CheckOtherBDExists()
        {
            string connectString;
            string sTipoBD = "";
            //Utilitarios Utils; // = new Utilitarios();

            if (Utilitarios.IsSQLite())
                sTipoBD = "SqLite";
            if (Utilitarios.IsAccess())
                sTipoBD = "MSAccessCon";

            connectString = ConfigurationManager.ConnectionStrings[sTipoBD].ToString();
            String[] parts = connectString.Split(';');

            string sDataSource;
            if (Utilitarios.IsAccess())
                sDataSource = parts[1];
            else
                sDataSource = parts[0];

            sDataSource = sDataSource.Replace(@"|DataDirectory|",
                AppDomain.CurrentDomain.BaseDirectory);

            int iPos = sDataSource.IndexOf("=");
            string sFileLocation = sDataSource[(iPos + 1)..];
            bool bRet;
            if (File.Exists(sFileLocation))
                bRet = true;
            else
                bRet = false;
            //else if (sTipoBD == "SqLite")
            //{
            //    CriaBD_Livros_SqLite(sFileLocation);
            //    bRet = true;
            //}
            return bRet;
        }

        private static Uri GetUri(string s)
        {
            return new UriBuilder(s).Uri;
        }

        public List<string> PopulateUrlList()
        {
            string regKey = "Software\\Microsoft\\Internet Explorer\\TypedURLs";
            RegistryKey subKey = Registry.CurrentUser.OpenSubKey(regKey);
            List<string> urlList = new();
            int counter = 1;
            while (true)
            {
                string sValName = "url" + counter.ToString();
                string url = (string)subKey.GetValue(sValName);
                if (url is null)
                {
                    break; 
                }
                urlList.Add(url);
                counter += 1;
            }
            return urlList;
        }

        public static string ValorPorExtenso(decimal valor)
        {
            try
            {
                // se for =0 retorna 0 eros
                if (valor == 0)
                {
                    return "Zero Euros";
                }
                // Verifica a parte decimal, ou seja, os cêntimos
                int cent = Convert.ToInt32(decimal.Round((valor - (int)valor) * 100, MidpointRounding.ToEven));
                // Verifica apenas a parte inteira
                //number = Convert.ToInt32(number);
                valor = (int)valor;
                // Caso existam cêntimos
                if (cent > 0)
                {
                    // Caso seja 1 não coloca "euros" mas sim "euro"
                    if (valor == 1)
                    {
                        return "Um Euro e " + GetDecimal(Convert.ToByte(cent)) + "Cêntimos";
                    }
                    // Caso o valor seja inferior a 1 euro
                    else if (valor == 0)
                    {
                        return GetDecimal(Convert.ToByte(cent)) + "Cêntimos";
                    }
                    else
                    {
                        return GetInteger(valor) + "Euros e " + GetDecimal(Convert.ToByte(cent)) + "Cêntimos";
                    }
                }
                else
                {
                    // Caso seja 1 não coloca "euros" mas sim "euro"
                    if (valor == 1)
                    {
                        return "Um Euro";
                    }
                    else
                    {
                        return GetInteger(valor) + "Euros";
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        public static string GetDecimal(byte number)
        {
            try
            {
                Convert.ToInt32(number);

                if (number == 0)
                {
                    return "";
                }

                if (number >= 1 && number <= 19)
                {
                    string[] strArray = {"Um", "Dois", "Três", "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove", "Dez",
                "Onze", "Doze", "Treze", "Quatorze", "Quinze", "Dezasseis", "Dezassete", "Dezoito", "Dezanove"};

                    return strArray[number - 1] + " ";
                }

                if (number >= 20 && number <= 99)
                {
                    string[] strArray = { "Vinte", "Trinta", "Quarenta", "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa" };

                    if ((number % 10) == 0)
                    {
                        return strArray[number / 10 - 2]; // + " ";
                    }
                    else
                    {
                        return strArray[number / 10 - 2] + " e " + GetDecimal(Convert.ToByte(number % 10)); // + " ";
                    }
                }

                else
                {
                    return "";
                }
            }

            catch
            {
                return "";
            }
        }


        public static string GetInteger(decimal number)
        {

            try
            {
                number = (int)number;

                if (number < 0)
                {
                    return "-" + GetInteger(-number);
                }

                if (number == 0)
                {
                    return "";
                }

                if (number >= 1 && number <= 19)
                {
                    string[] strArray = { "Um", "Dois", "Três", "Quatro", "Cinco", "Seis", "Sete", "Oito", "Nove", "Dez",
            "Onze", "Doze", "Treze", "Quatorze", "Quinze", "Dezasseis", "Dezassete", "Dezoito", "Dezanove" };

                    return strArray[Convert.ToInt32(number) - 1] + " ";
                }

                if (number >= 20 && number <= 99)
                {
                    string[] strArray = { "Vinte", "Trinta", "Quarenta", "Cinquenta", "Sessenta", "Setenta", "Oitenta", "Noventa" };

                    if ((number % 10) == 0)
                    {
                        return strArray[Convert.ToInt32(number) / 10 - 2] + " ";
                    }
                    else
                    {
                        return strArray[Convert.ToInt32(number) / 10 - 2] + " e " + GetInteger(number % 10); // + " ";
                    }
                }

                if (number == 100)
                {
                    return "Cem ";
                }

                if (number >= 101 && number <= 999)
                {
                    string[] strArray = { "Cento", "Duzentos", "Trezentos", "Quatrocentos", "Quinhentos", "Seiscentos", "Setecentos", "Oitocentos", "Novecentos" };

                    if ((number % 100) == 0)
                    {
                        return strArray[Convert.ToInt32(number) / 100 - 1]; // + " ";
                    }
                    else
                    {
                        return strArray[Convert.ToInt32(number) / 100 - 1] + " e " + GetInteger(number % 100); // + " ";
                    }
                }

                if (number >= 1000 && number <= 1999)
                {
                    int resto = Convert.ToInt32(number) % 1000;

                    if (resto == 0)
                    {
                        return "Mil ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return "Mil e " + GetInteger(number % 1000);
                        }

                        else
                        {
                            return "Mil, " + GetInteger(number % 1000);
                        }
                    }
                }


                if (number >= 2000 && number <= 999999)
                {
                    int resto = Convert.ToInt32(number) % 1000;

                    if (resto == 0)
                    {
                        return GetInteger(number / 1000) + "Mil ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return GetInteger(number / 1000) + "Mil e " + GetInteger(number % 1000);
                        }

                        else
                        {
                            return GetInteger(number / 1000) + "Mil, " + GetInteger(number % 1000);
                        }
                    }
                }


                if (number >= 1000000 && number <= 1999999)
                {
                    int resto = Convert.ToInt32(number) % 1000000;

                    if (resto == 0)
                    {
                        return "Um Milhão ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return GetInteger(number / 1000000) + "Milhão e " + GetInteger(number % 1000000);
                        }

                        else
                        {
                            return GetInteger(number / 1000000) + "Milhão, " + GetInteger(number % 1000000);
                        }
                    }
                }


                if (number >= 2000000 && number <= 999999999)
                {
                    int resto = Convert.ToInt32(number) % 1000000;

                    if (resto == 0)
                    {
                        return GetInteger(number / 1000000) + " Milhões ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return GetInteger(number / 1000000) + "Milhões e " + GetInteger(number % 1000000);
                        }

                        else
                        {
                            return GetInteger(number / 1000000) + "Milhões, " + GetInteger(number % 1000000);
                        }
                    }
                }

                if (number >= 1000000000 && number <= 1999999999)
                {
                    int resto = Convert.ToInt32(number) % 1000000000;

                    if (resto == 0)
                    {
                        return "Um Bilião ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return GetInteger(number / 1000000000) + "Bilião e " + GetInteger(number % 1000000000);
                        }

                        else
                        {
                            return GetInteger(number / 1000000000) + "Bilião, " + GetInteger(number % 1000000000);
                        }
                    }
                }


                else
                {
                    int resto = Convert.ToInt32(number) % 1000000000;

                    if (resto == 0)
                    {
                        return GetInteger(number / 1000000000) + " Biliões ";
                    }
                    else
                    {
                        if (resto <= 100)
                        {
                            return GetInteger(number / 1000000000) + "Biliões e " + GetInteger(number % 1000000000);
                        }

                        else
                        {
                            return GetInteger(number / 1000000000) + "Biliões, " + GetInteger(number % 1000000000);
                        }
                    }
                }

            }

            catch
            {
                return "";
            }
        }

        public string GetLinguagem()
        {
            string sIdioma = ApplicationConfiguration.Language;
            return sIdioma;
        }
    }
}
