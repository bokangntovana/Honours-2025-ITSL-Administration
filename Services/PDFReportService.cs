using ITSL_Administration.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.Linq;

namespace ITSL_Administration.Services
{
    public class PDFReportService
    {
        public byte[] GenerateParticipantReport(User participant, Course course, List<Submission> submissions)
        {
            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 12, XFontStyle.Regular);
                var fontTable = new XFont("Arial", 10, XFontStyle.Regular);
                var fontTableBold = new XFont("Arial", 10, XFontStyle.Bold);

                int y = 40;
                int leftMargin = 40;
                int rightMargin = 40;
                double maxWidth = page.Width - leftMargin - rightMargin;

                // Title
                gfx.DrawString($"Academic Record: {participant.FullName}", fontTitle, XBrushes.DarkBlue,
                    new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                gfx.DrawString($"Course: {course.CourseName}", fontSub, XBrushes.Black, leftMargin, y);
                y += 20;
                gfx.DrawString($"Participant ID: {participant.Id}", fontSub, XBrushes.Black, leftMargin, y);
                y += 20;
                gfx.DrawString($"Generated on: {DateTime.Now:dd MMM yyyy}", fontSub, XBrushes.Black, leftMargin, y);
                y += 30;

                // Check if participant has any submissions
                if (!submissions.Any())
                {
                    gfx.DrawString("No assignments submitted for this course.", fontSub, XBrushes.Black, leftMargin, y);
                    using (var stream = new MemoryStream())
                    {
                        doc.Save(stream, false);
                        return stream.ToArray();
                    }
                }

                // Table Header
                DrawTableRow(gfx, fontTableBold, leftMargin, y, maxWidth,
                    new[] { "Assignment", "Type", "Due Date", "Mark", "Status", "Feedback" },
                    new[] { 0.25, 0.15, 0.15, 0.10, 0.15, 0.20 });
                y += 20;

                // Draw a line under header
                gfx.DrawLine(XPens.Gray, leftMargin, y - 5, page.Width - rightMargin, y - 5);
                y += 10;

                // Process each assignment in the course
                foreach (var assignment in course.Assignments.OrderBy(a => a.DueDate))
                {
                    var submission = submissions.FirstOrDefault(s => s.AssignmentId == assignment.AssignmentID);
                    var grade = submission?.Grade;

                    // Handle page breaks
                    if (y > page.Height - 100)
                    {
                        page = doc.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;

                        // Redraw header on new page
                        DrawTableRow(gfx, fontTableBold, leftMargin, y, maxWidth,
                            new[] { "Assignment", "Type", "Due Date", "Mark", "Status", "Feedback" },
                            new[] { 0.25, 0.15, 0.15, 0.10, 0.15, 0.20 });
                        y += 20;
                        gfx.DrawLine(XPens.Gray, leftMargin, y - 5, page.Width - rightMargin, y - 5);
                        y += 10;
                    }

                    string assignmentName = assignment.Title ?? "-";
                    string assignmentType = assignment.AssignmentType.ToString();
                    string dueDate = assignment.DueDate.ToString("dd MMM yyyy");
                    string mark;
                    string status;
                    string feedback;

                    if (submission == null)
                    {
                        mark = "N/A";
                        status = "Not Submitted";
                        feedback = "-";
                    }
                    else if (grade == null)
                    {
                        mark = "N/A";
                        status = "Submitted (Not Graded)";
                        feedback = "-";
                    }
                    else
                    {
                        mark = $"{grade.AssignmentMark:F1}/{assignment.SetAssignmentMark}";
                        status = grade.HasPassed ? "Pass" : "Fail";
                        feedback = grade.GradesFeedback ?? "-";
                    }

                    DrawTableRow(gfx, fontTable, leftMargin, y, maxWidth,
                        new[] { assignmentName, assignmentType, dueDate, mark, status, feedback },
                        new[] { 0.25, 0.15, 0.15, 0.10, 0.15, 0.20 });

                    y += 20;
                }

                // Calculate and display final grade if available
                var gradedSubmissions = submissions.Where(s => s.Grade != null).ToList();
                if (gradedSubmissions.Any())
                {
                    y += 30;
                    gfx.DrawLine(XPens.Gray, leftMargin, y, page.Width - rightMargin, y);
                    y += 20;

                    // Calculate weighted final mark
                    double totalWeightedMarks = 0;
                    double totalWeight = 0;

                    foreach (var submission in gradedSubmissions)
                    {
                        var assignment = course.Assignments.FirstOrDefault(a => a.AssignmentID == submission.AssignmentId);
                        if (assignment != null && submission.Grade != null)
                        {
                            totalWeightedMarks += submission.Grade.AssignmentMark * assignment.Weight;
                            totalWeight += assignment.Weight;
                        }
                    }

                    double finalMark = totalWeight > 0 ? totalWeightedMarks / totalWeight : 0;
                    bool hasPassed = finalMark >= 50;

                    gfx.DrawString($"Final Course Mark: {finalMark:F1}%", fontTableBold,
                        XBrushes.DarkBlue, leftMargin, y);
                    y += 20;

                    gfx.DrawString($"Status: {(hasPassed ? "PASS" : "FAIL")}",
                        fontTableBold, hasPassed ? XBrushes.Green : XBrushes.Red,
                        leftMargin, y);
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
                page.Size = PdfSharpCore.PageSize.A4;
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                var gfx = XGraphics.FromPdfPage(page);

                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontSub = new XFont("Arial", 12, XFontStyle.Regular);
                var fontTable = new XFont("Arial", 8, XFontStyle.Regular);
                var fontTableBold = new XFont("Arial", 8, XFontStyle.Bold);

                int y = 40;
                int leftMargin = 30;
                int rightMargin = 30;
                double maxWidth = page.Width - leftMargin - rightMargin;

                // Title
                gfx.DrawString($"Gradebook Report: {course.CourseName}", fontTitle, XBrushes.DarkBlue,
                    new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
                y += 30;

                // Course information
                gfx.DrawString($"Course Code: {course.CourseCode}", fontSub, XBrushes.Black, leftMargin, y);
                y += 15;
                gfx.DrawString($"Credits: {course.CourseCredits}", fontSub, XBrushes.Black, leftMargin, y);
                y += 15;
                gfx.DrawString($"Generated on: {DateTime.Now:dd MMM yyyy}", fontSub, XBrushes.Black, leftMargin, y);
                y += 30;

                // Get participants who have submissions for this course
                var participantsWithSubmissions = course.Assignments
                    .SelectMany(a => a.Submissions)
                    .Select(s => s.Participant)
                    .Where(p => p != null)
                    .Distinct()
                    .OrderBy(p => p!.FullName)
                    .ToList();

                if (!participantsWithSubmissions.Any())
                {
                    gfx.DrawString("No participants with submissions in this course.", fontSub, XBrushes.Black, leftMargin, y);
                    using (var stream = new MemoryStream())
                    {
                        doc.Save(stream, false);
                        return stream.ToArray();
                    }
                }

                // Get course assignments ordered by due date
                var assignments = course.Assignments
                    .OrderBy(a => a.DueDate)
                    .ToList();

                if (!assignments.Any())
                {
                    gfx.DrawString("No assignments in this course.", fontSub, XBrushes.Black, leftMargin, y);
                    using (var stream = new MemoryStream())
                    {
                        doc.Save(stream, false);
                        return stream.ToArray();
                    }
                }

                // Calculate column widths dynamically based on content
                double participantColWidth = 0.20;
                double assignmentColWidth = (1.0 - participantColWidth) / (assignments.Count + 1); // +1 for final grade column

                // Prepare header texts
                var headers = new List<string> { "Participant Name" };
                headers.AddRange(assignments.Select(a =>
                    a.Title.Length > 15 ? a.Title.Substring(0, 12) + "..." : a.Title));
                headers.Add("Final Grade");

                // Prepare column widths
                var colWidths = new List<double> { participantColWidth };
                colWidths.AddRange(Enumerable.Repeat(assignmentColWidth, assignments.Count + 1));

                // Header Row
                DrawTableRow(gfx, fontTableBold, leftMargin, y, maxWidth, headers.ToArray(), colWidths.ToArray());
                y += 20;

                // Draw line under header
                gfx.DrawLine(XPens.Gray, leftMargin, y - 5, page.Width - rightMargin, y - 5);
                y += 10;

                // Data Rows - Use participants with submissions
                foreach (var participant in participantsWithSubmissions)
                {
                    if (participant == null) continue;

                    // Handle page breaks
                    if (y > page.Height - 50)
                    {
                        page = doc.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        gfx = XGraphics.FromPdfPage(page);
                        y = 40;

                        // Redraw header on new page
                        DrawTableRow(gfx, fontTableBold, leftMargin, y, maxWidth, headers.ToArray(), colWidths.ToArray());
                        y += 20;
                        gfx.DrawLine(XPens.Gray, leftMargin, y - 5, page.Width - rightMargin, y - 5);
                        y += 10;
                    }

                    var rowData = new List<string> { participant.FullName ?? "Unknown" };
                    double totalWeightedMarks = 0;
                    double totalWeight = 0;
                    int gradedAssignmentsCount = 0;

                    foreach (var assignment in assignments)
                    {
                        var submission = assignment.Submissions
                            .FirstOrDefault(s => s.ParticipantId == participant.Id);

                        if (submission?.Grade != null)
                        {
                            var grade = submission.Grade;
                            rowData.Add($"{grade.AssignmentMark:F0}/{assignment.SetAssignmentMark}");

                            // Calculate weighted contribution to final grade
                            totalWeightedMarks += grade.AssignmentMark * assignment.Weight;
                            totalWeight += assignment.Weight;
                            gradedAssignmentsCount++;
                        }
                        else if (submission != null)
                        {
                            rowData.Add("Submitted");
                        }
                        else
                        {
                            rowData.Add("-");
                        }
                    }

                    // Calculate final grade (only if there are graded assignments)
                    string finalGrade = "-";
                    if (totalWeight > 0 && gradedAssignmentsCount > 0)
                    {
                        double finalMark = totalWeightedMarks / totalWeight;
                        finalGrade = $"{finalMark:F1}%";
                    }
                    rowData.Add(finalGrade);

                    DrawTableRow(gfx, fontTable, leftMargin, y, maxWidth, rowData.ToArray(), colWidths.ToArray());
                    y += 15;
                }

                // Add summary statistics section
                y += 30;
                gfx.DrawLine(XPens.Gray, leftMargin, y, page.Width - rightMargin, y);
                y += 20;

                gfx.DrawString("Course Statistics:", fontTableBold, XBrushes.DarkBlue, leftMargin, y);
                y += 15;

                gfx.DrawString($"Total Participants with Submissions: {participantsWithSubmissions.Count}",
                    fontTable, XBrushes.Black, leftMargin, y);
                y += 12;

                gfx.DrawString($"Total Assignments: {assignments.Count}",
                    fontTable, XBrushes.Black, leftMargin, y);
                y += 12;

                // Calculate statistics for each assignment
                foreach (var assignment in assignments)
                {
                    var gradedSubmissions = assignment.Submissions
                        .Where(s => s.Grade != null)
                        .ToList();

                    if (gradedSubmissions.Any())
                    {
                        double average = gradedSubmissions.Average(s => s.Grade!.AssignmentMark);
                        double max = gradedSubmissions.Max(s => s.Grade!.AssignmentMark);
                        double min = gradedSubmissions.Min(s => s.Grade!.AssignmentMark);
                        int submittedCount = assignment.Submissions.Count;
                        int gradedCount = gradedSubmissions.Count;

                        string stats = $"{assignment.Title}: Avg: {average:F1}, Max: {max:F1}, Min: {min:F1} " +
                                     $"(Submitted: {submittedCount}, Graded: {gradedCount})";

                        // Handle text wrapping for long assignment names
                        if (stats.Length > 80)
                        {
                            var parts = SplitString(stats, 80);
                            foreach (var part in parts)
                            {
                                if (y > page.Height - 30)
                                {
                                    page = doc.AddPage();
                                    page.Size = PdfSharpCore.PageSize.A4;
                                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                    gfx = XGraphics.FromPdfPage(page);
                                    y = 40;
                                }
                                gfx.DrawString(part, fontTable, XBrushes.DarkBlue, leftMargin, y);
                                y += 12;
                            }
                        }
                        else
                        {
                            if (y > page.Height - 30)
                            {
                                page = doc.AddPage();
                                page.Size = PdfSharpCore.PageSize.A4;
                                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                                gfx = XGraphics.FromPdfPage(page);
                                y = 40;
                            }
                            gfx.DrawString(stats, fontTable, XBrushes.DarkBlue, leftMargin, y);
                            y += 12;
                        }
                    }
                }

                // REMOVED: Teaching staff information section (was enrollment-dependent)

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }

        private void DrawTableRow(XGraphics gfx, XFont font, double x, double y, double maxWidth,
            string[] texts, double[] columnWidths)
        {
            double currentX = x;

            for (int i = 0; i < texts.Length; i++)
            {
                double colWidth = maxWidth * columnWidths[i];
                var rect = new XRect(currentX, y, colWidth, 20);

                // Create string format for wrapping
                var format = new XStringFormat
                {
                    Alignment = XStringAlignment.Near,
                    LineAlignment = XLineAlignment.Near
                };

                gfx.DrawString(texts[i], font, XBrushes.Black, rect, format);
                currentX += colWidth;
            }
        }

        private List<string> SplitString(string text, int maxLength)
        {
            var parts = new List<string>();
            for (int i = 0; i < text.Length; i += maxLength)
            {
                parts.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
            }
            return parts;
        }
    }
}