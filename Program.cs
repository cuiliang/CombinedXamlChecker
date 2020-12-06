using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CombinedXamlChecker
{
    class Program
    {
        /// <summary>
        /// Check combined xaml file to find resources defined later than usage
        /// </summary>
        /// <param name="args"></param>
        static int Main(string[] args)
        {

            string fileName = String.Empty;

            if (args.Length < 1)
            {
#if DEBUG
                // for test
                fileName = @"D:\Github\HandyControl\src\Shared\HandyControl_Shared\Themes\Theme.xaml";
#else
                Console.Error.WriteLine("Tell me which file to check.");
                return 1;
#endif
            }
            else
            {
                fileName = args[0];
            }


            var path = GetFilePath(fileName);
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"File not exists: {path}");
                return 1;
            }




            int rtn = CheckContent(path);


            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return rtn;
        }

        private static int CheckContent(string path)
        {
            var content = File.ReadAllText(path);

            var reg = new Regex(@"\{(StaticResource|DynamicResource) ([^}]*)\}", RegexOptions.Compiled);

            var matches = reg.Matches(content);

            Console.WriteLine($"Found resource key usage count: {matches.Count}");
            Console.WriteLine("Searching problem keys....");

            IDictionary<string, KeyProblemInfo> problemKeys = new Dictionary<string, KeyProblemInfo>();

            foreach (Match match in matches)
            {
                var key = match.Groups[2].Value;

                if (problemKeys.ContainsKey(key))
                {
                    // skip duplicated keys
                    continue;
                }

                var definePos = content.IndexOf($"x:Key=\"{key}\"");
                if (definePos < 0)
                {
                    // key not defined
                    problemKeys.Add(key, new KeyProblemInfo()
                    {
                        Problem = "Key not defined",
                        UsagePos = match.Index,
                        DefinePos = -1
                    });
                }else if (definePos > match.Index)
                {
                    // key defined later than use
                    problemKeys.Add(key, new KeyProblemInfo()
                    {
                        Problem = "Defined later than usage",
                        UsagePos = match.Index,
                        DefinePos = definePos
                    });
                }
            }

            Console.WriteLine($"Total problem keys: {problemKeys.Count}");
            foreach (var problemKey in problemKeys)
            {
                Console.WriteLine($"{problemKey.Key} : {problemKey.Value.Problem}");
            }

            return 0;
        }




        /// <summary>
        /// Get full path of xaml file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static string GetFilePath(string filename)
        {
            // if providing full path
            if (File.Exists(filename))
            {
                return filename;
            }
            else
            {
                return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().FullName),
                    filename
                );
            }
        }
    }

    public class KeyProblemInfo
    {
        /// <summary>
        /// problem info
        /// </summary>
        public string Problem { get; set; }

        /// <summary>
        /// first usage position
        /// </summary>
        public int  UsagePos { get; set; }

        /// <summary>
        /// x:Key define position
        /// </summary>
        public int  DefinePos { get; set; }
    }
}
