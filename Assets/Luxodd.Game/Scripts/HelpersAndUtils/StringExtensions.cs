using System.Linq;
using System.Threading;
using UnityEngine;

namespace Luxodd.Game.Scripts.HelpersAndUtils
{
    public static class StringExtensions
    {
        public static string ToPascalCaseStyle(this string str)
        {
            return string.Concat(
                str.Split('_')
                    .Select(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase)
            );
        }
    }
}
