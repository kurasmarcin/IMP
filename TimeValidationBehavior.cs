using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace IMP.Behaviors;

public class TimeValidationBehavior : Behavior<Entry>
{
    // Regex sprawdzający poprawność formatu HH:mm
    private static readonly Regex TimeRegex = new Regex(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", RegexOptions.Compiled);

    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;

        // Sprawdź, czy nowa wartość spełnia wymagania regexa
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || TimeRegex.IsMatch(e.NewTextValue))
        {
            entry.BackgroundColor = Colors.Transparent; // Poprawny format
        }
        else
        {
            entry.BackgroundColor = Colors.LightPink; // Niepoprawny format
        }
    }
}
