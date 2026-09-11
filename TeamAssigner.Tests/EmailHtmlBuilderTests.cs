namespace TeamAssigner.Tests
{
    using TeamAssigner.Models;
    using TeamAssigner.Services;
    using Xunit;

    public class EmailHtmlBuilderTests
    {
        static readonly AssignmentRow[] SampleRows =
        [
            new() { PlayerName = "Ada", Team1 = "Packers", Team2 = "Bears", Team1IsBye = true },
            new() { PlayerName = "Grace", Team1 = "Chiefs", Team2 = "Eagles" }
        ];

        static readonly Quote SampleQuote = new()
        {
            text = "A house divided against itself cannot stand.",
            author = "Abraham Lincoln"
        };

        [Fact]
        public void Week1EmailIncludesHeadingQuoteAndFooterWithoutLastWeekScoreboard()
        {
            string extra = EmailHtmlBuilder.BuildQuoteBlock(SampleQuote);
            string html = EmailHtmlBuilder.BuildEmail(1, SampleRows, extra);

            Assert.Contains("Week 1 Team Assignments", html);
            Assert.Contains("A house divided against itself cannot stand.", html);
            Assert.Contains("Abraham Lincoln", html);
            Assert.Contains("Sent by Team Assigner", html);
            Assert.DoesNotContain("No team had 33 points last week.", html);
            Assert.DoesNotContain("Previous-week scores were unavailable", html);
            Assert.DoesNotContain("<style", html, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void LaterWeekWithNoThirtyThreeIncludesNoteAndQuote()
        {
            string extra = EmailHtmlBuilder.BuildNote("No team had 33 points last week.")
                + EmailHtmlBuilder.BuildQuoteBlock(SampleQuote);
            string html = EmailHtmlBuilder.BuildEmail(2, SampleRows, extra);

            Assert.Contains("Week 2 Team Assignments", html);
            Assert.Contains("No team had 33 points last week.", html);
            Assert.Contains("Abraham Lincoln", html);
        }

        [Fact]
        public void UnavailableScoresIncludeNoteAndQuote()
        {
            string extra = EmailHtmlBuilder.BuildNote("Previous-week scores were unavailable this run.")
                + EmailHtmlBuilder.BuildQuoteBlock(SampleQuote);

            Assert.Contains("Previous-week scores were unavailable this run.", extra);
            Assert.Contains("font-style:italic", extra);
            Assert.Contains("Abraham Lincoln", extra);
        }

        [Fact]
        public void ThirtyThreeCongratulationsOmitsQuote()
        {
            string extra = EmailHtmlBuilder.BuildCongratulations("Green Bay Packers", "Packers win in overtime");
            string html = EmailHtmlBuilder.BuildEmail(3, SampleRows, extra);

            Assert.Contains("Congratulations to the player who had the Green Bay Packers last week.", html);
            Assert.Contains("Packers win in overtime", html);
            Assert.DoesNotContain("Abraham Lincoln", html);
            Assert.DoesNotContain("No team had 33 points last week.", html);
        }

        [Fact]
        public void AssignmentTableUsesInlineStylesByeHighlightAndZebraRows()
        {
            string table = EmailHtmlBuilder.BuildAssignmentTable(SampleRows);

            Assert.Contains(">Player<", table);
            Assert.Contains(">Team 1<", table);
            Assert.Contains(">Team 2<", table);
            Assert.Contains("Packers", table);
            Assert.Contains("Bye this week", table);
            Assert.Contains("#ffe08a", table);
            Assert.Contains("#1e3a5f", table);
            Assert.Contains("padding:10px 12px", table);
            Assert.Contains("#f4f7fb", table);
            Assert.DoesNotContain("lightgray", table);
        }

        [Fact]
        public void EmailUsesInlineStylesAndSixHundredPixelWrapper()
        {
            string html = EmailHtmlBuilder.BuildEmail(1, SampleRows, EmailHtmlBuilder.BuildQuoteBlock(SampleQuote));

            Assert.Contains("max-width:600px", html);
            Assert.Contains("font-family:Arial, Helvetica, sans-serif", html);
            Assert.Contains("padding:28px 24px 24px 24px", html);
            Assert.DoesNotContain("<style", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("<link", html, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("src=", html, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void HtmlEncodesPlayerNamesAndQuotes()
        {
            var rows = new[]
            {
                new AssignmentRow { PlayerName = "<script>alert(1)</script>", Team1 = "49ers", Team2 = "Bears & Birds" }
            };
            var quote = new Quote { text = "Less than < three", author = "O'Malley" };
            string html = EmailHtmlBuilder.BuildEmail(1, rows, EmailHtmlBuilder.BuildQuoteBlock(quote));

            Assert.DoesNotContain("<script>alert(1)</script>", html);
            Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", html);
            Assert.Contains("Bears &amp; Birds", html);
            Assert.Contains("Less than &lt; three", html);
            Assert.Contains("O&#39;Malley", html);
        }

        [Fact]
        public void MissingQuoteRendersNothing()
        {
            Assert.Equal(string.Empty, EmailHtmlBuilder.BuildQuoteBlock(null));
            Assert.Equal(string.Empty, EmailHtmlBuilder.BuildQuoteBlock(new Quote { text = "  ", author = "X" }));
        }
    }
}
