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
