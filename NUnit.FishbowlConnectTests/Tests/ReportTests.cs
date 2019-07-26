using FishbowlConnect;
using FishbowlConnect.Json.APIObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.FishbowlConnectTests.Tests
{

    [TestFixture]
    public class ReportTests
    {
        const string GoodServerAddress = "192.168.150.4";
        const string GoodUserName = "admin";
        const string GoodPassword = "does1tall";

        const string DatabaseAddress = "192.168.150.2";
        const int DatabasePort = 2361;
        const string DatabaseUser = "gone";
        const string DatabasePassword = "fishing";
        const string DatabaseName = "BRITEIDEASUPDATE";

        [TestCase("New Receipt 3in", "CutePDF Writer", 1)]
        public async Task PrintReportToPrinterTest(string ReportName, string PrinterName, int NumberOfCopies)
        {
            SessionConfig config = new SessionConfig(GoodServerAddress, 28192, GoodUserName, GoodPassword);



            using (FishbowlSession session = new FishbowlSession(config))
            {
                List<ReportParam> reportParams = new List<ReportParam>();

                reportParams.Add(new ReportParam { Name = "SONumber", Value = "71230" });

                await session.PrintReportToPrinter(ReportName, PrinterName, reportParams);

            }
        }
    }
}
