using IP_8IEN.BL.Domain.Dashboard;
using IP_8IEN.BL.Domain.Gebruikers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_8IEN.BL.Domain.Data
{
    public class Persoon : Onderwerp
    {
        //Achternaam kan meerdere woorden bevatten vb: "van de .."
        public string Naam { get; set; }

        public ICollection<Tewerkstelling> Tewerkstellingen { get; set; }
    }
}
