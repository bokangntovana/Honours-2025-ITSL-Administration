using ITSL_Administration.Models;
//using PdfSharp.Drawing;
//using PdfSharp.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace ITSL_Administration.Services
{
    public class PDFReportService
    {
        public byte[] GenerateStudentReport(User student, Course course, List<Submission> submissions)
        {
            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 12, XFontStyle.Regular);
                var fontTable = new XFont("Arial", 10, XFontStyle.Regular);

                int y = 40;

                // Title
                gfx.DrawString($"Academic Record: {student.FullName}", fontTitle, XBrushes.DarkBlue, new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;
                gfx.DrawString($"Course: {course.CourseName}", fontSub, XBrushes.Black, 40, y);
                y += 20;
                gfx.DrawString($"Generated on: {DateTime.Now:dd MMM yyyy}", fontSub, XBrushes.Black, 40, y);
                y += 30;

                // Table Header
                gfx.DrawString("Assignment", fontTable, XBrushes.Black, 40, y);
                gfx.DrawString("Type", fontTable, XBrushes.Black, 200, y);
                gfx.DrawString("Mark", fontTable, XBrushes.Black, 300, y);
                gfx.DrawString("Feedback", fontTable, XBrushes.Black, 380, y);
                y += 20;

                foreach (var sub in submissions)
                {
                    var grade = sub.Grade;

                    gfx.DrawString(sub.Assignment?.Title ?? "-", fontTable, XBrushes.Black, 40, y);
                    gfx.DrawString(sub.Assignment?.AssignmentType.ToString() ?? "-", fontTable, XBrushes.Black, 200, y);
                    gfx.DrawString(grade != null ? $"{grade.AssignmentMark}/{sub.Assignment?.SetAssignmentMark}" : "Not graded", fontTable, XBrushes.Black, 300, y);
                    gfx.DrawString(grade?.GradesFeedback ?? "-", fontTable, XBrushes.Black, 380, y);

                    y += 20;
                }

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerateGradebookReport(Course course)
        {
            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 12, XFontStyle.Regular);
                var fontTable = new XFont("Arial", 10, XFontStyle.Regular);

                int y = 40;

                // Title
                gfx.DrawString($"Gradebook Report: {course.CourseName}", fontTitle, XBrushes.DarkBlue,
                    new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;
                gfx.DrawString($"Generated on: {DateTime.Now:dd MMM yyyy}", fontSub, XBrushes.Black, 40, y);
                y += 30;

                // Header Row
                gfx.DrawString("Student", fontTable, XBrushes.Black, 40, y);
                int xPos = 180;
                foreach (var a in course.Assignments)
                {
                    gfx.DrawString(a.Title, fontTable, XBrushes.Black, xPos, y);
                    xPos += 100;
                }
                gfx.DrawString("Final Avg", fontTable, XBrushes.Black, xPos, y);
                y += 20;

                // Data Rows
                var students = course.Assignments
                    .SelectMany(a => a.Submissions)
                    .GroupBy(s => s.ParticipantId);

                foreach (var group in students)
                {
                    var studentName = group.First().Participant?.FullName ?? "Unknown";
                    gfx.DrawString(studentName, fontTable, XBrushes.Black, 40, y);

                    xPos = 180;
                    double totalMarks = 0, totalWeight = 0;

                    foreach (var a in course.Assignments)
                    {
                        var sub = group.FirstOrDefault(s => s.AssignmentId == a.AssignmentID);
                        if (sub?.Grade != null)
                        {
                            gfx.DrawString($"{sub.Grade.AssignmentMark}/{a.SetAssignmentMark}", fontTable, XBrushes.Black, xPos, y);
                            totalMarks += sub.Grade.AssignmentMark * a.Weight;
                            totalWeight += a.Weight;
                        }
                        else
                        {
                            gfx.DrawString("N/A", fontTable, XBrushes.Black, xPos, y);
                        }
                        xPos += 100;
                    }

                    var avg = totalWeight > 0 ? (totalMarks / totalWeight) : 0;
                    gfx.DrawString($"{Math.Round(avg, 2)}%", fontTable, XBrushes.Black, xPos, y);

                    y += 20;
                }

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }
    }
}

