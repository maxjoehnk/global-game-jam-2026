using Godot;
using System;
using System.Text;
using HtmlAgilityPack;
using System.Collections.Generic;

public partial class JensSpahnFordert : Control
{
    private HttpRequest Request => GetNode<HttpRequest>("HTTPRequest");
    private Timer Timer => GetNode<Timer>("NextQuoteTimer");
    
    private Label QuoteLabel => GetNode<Label>("MarginContainer/VBoxContainer/Quote");
    private Label DateLabel => GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/Date");
    private LinkButton SourceButton => GetNode<LinkButton>("MarginContainer/VBoxContainer/HBoxContainer/Source");

    private Quote[] Quotes = [];

    [Signal]
    public delegate void NextQuoteEventHandler(string quote, string source);
    
    public override void _Ready()
    {
        this.Visible = false;
        this.Request.RequestCompleted += ((result, code, headers, body) => this.ParseQuotes(body));
        this.Request.Request("https://jens-spahn-fordert.de");
        this.Timer.Timeout += this.GetNextQuote;
    }

    private void GetNextQuote()
    {
        if (this.Quotes.Length == 0)
        {
            GD.PrintErr("No quotes available");
            return;
        }

        int nextQuoteIndex = GD.RandRange(0, this.Quotes.Length - 1);
        Quote quote = this.Quotes[nextQuoteIndex];
        this.EmitSignalNextQuote(quote.Text, quote.Source);
        this.SourceButton.Uri = quote.Source;
        this.DateLabel.Text = quote.Date;
        this.QuoteLabel.Text = quote.Text;
    }

    private void ParseQuotes(byte[] body)
    {
        try
        {
            string htmlContent = Encoding.UTF8.GetString(body);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Find all table rows with links
            var tableRows = htmlDoc.DocumentNode
                .SelectNodes("//tr[td/a[@href]]");

            if (tableRows == null)
            {
                GD.PrintErr("No table rows with links found");
                return;
            }

            var quotesList = new List<Quote>();

            foreach (var row in tableRows)
            {
                var linkNode = row.SelectSingleNode(".//a[@href]");
                var dateNode = row.SelectSingleNode(".//td[last()]"); // Assumes date is in the last td

                if (linkNode != null && dateNode != null)
                {
                    string quoteText = linkNode.InnerText?.Trim() ?? "";
                    string sourceUrl = linkNode.GetAttributeValue("href", "");
                    string date = dateNode.InnerText?.Trim() ?? "";

                    if (!string.IsNullOrEmpty(quoteText) && !string.IsNullOrEmpty(sourceUrl))
                    {
                        var quote = new Quote
                        {
                            Text = quoteText,
                            Source = sourceUrl,
                            Date = date
                        };
                        quotesList.Add(quote);
                    }
                }
            }

            this.Quotes = quotesList.ToArray();
            GD.Print($"Parsed {this.Quotes.Length} quotes");

            if (this.Quotes.Length > 0)
            {
                this.StartTimer();
            }
            else
            {
                GD.PrintErr("No valid quotes found");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error parsing quotes: {ex.Message}");
        }
    }

    private void StartTimer()
    {
        this.GetNextQuote();
        this.Visible = true;
        this.Timer.Start();
    }

    class Quote
    {
        public required string Text;
        public required string Source;
        public required string Date;
    }
}