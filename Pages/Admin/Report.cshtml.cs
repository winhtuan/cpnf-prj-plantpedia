
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Plantpedia.Models;

namespace Plantpedia.Pages.Admin;
public class ReportModel : PageModel
{
    private readonly Plantpedia2Context _db;

    public ReportModel(Plantpedia2Context db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? PlantType { get; set; }

    public List<string>? PlantTypes { get; set; }
    public List<PlantInfo>? PlantReports { get; set; }

    public IActionResult OnGet(string? Export)
{
    // Lấy loại cây cho dropdown
    PlantTypes = _db.PlantInfos.Select(p => p.PlantTypeId).Distinct().ToList();

    var query = _db.PlantInfos.AsQueryable();
    if (StartDate.HasValue)
        query = query.Where(x => x.CreatedDate >= StartDate.Value);
    if (EndDate.HasValue)
        query = query.Where(x => x.CreatedDate <= EndDate.Value);
    if (!string.IsNullOrEmpty(PlantType))
        query = query.Where(x => x.PlantTypeId == PlantType);

    PlantReports = query.OrderByDescending(x => x.CreatedDate).ToList();

    // Nếu có yêu cầu export PDF/Excel
    if (!string.IsNullOrEmpty(Export))
    {
        var mesciusReportService = new MesciusReportService(); // inject hoặc new trực tiếp
        var fileBytes = mesciusReportService.ExportPlantReport(PlantReports, Export);
        string fileType = Export == "excel"
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";
        string fileName = Export == "excel"
            ? "BaoCaoCayTrong.xlsx"
            : "BaoCaoCayTrong.pdf";
        return File(fileBytes, fileType, fileName);
    }

    return Page();
}

}
