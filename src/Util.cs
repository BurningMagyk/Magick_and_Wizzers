namespace Main {
    public static class Util {
        private static readonly System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
        public static string ToTitleCase(string s) {
            return textInfo.ToTitleCase(s.ToLower());
        }
    }
}