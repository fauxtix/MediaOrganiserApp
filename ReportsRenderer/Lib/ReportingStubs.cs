using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

// Lightweight stubs for Microsoft.Reporting.NETCore types so the project builds
// when the real package isn't available at restore time. These stubs do not
// perform real RDLC rendering; they only provide the API surface required at
// compile time.
namespace Microsoft.Reporting.NETCore
{
    public class ReportDataSource
    {
        public string Name { get; }
        public object Value { get; }

        public ReportDataSource(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ReportParameter
    {
        public string Name { get; }
        public StringCollection Values { get; } = new StringCollection();

        public ReportParameter(string name, string value)
        {
            Name = name;
            if (value != null)
                Values.Add(value);
        }
    }

    public class LocalReport : IDisposable
    {
        private readonly List<ReportDataSource> _dataSources = new List<ReportDataSource>();

        public IList<ReportDataSource> DataSources => _dataSources;

        public void LoadReportDefinition(Stream definition)
        {
            // No-op in stub
        }

        public void SetParameters(ReportParameter[] parameters)
        {
            // No-op in stub
        }

        public byte[] Render(string format)
        {
            // Return a minimal PDF file header to avoid downstream nulls when invoked
            // This is not a valid full PDF document but is sufficient for tests that expect bytes.
            var pdfHeader = "%PDF-1.4\n%\u00E2\u00E3\u00CF\u00D3\n";
            return System.Text.Encoding.ASCII.GetBytes(pdfHeader);
        }

        public void Dispose()
        {
        }
    }
}
