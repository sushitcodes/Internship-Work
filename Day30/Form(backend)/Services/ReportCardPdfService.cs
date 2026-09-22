using Form.DTOs;
using Form.Interface;
using Form.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Form.Services;


public class ReportCardPdfService : IReportCardPdfService
{
    public byte[] GenerateReportCardPdf(StudentReportCardDto reportCard, string classRoomName)
    {
        // Calculate Summary Metrics (Total marks, percentages, pass/fail)
        var gradedSubjects = reportCard.Subjects.Where(s => s.MarksObtained.HasValue).ToList();
        var totalObtained = gradedSubjects.Sum(s => s.MarksObtained!.Value);
        var totalMax = gradedSubjects.Sum(s => s.MaxMarks);
        var overallPercentage = totalMax > 0 ? Math.Round((totalObtained * 100) / totalMax, 1) : 0;
        var isPassed = overallPercentage >= 40 && !gradedSubjects.Any(s => (s.MarksObtained!.Value / s.MaxMarks) < 0.35m);

        //  Generate QuestPDF Document structure
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Page Layout & Margins
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(Colors.Grey.Darken3));

                // ----------------------------------------------------
                // HEADER: School Letterhead & Official Title
                // ----------------------------------------------------
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(titleCol =>
                        {
                            titleCol.Item().Text("ACADEMIX ACADEMY").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
                            titleCol.Item().Text("Excellence in Education & Student Development").FontSize(9).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(120).AlignRight().Column(metaCol =>
                        {
                            metaCol.Item().Text("OFFICIAL REPORT").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                            metaCol.Item().Text($"Date: {DateTime.UtcNow:MMM dd, yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken2);
                });

                // ----------------------------------------------------
                // CONTENT: Student Information, Grades Table & Summary
                // ----------------------------------------------------
                page.Content().PaddingVertical(15).Column(col =>
                {
                    // Student Info Card
                    col.Item().Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t =>
                            {
                                t.Span("Student Name: ").SemiBold();
                                t.Span(reportCard.StudentName).Bold().FontColor(Colors.Blue.Darken3);
                            });
                            c.Item().Text(t =>
                            {
                                t.Span("Class / Section: ").SemiBold();
                                t.Span(classRoomName);
                            });
                        });

                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text(t =>
                            {
                                t.Span("Academic Status: ").SemiBold();
                                t.Span(gradedSubjects.Count == reportCard.Subjects.Count ? "Evaluated" : "In Progress")
                                 .FontColor(Colors.Green.Darken2);
                            });
                            c.Item().Text(t =>
                            {
                                t.Span("Subjects Evaluated: ").SemiBold();
                                t.Span($"{gradedSubjects.Count} / {reportCard.Subjects.Count}");
                            });
                        });
                    });

                    col.Item().Height(15);

                    // Grades Table
                    col.Item().Table(table =>
                    {
                        // Column definitions (relative proportions)
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);   // S.N.
                            columns.RelativeColumn(3);    // Subject Name
                            columns.RelativeColumn(1.5f); // Marks
                            columns.RelativeColumn(1.5f); // Percentage
                            columns.RelativeColumn(1.2f); // Grade
                            columns.RelativeColumn(2);    // Remarks
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("#").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Subject").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Marks").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Percentage").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Grade").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Remarks").Bold().FontColor(Colors.White);
                        });

                        // Table Rows
                        int index = 1;
                        foreach (var subject in reportCard.Subjects)
                        {
                            var rowBg = index % 2 == 0 ? Colors.Grey.Lighten5 : Colors.White;
                            var pct = subject.MarksObtained.HasValue && subject.MaxMarks > 0
                                ? Math.Round((subject.MarksObtained.Value * 100) / subject.MaxMarks, 1)
                                : (decimal?)null;

                            string letterGrade = pct switch
                            {
                                >= 90 => "A+",
                                >= 80 => "A",
                                >= 70 => "B+",
                                >= 60 => "B",
                                >= 50 => "C",
                                >= 40 => "D",
                                _ => "F"
                            };

                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(index.ToString());
                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(subject.SubjectName).Bold();
                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(subject.MarksObtained.HasValue ? $"{subject.MarksObtained} / {subject.MaxMarks}" : "Pending");
                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(pct.HasValue ? $"{pct}%" : "—");
                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(pct.HasValue ? letterGrade : "—").Bold().FontColor(letterGrade == "F" ? Colors.Red.Medium : Colors.Blue.Darken2);
                            table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(subject.Remarks ?? "—").Italic().FontSize(8.5f);

                            index++;
                        }
                    });

                    col.Item().Height(15);

                    // Performance Summary Box
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("TOTAL MARKS").FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Text($"{totalObtained} / {totalMax}").FontSize(14).Bold();
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("OVERALL PERCENTAGE").FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Text($"{overallPercentage}%").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("FINAL RESULT").FontSize(8).FontColor(Colors.Grey.Darken1).Bold();
                            c.Item().Text(isPassed ? "PASSED" : "NEEDS IMPROVEMENT")
                                    .FontSize(14).Bold()
                                    .FontColor(isPassed ? Colors.Green.Darken2 : Colors.Red.Darken1);
                        });
                    });

                    col.Item().Height(40);

                    // Signature Section
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.8f).LineColor(Colors.Grey.Darken1);
                            c.Item().PaddingTop(4).AlignCenter().Text("Class Teacher Signature").FontSize(8.5f);
                        });

                        row.ConstantItem(60); // Spacer

                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.8f).LineColor(Colors.Grey.Darken1);
                            c.Item().PaddingTop(4).AlignCenter().Text("Principal / Head of School").FontSize(8.5f);
                        });
                    });
                });

                // ----------------------------------------------------
                // FOOTER: Page Numbering & Verification Note
                // ----------------------------------------------------
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text("This is an official computer-generated document from Academix Portal.").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        });

        // Convert the QuestPDF declarative tree into raw binary bytes
        return document.GeneratePdf();
    }
}