
namespace BillSplitter.Models;
public class User
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Bill
{
    public int BillId { get; set; }
    public string BillName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TipPercentage { get; set; }
    public DateTime DateCreated { get; set; }
        public int ParticipantsCount { get; set; }


 public decimal AmountPerPerson { get; set; }
        public bool HasIOU { get; set; }
}

public class Participant
{
    public int ParticipantId { get; set; }
    public string Name { get; set; }
    public decimal OwesAmount { get; set; }

    public Bill  Bill { get; set; }
}

public class IOU
{
    public string IOUId { get; set; }
    public string Email {get;set;}
    public string ParticipantName { get; set; }
    public double Amount { get; set; }
}