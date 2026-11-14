using CareFlow.Application.Abstractions;
using CareFlow.Application.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CareFlow.Desktop.ViewModels;

public partial class BookingHistoryViewModel : ViewModelBase
{
    private readonly IBookingService _service;

    public ObservableCollection<BookingSummary> Bookings { get; } = new();

    [ObservableProperty] private BookingSummary? selectedBooking;
    [ObservableProperty] private BookingDetail? detail;
    [ObservableProperty] private string statusMessage = string.Empty;

    public BookingHistoryViewModel(IBookingService service) => _service = service;

    public async Task LoadAsync()
    {
        try
        {
            var rows = await _service.GetBookingsAsync();
            Bookings.Clear();
            foreach (var r in rows) Bookings.Add(r);
            StatusMessage = rows.Count == 0 ? "No confirmed bookings yet." : string.Empty;
        }
        catch (Exception)
        {
            StatusMessage = "✖ Could not load bookings.";
        }
    }

    partial void OnSelectedBookingChanged(BookingSummary? value) => _ = LoadDetailAsync(value);

    private async Task LoadDetailAsync(BookingSummary? summary)
    {
        if (summary is null) { Detail = null; return; }

        try { Detail = await _service.GetBookingDetailAsync(summary.BookingId); }
        catch (Exception) { StatusMessage = "✖ Could not load booking details."; }
    }
}