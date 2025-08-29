using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Production.Data;
using WebApplication2.Pages.Shared.Models;
using Microsoft.EntityFrameworkCore;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public List<Prod_Areas_TBL> Prod_Areas_TBL { get; set; } = new List<Prod_Areas_TBL>();

    [BindProperty]
    public DailyFieldProduction Form { get; set; }

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnGetAsync()
    {
        Prod_Areas_TBL = await _db.Prod_Areas_TBL.ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Prod_Areas_TBL = await _db.Prod_Areas_TBL.ToListAsync();
            return Page();
        }

        _db.DailyFieldProductionEntries.Add(Form);
        await _db.SaveChangesAsync();
        return RedirectToPage("Results");
    }
}