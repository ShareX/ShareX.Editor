using ShareX.Editor.ImageEffects;
using ShareX.Editor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShareX.Editor.Services;

/// <summary>
/// Discovers available ImageEffects and wraps them for the UI.
/// </summary>
public static class EffectCatalogService
{
    public static List<EffectViewModel> GetAllEffects()
    {
        var assembly = typeof(ImageEffect).Assembly;
        var effectTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ImageEffect)) && !t.IsAbstract)
            .ToList();

        var effectViewModels = new List<EffectViewModel>();

        foreach (var type in effectTypes)
        {
            try
            {
                if (Activator.CreateInstance(type) is ImageEffect instance)
                {
                    effectViewModels.Add(new EffectViewModel(instance));
                }
            }
            catch
            {
                // Ignore effects that fail to instantiate; keep editor resilient.
            }
        }

        return effectViewModels;
    }

    public static List<string> GetCategories()
    {
        return GetAllEffects()
            .Select(e => e.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
