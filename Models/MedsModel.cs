using System.Globalization;

namespace Aspnet.React.Models
{
    public class Medication
    {
        public string Ndc { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public string Packaging { get; set; }
    }

    public class MedicationMeta
    {
        public int Total { get; set; }
        public int Num_Oral { get; set; }
        public int Num_Topical { get; set; }
        public int Num_Other { get; set; }
    }
}