using ActivityImporter.Engine.Graph.O365UsageReports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityImporter.Engine.Graph.O365UsageReports;


public interface IUserActivityLoader
{
    Task<List<TAbstractActivityRecord>> LoadReport<TAbstractActivityRecord>(DateTime dt, string reportGraphURL) where TAbstractActivityRecord : AbstractActivityRecord;
}
