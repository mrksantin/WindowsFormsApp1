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
        public double z;
        public PointDooble() { }
        public PointDooble(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;

        }
    }
    public class Data
    {
        public List<double> MainList = new List<double>();
        public List<PointDooble> ResultList1 = new List<PointDooble>();
       // public List<PointDooble> ResultList2 = new List<PointDooble>();
      //  public List<PointDooble> ResultList3 = new List<PointDooble>();
      //  public List<PointDooble> ResultList4 = new List<PointDooble>();
        public double SelectMid;
        public double Disperse;
        public double DeltaX;
        public double CoefficienAssim;
        public double Eccess;
    }
    
    
}
