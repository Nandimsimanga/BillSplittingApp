using BillSplitter.Views;
using BillSplitter.DBContext;

namespace BillSplitter;

public partial class App : Application
{
	public readonly ApplicationDbContext applicationDb;
	public App()
	{
		InitializeComponent();
		MainPage =  new NavigationPage(new StartPage(applicationDb));

	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
