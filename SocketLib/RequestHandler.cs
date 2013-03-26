using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SocketLib
{
    public class RequestHandler
    {
        private string temp = string.Empty;
        public string[] GetActualString(string input)
        {
            return GetActualString(input, null);
        }
        private string[] GetActualString(string input, List<string> outputList)
        {
            if (outputList == null)
                outputList = new List<string>();
            if (!String.IsNullOrEmpty(temp))
                input = temp + input;
            string output = "";
            string pattern = @"(?<=^\[length=)(\d+)(?=\])";
            int length;
            if (Regex.IsMatch(input, pattern))
            {
                Match m = Regex.Match(input, pattern);
                length = Convert.ToInt32(m.Groups[0].Value);
                int startIndex = input.IndexOf(']') + 1;
                output = input.Substring(startIndex);
                if (output.Length == length)
                {
                    outputList.Add(output);
                    temp = "";
                }
                else if (output.Length < length)
                {
                    temp = input;
                }
                else if (output.Length > length)
                {
                    output = output.Substring(0, length);
                    outputList.Add(output);
                    temp = "";
                    input = input.Substring(startIndex + length);
                    GetActualString(input, outputList);
                }
            }
            else
            {
                temp = input;
            }
            return outputList.ToArray();
        }
    }
}
