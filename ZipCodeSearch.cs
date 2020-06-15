using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Yubin.Properties;

namespace Yubin
{
    public class ZipCodeSearch
    {
        /// <summary>
        /// </summary>
        /// <param name="code">5440013</param>
        /// <returns>大阪府 大阪市生野区 巽中</returns>
        public string Lookup(string code)
        {
            CodeToAddressDictLazy.Value.TryGetValue(NormalizeCode(code), out string text);
            return text;
        }

        private string NormalizeCode(string code)
        {
            code = code
                .Replace("０", "0")
                .Replace("１", "1")
                .Replace("２", "2")
                .Replace("３", "3")
                .Replace("４", "4")
                .Replace("５", "5")
                .Replace("６", "6")
                .Replace("７", "7")
                .Replace("８", "8")
                .Replace("９", "9")
                ;
            return Regex.Replace(code, "[^0-9]+", "");
        }

        public static readonly Lazy<string> KenAllCsvLazy = new Lazy<string>(() => KenAllCsv());

        private static readonly Lazy<IDictionary<string, string>> CodeToAddressDictLazy = new Lazy<IDictionary<string, string>>(
            () =>
            {
                SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
                var rows = KenAllCsvLazy.Value.Replace("\"", "").Replace("\r\n", "\n").Split('\n');
                foreach (var row in rows)
                {
                    var cols = row.Split(',');
                    if (cols.Length >= 9)
                    {
                        dict[cols[2]] = $"{cols[6]} {cols[7]} {Normalize8(cols[8])}";
                    }
                }
                return dict;
            }
        );

        private static object Normalize8(string v)
        {
            return v
                .Replace("以下に掲載がない場合", "")
                .Split('（')[0];
        }

        public static string KenAllCsv()
        {
            MemoryStream output = new MemoryStream(15 * 1024 * 1024);
            using (var input = new GZipStream(new MemoryStream(Resources.KEN_ALL_CSV, false), CompressionMode.Decompress))
            {
                input.CopyTo(output);
            }
            output.Position = 0;
            return Encoding.GetEncoding(932).GetString(output.ToArray());
        }
    }
}
