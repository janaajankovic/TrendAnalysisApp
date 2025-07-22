using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAnalysis.Contracts
{
    public interface IFileService
    {
        /// <param name="defaultFileName">Podrazumevano ime fajla.</param>
        /// <param name="filter">Filter za tipove fajlova (npr. "CSV files (*.csv)|*.csv|All files (*.*)|*.*").</param>
        /// <returns>Putanja do izabranog fajla, ili null ako korisnik otkaže dijalog.</returns>
        string SaveFile(string defaultFileName, string filter);
    }
}
