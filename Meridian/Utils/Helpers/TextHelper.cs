using System;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Documents;

namespace Meridian.Utils.Helpers
{
    public static class TextHelper
    {
        private static Regex _linksRegex = new Regex(@"(?:(?:http|https):\/\/)?([-a-zA-Z0-9а-яА-Я.]{2,256}\.[a-zа-я]{2,4})\b(?:\/[-a-zA-Z0-9а-яА-Я@:%_\+.~#?&//=]*)?", RegexOptions.Compiled);
        private static Regex _hashtagsRegex = new Regex("(#[a-zA-Zа-яА-Я0-9@_]+)", RegexOptions.Compiled);
        private static Regex _internalLinksRegex = new Regex(@"\[(club|id).*?\]", RegexOptions.Compiled);

        public static Paragraph ParseHyperlinks(string text, UnderlineStyle underlineStyle = UnderlineStyle.None)
        {
            var paragraph = new Paragraph();

            if (string.IsNullOrEmpty(text))
                return paragraph;

            int index = 0;

            var linksMatches = _linksRegex.Matches(text);

            foreach (Match match in linksMatches)
            {
                int matchIndex = match.Index;

                //Add text before link.
                var beforeText = text.Substring(index, matchIndex - index);
                var run = new Run() { Text = beforeText };
                paragraph.Inlines.Add(run);

                // Add link.
                string linkString = match.Value;

                var hyperlink = new Hyperlink();
                hyperlink.UnderlineStyle = underlineStyle;
                run = new Run() { Text = linkString };
                hyperlink.Inlines.Add(run);

                // Complete link if necessary.
                if (!linkString.StartsWith("http"))
                {
                    linkString = @"http://" + linkString;
                }

                if (Uri.IsWellFormedUriString(linkString, UriKind.Absolute))
                {
                    hyperlink.NavigateUri = new Uri(linkString);
                    paragraph.Inlines.Add(hyperlink);

                    index = matchIndex + match.Length;
                }
            }

            //Add text after all links.
            var afterText = text.Substring(index, text.Length - index);
            var afterRun = new Run() { Text = afterText };
            paragraph.Inlines.Add(afterRun);

            return paragraph;
        }

        public static void ParseHashtags(Paragraph paragraph, UnderlineStyle underlineStyle = UnderlineStyle.None)
        {
            int i = 0;

            while (i < paragraph.Inlines.Count)
            {
                var inline = paragraph.Inlines[i];
                if (!(inline is Run))
                {
                    i++;
                    continue;
                }

                var content = ((Run)inline).Text;

                var regex = _hashtagsRegex;

                var matches = regex.Matches(content);

                if (matches.Count > 0)
                {

                    int index = 0;


                    foreach (Match match in matches)
                    {
                        paragraph.Inlines.Remove(inline);

                        int matchIndex = match.Index;

                        //Add text before link.
                        var beforeText = content.Substring(index, matchIndex - index);
                        var run = new Run() { Text = beforeText };
                        paragraph.Inlines.Insert(i, run);

                        i++;

                        // Add link.
                        string linkString = match.Value;

                        var hyperlink = new Hyperlink();
                        hyperlink.UnderlineStyle = underlineStyle;
                        run = new Run() { Text = linkString };
                        hyperlink.Inlines.Add(run);

                        // Complete link if necessary.
                        linkString = $"https://vk.com/feed?section=search&q={linkString}";

                        if (Uri.IsWellFormedUriString(linkString, UriKind.Absolute))
                        {
                            hyperlink.NavigateUri = new Uri(linkString);
                            paragraph.Inlines.Insert(i, hyperlink);

                            index = matchIndex + match.Length;

                            i++;
                        }
                    }

                    //Add text after all links.
                    var afterText = content.Substring(index, content.Length - index);
                    var afterRun = new Run() { Text = afterText };
                    paragraph.Inlines.Insert(i, afterRun);
                }

                i++;
            }
        }

        public static void ParseInternalLinks(Paragraph paragraph, UnderlineStyle underlineStyle = UnderlineStyle.None)
        {
            int i = 0;

            while (i < paragraph.Inlines.Count)
            {
                var inline = paragraph.Inlines[i];
                if (!(inline is Run))
                {
                    i++;
                    continue;
                }

                var content = ((Run)inline).Text;

                var regex = _internalLinksRegex;

                var matches = regex.Matches(content);

                if (matches.Count > 0)
                {
                    int index = 0;

                    foreach (Match match in matches)
                    {
                        paragraph.Inlines.Remove(inline);

                        int matchIndex = match.Index;

                        //Add text before link.
                        var beforeText = content.Substring(index, matchIndex - index);
                        var run = new Run() { Text = beforeText };
                        paragraph.Inlines.Insert(i, run);

                        i++;

                        // Add link.
                        string linkString = match.Value.Trim(new char[] { '[', ']' });
                        string title = linkString;

                        if (linkString.Contains("|"))
                        {
                            title = linkString.Substring(linkString.IndexOf("|") + 1);
                            linkString = linkString.Substring(0, linkString.Length - title.Length - 1);
                        }

                        var hyperlink = new Hyperlink();
                        hyperlink.UnderlineStyle = underlineStyle;
                        run = new Run() { Text = title };
                        hyperlink.Inlines.Add(run);

                        // Complete link if necessary.
                        linkString = $"https://vk.com/{linkString}";

                        if (Uri.IsWellFormedUriString(linkString, UriKind.Absolute))
                        {
                            hyperlink.NavigateUri = new Uri(linkString);
                            paragraph.Inlines.Insert(i, hyperlink);

                            index = matchIndex + match.Length;

                            i++;
                        }
                    }

                    //Add text after all links.
                    var afterText = content.Substring(index, content.Length - index);
                    var afterRun = new Run() { Text = afterText };
                    paragraph.Inlines.Insert(i, afterRun);
                }

                i++;
            }
        }
    }
}
