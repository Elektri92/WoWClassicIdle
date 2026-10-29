// Profession trainer in the subpage header: open by click, speech bubble like the merchant in the inventory
using Models;

namespace WoWClassicIdle.Components.Pages.ProfessionPages;

public partial class ProfessionDetail
{
    private bool _trainerOpen;

    // Speech bubble of the trainer; counter as @key so the animation restarts on every line
    private string? _speech;
    private int _speechKey;

    private ProfessionTrainer? Trainer => _def is null ? null : ProfessionTrainerRegistry.Get(_def.Profession);

    private void ToggleTrainer()
    {
        if (_trainerOpen)
        {
            CloseTrainer();
            return;
        }
        _trainerOpen = true;
        if (Trainer is { } trainer) Say(trainer.GreetingLines);
    }

    private void CloseTrainer()
    {
        _trainerOpen = false;
        if (Trainer is { } trainer) Say(trainer.FarewellLines);
    }

    private void ResetTrainer()
    {
        _trainerOpen = false;
        _speech = null;
        _speechKey = 0;
    }

    private void Say(IReadOnlyList<string> lines)
    {
        // Never the same line twice in a row
        var options = lines.Where(l => l != _speech).ToList();
        if (options.Count == 0) return;
        _speech = options[Random.Shared.Next(options.Count)];
        _speechKey++;
    }
}
