using Form.DTOs;
namespace Form.Interface
{
    public interface IReportCardPdfService
    {
        byte[] GenerateReportCardPdf(StudentReportCardDto reportCard, string classRoomName);
    }
}
