using Microsoft.AspNetCore.Mvc;

namespace laptop_service.Controllers
{
    [ApiController]
    [Route("api/attributes")]
    public class AttributesController : ControllerBase
    {
        [HttpGet("printer-width")]
        public IActionResult GetPrinterWidth()
        {
            var data = new[]
            {
                new { Printer_Width = "2 Inch / 48 mm", DPI = "203", Pixel = "384" },
                new { Printer_Width = "3 Inch / 72 mm", DPI = "203", Pixel = "576" },
                new { Printer_Width = "4 Inch / 96 mm", DPI = "203", Pixel = "768" },
                new { Printer_Width = "A4 / 8 Inch / 192 mm", DPI = "203", Pixel = "1536" },
                new { Printer_Width = "A4 / 8 Inch / 210 mm", DPI = "203", Pixel = "1680" },
                new { Printer_Width = "2 Inch / 46 mm", DPI = "203", Pixel = "368" },
                new { Printer_Width = "2 Inch / 50 mm", DPI = "203", Pixel = "400" },
                new { Printer_Width = "3 Inch / 64 mm", DPI = "203", Pixel = "512" },
                new { Printer_Width = "3 Inch / 76 mm", DPI = "203", Pixel = "608" },
                new { Printer_Width = "3 Inch / 80 mm", DPI = "203", Pixel = "640" },
                new { Printer_Width = "4 Inch / 100 mm", DPI = "203", Pixel = "800" },
                new { Printer_Width = "4 Inch / 104 mm", DPI = "203", Pixel = "832" },
                new { Printer_Width = "4 Inch / 108 mm", DPI = "203", Pixel = "864" },
                new { Printer_Width = "4 Inch / 112 mm", DPI = "203", Pixel = "896" }
            };
            return Ok(new { status = true, data });
        }

        [HttpGet("printer-interface")]
        public IActionResult GetPrinterInterface()
        {
            var data = new[]
            {
                new { Printer_Interface = "Printer Driver" },
                new { Printer_Interface = "Ethernet" },
                new { Printer_Interface = "Bluetooth" }
            };
            return Ok(new { status = true, data });
        }

        [HttpGet("escpos-command")]
        public IActionResult GetEscposCommand()
        {
            var data = new[]
            {
                new { Command_Name = "None", Command_Type = "Start", Command_Value = "" },
                new { Command_Name = "InitializePrinter", Command_Type = "Start", Command_Value = "27,64" },

                new { Command_Name = "None", Command_Type = "Drawer", Command_Value = "" },
                new { Command_Name = "OpenDrawer-1-DLE;DC4", Command_Type = "Drawer", Command_Value = "16,20" },
                new { Command_Name = "OpenDrawer-2-ESC;p;0", Command_Type = "Drawer", Command_Value = "27,112,0,25,250" },

                new { Command_Name = "None", Command_Type = "End", Command_Value = "" },
                new { Command_Name = "FullCut-1-GS;V;0", Command_Type = "End", Command_Value = "29,86,0" },
                new { Command_Name = "FullCut-2-GS;V;41;0", Command_Type = "End", Command_Value = "29,86,65,0" },
                new { Command_Name = "FullCut-3-ESC;i", Command_Type = "End", Command_Value = "27,105" },
                new { Command_Name = "FullCut-4-ESC;d;0", Command_Type = "End", Command_Value = "27,100,0" },
                new { Command_Name = "PartialCut-1-GS;V;1", Command_Type = "End", Command_Value = "29,86,1" },
                new { Command_Name = "PartialCut-2-GS;V;42;0", Command_Type = "End", Command_Value = "29,86,66,0" },
                new { Command_Name = "PartialCut-3-ESC;m", Command_Type = "End", Command_Value = "27,109" },
                new { Command_Name = "PartialCut-4-ESC;d;1", Command_Type = "End", Command_Value = "27,100,1" },

                new { Command_Name = "None", Command_Type = "Image", Command_Value = "" },
                new { Command_Name = "BitImageMode-ESC;*;!", Command_Type = "Image", Command_Value = "27,42,33" },
                new { Command_Name = "BitImageMode-GS;v;0", Command_Type = "Image", Command_Value = "29,118,48" }
            };
            return Ok(new { status = true, data });
        }
    }
}
