using IP3_8IEN.BL.Domain.Gebruikers;

namespace IP3_8IEN.BL
{
    public interface IGebruikerManager
    {
        //30 mrt 2018 : stephane
        void AddGebruikers(string filePath);
        void AddAlertInstelling(string filePath);
        Gebruiker FindUser(string username);

        //31 mrt 2018 : Stephane
        void AddAlerts(string filePath);
        void AddAlert(string alertContent, int alertInstelling);
    }
}
