
namespace BillSplitter.Operations;
using BillSplitter.DBContext;
using BillSplitter.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Net;
using System.Net.Mail;

public class UserService
{
    private readonly ApplicationDbContext _context;

    public UserService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public User GetUserById(int userId)
    {
        return _context.Users.Find(userId);
    }

    public List<User> GetAllUsers()
    {
        return _context.Users.ToList();
    }

    public void UpdateUser(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    public void DeleteUser(int userId)
    {
        var user = _context.Users.Find(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}

public class BillService
{
    private readonly ApplicationDbContext _context;

    public BillService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddBill(Bill bill)
    {
        _context.Bills.Add(bill);
        _context.SaveChanges();
    }

    public Bill GetBillById(int billId)
    {
        return _context.Bills.Include(b => b.Participants).Include(b => b.IOUs).FirstOrDefault(b => b.BillId == billId);
    }

    public List<Bill> GetAllBills()
    {
        return _context.Bills.Include(b => b.Participants).Include(b => b.IOUs).ToList();
    }

    public void UpdateBill(Bill bill)
    {
        _context.Bills.Update(bill);
        _context.SaveChanges();
    }

    public void DeleteBill(int billId)
    {
        var bill = _context.Bills.Find(billId);
        if (bill != null)
        {
            _context.Bills.Remove(bill);
            _context.SaveChanges();
        }
    }
}
public class ParticipantService
{
    private readonly ApplicationDbContext _context;

    public ParticipantService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddParticipant(Participant participant)
    {
        _context.Participants.Add(participant);
        _context.SaveChanges();
    }

  public List<Participant> GetParticipantsByBillId(int billId)
    {
        return _context.Participants.Where(p => p.BillId == billId).ToList();
    }

    public void UpdateParticipant(Participant participant)
    {
        _context.Participants.Update(participant);
        _context.SaveChanges();
    }

    public void DeleteParticipant(int participantId)
    {
        var participant = _context.Participants.Find(participantId);
        if (participant != null)
        {
            _context.Participants.Remove(participant);
            _context.SaveChanges();
        }
    }
}
public class IOUService
{
    private readonly ApplicationDbContext _context;

    public IOUService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddIOU(IOU iou)
    {
        _context.IOUs.Add(iou);
        _context.SaveChanges();
    }

    public List<IOU> GetIOUsByBillId(int billId)
    {
        return _context.IOUs.Where(i => i.BillId == billId).ToList();
    }

    public void UpdateIOU(IOU iou)
    {
        _context.IOUs.Update(iou);
        _context.SaveChanges();
    }

    public void DeleteIOU(int iouId)
    {
        var iou = _context.IOUs.Find(iouId);
        if (iou != null)
        {
            _context.IOUs.Remove(iou);
            _context.SaveChanges();
        }
    }
}
public class EmailService
{
     private readonly ApplicationDbContext _context;
     public EmailService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void SendBillBreakdownEmail(string userEmail, string emailBody, string subject)
    {
        var smtpClient = new SmtpClient("smtp.example.com") // Your SMTP server
        {
            Port = 587, // Common port for TLS
            Credentials = new NetworkCredential("your-email@example.com", "your-password"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("your-email@example.com"),
            Subject = subject,
            Body = emailBody,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(userEmail);

        smtpClient.Send(mailMessage);
    }
    public string GenerateEmailBody(string userName, decimal totalBill, decimal tipAmount, decimal totalDue, List<Participant> participants, List<IOU> ioUs)
{
    var emailBody = $@"
    <html>
    <body>
        <h2>Your Bill Breakdown</h2>
        <p>Dear {userName},</p>
        <p>Here are the details for your recent bill:</p>
        <h3>Bill Details</h3>
        <p>Total Bill: ${totalBill}</p>
        <p>Tip Amount: ${tipAmount}</p>
        <p>Total Amount Due: ${totalDue}</p>
        
        <h3>Individual Contributions</h3>
        <ul>";

    foreach (var participant in participants)
    {
        emailBody += $"<li>{participant.User.Name}: ${participant.Contribution}</li>";
    }

    emailBody += @"</ul>
        <h3>Unpaid IOUs</h3>
        <ul>";

    foreach (var iou in ioUs)
    {
        emailBody += $"<li>{iou.Description}: ${iou.Amount} - Due {iou.DueDate.ToShortDateString()}</li>";
    }

    emailBody += @"
        </ul>
        <p>Thank you for using [App Name]!</p>
    </body>
    </html>";

    return emailBody;
}
public void FinalizeBill(Bill bill)
{
    // Gather the necessary data
    EmailService emailService = new EmailService(_context);
    var participants = emailService.GetParticipants(bill);
    var ioUs = emailService.GetUnpaidIOUs(bill);
    
    // Calculate totals
    decimal totalBill = bill.TotalAmount;
    decimal tipAmount = bill.TipAmount;
    decimal totalDue = totalBill + tipAmount;
    
    // Generate email body
    string emailBody = emailService.GenerateEmailBody(bill.Initiator.Name, totalBill, tipAmount, totalDue, participants, ioUs);
    
    // Send the email
    emailService.SendBillBreakdownEmail(bill.Initiator.Email, emailBody, "Your Bill Breakdown from [App Name]");
}
   public List<Participant> GetParticipants(Bill bill)
    {
        return _context.Participants
            .Where(p => p.BillId == bill.BillId)
            .ToList();
    }

    public List<IOU> GetUnpaidIOUs(Bill bill)
    {
        return _context.IOUs
            .Where(i => i.BillId == bill.BillId && !i.IsPaid)
            .ToList();
    }
}

