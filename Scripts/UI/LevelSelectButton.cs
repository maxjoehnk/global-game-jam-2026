using Godot;

public partial class LevelSelectButton : Button
{
    private Label HighScoreLabel => this.GetNode<Label>("HighScoreContainer/HighScore");

    public bool IsTestLevel
    {
        set
        {
            if (value)
            {
                this.AddThemeColorOverride("font_color", Colors.Red);
                this.AddThemeColorOverride("font_hover_color", Colors.Red);
            }
            else
            {
                this.RemoveThemeColorOverride("font_color");
                this.RemoveThemeColorOverride("font_hover_color");
            }
        }
    }

    public double? HighScore
    {
        set
        {
            this.GetNode<Control>("HighScoreContainer").Visible = value != null;
            if (value != null)
            {
                long minutes = (long)value / 60;
                long seconds = (long)value % 60;

                if (minutes > 99) minutes = 99;

                this.HighScoreLabel.Text = $"{minutes:D2}:{seconds:D2}";
            }
        }
    }
}