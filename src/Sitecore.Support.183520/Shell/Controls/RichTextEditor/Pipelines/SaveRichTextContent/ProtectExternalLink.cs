namespace Sitecore.Support.Shell.Controls.RichTextEditor.Pipelines.SaveRichTextContent
{
    using Sitecore.Configuration;
    using Sitecore.Resources.Media;
    using Sitecore.Web;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Sitecore.Shell.Controls.RichTextEditor.Pipelines.SaveRichTextContent;


    public class ProtectExternalLink : Sitecore.Shell.Controls.RichTextEditor.Pipelines.SaveRichTextContent.ProtectExternalLink
    {
        public new void Process(SaveRichTextContentArgs args)
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
                        #region --------- OLD CODE -----------------

                        //args.Content = this.GetProtectedHtml(args, args.Content.IndexOf(" ", match.Index, StringComparison.InvariantCultureIgnoreCase), match.Length);

                        #endregion --------- OLD CODE -----------------

                        #region --------- NEW CODE -----------------

                        string result = this.GetProtectedHtml(args, value);
                        args.Content = args.Content.Replace(value, result);

                        #endregion --------- NEW CODE -----------------


                    }
                }
            }
        }

        #region ------ OLD GetProtectedHtml method  ------

        //protected virtual string GetProtectedHtml(SaveRichTextContentArgs args, int startWriteIndex, int length)
        //{
        //    string text = args.Content.Substring(startWriteIndex, length);
        //    if (!text.Contains("rel=\"noopener noreferrer\""))
        //    {
        //        return args.Content.Substring(0, startWriteIndex + 1) + "rel=\"noopener noreferrer\"" + args.Content.Substring(startWriteIndex);
        //    }
        //    return args.Content;
        //}

        #endregion ------ OLD GetProtectedHtml method  ------

        #region ------ NEW GetProtectedHtml method  ------

        protected string GetProtectedHtml(SaveRichTextContentArgs args, string link)
        {
            if (!link.Contains("rel=\"noopener noreferrer\""))
            {
                return link.Insert(link.IndexOf(" "), " rel=\"noopener noreferrer\" ");
            }
            return link;
        }

        #endregion ------ NEW GetProtectedHtml method  ------
    }
}
