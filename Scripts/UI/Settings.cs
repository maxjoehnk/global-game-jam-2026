using GlobalGameJam.Scripts.Core;
using Godot;

public partial class Settings : Panel
{
	private Slider MainVolumeSlider => this.FindSlider("MainVolume");
	private Slider EffectsVolumeSlider => this.FindSlider("EffectsVolume");
	private Slider QuotesVolumeSlider => this.FindSlider("QuotesVolume");
	private Slider MusicVolumeSlider => this.FindSlider("MusicVolume");

	private Slider FindSlider(string name)
	{
		return (this.FindChild(name) as Slider)!;
	}
	
	public override void _Ready()
	{
		this.MainVolumeSlider.GrabFocus();
		this.MainVolumeSlider.Value = SettingsManager.MainVolume;
		this.MainVolumeSlider.ValueChanged += this.OnMainVolumeChanged;
		this.EffectsVolumeSlider.Value = SettingsManager.EffectsVolume;
		this.EffectsVolumeSlider.ValueChanged += this.OnEffectsVolumeChanged;
		this.MusicVolumeSlider.Value = SettingsManager.MusicVolume;
		this.MusicVolumeSlider.ValueChanged += this.OnMusicVolumeChanged;
		this.QuotesVolumeSlider.Value = SettingsManager.QuotesVolume;
		this.QuotesVolumeSlider.ValueChanged += this.OnQuotesVolumeChanged;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressedByEvent(InputAction.Cancel, @event))
		{
			this.OnBackPressed();
		}
	}

	private void OnMainVolumeChanged(double value)
	{
		SettingsManager.MainVolume = value;
	}
	
	private void OnEffectsVolumeChanged(double value)
	{
		SettingsManager.EffectsVolume = value;
	}
	
	private void OnMusicVolumeChanged(double value)
	{
		SettingsManager.MusicVolume = value;
	}
	
	private void OnQuotesVolumeChanged(double value)
	{
		SettingsManager.QuotesVolume = value;
	}

	public void OnClearUserData()
	{
		UserDataManager.ClearUserData();
		SceneManager.Instance.ReloadUserData();
		SceneManager.Instance.OpenMainMenu();
	}
	
	public void OnSaveSettings()
	{
		SettingsManager.SaveSettings();
		SceneManager.Instance.OpenMainMenu();
	}

	public void OnBackPressed()
	{
		SettingsManager.LoadSettings();
		SceneManager.Instance.OpenMainMenu();
	}
}
