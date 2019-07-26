using FishbowlConnect.Json;
using FishbowlConnect.Json.APIObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishbowlConnect
{
    public partial class FishbowlSession
    {
        /// <summary>
        /// Prints any sepcified Fishbowl report to the specified printer. The printer must be found on the fishbowl server machine
        /// </summary>
        /// <param name="ReportName">Name of the Fishbowl report</param>
        /// <param name="PrinterName">Name of the printer to print to</param>

        /// <param name="ParameterList">List of parameters and values. Names must match the actula parameter name in iReport, not the displayed name in fishbowl</param>
        /// <param name="NumOfCopies">Number of copies to print</param>
        /// <returns></returns>
        public async Task PrintReportToPrinter(string ReportName, string PrinterName,  List<ReportParam> ParameterList, int NumOfCopies = 1)
        {
            if (string.IsNullOrEmpty(ReportName))
            {
                throw new ArgumentException("Report name cannot be missing");
            }

            if (string.IsNullOrEmpty(PrinterName))
            {
                throw new ArgumentException("Printer Name cannot be missing");
            }

            PrintReportToPrinterRq reportToPrinterRq = new PrintReportToPrinterRq();

            reportToPrinterRq.PrinterName = PrinterName;
            reportToPrinterRq.NumberOfCopies = NumOfCopies;
            reportToPrinterRq.ReportName = ReportName;

            if (ParameterList != null && ParameterList?.Count > 0)
            {
                reportToPrinterRq.ParameterList = new ParameterList();
                reportToPrinterRq.ParameterList.ReportParam = ParameterList;
            }

            PrintReportToPrinterRs reportToPrinterRs = await this.IssueJsonRequestAsync<PrintReportToPrinterRs>(reportToPrinterRq);


        }
    }
}
