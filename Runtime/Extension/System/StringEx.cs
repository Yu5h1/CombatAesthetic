
using System;

namespace Yu5h1Lib
{
	public static class StringEx {
		public static bool EqualAny(this string s,params string[] values)
			=> s.EqualAny(StringComparison.CurrentCulture, values);
		public static bool EqualAny(this string s,StringComparison comparison, params string[] values)
		{
			foreach (var item in values)
				if (s.Equals(item, comparison))
					return true;
			return false;
		}
	}
    public enum StringSearchOption
    {
        Equals,
        StartsWith,
        Contains,
        EndsWith
    }
}