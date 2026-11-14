using System.Threading.Tasks;

namespace CareFlow.Desktop.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public BookingFormViewModel Form { get; }
    public BookingHistoryViewModel History { get; }

    public MainWindowViewModel(BookingFormViewModel form, BookingHistoryViewModel history)
    {
        Form = form;
        History = history;
        Form.BookingConfirmed += async (_, _) => await History.LoadAsync();
    }

    public async Task InitializeAsync()
    {
        await Form.InitializeAsync();
        await History.LoadAsync();   // AC-06: bookings from earlier runs appear here
    }
}