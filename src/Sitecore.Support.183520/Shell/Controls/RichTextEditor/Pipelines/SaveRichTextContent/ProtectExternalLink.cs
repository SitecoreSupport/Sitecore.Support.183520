using Sitecore.Configuration;
using Sitecore.Resources.Media;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitecore.Shell.Controls.RichTextEditor.Pipelines.SaveRichTextContent
{
    /// <summary>
    /// Protect external link a tag from security vulnerability
    /// </summary>
    public class ProtectExternalLink
    {
        private const string HrefRegex = "href[ ]*=[ ]*[\"']([\\w:\\/\\.\\~\\-\\?\\&\\=\\;]+)[\"]";

        private const string ATagRegex = "(i?)<a([^>]+)>(.+?)</a>";

        private const string ProtectionTag = "rel=\"noopener noreferrer\"";

        /// <summary>
        /// Process
        /// </summary>
        /// <param name="args"></param>
        public void Process(SaveRichTextContentArgs args)
        {
            if (string.IsNullOrEmpty(args.Content))
            {
                return;
            }
            if (!Settings.ProtectExternalLinksWithBlankTarget)
            {
                return;
            }
            MatchCollection aTagMatchCollection = this.GetATagMatchCollection(args);
            foreach (Match match in aTagMatchCollection)
            {
                if (match.Success)
                {
                    string value = match.Value;
                    if (!string.IsNullOrEmpty(value) && value.Contains("_blank") && !this.IsInternalLink(value))
                    {
                        args.Content = this.GetProtectedHtml(args, args.Content.IndexOf(" ", match.Index, StringComparison.InvariantCultureIgnoreCase), match.Length);
                    }
                }
            }
        }

        /// <summary>
        /// Is Internal Link
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected virtual bool IsInternalLink(string html)
        {
            string hrefValue = this.GetHref(html);
            bool flag = this.GetMediaPrefixes().Any((string m) => hrefValue.StartsWith(m)) || hrefValue.StartsWith("~/link.aspx?") || hrefValue.StartsWith("/") || hrefValue.StartsWith("~") || hrefValue.StartsWith("-");
            if (flag)
            {
                return true;
            }
            string serverUrl = WebUtil.GetServerUrl(false);
            return !string.IsNullOrEmpty(serverUrl) && html.Contains(serverUrl);
        }

        /// <summary>
        /// Get the matches of Html A tag
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual MatchCollection GetATagMatchCollection(SaveRichTextContentArgs args)
        {
            Regex regex = new Regex("(i?)<a([^>]+)>(.+?)</a>", RegexOptions.ECMAScript);
            return regex.Matches(args.Content, 0);
        }

        /// <summary>
        /// Get Href from a tag / node
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected virtual string GetHref(string html)
        {
            Regex regex = new Regex("href[ ]*=[ ]*[\"']([\\w:\\/\\.\\~\\-\\?\\&\\=\\;]+)[\"]", RegexOptions.ECMAScript);
            Match match = regex.Match(html);
            if (!match.Success || match.Groups.Count <= 0)
            {
                return string.Empty;
            }
            if (match.Groups[1] == null)
            {
                return match.Groups[0].Value;
            }
            return match.Groups[1].Value;
        }

        /// <summary>
        /// Get Protected Html
        /// </summary>
        /// <param name="args"></param>
        /// <param name="startWriteIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected virtual string GetProtectedHtml(SaveRichTextContentArgs args, int startWriteIndex, int length)
        {
            string text = args.Content.Substring(startWriteIndex, length);
            if (!text.Contains("rel=\"noopener noreferrer\""))
            {
                return args.Content.Substring(0, startWriteIndex + 1) + "rel=\"noopener noreferrer\"" + args.Content.Substring(startWriteIndex);
            }
            return args.Content;
        }

        /// <summary>
        /// Get Media Prefixes
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> GetMediaPrefixes()
        {
            return MediaManager.Config.MediaPrefixes;
        }
    }
}
