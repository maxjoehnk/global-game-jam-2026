using Godot;

public partial class RandomAudioPlayback : AudioStreamPlayer2D
{
    [Export]
    private AudioStreamPlaylist Playlist { get; set; }

    public void PlayRandomSound()
    {
        int quoteIndex = GD.RandRange(0, this.Playlist.StreamCount - 1);
        this.Stream = this.Playlist.GetListStream(quoteIndex);
        this.Play();
    }
}
