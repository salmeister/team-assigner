namespace TeamAssigner.Services
{
    using System.Net;
    using System.Text;
    using TeamAssigner.Models;

    public static class EmailHtmlBuilder
    {
        const string FontStack = "Arial, Helvetica, sans-serif";
        const string Navy = "#1e3a5f";
        const string ByeBackground = "#ffe08a";
        const string ByeText = "#5c4300";
        const string Border = "#d0d7e2";
        const string Zebra = "#f4f7fb";
        const string Muted = "#6b7c93";

        public static string BuildEmail(int week, IReadOnlyList<AssignmentRow> rows, string extraSectionHtml)
        {
            string heading = $"Week {week} Team Assignments";
            string table = BuildAssignmentTable(rows);
            string extra = extraSectionHtml ?? "";

            return $"""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1">
                  <title>{Encode(heading)}</title>
                </head>
                <body style="margin:0;padding:0;background-color:#eef1f6;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#eef1f6" style="background-color:#eef1f6;margin:0;padding:0;">
                    <tr>
                      <td align="center" style="padding:24px 12px;">
                        <table role="presentation" width="600" cellpadding="0" cellspacing="0" border="0" bgcolor="#ffffff" style="max-width:600px;width:100%;background-color:#ffffff;font-family:{FontStack};color:#1a1a1a;">
                          <tr>
                            <td style="padding:28px 24px 24px 24px;font-family:{FontStack};">
                              <p style="margin:0 0 4px 0;font-size:12px;letter-spacing:0.08em;text-transform:uppercase;color:{Muted};font-family:{FontStack};">NFL Team Assigner</p>
                              <h1 style="margin:0 0 20px 0;font-size:22px;line-height:1.3;color:{Navy};font-weight:bold;font-family:{FontStack};">{Encode(heading)}</h1>
                              {table}
                              {extra}
                              <p style="margin:28px 0 0 0;padding-top:16px;border-top:1px solid #e4e9f0;font-size:12px;color:{Muted};font-family:{FontStack};">Sent by Team Assigner</p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
        }

        public static string BuildAssignmentTable(IReadOnlyList<AssignmentRow> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse;width:100%;font-family:{FontStack};font-size:14px;\">");
            sb.AppendLine("<tr>");
            sb.AppendLine(HeaderCell("Player"));
            sb.AppendLine(HeaderCell("Team 1"));
            sb.AppendLine(HeaderCell("Team 2"));
            sb.AppendLine("</tr>");

            for (int i = 0; i < rows.Count; i++)
            {
                AssignmentRow row = rows[i];
                string rowBg = i % 2 == 0 ? "#ffffff" : Zebra;
                sb.AppendLine("<tr>");
                sb.AppendLine(BodyCell(row.PlayerName, rowBg, bold: true));
                sb.AppendLine(row.Team1IsBye ? ByeCell(row.Team1) : BodyCell(row.Team1, rowBg));
                sb.AppendLine(BodyCell(row.Team2, rowBg));
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            return sb.ToString();
        }

        public static string BuildQuoteBlock(Quote? quote)
        {
            if (quote == null || string.IsNullOrWhiteSpace(quote.text))
            {
                return string.Empty;
            }

            string author = string.IsNullOrWhiteSpace(quote.author) ? "Unknown" : quote.author.Trim();
            return $"""
                <table width="100%" cellpadding="0" cellspacing="0" border="0" style="margin:20px 0 0 0;">
                  <tr>
                    <td bgcolor="#f4f6f9" style="padding:16px 18px;background-color:#f4f6f9;border-left:4px solid {Muted};color:#4a5568;font-family:Georgia, 'Times New Roman', Times, serif;">
                      <p style="margin:0;font-style:italic;font-size:15px;line-height:1.5;color:#4a5568;">“{Encode(quote.text.Trim())}”</p>
                      <p style="margin:10px 0 0 0;font-size:13px;font-style:normal;color:{Muted};font-family:{FontStack};">— {Encode(author)}</p>
                    </td>
                  </tr>
                </table>
                """;
        }

        public static string BuildNote(string message)
        {
            return $"""
                <p style="margin:20px 0 0 0;font-size:14px;line-height:1.5;color:#334155;font-family:{FontStack};">{Encode(message)}</p>
                """;
        }

        public static string BuildCongratulations(string teamDisplayName, string? headline)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<p style=\"margin:20px 0 0 0;font-size:14px;line-height:1.5;color:{Navy};font-weight:bold;font-family:{FontStack};\">Congratulations to the player who had the {Encode(teamDisplayName)} last week.</p>");
            if (!string.IsNullOrWhiteSpace(headline))
            {
                sb.AppendLine($"<p style=\"margin:8px 0 0 0;font-size:14px;font-style:italic;color:#4a5568;font-family:{FontStack};\">{Encode(headline)}</p>");
            }
            return sb.ToString();
        }

        static string HeaderCell(string text) =>
            $"<th align=\"left\" bgcolor=\"{Navy}\" style=\"background-color:{Navy};color:#ffffff;padding:10px 12px;border:1px solid {Navy};font-family:{FontStack};font-size:13px;font-weight:bold;\">{Encode(text)}</th>";

        static string BodyCell(string text, string background, bool bold = false)
        {
            string weight = bold ? "font-weight:bold;" : "font-weight:normal;";
            return $"<td bgcolor=\"{background}\" style=\"padding:10px 12px;border:1px solid {Border};background-color:{background};color:#1a1a1a;font-family:{FontStack};{weight}\">{Encode(text)}</td>";
        }

        static string ByeCell(string teamName)
        {
            return $"<td bgcolor=\"{ByeBackground}\" style=\"padding:10px 12px;border:1px solid #e0b84a;background-color:{ByeBackground};color:{ByeText};font-family:{FontStack};font-weight:bold;\">{Encode(teamName)}<br><span style=\"font-size:11px;font-weight:normal;color:{ByeText};\">Bye this week</span></td>";
        }

        static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
    }
}
