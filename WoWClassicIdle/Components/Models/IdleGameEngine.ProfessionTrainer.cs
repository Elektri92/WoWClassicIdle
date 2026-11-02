// Models/IdleGameEngine.ProfessionTrainer.cs
using Models;

/// <summary>Profession trainer: learn training tiers and recipes for copper of the active character.</summary>
public partial class IdleGameEngine
{
    /// <summary>Reason why the next training tier cannot be learned (UI text) – <c>null</c> if allowed.</summary>
    public string? RankTrainBlocker(Profession profession)
    {
        if (GetProfessionProgress(profession) is not { } progress) return "You have not learned this profession.";
        if (progress.NextRank is not { } next) return "You have mastered every rank.";
        if (ProfessionTrainingLevel(profession) < next.RequiredLevel) return $"Requires level {next.RequiredLevel}";
        if (progress.Skill < next.RequiredSkill) return $"Requires {progress.Definition.Name} ({next.RequiredSkill})";
        if (Player.Copper < next.Cost) return "You don't have enough money.";
        return null;
    }

    /// <summary>Reason why the recipe cannot be learned (UI text) – <c>null</c> if allowed.</summary>
    public string? RecipeTrainBlocker(Recipe recipe)
    {
        if (GetProfessionProgress(recipe.Profession) is not { } progress) return "You have not learned this profession.";
        if (progress.KnowsRecipe(recipe.Id)) return "Already known";
        if (progress.Skill < recipe.RequiredSkill) return $"Requires {progress.Definition.Name} ({recipe.RequiredSkill})";
        if (Player.Copper < ProfessionTrainerRegistry.RecipeCost(recipe)) return "You don't have enough money.";
        return null;
    }

    /// <summary>
    /// Is there something new at the trainer, whoever is being played? A reachable rank stays new until it is trained,
    /// recipes only until the trainer was visited at their skill (otherwise recipes for other classes would glow forever).
    /// </summary>
    public bool HasTrainerNews(Profession profession)
    {
        lock (_simLock)
        {
            if (TrainerProgress(profession) is not { } progress) return false;
            if (progress.NextRank is { } next && progress.Skill >= next.RequiredSkill && TrainerLevel(profession) >= next.RequiredLevel)
                return true;
            return ProfessionRegistry.GetRecipes(profession).Any(r =>
                r.RequiredSkill > progress.TrainerSeenSkill && r.RequiredSkill <= progress.Skill && !progress.KnowsRecipe(r.Id));
        }
    }

    /// <summary>The player is looking at the trainer's offers: recipes up to the current skill are no longer new.</summary>
    public void MarkTrainerSeen(Profession profession)
    {
        lock (_simLock)
            if (TrainerProgress(profession) is { } progress)
                progress.TrainerSeenSkill = progress.Skill;
    }

    /// <summary>Why the profession glows in the menus (UI text) – <c>null</c> if there is nothing new.</summary>
    public string? ProfessionNews(Profession profession) => (HasTrainerNews(profession), CanImproveBonuses(profession)) switch
    {
        (true, true)  => "New training and a better bonus are available",
        (true, false) => "New training available",
        (false, true) => "A better bonus is available",
        _             => null,
    };

    private ProfessionProgress? TrainerProgress(Profession profession) =>
        Professions.Get(profession).IsPrimary
            ? ProfessionOwner(profession)?.Player.ProfessionState.Get(profession)
            : _sharedProfessions.GetValueOrDefault(profession);

    private int TrainerLevel(Profession profession) =>
        Professions.Get(profession).IsPrimary
            ? ProfessionOwner(profession)?.Player.Level ?? 0
            : _roster.Select(s => s.Player.Level).DefaultIfEmpty().Max();

    /// <summary>Buys the next training tier. Returns an English error message on failure, otherwise <c>null</c>.</summary>
    public string? TrainProfessionRank(Profession profession)
    {
        lock (_simLock)
        {
            if (RankTrainBlocker(profession) is { } blocker) return blocker;
            var progress = GetProfessionProgress(profession)!;
            long cost = progress.NextRank!.Cost;
            if (!progress.TrainNextRank(ProfessionTrainingLevel(profession))) return "You cannot train that yet.";
            Player.Copper -= cost;
        }
        ProfessionUpdated?.Invoke();
        return null;
    }

    /// <summary>Buys a recipe from the trainer. Returns an English error message on failure, otherwise <c>null</c>.</summary>
    public string? LearnProfessionRecipe(Recipe recipe)
    {
        lock (_simLock)
        {
            if (RecipeTrainBlocker(recipe) is { } blocker) return blocker;
            if (!GetProfessionProgress(recipe.Profession)!.LearnRecipe(recipe)) return "Already known";
            Player.Copper -= ProfessionTrainerRegistry.RecipeCost(recipe);
        }
        ProfessionUpdated?.Invoke();
        return null;
    }
}
