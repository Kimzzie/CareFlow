using CareFlow.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace CareFlow.Desktop.ViewModels;

public partial class ServiceChoiceViewModel : ObservableObject
{
    public int ServiceId { get; }
    public string Name { get; }
    public string AvailabilityText { get; }
    public bool IsFull { get; }

    [ObservableProperty]
    private bool isSelected;

    public event Action? SelectionChanged;

    public ServiceChoiceViewModel(ServiceAvailability a)
    {
        ServiceId = a.ServiceId;
        Name = a.Name;
        IsFull = a.Remaining < 1;
        // Text status, not just color
        AvailabilityText = IsFull ? "✖ FULL" : $"✓ {a.Remaining} of {a.DailyCapacity} left";
    }

    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke();
}