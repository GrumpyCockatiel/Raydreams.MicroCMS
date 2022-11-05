using System;
using Markdig;

namespace Raydreams.MicroCMS
{
    /// <summary>The only place a Markdown conveter dep should be referenced</summary>
    public static class MarkdownEngine
    {
        public static string Markdown2HTML( string md )
        {
            if ( String.IsNullOrWhiteSpace( md ) )
                return "<p>&nbsp;</p>";

            MarkdownPipeline pipe = new MarkdownPipelineBuilder().UseSoftlineBreakAsHardlineBreak().Build();
            string html = Markdown.ToHtml( md, pipe );

            return html;
        }

        public static string Null( string md )
        {
            return "<h1>Markdown not supported!</h1>";
        }
    }

}

