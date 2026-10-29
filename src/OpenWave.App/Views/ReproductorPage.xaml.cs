using OpenWave.App.Services;
using OpenWave.App.ViewModels;

namespace OpenWave.App.Views;

public partial class ReproductorPage : ContentPage
{
    private readonly MediaElementAudioPlayer _audioPlayer;

    public ReproductorPage(ReproductorViewModel viewModel, MediaElementAudioPlayer audioPlayer)
    {
        InitializeComponent();
        _audioPlayer = audioPlayer;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _audioPlayer.Attach(Player);
    }

    // Playback is scoped to this page: leaving it stops the audio.
    protected override void OnDisappearing()
    {
        _audioPlayer.Detach(Player);
        base.OnDisappearing();
    }
}
