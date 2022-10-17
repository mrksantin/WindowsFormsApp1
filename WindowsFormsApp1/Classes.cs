using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class PointDooble
    {
        public double x;
        public double y;
        public PointDooble() { }
        public PointDooble(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
    public class Data
    {
        public List<double> MainList = new List<double>();
        public List<PointDooble> ResultList1 = new List<PointDooble>();
        public List<PointDooble> ResultList2 = new List<PointDooble>();
        public List<PointDooble> ResultList3 = new List<PointDooble>();
        public double SelectMid;
        public double Disperse;
        public double DeltaX;
        public double CoefficienAssim;
        public double Eccess;

    }

}
