public class MesciusReportService
{
    // ExportType: "pdf" hoặc "excel"
    public byte[] ExportPlantReport(List<Plant> data, string exportType)
    {
        // 1. Tạo report (từ template .rpx hoặc build code)
        var rpt = new MyPlantReport(); // SectionReport
        rpt.DataSource = data;
        rpt.Run(false);

        using (var ms = new MemoryStream())
        {
            if (exportType == "excel")
            {
                var exporter = new GrapeCity.ActiveReports.Export.Excel.Section.XlsExport();
                exporter.Export(rpt.Document, ms);
            }
            else
            {
                var exporter = new GrapeCity.ActiveReports.Export.Pdf.Section.PdfExport();
                exporter.Export(rpt.Document, ms);
            }
            return ms.ToArray();
        }
    }
}
