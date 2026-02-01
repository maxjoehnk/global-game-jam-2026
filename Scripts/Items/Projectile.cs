using Godot;

public partial class Projectile : RigidBody2D
{
    private AudioStreamPlayer2D AudioPlayer => GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    
    [Export]
    private AudioStreamPlaylist Playlist { get; set; }
    
    public override void _Ready()
    {
        this.BodyEntered += body =>
        {
            this.PlayHitEffect();
            if (body is Enemy enemy)
            {
                enemy.Hit(this);
                this.QueueFree();
            }
        };
    }

    private void PlayHitEffect()
    {
        int quoteIndex = GD.RandRange(0, this.Playlist.StreamCount - 1);
        this.AudioPlayer.Stream = this.Playlist.GetListStream(quoteIndex);
        this.AudioPlayer.Play();
    }
}
