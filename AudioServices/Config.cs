﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public static class Config
    {
        //import dll from use configuration file
        [DllImport("kernel32.dll")]
        static extern uint GetPrivateProfileString(
        string lpAppName,
        string lpKeyName,
        string lpDefault,
        StringBuilder lpReturnedString,
        uint nSize,
        string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName,
           string lpKeyName, string lpString, string lpFileName);

        public static void Set(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Environment.CurrentDirectory + "\\config.ini");
        }

        public static string GetParametr(string par)
        {
            StringBuilder buffer = new StringBuilder(50, 50);

            GetPrivateProfileString("SETTINGS", par, "null", buffer, 50, Environment.CurrentDirectory + "\\config.ini");

            if (buffer.ToString() == "null")
            {
                return "";
            }

            return buffer.ToString();
        }
    }
}
