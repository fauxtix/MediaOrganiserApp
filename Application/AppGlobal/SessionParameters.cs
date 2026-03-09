using System;
using System.Collections.Generic;
using static MediaOrganizerApp.Application.Enums.AppDefinitions;

namespace MediaOrganizerApp.Application.AppGlobal
{
    public static class SessionParameters
    {
        public static bool IsAdmin { get; set; }
        public static bool IsFirstTime { get; set; }
        public static string FormName { get; set; }
        public static TipoPesquisa OptPesquisa { get; set; }
        public static int CriterioPesquisa { get; set; }
        public static int IndiceTipo { get; set; }
        public static int IndiceCriterio { get; set; }
        public static bool CalledFromBooks { get; set; }
        public static int ParamCode { get; set; } = 0;

        public static string PathVideo { get; set; }
        public static string UrlToSurf { get; set; }
        public static bool IsVideo { get; set; }
        public static string MediaToPlay { get; set; } = "";
        public static string MediaTitle { get; set; } = "";
        public static string StringParam { get; set; }
        public static OpcaoSeguranca OpcaoSeguranca { get; set; }
        public static string NomeFicheiro { get; set; }
        public static List<string> ListParam { get; set; }

        private static int _userId = 0;
        /// <summary>
        /// Gets or Sets UserId
        /// </summary>
        public static int UserID
        {
            get 
            {
                return _userId;
            }
            set 
            {
                _userId = value;
            }
        }

        private static UserRole _userRole;

        /// <summary>
        /// Gets or Sets UserRole
        /// </summary>
        public static UserRole UserRole
        {
            get
            {
                return _userRole;
            }
            set
            {
                _userRole = value;
            }
        }


        private static string _userName = String.Empty;

        /// <summary>
        /// Gets or Sets UserName
        /// </summary>
        public static string UserName
        {
            get 
            {
                return _userName;
            }
            set 
            {
                _userName = value;                
            }
        }
    }
}
