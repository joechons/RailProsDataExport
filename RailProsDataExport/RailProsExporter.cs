using System;

namespace RailProsDataExport
{
    class RailProsExporter
    {
        static void Main(string[] args)
        {
            try
            {
                XSignalsDataContext XSignalsDC = new XSignalsDataContext();
                XSignalsDC.GetChangesFile();       
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}

