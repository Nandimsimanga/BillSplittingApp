namespace BillSplit;

using System.Net;
using System.Net.Mail;
using BillSplitter.DBContext;
using BillSplitter.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

public partial class MainPage : ContentPage
{
	int count = 0;
	string currentUser;
 private readonly ApplicationDbContext _applicationDbContext;
	public MainPage()
	{
		
		InitializeComponent();
		IOUListComponents.IsVisible = false;
            MainPageComponents.IsVisible = false;
            CreateBillSplitComponents.IsVisible = false;
            BillDetailsComponents.IsVisible = false;
            IOUChecked.IsVisible =false;
	}

	//initial user entry
        private async void SubmitButton_Clicked(object sender, EventArgs e)
        {
            // Basic input validation
            if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(NameEntry.Text))
            {
                //await DisplayAlert("Error", "Please enter both your email and name.", "OK");
                return; 
            }

            // Check if the user already exists using the email entered
            var user =  _applicationDbContext.Users
                .FirstOrDefault(u => u.Email == EmailEntry.Text);

            if (user == null)
            {
                // Create a new user
                user = new User
                {
                    Email = EmailEntry.Text,
                    Name = NameEntry.Text
                };

                // Save the new user to the database
                _applicationDbContext.Users.Add(user);
                await _applicationDbContext.SaveChangesAsync();
                currentUser = $"{user.Name};{user.Email}";

                //await DisplayAlert("Success", "User created successfully!", "OK");
            }
            else
            {
                //await DisplayAlert("Welcome Back", $"Welcome back, {user.Name}!", "OK");
            }

            LoadUserData(user); // Pass the user to load their data
        }

//loading data for main page
        private async void LoadUserData(User user)
        {
            // Get info of signed-in user
            CreateUser.IsVisible= false;
            MainPageComponents.IsVisible = true;

            string current = $"{user.Name};{user.Email}"; // Directly using the user object

            // Split into currentName and currentEmail
            var userParts = current.Split(';');

            // Validate userParts
            
                string currentName = userParts[0];
                string currentEmail = userParts[1];

                double iouAmount = await GetUserIOUAmount(currentEmail);
                if (iouAmount== 0){
                    AmountInIOULbl.Text = $"Amount in IOU: You have no current IOUs";
                }

                NameLbl.Text = $"Welcome back, {currentName}";
                AmountInIOULbl.Text = $"Amount in IOU: ${iouAmount:F2}";
            
            
        }
         private async Task<double> GetUserIOUAmount(string email)
        {
            // Replace this with your actual database context and logic
            var ious =  _applicationDbContext.IOUs
                .Where(i => i.Email == email)
                .ToList();

            // Sum the amounts of all IOUs associated with the user
            
            return ious.Sum(i => i.Amount); // Assuming IOU has a property Amount
        }
        
        //bill split
         private  void StartSplit(object sender, EventArgs e){
    MainPageComponents.IsVisible = false;
    CreateBillSplitComponents.IsVisible = true;

 }
 string Result;
  private void OnCalculateSplitClicked(object sender, EventArgs e)
    {
        // Get input values
        var billName = BillNameEntry.Text;
        var totalAmount = decimal.TryParse(TotalAmountEntry.Text, out var amount) ? amount : 0;
        var tipPercentage = (int)TipPercentagePicker.SelectedItem;
        var participantsCount = int.TryParse(ParticipantsCountEntry.Text, out var count) ? count : 0;

        // Calculate the split
        var totalWithTip = totalAmount + (totalAmount * tipPercentage / 100);
        var amountPerPerson = totalWithTip / participantsCount;

        // Display the result
        Result = $"Bill Name: {billName}\n" +
                 $"Total Amount: ${totalAmount}\n" +
                 $"Tip Percentage: {tipPercentage}%\n" +
                 $"Amount per Person: ${amountPerPerson:F2}";
        ResultLabel.Text = Result;
    }

    private async void OnActionButtonClicked(object sender, EventArgs e)
    {
        // Create a new bill instance
        var bill = new Bill
        {
            BillName = BillNameEntry.Text,
            TotalAmount = decimal.TryParse(TotalAmountEntry.Text, out var amount) ? amount : 0,
            TipPercentage = (int)TipPercentagePicker.SelectedItem,
            ParticipantsCount = int.TryParse(ParticipantsCountEntry.Text, out var count) ? count : 0,
            AmountPerPerson = CalculateAmountPerPerson(),
            HasIOU = IOUCheckBox.IsChecked
        };

        if (IOUCheckBox.IsChecked)
        {
            // Logic to save IOU details
            IOU iOU = new IOU();
            IOUChecked.IsVisible = true;
            iOU.IOUId = Guid.NewGuid().ToString();
            iOU.ParticipantName = ParticipantNameEntry.Text; // Assume you have this entry
            iOU.Amount = double.TryParse(AmountOwedEntry.Text, out var owedAmount) ? owedAmount : 0;
            

            // Save the IOU details as needed
await _applicationDbContext.IOUs.AddAsync(iOU);
            // Show success message for IOU
            //await DisplayAlert("Success", "IOU has been created.", "OK");
        }
        else
        {
            // Finalize the bill by saving to the database
            await _applicationDbContext.Bills.AddAsync(bill);
            //await DisplayAlert("Success", "Bill has been finalized.", "OK");
            // Take them back to the start page
            CreateBillSplitComponents.IsVisible = false;
           LoadBillDetails();

        }
    }

    private decimal CalculateAmountPerPerson()
    {
        // Logic to calculate the amount per person
        var totalAmount = decimal.TryParse(TotalAmountEntry.Text, out var amount) ? amount : 0;
        var tipPercentage = (int)TipPercentagePicker.SelectedItem;
        var participantsCount = int.TryParse(ParticipantsCountEntry.Text, out var count) ? count : 1; // Avoid division by zero

        var totalWithTip = totalAmount + (totalAmount * tipPercentage / 100);
        return totalWithTip / participantsCount;
    }

//have the full breakdown of bill 
private void LoadBillDetails()
        {
            // Implement logic to load bill and IOU details
            BillInfoLabel.Text = Result;
            IOUInfoLabel.Text = "IOU Details: User A owes User B $20";
            BillDetailsComponents.IsVisible = true;
        }


 private async void IOUlists(object sender, EventArgs e){
    MainPageComponents.IsVisible = false;
    //get IOU info from db and map info to components
    var IOU = _applicationDbContext.IOUs.ToList();
    foreach (var Item in IOU){
        ParticipantNameEntry.Text = Item.ParticipantName;
        AmountOwedEntry.Text = "R" + Item.Amount;
    }
    IOUListComponents.IsVisible = true;

 }

 private void DownloadBillAsPDF()
        {
            // Create a PDF document
            var pdfDocument = new PdfDocument();
            pdfDocument.Info.Title = "Bill Details";

            // Create an empty page
            var pdfPage = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(pdfPage);

            // Set font
            var font = new XFont("Verdana", 20, XFontStyle.Bold);
            graphics.DrawString("Bill and IOU Details", font, XBrushes.Black, new XRect(0, 0, pdfPage.Width, pdfPage.Height), XStringFormats.TopCenter);

            // TODO: Add bill and IOU details to the PDF here.
            // This is just a placeholder example
            graphics.DrawString("Bill Name: Example Bill", new XFont("Verdana", 12), XBrushes.Black, new XPoint(50, 100));
            graphics.DrawString("Total Amount: $100.00", new XFont("Verdana", 12), XBrushes.Black, new XPoint(50, 120));
            graphics.DrawString("Amount per Person: $25.00", new XFont("Verdana", 12), XBrushes.Black, new XPoint(50, 140));
            graphics.DrawString("IOUs: User A owes $10.00", new XFont("Verdana", 12), XBrushes.Black, new XPoint(50, 160));

            // Save the document to a file
            string filename = Path.Combine(FileSystem.AppDataDirectory, "BillDetails.pdf");
            pdfDocument.Save(filename);

            // Inform the user
           // DisplayAlert("Download Complete", $"Bill details have been saved to {filename}.", "OK");
        }

        private async void OnBackToCreateBillSplitClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnEmailBillClicked(object sender, EventArgs e)
        {
            // Logic to send email
            string emailRecipient = "recipient@example.com"; // Replace with actual recipient email
            string subject = "Bill Details";
            string body = "Attached are the bill and IOU details.";

            // Save the PDF to attach to email
            string filename = Path.Combine(FileSystem.AppDataDirectory, "BillDetails.pdf");
            DownloadBillAsPDF(); // Make sure the PDF is generated before emailing

            // Sending email logic
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress("your-email@example.com"); // Replace with your email
                    message.To.Add(emailRecipient);
                    message.Subject = subject;
                    message.Body = body;

                    // Attach the PDF file
                    message.Attachments.Add(new Attachment(filename));

                    using (var smtpClient = new SmtpClient("smtp.your-email-provider.com")) // Replace with your SMTP server
                    {
                        smtpClient.Port = 587; // Or use your email provider's port
                        smtpClient.Credentials = new NetworkCredential("your-email@example.com", "your-email-password"); // Use your email credentials
                        smtpClient.EnableSsl = true; // Enable SSL if required
                        await smtpClient.SendMailAsync(message);
                    }
                }

             //   await DisplayAlert("Email Sent", "Bill details have been emailed successfully.", "OK");
            }
            catch (Exception ex)
            {
              //  await DisplayAlert("Error", $"An error occurred while sending email: {ex.Message}", "OK");
            }
        }

        private async void BackToMain(object sender, EventArgs e){
                BillDetailsComponents.IsVisible= false;
                MainPageComponents.IsVisible = true;
        }


    }

	


