using CareFlow.Application.Abstractions;
using CareFlow.Application.Models;
using CareFlow.Domain;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CareFlow.Desktop.ViewModels;

public partial class BookingFormViewModel : ViewModelBase
{
    private readonly IBookingService _service;
    private readonly IClock _clock;

    public ObservableCollection<PatientOption> Patients { get; } = new();
    public ObservableCollection<ServiceChoiceViewModel> Services { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmBookingCommand))]
    private PatientOption? selectedPatient;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmBookingCommand))]
    private DateTimeOffset? selectedDate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmBookingCommand))]
    [NotifyPropertyChangedFor(nameof(IsFormEnabled))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionHint))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmBookingCommand))]
    private int selectedCount;

    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isError;

    public bool IsFormEnabled => !IsBusy;
    public string SelectionHint => $"Selected {SelectedCount} of {Booking.MaxServices}";

    /// <summary>Raised after a successful booking so the history list can refresh.</summary>
    public event EventHandler? BookingConfirmed;

    public BookingFormViewModel(IBookingService service, IClock clock)
    {
        _service = service;
        _clock = clock;
    }

    public async Task InitializeAsync()
    {
        try
        {
            foreach (var p in await _service.GetPatientsAsync())
                Patients.Add(p);

            // Triggers OnSelectedDateChanged, which loads availability
            SelectedDate = new DateTimeOffset(_clock.Today.ToDateTime(TimeOnly.MinValue));
        }
        catch (Exception)
        {
            ShowError("Could not load patients. Please restart the application.");
        }
    }

    partial void OnSelectedDateChanged(DateTimeOffset? value) => _ = RefreshAvailabilityAsync();

    private async Task RefreshAvailabilityAsync()
    {
        if (SelectedDate is null) return;

        try
        {
            var date = ToDateOnly(SelectedDate.Value);
            var availability = await _service.GetAvailabilityAsync(date);

            // Keep the user's ticks when the list is rebuilt
            var ticked = Services.Where(s => s.IsSelected).Select(s => s.ServiceId).ToHashSet();

            Services.Clear();
            foreach (var a in availability)
            {
                var row = new ServiceChoiceViewModel(a) { IsSelected = ticked.Contains(a.ServiceId) };
                row.SelectionChanged += UpdateSelectedCount;
                Services.Add(row);
            }
            UpdateSelectedCount();
        }
        catch (Exception)
        {
            ShowError("Could not load service availability.");
        }
    }

    private void UpdateSelectedCount() => SelectedCount = Services.Count(s => s.IsSelected);

    private bool CanConfirm() =>
        !IsBusy && SelectedPatient is not null && SelectedDate is not null
        && SelectedCount is >= 1 and <= Booking.MaxServices;

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmBookingAsync()
    {
        IsBusy = true;
        try
        {
            var request = new BookingRequest(
                SelectedPatient!.Id,
                ToDateOnly(SelectedDate!.Value),
                Services.Where(s => s.IsSelected).Select(s => s.ServiceId).ToList());

            var result = await _service.CreateBookingAsync(request);

            if (result.Success)
            {
                foreach (var s in Services) s.IsSelected = false;
                IsError = false;
                StatusMessage = $"✓ Booking #{result.BookingId} confirmed.";
                BookingConfirmed?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ShowError(result.Message);
            }

            // Success OR failure: reload, so the screen never keeps stale numbers (BR-08 / AC-04)
            await RefreshAvailabilityAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowError(string message)
    {
        IsError = true;
        StatusMessage = $"✖ {message}";
    }

    private static DateOnly ToDateOnly(DateTimeOffset d) => DateOnly.FromDateTime(d.DateTime);
}