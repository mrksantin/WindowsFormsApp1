using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using Excel = Microsoft.Office.Interop.Excel;

namespace WindowsFormsApp1
{    
    public partial class Form1 : Form
    {
        
        public List<double> MainList;
        public Data Data;
        
        public Form1()
        {
            InitializeComponent();
            chart1.Series[0]["PointWidth"] = "1";
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Data = new Data();
                chart1.Series[0].Points.Clear();
                chart2.Series[0].Points.Clear();

                MainList = new List<double>();
                for (int i = 0; i < dataGridView1.RowCount - 1; i++)
                {
                    double x;
                    double.TryParse(dataGridView1[0, i].Value.ToString(), out x);
                    MainList.Add(x);
                }
                Data.SelectMid = MainList.Sum() / MainList.Count;
                label1.Text = "Выборочная средняя = " + Math.Round(Data.SelectMid,3);
                Data.Disperse = MainList.Sum(x => Math.Pow(x - Data.SelectMid, 2)) / (MainList.Count - 1);
                label2.Text = "Оценка дисперсии = " + Math.Round(Data.Disperse,3);
                Data.DeltaX = (MainList.Max() - MainList.Min()) / (1 + 3.22 * Math.Log(MainList.Count));
                double Xn = MainList.Min();
                MainList.Sort();
                Data.MainList = MainList;
                dataGridView2.Rows.Clear();
                
                double probab = 0;
                Excel.Application excel = new Excel.Application();
                while (Xn < MainList.Max())
                {
                    double XnNext = Xn + Data.DeltaX;
                    int n = 0;
                    double p = 0;
                    for (int i = 0; i < MainList.Count; i++)
                    {
                        if ((MainList[i] <= XnNext) && (MainList[i] >= Xn))
                        {
                            n++;
                        }
                    }
                    
                    
                    chart1.Series[0].Points.AddXY(Math.Round(Xn + Data.DeltaX / 2, 3), (double)n / (double)MainList.Count);
                    p = excel.WorksheetFunction.Norm_Dist(XnNext, Data.SelectMid, Math.Sqrt(Data.Disperse), true) - excel.WorksheetFunction.Norm_Dist(Xn, Data.SelectMid, Math.Sqrt(Data.Disperse), true);
                    dataGridView2.Rows.Add($"{Math.Round(Xn, 3)}-{Math.Round(XnNext, 3)}", Math.Round((double)n / (double)MainList.Count,3), Math.Round(p,3));
                    Data.ResultList1.Add(new PointDooble(Xn, n, p));                  
                    probab += (double)n / (double)MainList.Count;
                    chart2.Series[0].Points.AddXY(Math.Round(Xn, 3), probab);
                    Xn = XnNext;
                }
                chart2.Series[0].Points.AddXY(Math.Round(Xn + Data.DeltaX, 3), probab);
                Data.CoefficienAssim = Data.MainList.Sum(x => Math.Pow(x - Data.SelectMid, 3) / ((Data.MainList.Count - 1) * Math.Pow(Data.Disperse, 1.5)));
                label3.Text = "Коэффициент ассиметрии = " + Math.Round(Data.CoefficienAssim, 3);
                Data.Eccess = Data.MainList.Sum(x => Math.Pow(x - Data.SelectMid, 4) / ((Data.MainList.Count - 1) * Math.Pow(Data.Disperse, 2))) - 3;
                label4.Text = "Эксцесс = " + Math.Round(Data.Eccess, 3);
                {
                    Data.Xipow2calc = Data.ResultList1.Sum(x => Math.Pow((x.y - Data.MainList.Count() * x.z), 2) / (Data.MainList.Count() * x.z));
                    label6.Text = "Хи ^ 2 выч = " + Data.Xipow2calc;
                    
                    double.TryParse(textBox1.Text, out Data.Level);
                    if (Data.Level < 0)
                    {
                        Data.Level = 0.05;
                    }
                    else if (Data.Level > 1)
                    {
                        Data.Level = 0.95;
                    }
                    Data.Xipow2tabl = excel.WorksheetFunction.ChiSq_Inv_RT(Data.Level, MainList.Count - 1);
                    label5.Text = "Хи ^ 2 табл = " + Data.Xipow2tabl;

                    if ((Data.CoefficienAssim == 0) && (Data.Eccess == 0) || (Data.Xipow2calc <= Data.Xipow2tabl))
                    {
                        if((Data.CoefficienAssim == 0) && (Data.Eccess == 0) && (Data.Xipow2calc <= Data.Xipow2tabl))
                        {
                            MessageBox.Show("Эмпирический закон случайной величины \nсоответствует теоретическому нормальному закону.");
                        }
                        else
                        {
                            if (Data.Xipow2calc <= Data.Xipow2tabl)
                            {
                                MessageBox.Show("Эмпирический закон случайной величины \nсоответствует теоретическому нормальному закону, но только по критерию Пирсона.");
                            }
                            else
                            {
                                MessageBox.Show("Эмпирический закон случайной величины \nсоответствует теоретическому нормальному закону,\nно только по значениям коэффициента асимметрии А и эксцесса Е");
                            }                            
                        }                        
                    }                                      
                    else
                    {
                        MessageBox.Show("Эмпирический закон случайной величины \nне соответствует теоретическому нормальному закону.");
                    }
                }
            }            
            catch (Exception exp)
            {
                MessageBox.Show($"Введенные вами данные вызвали исключение: {exp}, пожалуйста введите корректные данные");
            } 

        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "xml Файл|*.xml";
            XmlSerializer serializer = new XmlSerializer(typeof(Data));
            if (saveFile.ShowDialog()== DialogResult.OK)
            {
                using (FileStream file = new FileStream(saveFile.FileName, FileMode.OpenOrCreate))
                {
                    serializer.Serialize(file, Data);
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {   
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "xml Файл|*.xml";
            XmlSerializer deserializer = new XmlSerializer(typeof(Data));
            if (openFile.ShowDialog() == DialogResult.OK)
            {     
                using (FileStream file = new FileStream(openFile.FileName, FileMode.OpenOrCreate))
                {
                    Data = (Data)deserializer.Deserialize(file);
                }                    
            }
            label1.Text = "Выборочная средняя = " + Math.Round(Data.SelectMid);
            label2.Text = "Оценка дисперсии = " + Math.Round(Data.Disperse);
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
                    
            chart1.Series[0].Points.Clear();
            chart2.Series[0].Points.Clear();
            foreach (double x in Data.MainList)
            {
                dataGridView1.Rows.Add(x);
            }
            double probab = 0; 
            foreach (PointDooble point in Data.ResultList1)
            {
                dataGridView2.Rows.Add($"{Math.Round(point.x,3)} - {Math.Round(point.x + Data.DeltaX,3)}", Math.Round((double)point.y / (double)Data.MainList.Count, 3), point.z);
                chart1.Series[0].Points.AddXY(Math.Round(point.x + Data.DeltaX / 2, 3), point.y);
                probab += (double)point.y / (double)Data.MainList.Count;
                chart2.Series[0].Points.AddXY(Math.Round(point.x, 3), Math.Round(probab, 3));
            }
            chart2.Series[0].Points.AddXY(Math.Round(Data.ResultList1.Last().x+Data.DeltaX, 3), Math.Round(probab, 3));
            label3.Text = "Коэффициент ассиметрии = " + Math.Round(Data.CoefficienAssim,3);
            label4.Text = "Эксцесс = " + Math.Round(Data.Eccess,3);
            label5.Text = "Хи ^ 2 табл = " + Data.Xipow2tabl;
            label6.Text = "Хи ^ 2 выч = " + Data.Xipow2calc;
            textBox1.Text = Data.Level.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            List<double> List1 = new List<double>();
            List<double> List2 = new List<double>();
            for (int i = 0; i < dataGridView3.RowCount - 1; i++)
            {
                double x;
                if (dataGridView3[0, i].Value != null && double.TryParse(dataGridView3[0, i].Value.ToString(), out x))
                {
                    List1.Add(x);
                }
                if (dataGridView3[1, i].Value!=null && double.TryParse(dataGridView3[1, i].Value.ToString(), out x))
                {
                    List2.Add(x);
                }
            }
            double Disperce1 = List1.Sum(x => Math.Pow(x - List1.Sum() / List1.Count, 2)) / (List1.Count - 1);
            label9.Text = "Дисперсия 1 = " + Math.Round(Disperce1,3);
            double Disperce2 = List2.Sum(x => Math.Pow(x - List2.Sum() / List2.Count, 2)) / (List2.Count - 1);
            label10.Text = "Дисперсия 2 =" + Math.Round(Disperce2,3);
            double Fcalc;
            double Ftabl;
            double Level;
            Excel.Application excel = new Excel.Application();
            double.TryParse(textBox2.Text, out Level);
            if (Disperce1 > Disperce2)
            {
                Fcalc = (double)Disperce1 / Disperce2;
                Ftabl = excel.WorksheetFunction.F_Inv_RT(Level, List1.Count - 1, List2.Count - 1); 
            }
            else
            {
                Fcalc = (double)Disperce2 / Disperce1;
                Ftabl = excel.WorksheetFunction.F_Inv_RT(Level, List2.Count - 1, List1.Count - 1);
            }
            label11.Text = "Fвыч. = " + Fcalc;
            label12.Text = "Fтабл. = " + Ftabl;

            if (Fcalc>Ftabl)
            {
                MessageBox.Show("Гипотеза о том, что дисперсии равны - отвергнута.");
            }
            else
            {
                MessageBox.Show("Гипотеза о том, что дисперсии равны - подтвердилась.");
            }

        }

        private void dataGridView4_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView4[0, dataGridView4.RowCount-2].Value == null)
            {
                dataGridView4[0, dataGridView4.RowCount - 2].Value = 0;
            }
            if (dataGridView4[1, dataGridView4.RowCount - 2].Value == null)
            {
                dataGridView4[1, dataGridView4.RowCount - 2].Value = 0;
            }
            if (dataGridView4[2, dataGridView4.RowCount - 2].Value == null)
            {
                dataGridView4[2, dataGridView4.RowCount - 2].Value = 0;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<PointDooble> points = new List<PointDooble>();
            double x, y, z;
            for (int i = 0; i < dataGridView4.RowCount-1; i++)
            {
                double.TryParse(dataGridView4[0, i].Value.ToString(), out x);
                double.TryParse(dataGridView4[1, i].Value.ToString(), out y);
                double.TryParse(dataGridView4[2, i].Value.ToString(), out z);
                points.Add(new PointDooble(x, y, z));
            }
            double xmid = points.Sum(n=>n.x) / points.Count, ymid=points.Sum(n=>n.y) / points.Count, zmid=points.Sum(n=>n.z) / points.Count;
            double xdisperse = points.Sum(n => Math.Pow(n.x - xmid, 2)) / (points.Count - 1), ydisperse = points.Sum(n => Math.Pow(n.y - ymid, 2)) / (points.Count - 1), zdisperse = points.Sum(n => Math.Pow(n.z - zmid, 2)) / (points.Count - 1);
            x = points.Sum(n => (n.x - xmid) * (n.y - ymid) / (points.Count - 1) / xdisperse / ydisperse);
            y = points.Sum(n => (n.y - ymid) * (n.z - zmid) / (points.Count - 1) / ydisperse / zdisperse);
            z = points.Sum(n => (n.z - zmid) * (n.x - xmid) / (points.Count - 1) / zdisperse / xdisperse);
            dataGridView5.ColumnCount = 3;
            dataGridView5.RowCount = 3;
            for(int i = 0;i<3; i++) dataGridView5.Rows[i].Cells[i].Value = 1;
            dataGridView5.Rows[0].Cells[1].Value = Math.Round(x, 3);
            dataGridView5.Rows[0].Cells[2].Value = Math.Round(y, 3);
            dataGridView5.Rows[1].Cells[2].Value = Math.Round(z, 3);
            dataGridView5.AutoResizeColumns();
            

        }
    }
}
