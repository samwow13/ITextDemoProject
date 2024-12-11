using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Geom;
using iText.IO.Image;
using Microsoft.Data.Sqlite;
using Dapper;
using System.Windows.Forms;
using System.IO;
using iText.Html2pdf;
using System.Text;

namespace ITextDemo
{
    public class Program
    {
        /*
         * Application Constants and Theme Colors
         * ------------------------------------
         * Defines the core visual elements used throughout the application:
         * - Database connection string for SQLite
         * - Corporate color scheme:
         *   * Header: Professional blue for report headers and table headers
         *   * Alternate Row: Light gray for improved table readability
         *   * Total Section: Dark blue for emphasis on summary sections
         */
        private static readonly iText.Kernel.Colors.Color HEADER_BACKGROUND = new DeviceRgb(41, 128, 185);
        private static readonly iText.Kernel.Colors.Color ALTERNATE_ROW = new DeviceRgb(236, 240, 241);
        private static readonly iText.Kernel.Colors.Color TOTAL_BACKGROUND = new DeviceRgb(44, 62, 80);
        public const string ConnectionString = "Data Source=clients.db";

        [STAThread]
        static void Main(string[] args)
        {
            /*
             * Application Entry Point
             * ----------------------
             * Initializes the application with the following steps:
             * 1. Sets up the SQLite database and sample data
             * 2. Configures Windows Forms visual styles
             * 3. Launches the main application window
             */
            DatabaseInitializer.Initialize();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /*
         * Default Report Generation
         * -----------------------
         * Overloaded method that generates a report with default filename.
         * Delegates to the main report generation method using "ClientReport"
         * as the default prefix for the PDF filename.
         */
        public static string GenerateClientReport(List<Client> clients)
        {
            return GenerateClientReport(clients, "ClientReport");
        }

        /*
         * Customizable Report Generation
         * -----------------------------
         * Generates a professional PDF report for client data using iText 7.
         * The report includes:
         * 1. A styled header with the DHS logo and report title
         * 2. Generation timestamp
         * 3. Formatted table of client data with alternating row colors
         * 4. Total balance summary section
         * 
         * Layout Structure:
         * - Page Size: A4
         * - Margins: 36 points on all sides
         * - Colors: 
         *   * Header Background: RGB(41, 128, 185) - Professional blue
         *   * Alternate Rows: RGB(236, 240, 241) - Light gray
         *   * Total Section: RGB(44, 62, 80) - Dark blue
         * 
         * Parameters:
         * - clients: List of Client objects to include in the report
         * - fileNamePrefix: Prefix for the generated PDF filename
         * 
         * Returns:
         * - Path to the generated PDF file
         */
        public static string GenerateClientReport(List<Client> clients, string fileNamePrefix)
        {
            string pdfPath = System.IO.Path.Combine(Application.StartupPath, $"{fileNamePrefix}.pdf");
            
            using var writer = new PdfWriter(pdfPath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf, PageSize.A4);
            
            // Configure page layout with professional margins
            document.SetMargins(36, 36, 36, 36);

            /*
             * Header Section
             * --------------
             * Creates a two-column header:
             * - Left column: Company logo (100px width)
             * - Right column: Report title with client name for individual reports
             * The header uses a blue background with white text for contrast
             */
            var header = new Table(new float[] { 1, 3 })
                .SetWidth(UnitValue.CreatePercentValue(100))  // Set width to 100% of available width
                .SetMarginLeft(0)
                .SetMarginRight(0);
            header.SetBackgroundColor(HEADER_BACKGROUND);
            header.SetPadding(20);

            // Add logo
            var projectDir = System.IO.Path.GetDirectoryName(Application.ExecutablePath) ?? "";
            var logoPath = System.IO.Path.Combine(projectDir, "..", "..", "..", "DHSLogo.jpg");
            logoPath = System.IO.Path.GetFullPath(logoPath);

            if (System.IO.File.Exists(logoPath))
            {
                var logo = ImageDataFactory.Create(logoPath);
                var img = new iText.Layout.Element.Image(logo).SetWidth(100);
                header.AddCell(new Cell()
                    .Add(img)
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPaddingRight(50)
                    .SetTextAlignment(TextAlignment.LEFT));
            }
            else
            {
                header.AddCell(new Cell().SetBorder(Border.NO_BORDER));
            }

            var title = clients.Count == 1 ? $"Client Report - {clients[0].Name}" : "Client Report";
            var headerCell = new Cell()
                .Add(new Paragraph(title)
                    .SetFontSize(24)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetTextAlignment(TextAlignment.LEFT))
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPaddingLeft(20);
            header.AddCell(headerCell);
            document.Add(header);

            // Add generation date with some spacing
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph($"Generated on: {DateTime.Now:MMMM dd, yyyy HH:mm:ss}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMarginLeft(0)  // Remove left margin
                .SetItalic());
            document.Add(new Paragraph("\n"));

            /*
             * Data Table Section
             * -----------------
             * Creates a formatted table with 7 columns for client information:
             * - Uses alternating row colors for better readability
             * - Headers are styled with blue background and white text
             * - Numeric values (Balance) are right-aligned
             * - All cells have consistent padding (5 points)
             */
            Table table = new Table(7).UseAllAvailableWidth();
            table.SetFontSize(10);
            
            // Style headers
            string[] headers = { "Name", "Address", "Account Number", "Balance", "Email Address", "Phone Number", "Last Transaction" };
            foreach (var headerText in headers)
            {
                table.AddHeaderCell(new Cell()
                    .Add(new Paragraph(headerText))
                    .SetBackgroundColor(HEADER_BACKGROUND)
                    .SetFontColor(ColorConstants.WHITE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBold());
            }

            // Add data with alternating row colors
            for (int i = 0; i < clients.Count; i++)
            {
                var client = clients[i];
                var rowColor = i % 2 == 0 ? ALTERNATE_ROW : ColorConstants.WHITE;
                
                AddStyledCell(table, client.Name, rowColor);
                AddStyledCell(table, client.Address, rowColor);
                AddStyledCell(table, client.AccountNumber, rowColor);
                AddStyledCell(table, $"${client.Balance:N2}", rowColor, TextAlignment.RIGHT);
                AddStyledCell(table, client.EmailAddress, rowColor);
                AddStyledCell(table, client.PhoneNumber, rowColor);
                AddStyledCell(table, client.LastTransactionDate, rowColor);
            }

            document.Add(table);

            /*
             * Total Balance Section
             * --------------------
             * Adds a summary section showing the total balance:
             * - Dark blue background for emphasis
             * - Two-column layout: "Total Balance" label and amount
             * - Right-aligned amount with currency formatting
             * - Only displayed if there are clients in the report
             */
            if (clients.Count > 0)
            {
                var totalBalance = clients.Sum(c => c.Balance);
                var totalTable = new Table(new float[] { 5, 2 })
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginTop(10);

                totalTable.AddCell(new Cell()
                    .Add(new Paragraph("Total Balance")
                        .SetFontSize(12)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(TOTAL_BACKGROUND)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(10));

                totalTable.AddCell(new Cell()
                    .Add(new Paragraph($"${totalBalance:N2}")
                        .SetFontSize(12)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBackgroundColor(TOTAL_BACKGROUND)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(10));

                document.Add(totalTable);
            }
                
            return pdfPath;
        }

        public static void GenerateHtmlReport(string outputPath)
        {
            // Read the HTML template
            string htmlTemplate = File.ReadAllText("ReportTemplate.html");
            
            // Get client data
            using var connection = new SqliteConnection(ConnectionString);
            var clients = connection.Query<Client>("SELECT * FROM Clients ORDER BY Name").ToList();
            
            // Generate client table HTML
            StringBuilder tableHtml = new StringBuilder();
            tableHtml.Append("<table>");
            tableHtml.Append("<tr><th>Name</th><th>Email</th><th>Phone</th><th>Balance</th></tr>");
            
            foreach (var client in clients)
            {
                tableHtml.Append($"<tr>");
                tableHtml.Append($"<td>{client.Name}</td>");
                tableHtml.Append($"<td>{client.EmailAddress}</td>");
                tableHtml.Append($"<td>{client.PhoneNumber}</td>");
                tableHtml.Append($"<td>${client.Balance:N2}</td>");
                tableHtml.Append($"</tr>");
            }
            tableHtml.Append("</table>");

            // Calculate totals
            decimal totalBalance = clients.Sum(c => c.Balance);
            
            // Read and encode logo
            byte[] logoBytes = File.ReadAllBytes("DHSLogo.jpg");
            string logoBase64 = Convert.ToBase64String(logoBytes);
            
            // Replace placeholders
            htmlTemplate = htmlTemplate.Replace("{logo-placeholder}", logoBase64);
            htmlTemplate = htmlTemplate.Replace("{client-table}", tableHtml.ToString());
            htmlTemplate = htmlTemplate.Replace("{total-clients}", clients.Count.ToString());
            htmlTemplate = htmlTemplate.Replace("{total-balance}", totalBalance.ToString("N2"));
            htmlTemplate = htmlTemplate.Replace("{timestamp}", DateTime.Now.ToString("g"));

            // Convert HTML to PDF using ConverterProperties
            using (var writer = new PdfWriter(outputPath))
            using (var pdf = new PdfDocument(writer))
            {
                ConverterProperties converterProperties = new ConverterProperties();
                HtmlConverter.ConvertToPdf(htmlTemplate, pdf, converterProperties);
            }
        }

        /*
         * Styled Cell Addition Helper
         * --------------------------
         * Utility method for adding consistently formatted cells to the report table.
         * Parameters:
         * - table: Target table to add the cell to
         * - content: Text content for the cell
         * - backgroundColor: Background color for the entire cell
         * - alignment: Text alignment within the cell (defaults to LEFT)
         * 
         * Styling:
         * - Consistent 5-point padding
         * - Configurable text alignment
         * - Background color for row alternation
         */
        private static void AddStyledCell(Table table, string content, iText.Kernel.Colors.Color backgroundColor, TextAlignment alignment = TextAlignment.LEFT)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(content))
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(alignment)
                .SetPadding(5));
        }
    }
}
