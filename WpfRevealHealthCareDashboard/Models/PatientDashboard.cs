using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfRevealHealthCareDashboard.Models
{
    public class PatientDashboard
    {
        public DateTime Date { get; set; }

        public string Gender { get; set; }

        public string PatientType { get; set; }

        public string Patient { get; set; }

        public int Weight { get; set; }

        public double HeartRate { get; set; }

        public string Age { get; set; }

        public string VisitReason { get; set; }

        public string MedicationGiven { get; set; }
    }
}
