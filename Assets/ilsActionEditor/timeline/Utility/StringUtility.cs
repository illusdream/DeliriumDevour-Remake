using UnityEngine;

namespace ilsActionEditor
{

    public static class StringUtility
    {

        ///<summary>如名称所示，将camelCase转换为单词。（看不懂啊）</summary>
        public static string SplitCamelCase(this string s) {
            if ( string.IsNullOrEmpty(s) ) return s;
            s = char.ToUpper(s[0]) + s.Substring(1);
            return System.Text.RegularExpressions.Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1").Trim();
        }

        ///<summary>相对项目路径的绝对路径</summary>
        public static string AbsToRelativePath(string absolutepath) {
            if ( absolutepath.StartsWith(Application.dataPath) ) {
                return "Assets" + absolutepath.Substring(Application.dataPath.Length);
            }
            return null;
        }

        ///<summary>Get the string result within first from and last to</summary>
        public static string GetStringWithinOuter(this string input, char from, char to) {
            var start = input.IndexOf(from) + 1;
            var end = input.LastIndexOf(to);
            if ( start < 0 || end < start ) { return null; }
            return input.Substring(start, end - start);
        }
    }
}