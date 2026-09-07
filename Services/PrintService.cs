using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json;
using laptop_service.Models;
using System.Net.Http;
using System.Text;
using System.IO;
using System;

namespace laptop_service.Services
{
    public class PrintService
    {
        private readonly ILogger<PrintService> _logger;

        public PrintService(ILogger<PrintService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> ProcessPrintRequestAsync(PrintRequest request)
        {
            string html = request.Data_To_Print ?? string.Empty;

            bool isPrintAsImage = request.Print_As_Image != null &&
                                  (request.Print_As_Image.ToString() == "1" ||
                                   request.Print_As_Image.ToString().ToLower() == "true");

            // ── Regex-based Classifications to prevent conflicts ──
            bool isTestPrint = System.Text.RegularExpressions.Regex.IsMatch(html, @"TEST PRINT|Settings Test Print", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            bool isToken = !isTestPrint && System.Text.RegularExpressions.Regex.IsMatch(html, @"IS_DIRECT_BILL_TOKEN_FLOW|<title>Print Token</title>|TOKEN NUMBER:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            bool isBillOrInvoice = !isTestPrint && !isToken && System.Text.RegularExpressions.Regex.IsMatch(html, @"receipt-paper-sim|receipt-items-table|receipt-items-list|receipt-brand|grand-total-row|totals-row|Bill No:|Invoice|Grand Total", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            bool isKOT = !isTestPrint && !isToken && !isBillOrInvoice && System.Text.RegularExpressions.Regex.IsMatch(html, @"KOT No:|KOT ORDER|KITCHEN ORDER|KOT Token:", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            _logger.LogInformation("Print classification — isTestPrint:{TS} isToken:{T} isKOT:{K} isBillOrInvoice:{B} isPrintAsImage:{I}",
                isTestPrint, isToken, isKOT, isBillOrInvoice, isPrintAsImage);

            // ── ESC/POS-via-Agent Path (FAST path forwarding to local Print Agent) ──
            if ((request.Printer_Interface == "Printer Driver" || request.Printer_Interface == "Ethernet") && !string.IsNullOrWhiteSpace(request.Local_App_Url) && (!isPrintAsImage || isTestPrint || isKOT || isToken))
            {
                try
                {
                    _logger.LogInformation("[ESC/POS-Agent] Generating ESC/POS bytes for {Interface} path.", request.Printer_Interface);

                    // Extract driver name and Ethernet settings from Interface_Detail for the agent payload
                    string driverNameForAgent = string.Empty;
                    string ipAddressForAgent = string.Empty;
                    int portForAgent = 9100;
                    int agentCopies = request.No_Of_Print ?? 1;
                    if (agentCopies <= 0) agentCopies = 1;

                    if (!string.IsNullOrWhiteSpace(request.Interface_Detail))
                    {
                        try
                        {
                            using var dDoc = JsonDocument.Parse(request.Interface_Detail);
                            if (dDoc.RootElement.TryGetProperty("Printer_Driver", out var dp))
                                driverNameForAgent = dp.GetString() ?? string.Empty;
                            
                            if (dDoc.RootElement.TryGetProperty("IP_Address", out var ipProp))
                                ipAddressForAgent = ipProp.GetString() ?? string.Empty;

                            if (dDoc.RootElement.TryGetProperty("Port_Number", out var portProp))
                            {
                                if (portProp.ValueKind == JsonValueKind.Number)
                                    portForAgent = portProp.GetInt32();
                                else if (portProp.ValueKind == JsonValueKind.String && int.TryParse(portProp.GetString(), out int p))
                                    portForAgent = p;
                            }
                        }
                        catch { /* ignore parse errors */ }
                    }

                    // Generate ESC/POS bytes using the same methods as the Ethernet path
                    byte[] escposBytes;
                    string htmlContent = request.Data_To_Print ?? string.Empty;

                    if (isToken)
                    {
                        _logger.LogInformation("[ESC/POS-Agent] Routing to GetTokenReceiptBytes.");
                        escposBytes = RawPrinterHelper.GetTokenReceiptBytes(
                            htmlContent, request.Printer_Width,
                            request.Start_Command, request.End_Command,
                            request.Cash_Drawer_Command,
                            request.Start_Feed_Length, request.End_Feed_Length);
                    }
                    else if (isKOT)
                    {
                        _logger.LogInformation("[ESC/POS-Agent] Routing to GetKOTReceiptBytes.");
                        escposBytes = RawPrinterHelper.GetKOTReceiptBytes(
                            htmlContent, request.Printer_Width,
                            request.Start_Command, request.End_Command,
                            request.Cash_Drawer_Command,
                            request.Start_Feed_Length, request.End_Feed_Length,
                            request.Branch_Code, request.Till_Code);
                    }
                    else if (isBillOrInvoice)
                    {
                        _logger.LogInformation("[ESC/POS-Agent] Routing to GetReceiptBytes.");
                        escposBytes = RawPrinterHelper.GetReceiptBytes(
                            htmlContent, request.Printer_Width,
                            request.Start_Command, request.End_Command,
                            request.Cash_Drawer_Command,
                            request.Start_Feed_Length, request.End_Feed_Length);
                    }
                    else
                    {
                        // Test print or unknown — generate test receipt bytes
                        string testTarget = request.Printer_Interface == "Ethernet" ? $"{ipAddressForAgent}:{portForAgent}" : driverNameForAgent;
                        escposBytes = RawPrinterHelper.GetTestReceiptBytes(
                            testTarget, request.Printer_Interface ?? "Printer Driver",
                            request.Printer_Width ?? "80mm", "Test Print",
                            request.Test_Print_Text ?? string.Empty,
                            request.Start_Command, request.End_Command,
                            request.Cash_Drawer_Command,
                            request.Start_Feed_Length, request.End_Feed_Length);
                    }

                    string escposBase64 = Convert.ToBase64String(escposBytes);
                    _logger.LogInformation("[ESC/POS-Agent] Generated {Bytes} ESC/POS bytes. Sending to agent.", escposBytes.Length);

                    // Build the ESC/POS agent payload — forwards connection parameters so agent can print directly via TCP or raw driver
                    var agentPayload = new
                    {
                        mode = "escpos",
                        printer_interface = request.Printer_Interface ?? "Printer Driver",
                        printer_driver_name = driverNameForAgent,
                        printer_ip = ipAddressForAgent,
                        port = portForAgent,
                        no_of_print = agentCopies,
                        escpos_data = escposBase64
                    };

                    using (var agentClient = new HttpClient())
                    {
                        agentClient.Timeout = TimeSpan.FromSeconds(8);
                        var agentJson = JsonSerializer.Serialize(agentPayload);
                        var agentContent = new StringContent(agentJson, Encoding.UTF8, "application/json");

                        var agentResponse = await agentClient.PostAsync(request.Local_App_Url, agentContent);
                        if (agentResponse.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("[ESC/POS-Agent] Agent printed successfully via ESC/POS raw path.");
                            return true;
                        }
                        else
                        {
                            var errBody = await agentResponse.Content.ReadAsStringAsync();
                            _logger.LogWarning("[ESC/POS-Agent] Agent returned {Status}: {Error}. Falling back to direct ESC/POS.", agentResponse.StatusCode, errBody);
                            // Fall through to direct ESC/POS below
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[ESC/POS-Agent] ESC/POS agent path failed. Falling through to ESC/POS fallback.");
                    // Fall through to ESC/POS below
                }
            }

            // ── HTML Image Print Path (kept as fallback for non-Printer-Driver modes) ──────
            // Only runs for Ethernet + Print_As_Image mode, OR if Printer Driver path above failed.
            if (isPrintAsImage && !string.IsNullOrWhiteSpace(request.Local_App_Url))
            {
                try
                {
                    // Extract and replace base64 logo with a localhost URL so the
                    // print agent can load it from a static file instead of inline data.
                    string modifiedHtml = request.Data_To_Print ?? string.Empty;
                    var base64Match = System.Text.RegularExpressions.Regex.Match(modifiedHtml,
                        @"<img[^>]+src=""data:image/(png|jpeg|jpg);base64,([^""]+)""",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (base64Match.Success)
                    {
                        try
                        {
                            string extension = base64Match.Groups[1].Value;
                            string base64Data = base64Match.Groups[2].Value;
                            byte[] imageBytes = Convert.FromBase64String(base64Data);

                            string wwwrootPath = global::AppSettings.AppSetting.ServerwwwwrootPath;
                            if (string.IsNullOrWhiteSpace(wwwrootPath))
                                wwwrootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
                            if (!Directory.Exists(wwwrootPath))
                                Directory.CreateDirectory(wwwrootPath);

                            string filePath = Path.Combine(wwwrootPath, "logo_temp." + extension);
                            File.WriteAllBytes(filePath, imageBytes);

                            string imageUrl = $"http://localhost:5153/logo_temp.{extension}";
                            modifiedHtml = System.Text.RegularExpressions.Regex.Replace(
                                modifiedHtml,
                                @"src=""data:image/(png|jpeg|jpg);base64,[^""]+""",
                                $"src=\"{imageUrl}\"",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            request.Data_To_Print = modifiedHtml;
                            _logger.LogInformation("Saved base64 logo to {Path}, updated HTML src to {URL}", filePath, imageUrl);
                        }
                        catch (Exception imgEx)
                        {
                            _logger.LogError(imgEx, "Error processing base64 logo; continuing with original HTML.");
                        }
                    }

                    _logger.LogInformation("Forwarding receipt to image print agent at {URL}", request.Local_App_Url);
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(10);
                        var payload = new List<PrintRequest> { request };
                        var json = JsonSerializer.Serialize(payload);
                        var content = new StringContent(json, Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(request.Local_App_Url, content);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Image print agent printed successfully.");
                            return true;
                        }
                        else
                        {
                            var errorMsg = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning("Image agent returned {Status}: {Error}. Falling back to ESC/POS.", response.StatusCode, errorMsg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Image agent unreachable at {URL}. Falling back to ESC/POS.", request.Local_App_Url);
                }
            }

            // ── ESC/POS Path ─────────────────────────────────────────────────────────────
            // Extract printer name or connection details from Interface_Detail JSON
            string printerName = string.Empty;
            string ipAddress = string.Empty;
            int portNumber = 9100; // Default ESC/POS raw port

            if (!string.IsNullOrWhiteSpace(request.Interface_Detail))
            {
                try
                {
                    using var doc = JsonDocument.Parse(request.Interface_Detail);
                    var root = doc.RootElement;

                    if (request.Printer_Interface == "Printer Driver" && root.TryGetProperty("Printer_Driver", out var driverProp))
                    {
                        printerName = driverProp.GetString() ?? string.Empty;
                    }
                    else if (request.Printer_Interface == "Bluetooth" && root.TryGetProperty("Bluetooth_Name", out var btProp))
                    {
                        printerName = btProp.GetString() ?? string.Empty;
                    }
                    else if (request.Printer_Interface == "Ethernet")
                    {
                        if (root.TryGetProperty("IP_Address", out var ipProp))
                        {
                            ipAddress = ipProp.GetString() ?? string.Empty;
                        }
                        if (root.TryGetProperty("Port_Number", out var portProp))
                        {
                            if (portProp.ValueKind == JsonValueKind.Number)
                            {
                                portNumber = portProp.GetInt32();
                            }
                            else if (portProp.ValueKind == JsonValueKind.String && int.TryParse(portProp.GetString(), out int p))
                            {
                                portNumber = p;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Interface_Detail JSON: {Detail}", request.Interface_Detail);
                    printerName = request.Interface_Detail; // fallback
                }
            }

            printerName = printerName.Replace("\"", "").Trim();
            ipAddress = ipAddress.Replace("\"", "").Trim();

            _logger.LogInformation("ESC/POS path. Interface:{Interface} Printer:{Printer} IP:{IP}:{Port}",
                request.Printer_Interface, printerName, ipAddress, portNumber);

            byte[] printBytes;
            if (!string.IsNullOrWhiteSpace(html) && !isTestPrint)
            {
                if (isToken)
                {
                    // Direct Bill Token: token number, date/time, item list, grand total
                    _logger.LogInformation("Routing to GetTokenReceiptBytes.");
                    printBytes = RawPrinterHelper.GetTokenReceiptBytes(
                        request.Data_To_Print,
                        request.Printer_Width,
                        request.Start_Command,
                        request.End_Command,
                        request.Cash_Drawer_Command,
                        request.Start_Feed_Length,
                        request.End_Feed_Length
                    );
                }
                else if (isKOT)
                {
                    // Kitchen Order Token: KOT No, table, items list per kitchen
                    _logger.LogInformation("Routing to GetKOTReceiptBytes.");
                    printBytes = RawPrinterHelper.GetKOTReceiptBytes(
                        request.Data_To_Print,
                        request.Printer_Width,
                        request.Start_Command,
                        request.End_Command,
                        request.Cash_Drawer_Command,
                        request.Start_Feed_Length,
                        request.End_Feed_Length,
                        request.Branch_Code,
                        request.Till_Code
                    );
                }
                else
                {
                    // Receipt/Invoice: full bill with company header, items, totals, payment
                    _logger.LogInformation("Routing to GetReceiptBytes (ESC/POS fallback for receipt).");
                    printBytes = RawPrinterHelper.GetReceiptBytes(
                        request.Data_To_Print,
                        request.Printer_Width,
                        request.Start_Command,
                        request.End_Command,
                        request.Cash_Drawer_Command,
                        request.Start_Feed_Length,
                        request.End_Feed_Length
                    );
                }
            }
            else
            {
                // Generate raw test print bytes
                printBytes = RawPrinterHelper.GetTestReceiptBytes(
                    request.Printer_Interface == "Ethernet" ? $"{ipAddress}:{portNumber}" : printerName,
                    request.Printer_Interface ?? "Unknown",
                    request.Printer_Width ?? "80mm",
                    "Settings Test Print",
                    request.Test_Print_Text ?? string.Empty,
                    request.Start_Command,
                    request.End_Command,
                    request.Cash_Drawer_Command,
                    request.Start_Feed_Length,
                    request.End_Feed_Length
                );
            }

            int copies = request.No_Of_Print ?? 1;
            if (copies <= 0) copies = 1;

            bool overallSuccess = true;

            for (int i = 0; i < copies; i++)
            {
                bool success = false;
                if (request.Printer_Interface == "Ethernet")
                {
                    if (string.IsNullOrWhiteSpace(ipAddress))
                    {
                        _logger.LogError("Ethernet print failed: IP Address is empty.");
                        return false;
                    }
                    success = await SendBytesToEthernetAsync(ipAddress, portNumber, printBytes);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(printerName))
                    {
                        _logger.LogError("Printer connection failed: Printer Name is empty.");
                        return false;
                    }
                    success = RawPrinterHelper.SendBytesToPrinter(printerName, printBytes);
                }

                if (!success)
                {
                    overallSuccess = false;
                }
            }

            return overallSuccess;
        }


        private async Task<bool> SendBytesToEthernetAsync(string ipAddress, int port, byte[] bytes)
        {
            try
            {
                using var client = new TcpClient();
                // 3 seconds timeout for TCP socket connection
                var result = client.BeginConnect(ipAddress, port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                if (!success)
                {
                    _logger.LogError("Ethernet print connection timed out to {IP}:{Port}", ipAddress, port);
                    return false;
                }
                client.EndConnect(result);

                using var stream = client.GetStream();
                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();
                _logger.LogInformation("Print job sent successfully to Ethernet printer at {IP}:{Port}", ipAddress, port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ethernet printing to {IP}:{Port} failed", ipAddress, port);
                return false;
            }
        }
    }

    public static class RawPrinterHelper
    {
        public static int GetMaxColumns(string? printerWidth)
        {
            if (string.IsNullOrWhiteSpace(printerWidth))
            {
                return 32; // Default fallback
            }

            string trimWidth = printerWidth.Trim();
            if (int.TryParse(trimWidth, out int pixels))
            {
                if (pixels >= 1500) return 80; // A4 (8 inch)
                if (pixels >= 750)  return 64; // 4 inch
                if (pixels >= 640)  return 48; // 3 inch (80mm)
                if (pixels >= 500)  return 42; // 3 inch (72mm)
                return 32; // 2 inch
            }

            string lower = printerWidth.ToLower();
            if (lower.Contains("a4") || lower.Contains("8 inch"))
            {
                return 80;
            }
            if (lower.Contains("4 inch"))
            {
                return 64;
            }
            if (lower.Contains("3 inch"))
            {
                if (lower.Contains("80 mm")) return 48;
                if (lower.Contains("76 mm")) return 46;
                return 42; // default 3 inch (72mm)
            }
            if (lower.Contains("2 inch"))
            {
                return 32;
            }

            // Fallback: try parsing number of mm
            var match = System.Text.RegularExpressions.Regex.Match(printerWidth, @"(\d+)\s*mm", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int mm))
            {
                if (mm >= 190) return 80;
                if (mm >= 96) return 64;
                if (mm >= 72) return 42;
                return 32;
            }

            return 32;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string? pDatatype;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
        {
            IntPtr hPrinter = IntPtr.Zero;
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false;

            di.pDocName = "SmartPOS RAW Test Print";
            di.pDatatype = "RAW";

            if (OpenPrinter(szPrinterName, out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        int dwWritten = 0;
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            return bSuccess;
        }

        public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes)
        {
            IntPtr pBytes = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, pBytes, bytes.Length);
            bool bSuccess = SendBytesToPrinter(szPrinterName, pBytes, bytes.Length);
            Marshal.FreeHGlobal(pBytes);
            return bSuccess;
        }

        public static byte[] GetTestReceiptBytes(string printerName, string printInterface, string printerWidth, string printFrom, string testPrintText,
                                                 string? startCommand, string? endCommand, string? cashDrawerCommand, object? startFeed, object? endFeed)
        {
            int maxColumns = GetMaxColumns(printerWidth);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // 1. Execute Cash Drawer Command if configured
                byte[] drawerBytes = GetCommandBytes(cashDrawerCommand);
                if (drawerBytes.Length > 0)
                {
                    writer.Write(drawerBytes);
                }

                // 2. Execute Start Command (Printer Initialization) if configured, else fallback standard ESC @
                byte[] startBytes = GetCommandBytes(startCommand);
                if (startBytes.Length > 0)
                {
                    writer.Write(startBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x40 });
                }

                // 3. Execute Start Feed Length if configured
                int startFeedLines = ParseInt(startFeed, 0);
                if (startFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)startFeedLines });
                }

                // Select Font: Font B (small characters) for A4 / wide widths, otherwise Font A
                if (maxColumns > 48)
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x01 });
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x00 });
                }

                // Center alignment
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 });

                // Bold ON
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 });

                // Double height & width
                writer.Write(new byte[] { 0x1D, 0x21, 0x11 });

                writer.Write(System.Text.Encoding.ASCII.GetBytes("SMART POS\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("TEST PRINT\n\n"));

                // Normal size, Bold ON
                writer.Write(new byte[] { 0x1D, 0x21, 0x00 });
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', maxColumns) + "\n"));

                // Left alignment
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 });

                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Printer: {printerName}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Interface: {printInterface}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Width: {printerWidth}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Source: {printFrom}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Time: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n"));

                if (!string.IsNullOrWhiteSpace(testPrintText))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Message: {testPrintText}\n"));
                }

                // Center alignment
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 });
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', maxColumns) + "\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("TEST PRINT SUCCESSFUL\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("THANK YOU\n"));

                // 4. Execute End Feed Length (default 2 lines feed if not specified)
                int endFeedLines = ParseInt(endFeed, 2);
                if (endFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)endFeedLines });
                }

                // 5. Execute End Command (Cutter) if configured, else fallback standard GS V 66 0
                byte[] cutBytes = GetCommandBytes(endCommand);
                if (cutBytes.Length > 0)
                {
                    writer.Write(cutBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1D, 0x56, 0x42, 0x00 });
                }

                return ms.ToArray();
            }
        }


        public static byte[] GetKOTReceiptBytes(string html, string? printerWidth,
                                                string? startCommand, string? endCommand, string? cashDrawerCommand, object? startFeed, object? endFeed,
                                                string? branchCode, string? tillCode)
        {
            int maxColumns = GetMaxColumns(printerWidth);
            int targetWidth = maxColumns - 1;

            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // 1. Execute Cash Drawer Command if configured
                byte[] drawerBytes = GetCommandBytes(cashDrawerCommand);
                if (drawerBytes.Length > 0)
                {
                    writer.Write(drawerBytes);
                }

                // 2. Execute Start Command (Printer Initialization) if configured, else fallback standard ESC @
                byte[] startBytes = GetCommandBytes(startCommand);
                if (startBytes.Length > 0)
                {
                    writer.Write(startBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x40 });
                }

                // Disable Chinese mode & select PC437 code page to avoid garbled Chinese overlays
                writer.Write(new byte[] { 0x1C, 0x2E }); // FS .
                writer.Write(new byte[] { 0x1B, 0x74, 0x00 }); // ESC t 0

                // 3. Execute Start Feed Length if configured
                int startFeedLines = ParseInt(startFeed, 0);
                if (startFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)startFeedLines });
                }

                // Select Font: Font B (small characters) for A4 / wide widths, otherwise Font A
                if (maxColumns > 48)
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x01 });
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x00 });
                }

                string kitchenName = "KITCHEN";
                var kitchenMatch = System.Text.RegularExpressions.Regex.Match(html, @"KOT ORDER</div>\s*<div[^>]*>([^<]+)</div>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!kitchenMatch.Success)
                {
                    kitchenMatch = System.Text.RegularExpressions.Regex.Match(html, @"<h3[^>]*>([^<]+)</h3>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                if (kitchenMatch.Success)
                {
                    kitchenName = kitchenMatch.Groups[1].Value.Trim().ToUpper();
                }

                // Center alignment
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 });
                // Bold ON
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 });
                // Double height & width
                writer.Write(new byte[] { 0x1D, 0x21, 0x11 });
                writer.Write(System.Text.Encoding.ASCII.GetBytes("KOT ORDER\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"{kitchenName}\n\n"));

                // Reset to normal size, Bold ON
                writer.Write(new byte[] { 0x1D, 0x21, 0x00 });
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Left alignment
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 });

                string kotNo = "";
                var kotMatch = System.Text.RegularExpressions.Regex.Match(html, @"KOT Token:\s*<span[^>]*>([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!kotMatch.Success)
                {
                    kotMatch = System.Text.RegularExpressions.Regex.Match(html, @"<strong>KOT (?:No|Token):</strong>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                if (kotMatch.Success)
                {
                    kotNo = kotMatch.Groups[1].Value.Trim();
                }

                string table = "";
                var tableMatch = System.Text.RegularExpressions.Regex.Match(html, @"Table:\s*<span[^>]*>([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!tableMatch.Success)
                {
                    tableMatch = System.Text.RegularExpressions.Regex.Match(html, @"<strong>Table:</strong>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                if (tableMatch.Success)
                {
                    table = tableMatch.Groups[1].Value.Trim();
                }

                string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                var dateMatch = System.Text.RegularExpressions.Regex.Match(html, @"Date:\s*<span[^>]*>([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!dateMatch.Success)
                {
                    dateMatch = System.Text.RegularExpressions.Regex.Match(html, @"<strong>Date:</strong>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                }
                if (dateMatch.Success)
                {
                    date = dateMatch.Groups[1].Value.Trim();
                }

                writer.Write(System.Text.Encoding.ASCII.GetBytes($"KOT Token: {kotNo}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Table    : {table}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Date     : {date}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Left alignment for table header
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 });
                // Bold ON
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 });
                string header = "Item".PadRight(targetWidth - 3) + "Qty\n";
                writer.Write(System.Text.Encoding.ASCII.GetBytes(header));
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Reset Bold, Left align
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 });
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 });

                var tbodyMatch = System.Text.RegularExpressions.Regex.Match(html, @"<tbody>(.*?)</tbody>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (tbodyMatch.Success)
                {
                    string tbodyHtml = tbodyMatch.Groups[1].Value;
                    var rowMatches = System.Text.RegularExpressions.Regex.Matches(tbodyHtml, @"<tr[^>]*>(.*?)</tr>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    foreach (System.Text.RegularExpressions.Match row in rowMatches)
                    {
                        var cellMatches = System.Text.RegularExpressions.Regex.Matches(row.Groups[1].Value, @"<td[^>]*>(.*?)</td>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (cellMatches.Count >= 2)
                        {
                            string rawItemInfo = cellMatches[0].Groups[1].Value;
                            string rawQty = cellMatches[1].Groups[1].Value.Trim();

                            string itemName = rawItemInfo;
                            string note = "";
                            var noteMatch = System.Text.RegularExpressions.Regex.Match(rawItemInfo, @"\* Note:\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            if (noteMatch.Success)
                            {
                                note = noteMatch.Groups[1].Value.Trim();
                                int divIdx = rawItemInfo.IndexOf("<div");
                                if (divIdx >= 0)
                                {
                                    itemName = rawItemInfo.Substring(0, divIdx).Trim();
                                }
                            }
                            
                            // Strip any nested html tags to get plain item name
                            itemName = System.Text.RegularExpressions.Regex.Replace(itemName, @"<[^>]*>", "").Trim();
                            // Decode basic HTML entities
                            itemName = itemName.Replace("&nbsp;", " ").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">");

                            // Format: Item Name (left), Qty (right) dynamically scaled by maxColumns
                            int itemWidth = targetWidth - 7; // reserve 7 chars for quantity padding
                            if (itemWidth < 15) itemWidth = 15;
                            string itemLine = itemName;
                            if (itemLine.Length > itemWidth)
                            {
                                itemLine = itemLine.Substring(0, itemWidth);
                            }
                            int spacesNeeded = targetWidth - itemLine.Length - rawQty.Length;
                            if (spacesNeeded < 1) spacesNeeded = 1;
                            string spaces = new string(' ', spacesNeeded);
                            writer.Write(System.Text.Encoding.ASCII.GetBytes($"{itemLine}{spaces}{rawQty}\n"));

                            if (!string.IsNullOrEmpty(note))
                            {
                                writer.Write(System.Text.Encoding.ASCII.GetBytes($"  * Note: {note}\n"));
                            }
                        }
                    }
                }

                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 });
                string footerText = $"* Sent to {kitchenName} *\n";
                if (!string.IsNullOrEmpty(branchCode) || !string.IsNullOrEmpty(tillCode))
                {
                    footerText = $"* Sent to {kitchenName} *\nBranch: {branchCode ?? ""} | Till: {tillCode ?? ""}\n";
                }
                writer.Write(System.Text.Encoding.ASCII.GetBytes(footerText));

                // 4. Execute End Feed Length (default 2 lines feed if not specified)
                int endFeedLines = ParseInt(endFeed, 2);
                if (endFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)endFeedLines });
                }

                // 5. Execute End Command (Cutter) if configured, else fallback standard GS V 66 0
                byte[] cutBytes = GetCommandBytes(endCommand);
                if (cutBytes.Length > 0)
                {
                    writer.Write(cutBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1D, 0x56, 0x42, 0x00 });
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Generates ESC/POS raw bytes for a Direct Bill Token receipt.
        /// Parses the unique token HTML template (identified by IS_DIRECT_BILL_TOKEN_FLOW comment).
        /// Output matches the physical token layout: centered TOKEN header, date/time,
        /// large bold token number, item list (name + count), and right-aligned grand total.
        /// </summary>
        public static byte[] GetTokenReceiptBytes(string html, string? printerWidth,
                                                 string? startCommand, string? endCommand, string? cashDrawerCommand, object? startFeed, object? endFeed)
        {
            int maxColumns = GetMaxColumns(printerWidth);
            int targetWidth = maxColumns - 1;

            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // 1. Cash Drawer & Printer Initialization
                byte[] drawerBytes = GetCommandBytes(cashDrawerCommand);
                if (drawerBytes.Length > 0) writer.Write(drawerBytes);

                byte[] startBytes = GetCommandBytes(startCommand);
                if (startBytes.Length > 0) writer.Write(startBytes);
                else writer.Write(new byte[] { 0x1B, 0x40 }); // ESC @ (reset printer)

                // 2. Start Feed Lines
                int startFeedLines = ParseInt(startFeed, 0);
                if (startFeedLines > 0)
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)startFeedLines });

                // 3. Font A (normal)
                writer.Write(new byte[] { 0x1B, 0x4D, 0x00 });

                // 3.5 Parse and print custom header text if exists in token HTML
                var headerTextMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"class=""text-center direct-bill-header-text""[^>]*>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (headerTextMatch.Success)
                {
                    string headerText = headerTextMatch.Groups[1].Value.Trim().ToUpper();
                    writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // Center
                    writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                    writer.Write(System.Text.Encoding.ASCII.GetBytes(headerText + "\n\n"));
                    writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF
                }

                // 4. Header — center aligned, double height+width, bold: "TOKEN"
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // center
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // bold ON
                writer.Write(new byte[] { 0x1D, 0x21, 0x11 }); // double width+height
                writer.Write(System.Text.Encoding.ASCII.GetBytes("TOKEN\n"));
                writer.Write(new byte[] { 0x1D, 0x21, 0x00 }); // back to normal
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // bold OFF

                // 5. Dashed separator line
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // 6. Date & Time — left aligned
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 }); // left

                string date = DateTime.Now.ToString("dd/MM/yyyy");
                var dateMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"Date\s*:\s*([^<\n\r]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (dateMatch.Success) date = dateMatch.Groups[1].Value.Trim();

                string time = DateTime.Now.ToString("HH:mm:ss");
                var timeMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"Time\s*:\s*([^<\n\r]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (timeMatch.Success) time = timeMatch.Groups[1].Value.Trim();

                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Date : {date}\n"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Time : {time}\n"));

                // 7. Dashed separator line
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // 8. Token Number — center, large bold (double width+height)
                string tokenNo = "1";
                var tokenMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"Token Number:\s*([^<\n\r]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (tokenMatch.Success) tokenNo = tokenMatch.Groups[1].Value.Trim();

                writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // center
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // bold ON
                writer.Write(new byte[] { 0x1D, 0x21, 0x11 }); // double width+height
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Token Number: {tokenNo}\n"));
                writer.Write(new byte[] { 0x1D, 0x21, 0x00 }); // back to normal
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // bold OFF

                // 9. Dashed separator line
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // 10. Item list header — left, bold
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 }); // left
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // bold ON
                int countColWidth = 5; // fixed right column width for Qty ("Count" header)
                // "Item Name" left-padded to fill (targetWidth - countColWidth), "Count" right-justified
                string colHeader = "Item Name".PadRight(targetWidth - countColWidth)
                                 + "Count".PadLeft(countColWidth) + "\n";
                writer.Write(System.Text.Encoding.ASCII.GetBytes(colHeader));
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // bold OFF

                // 11. Dashed separator line
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // 12. Parse item rows from d-flex divs: <span>Name</span><span>Qty</span>
                var itemMatches = System.Text.RegularExpressions.Regex.Matches(
                    html,
                    @"<div[^>]*class=""d-flex justify-content-between""[^>]*>\s*<span>([^<]+)</span>\s*<span>(\d+)</span>\s*</div>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

                foreach (System.Text.RegularExpressions.Match m in itemMatches)
                {
                    string itemName = m.Groups[1].Value.Trim().ToUpper();
                    string qty      = m.Groups[2].Value.Trim();

                    // Max name width = total width minus right qty column (countColWidth)
                    int nameWidth = targetWidth - countColWidth;
                    if (itemName.Length > nameWidth) itemName = itemName.Substring(0, nameWidth);

                    // Left-pad name, right-justify qty in fixed countColWidth column
                    // This matches the "Item Name" / "Count" header alignment exactly
                    string itemLine = itemName.PadRight(nameWidth)
                                    + qty.PadLeft(countColWidth) + "\n";
                    writer.Write(System.Text.Encoding.ASCII.GetBytes(itemLine));
                }

                // 13. Dashed separator line
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // 14. Grand Total — centered, double-height bold (prominent, like Token Number)
                string grandTotal = "0";
                var totalMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"Grand Total\s*:\s*[^0-9]*([\d\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (totalMatch.Success) grandTotal = totalMatch.Groups[1].Value.Trim();

                writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // center
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // bold ON
                writer.Write(new byte[] { 0x1D, 0x21, 0x01 }); // double height only
                writer.Write(System.Text.Encoding.ASCII.GetBytes($"Grand Total : Rs.{grandTotal}\n"));
                writer.Write(new byte[] { 0x1D, 0x21, 0x00 }); // back to normal
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // bold OFF

                // 14.5 Parse and print custom footer text if exists in token HTML
                var footerTextMatch = System.Text.RegularExpressions.Regex.Match(
                    html, @"class=""text-center direct-bill-footer-text""[^>]*>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (footerTextMatch.Success)
                {
                    string footerText = footerTextMatch.Groups[1].Value.Trim().ToUpper();
                    writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n")); // Dashed separator
                    writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // Center
                    writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                    writer.Write(System.Text.Encoding.ASCII.GetBytes(footerText + "\n"));
                    writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF
                }
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 }); // reset to left

                // 15. End Feed Lines
                int endFeedLines = ParseInt(endFeed, 3);
                if (endFeedLines > 0)
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)endFeedLines });

                // 16. End Command (Cutter) or fallback GS V 66 0
                byte[] cutBytes = GetCommandBytes(endCommand);
                if (cutBytes.Length > 0) writer.Write(cutBytes);
                else writer.Write(new byte[] { 0x1D, 0x56, 0x42, 0x00 });

                return ms.ToArray();
            }
        }

        public static byte[] GetReceiptBytes(string html, string? printerWidth,
                                             string? startCommand, string? endCommand, string? cashDrawerCommand, object? startFeed, object? endFeed)
        {
            int maxColumns = GetMaxColumns(printerWidth);
            int targetWidth = maxColumns - 1;

            using (var ms = new System.IO.MemoryStream())
            using (var writer = new System.IO.BinaryWriter(ms))
            {
                // 1. Execute Cash Drawer Command if configured
                byte[] drawerBytes = GetCommandBytes(cashDrawerCommand);
                if (drawerBytes.Length > 0)
                {
                    writer.Write(drawerBytes);
                }

                // 2. Execute Start Command (Printer Initialization) if configured, else fallback standard ESC @
                byte[] startBytes = GetCommandBytes(startCommand);
                if (startBytes.Length > 0)
                {
                    writer.Write(startBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x40 });
                }

                // 3. Execute Start Feed Length if configured
                int startFeedLines = ParseInt(startFeed, 0);
                if (startFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)startFeedLines });
                }

                // Font selection
                if (maxColumns > 48)
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x01 });
                }
                else
                {
                    writer.Write(new byte[] { 0x1B, 0x4D, 0x00 });
                }

                // Align Center
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 });

                // Brand/Company Name
                var brandMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div class=""receipt-brand""[^>]*>([^<]+)</div>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (brandMatch.Success)
                {
                    writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                    writer.Write(new byte[] { 0x1D, 0x21, 0x11 }); // Double height/width
                    writer.Write(System.Text.Encoding.ASCII.GetBytes(brandMatch.Groups[1].Value.Trim() + "\n"));
                    writer.Write(new byte[] { 0x1D, 0x21, 0x00 }); // Normal size
                    writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF
                }

                // Header & Address Metadata
                var addressMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div class=""receipt-address""[^>]*>(.*?)</div>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (addressMatch.Success)
                {
                    string addrHtml = addressMatch.Groups[1].Value;
                    string addrText = System.Text.RegularExpressions.Regex.Replace(addrHtml, @"<[^>]*>", "\n").Trim();
                    foreach (var line in addrText.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            writer.Write(System.Text.Encoding.ASCII.GetBytes(line.Trim() + "\n"));
                        }
                    }
                }
                
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Align Left for metadata
                writer.Write(new byte[] { 0x1B, 0x61, 0x00 });

                // Invoice metadata parsing
                var invoiceMatch = System.Text.RegularExpressions.Regex.Match(html, @"Invoice</span>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (invoiceMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Invoice  : {invoiceMatch.Groups[1].Value.Trim()}\n"));
                }

                // Date
                var dateMatch = System.Text.RegularExpressions.Regex.Match(html, @"info-column-right"">[\s\r\n]*<div>([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Date     : {dateMatch.Groups[1].Value.Trim()}\n"));
                }

                // Table
                var tableMatch = System.Text.RegularExpressions.Regex.Match(html, @"Table</span>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (tableMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Table    : {tableMatch.Groups[1].Value.Trim()}\n"));
                }

                // Customer
                var customerMatch = System.Text.RegularExpressions.Regex.Match(html, @"Customer</span>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (customerMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Customer : {customerMatch.Groups[1].Value.Trim()}\n"));
                }

                // Type
                var typeMatch = System.Text.RegularExpressions.Regex.Match(html, @"Type</span>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (typeMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Type     : {typeMatch.Groups[1].Value.Trim()}\n"));
                }

                // Cashier
                var cashierMatch = System.Text.RegularExpressions.Regex.Match(html, @"Cashier</span>\s*([^<]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (cashierMatch.Success)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"Cashier  : {cashierMatch.Groups[1].Value.Trim()}\n"));
                }

                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Table Items List Header
                writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                string itemHeader = "Item Description".PadRight(targetWidth - 12) + "Qty      Amt\n";
                writer.Write(System.Text.Encoding.ASCII.GetBytes(itemHeader));
                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));
                writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF

                // Parse and print items from buildReceiptHtml() output.
                // buildReceiptHtml generates: <div class="receipt-items-list">...<div class="receipt-item-row">...</div>...</div>
                // The lazy .*? stops at FIRST </div>, missing nested div content — use lookahead pattern instead.
                var itemsListMatch = System.Text.RegularExpressions.Regex.Match(
                    html,
                    @"<div class=""receipt-items-list""[^>]*>([\s\S]+?)<div class=""receipt-divider-solid""",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (itemsListMatch.Success)
                {
                    string itemsHtml = itemsListMatch.Groups[1].Value;
                    // Split on each receipt-item-row start tag (case-insensitive)
                    var rows = System.Text.RegularExpressions.Regex.Split(
                        itemsHtml,
                        @"<div class=""receipt-item-row""",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    foreach (var rowHtml in rows)
                    {
                        if (string.IsNullOrWhiteSpace(rowHtml)) continue;

                        string itemName = "";
                        string qty = "1";
                        string price = "0.00";
                        string total = "0.00";

                        // Item name from <span class="bold">NAME</span>
                        var nameMatch = System.Text.RegularExpressions.Regex.Match(
                            rowHtml,
                            @"<span class=""bold""[^>]*>([\s\S]+?)</span>",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (nameMatch.Success)
                        {
                            itemName = System.Text.RegularExpressions.Regex.Replace(nameMatch.Groups[1].Value, @"<[^>]*>", "").Trim();
                        }

                        // Qty x Price — buildReceiptHtml format: "1 x 210.00"
                        var qtyPriceMatch = System.Text.RegularExpressions.Regex.Match(
                            rowHtml,
                            @"(\d+)\s*x\s*([\d\.]+)",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (qtyPriceMatch.Success)
                        {
                            qty = qtyPriceMatch.Groups[1].Value;
                            price = qtyPriceMatch.Groups[2].Value;
                            // Compute total = qty x price
                            if (decimal.TryParse(qty, out decimal q) && decimal.TryParse(price, out decimal p))
                            {
                                total = (q * p).ToString("0.00");
                            }
                        }

                        // Try to get explicit total from receipt-item-right div (last decimal number)
                        var rightDivMatch = System.Text.RegularExpressions.Regex.Match(
                            rowHtml,
                            @"<div class=""receipt-item-right""[^>]*>([\s\S]+?)</div>",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        if (rightDivMatch.Success)
                        {
                            var rightText = System.Text.RegularExpressions.Regex.Replace(rightDivMatch.Groups[1].Value, @"<[^>]*>", " ").Trim();
                            var allAmounts = System.Text.RegularExpressions.Regex.Matches(rightText, @"[\d]+\.[\d]+");
                            if (allAmounts.Count >= 2)
                            {
                                total = allAmounts[allAmounts.Count - 1].Value; // Last decimal = total
                            }
                        }

                        itemName = itemName.Replace("&nbsp;", " ").Replace("&amp;", "&");

                        // Format line: Description | Qty | Total
                        int descWidth = targetWidth - 15;
                        if (descWidth < 10) descWidth = 10;
                        string descPart = itemName.Length > descWidth ? itemName.Substring(0, descWidth) : itemName;

                        string qtyPart = qty.PadRight(4);
                        string totalPart = total.PadLeft(8);

                        writer.Write(System.Text.Encoding.ASCII.GetBytes(
                            descPart.PadRight(descWidth) + "  " + qtyPart + " " + totalPart + "\n"));
                    }
                }
                else
                {
                    // Fallback table-based parser
                    var fallbackTableMatch = System.Text.RegularExpressions.Regex.Match(html, @"<table[^>]*class=""[^""]*receipt-items-table[^""]*""[^>]*>.*?<tbody>(.*?)</tbody>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (fallbackTableMatch.Success)
                    {
                        string tbodyHtml = fallbackTableMatch.Groups[1].Value;
                        var rows = System.Text.RegularExpressions.Regex.Matches(tbodyHtml, @"<tr[^>]*>(.*?)</tr>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        string currentItemName = "";
                        foreach (System.Text.RegularExpressions.Match row in rows)
                        {
                             string rowContent = row.Groups[1].Value;
                             if (rowContent.Contains("colspan=\"4\"") || rowContent.Contains("colspan='4'"))
                             {
                                 currentItemName = System.Text.RegularExpressions.Regex.Replace(rowContent, @"<[^>]*>", "").Trim();
                                 currentItemName = currentItemName.Replace("&nbsp;", " ").Replace("&amp;", "&");
                                 continue;
                             }

                             var cells = System.Text.RegularExpressions.Regex.Matches(rowContent, @"<td[^>]*>(.*?)</td>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                             if (cells.Count >= 4)
                             {
                                 string cell0 = System.Text.RegularExpressions.Regex.Replace(cells[0].Groups[1].Value, @"<[^>]*>", "").Trim();
                                 string cell1 = System.Text.RegularExpressions.Regex.Replace(cells[1].Groups[1].Value, @"<[^>]*>", "").Trim();
                                 string cell2 = System.Text.RegularExpressions.Regex.Replace(cells[2].Groups[1].Value, @"<[^>]*>", "").Trim();
                                 string cell3 = System.Text.RegularExpressions.Regex.Replace(cells[3].Groups[1].Value, @"<[^>]*>", "").Trim();

                                 string itemName = string.IsNullOrEmpty(cell0) ? currentItemName : cell0;
                                 string price = cell1;
                                 string qty = cell2;
                                 string total = cell3;

                                 itemName = itemName.Replace("&nbsp;", " ").Replace("&amp;", "&");

                                 int descWidth = targetWidth - 15;
                                 if (descWidth < 10) descWidth = 10;
                                 string descPart = itemName;
                                 if (descPart.Length > descWidth)
                                 {
                                     descPart = descPart.Substring(0, descWidth);
                                 }

                                 string qtyPart = qty.PadRight(4);
                                 string totalPart = total.PadLeft(8);

                                 string line = descPart.PadRight(descWidth) + "  " + qtyPart + " " + totalPart + "\n";
                                 writer.Write(System.Text.Encoding.ASCII.GetBytes(line));

                                 if (!string.IsNullOrEmpty(price) && price != "0.00")
                                 {
                                     writer.Write(System.Text.Encoding.ASCII.GetBytes($"  {qty} x {price}\n"));
                                 }
                             }
                         }
                     }
                 }

                writer.Write(System.Text.Encoding.ASCII.GetBytes(new string('-', targetWidth) + "\n"));

                // Totals
                writer.Write(new byte[] { 0x1B, 0x61, 0x02 }); // Align Right
                var totalsRows = System.Text.RegularExpressions.Regex.Matches(html, @"<div class=""totals-row"">[\s\r\n]*<span>([^<]+)</span>[\s\r\n]*<span>([^<]+)</span>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (totalsRows.Count > 0)
                {
                    foreach (System.Text.RegularExpressions.Match tr in totalsRows)
                    {
                        string label = tr.Groups[1].Value.Trim();
                        string val = tr.Groups[2].Value.Trim();
                        string line = $"{label}: {val}\n";
                        writer.Write(System.Text.Encoding.ASCII.GetBytes(line));
                    }
                }
                else
                {
                    // Fallback totals matching
                    var subtotalM = System.Text.RegularExpressions.Regex.Match(html, @"<td>Subtotal</td>[\s\r\n]*<td[^>]*>(?:[^<]*)?([\d\.,]+)</td>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (subtotalM.Success)
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"Subtotal: {subtotalM.Groups[1].Value.Trim()}\n"));
                    }
                    var taxM = System.Text.RegularExpressions.Regex.Match(html, @"<td>Total Tax</td>[\s\r\n]*<td[^>]*>(?:[^<]*)?([\d\.,]+)</td>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (taxM.Success)
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"Total Tax: {taxM.Groups[1].Value.Trim()}\n"));
                    }
                    var discountM = System.Text.RegularExpressions.Regex.Match(html, @"<td>Discount</td>[\s\r\n]*<td[^>]*>(?:[^<]*)?([\d\.,]+)</td>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (discountM.Success)
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"Discount: {discountM.Groups[1].Value.Trim()}\n"));
                    }
                    var roundOffM = System.Text.RegularExpressions.Regex.Match(html, @"<td>Round Off</td>[\s\r\n]*<td[^>]*>(?:[^<]*)?([\d\.,\-]+)</td>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (roundOffM.Success)
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"Round Off: {roundOffM.Groups[1].Value.Trim()}\n"));
                    }
                }

                // Grand Total
                var grandTotalMatch = System.Text.RegularExpressions.Regex.Match(html, @"<div class=""grand-total-row"">[\s\r\n]*<span>([^<]+)</span>[\s\r\n]*<span>([^<]+)</span>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (grandTotalMatch.Success)
                {
                    writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                    writer.Write(System.Text.Encoding.ASCII.GetBytes($"{grandTotalMatch.Groups[1].Value.Trim()}: {grandTotalMatch.Groups[2].Value.Trim()}\n"));
                    writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF
                }
                else
                {
                    var fallbackTotalM = System.Text.RegularExpressions.Regex.Match(html, @"(?:<td>Total</td>|<tr class=""grand-total-preview"">.*?font-weight: bold[^>]*>Total</td>)[\s\r\n]*<td[^>]*>(?:[^<]*)?([\d\.,]+)</td>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (fallbackTotalM.Success)
                    {
                        writer.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"Total: {fallbackTotalM.Groups[1].Value.Trim()}\n"));
                        writer.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF
                    }
                }

                // Payment Summary
                var paymentRows = System.Text.RegularExpressions.Regex.Matches(html, @"<div class=""payment-row"">[\s\r\n]*<span>([^<]+)</span>[\s\r\n]*<span>([^<]+)</span>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (paymentRows.Count > 0)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("\nPayment Summary\n"));
                    foreach (System.Text.RegularExpressions.Match pr in paymentRows)
                    {
                        writer.Write(System.Text.Encoding.ASCII.GetBytes($"{pr.Groups[1].Value.Trim()}: {pr.Groups[2].Value.Trim()}\n"));
                    }
                }

                // Terms / Footer
                writer.Write(new byte[] { 0x1B, 0x61, 0x01 }); // Align Center
                var promoRows = System.Text.RegularExpressions.Regex.Matches(html, @"<div class=""promo-text""[^>]*>([^<]+)</div>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                foreach (System.Text.RegularExpressions.Match pr in promoRows)
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("\n" + pr.Groups[1].Value.Trim() + "\n"));
                }

                writer.Write(System.Text.Encoding.ASCII.GetBytes("\nTHANK YOU FOR VISITING\n"));

                // 4. Execute End Feed Length (default 2 lines feed if not specified)
                int endFeedLines = ParseInt(endFeed, 2);
                if (endFeedLines > 0)
                {
                    writer.Write(new byte[] { 0x1B, 0x64, (byte)endFeedLines });
                }

                // 5. Execute End Command (Cutter) if configured, else fallback standard GS V 66 0
                byte[] cutBytes = GetCommandBytes(endCommand);
                if (cutBytes.Length > 0)
                {
                    writer.Write(cutBytes);
                }
                else
                {
                    writer.Write(new byte[] { 0x1D, 0x56, 0x42, 0x00 });
                }

                return ms.ToArray();
            }
        }

        private static byte[] GetCommandBytes(string? commandName)
        {
            if (string.IsNullOrWhiteSpace(commandName) || commandName.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<byte>();
            }

            switch (commandName)
            {
                case "InitializePrinter":
                    return new byte[] { 27, 64 };
                case "OpenDrawer-1-DLE;DC4":
                    return new byte[] { 16, 20 };
                case "OpenDrawer-2-ESC;p;0":
                    return new byte[] { 27, 112, 0, 25, 250 };
                case "FullCut-1-GS;V;0":
                    return new byte[] { 29, 86, 0 };
                case "FullCut-2-GS;V;41;0":
                    return new byte[] { 29, 86, 65, 0 };
                case "FullCut-3-ESC;i":
                    return new byte[] { 27, 105 };
                case "FullCut-4-ESC;d;0":
                    return new byte[] { 27, 100, 0 };
                case "PartialCut-1-GS;V;1":
                    return new byte[] { 29, 86, 1 };
                case "PartialCut-2-GS;V;42;0":
                    return new byte[] { 29, 86, 66, 0 };
                case "PartialCut-3-ESC;m":
                    return new byte[] { 27, 109 };
                case "PartialCut-4-ESC;d;1":
                    return new byte[] { 27, 100, 1 };
                case "BitImageMode-ESC;*;!":
                    return new byte[] { 27, 42, 33 };
                case "BitImageMode-GS;v;0":
                    return new byte[] { 29, 118, 48 };
                default:
                    return Array.Empty<byte>();
            }
        }

        private static int ParseInt(object? val, int defaultVal)
        {
            if (val == null) return defaultVal;
            if (val is int i) return i;
            if (val is double d) return (int)d;
            if (val is float f) return (int)f;
            if (int.TryParse(val.ToString(), out int parsed))
            {
                return parsed;
            }
            return defaultVal;
        }
    }
}
