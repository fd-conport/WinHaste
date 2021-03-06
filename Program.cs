﻿using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WinHaste
{
    class Program
    {
        private static readonly Regex _HasteKeyRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}", RegexOptions.Compiled);

        [STAThread]
        static void Main(string[] args)
        {
            var parameters = new Parameters(args);
            var parseResult = parameters.Parse();
            if (parseResult != Parameters.ParseResult.Success)
            {
                Console.WriteLine(parameters.Usage);
                Console.WriteLine(Environment.NewLine + "ERROR: " + parseResult);
                return;
            }

            string haste;
            if (!String.IsNullOrEmpty(parameters.Filename))
            {
                haste = File.ReadAllText(parameters.Filename);
            }
            else
            {
                bool piped = IsInputPiped();

                haste = String.Empty;

                int consoleKey = Console.Read();
                while (consoleKey != -1)
                {
                    var consoleChar = Convert.ToChar(consoleKey);

                    if (piped)
                    {
                        Console.Write(consoleChar);
                        Console.Out.Flush();
                    }

                    haste += consoleChar;

                    consoleKey = Console.Read();
                }
            }

            using (var client = new WebClient())
            {
                var response = client.UploadString(parameters.Url + "/documents", haste);
                var match = _HasteKeyRegex.Match(response);

                if (!match.Success)
                {
                    Console.WriteLine(response);
                    return;
                }

                string hasteUrl = String.Concat(parameters.Url, "/", match.Groups["key"]);

                Clipboard.SetText(hasteUrl);
                Console.WriteLine("\r\nHaste URL: {0}  (copied to clipboard)", hasteUrl);
            }
        }

        private static bool IsInputPiped()
        {
            try
            {
                var tmp = Console.KeyAvailable;
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }
    }
}
