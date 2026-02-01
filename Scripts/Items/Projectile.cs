using Godot;

public partial class Projectile : RigidBody2D
{
    private RandomAudioPlayback AudioPlayer => GetNode<RandomAudioPlayback>("RandomAudioPlayback");
    
    [Export]
    private AudioStreamPlaylist Playlist { get; set; }
    
    public override void _Ready()
    {
        this.BodyEntered += body =>
        {
            this.AudioPlayer.PlayRandomSound();
            if (body is Enemy enemy)
            {
                enemy.Hit(this);
                this.QueueFree();
            }
        };
    }
}
