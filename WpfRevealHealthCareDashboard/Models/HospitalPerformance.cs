using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRevealHealthCareDashboard.Models
{
    public class HospitalPerformance
    {
        public DateTime Date { get; set; }

        public int Patients { get; set; }

        public string Gender { get; set; }

        public string PatientType { get; set; }

        public double BedOccupancyRate { get; set; }

        public string Doctor { get; set; }

        public string Specialist { get; set; }


    }
}
