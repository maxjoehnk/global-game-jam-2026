using Godot;
using System;
using System.Linq;

public partial class GlobalAudioPlayback : Node
{
    public static GlobalAudioPlayback Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
    }

    public void StopAllPlayback()
    {
        foreach (AudioStreamPlayer audioStreamPlayer in this.GetChildren().Cast<AudioStreamPlayer>())
        {
            audioStreamPlayer.Stop();
        }
    }

    public void StartAllPlayback()
    {
        foreach (AudioStreamPlayer audioStreamPlayer in this.GetChildren().Cast<AudioStreamPlayer>())
        {
            if (audioStreamPlayer.Playing)
            {
                continue;
            }
            audioStreamPlayer.Play();
        }
    }
}
