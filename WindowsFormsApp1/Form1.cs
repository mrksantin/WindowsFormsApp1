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
                dataGridView3.Rows.Clear();
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
                    //Data.ResultList3.Add(new PointDooble(Xn, p));
                   // Data.ResultList4.Add(new PointDooble(n, p));
                   // dataGridView3.Rows.Add($"{Math.Round(Data.ResultList3.Last().x, 3)} - {Math.Round(XnNext, 3)}", Math.Round(Data.ResultList3.Last().y, 3), (double)n / (double)MainList.Count);

                    probab += (double)n / (double)MainList.Count;
                    //Data.ResultList2.Add(new PointDooble(Xn, probab));
                    chart2.Series[0].Points.AddXY(Math.Round(Xn, 3), probab);
                    Xn = XnNext;
                }
                chart2.Series[0].Points.AddXY(Math.Round(Xn + Data.DeltaX, 3), probab);
                //Data.ResultList2.Add(new PointDooble(Xn + Data.DeltaX, probab));
                Data.CoefficienAssim = Data.MainList.Sum(x => Math.Pow(x - Data.SelectMid, 3) / ((Data.MainList.Count - 1) * Math.Pow(Data.Disperse, 1.5)));
                label3.Text = "Коэффициент ассиметрии = " + Math.Round(Data.CoefficienAssim, 3);
                Data.Eccess = Data.MainList.Sum(x => Math.Pow(x - Data.SelectMid, 4) / ((Data.MainList.Count - 1) * Math.Pow(Data.Disperse, 2))) - 3;
                label4.Text = "Эксцесс = " + Math.Round(Data.Eccess, 3);
                {
                    double xipow2calc = Data.ResultList1.Sum(x => Math.Pow((x.y - Data.MainList.Count() * x.z), 2) / (Data.MainList.Count() * x.z));
                    label6.Text = "Хи ^ 2 выч = " + xipow2calc;
                    double level;
                    double.TryParse(textBox1.Text, out level);
                    if (level < 0)
                    {
                        level = 0.05;
                    }
                    else if (level > 1)
                    {
                        level = 0.95;
                    }
                    double xipow2tabl = excel.WorksheetFunction.ChiSq_Inv_RT(level, MainList.Count - 1);
                    label5.Text = "Хи ^ 2 табл = " + xipow2tabl;

                    if ((Data.CoefficienAssim == 0) && (Data.Eccess == 0) || (xipow2calc <= xipow2tabl))
                    {
                        if((Data.CoefficienAssim == 0) && (Data.Eccess == 0) && (xipow2calc <= xipow2tabl))
                        {
                            MessageBox.Show("Эмпирический закон случайной величины \nсоответствует теоретическому нормальному закону.");
                        }
                        else
                        {
                            if (xipow2calc <= xipow2tabl)
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
            dataGridView3.Rows.Clear();           
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
            /*foreach (PointDooble point in Data.ResultList2)
            {
                
            }
            foreach (PointDooble point in Data.ResultList3)
            {
                dataGridView3.Rows.Add($"{Math.Round(point.x, 3)} - {Math.Round(point.x + Data.DeltaX,3)}", point.y);
            }*/
            label3.Text = "Коэффициент ассиметрии = " + Math.Round(Data.CoefficienAssim,3);
            label4.Text = "Эксцесс = " + Math.Round(Data.Eccess,3);
        }
    }
}
