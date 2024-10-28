using BillSplitter.Models;
using BillSplitter.DBContext;
using Microsoft.EntityFrameworkCore;

public class BillsViewModel
{
    private readonly ApplicationDbContext _context;

    public BillsViewModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Bill>> GetBillsAsync()
    {
        return await _context.Bills.Include(b => b.Initiator).ToListAsync();
    }

    public async Task AddBillAsync(Bill bill)
    {
        _context.Bills.Add(bill);
        await _context.SaveChangesAsync();
    }
}