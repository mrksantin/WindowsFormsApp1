using System.Collections.Generic;

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
        public double SelectMid;
        public double Disperse;
        public double DeltaX;
        public double CoefficienAssim;
        public double Eccess;
        public double Xipow2calc;
        public double Xipow2tabl;
        public double Level;
    }   
}
