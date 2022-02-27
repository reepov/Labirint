using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Labirint
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int width, height;
        public int goalwidth, goalheight;
        public string[,] field;
        Rectangle[,] rects;
        public int[,] steps;
        public int start_row, start_column, finish_row, finish_column;
        List<Tuple<int, int>> koords = new List<Tuple<int, int>>();
        Point point;
        public PointCollection ways;
        public Polyline poly = new Polyline();
        public int nextrow, nextcolumn;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void Field_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out width) || !int.TryParse(textBox2.Text, out height))
            {
                MessageBox.Show("Некорректные размеры поля");
                textBox1.Clear();
                textBox2.Clear();
                return;
            }
            field = new string[height, width];
            rects = new Rectangle[height, width];
            steps = new int[height, width];
            for (int i = 0; i < width; i++)
            {
                InsideGRID.ColumnDefinitions.Add(new ColumnDefinition());       
            }
            for (int i = 0; i < height; i++)
            {
                InsideGRID.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Fill = Brushes.Transparent;
                    InsideGRID.Children.Add(rect);
                    Grid.SetColumn(rect, i);
                    Grid.SetRow(rect, j);
                    rects[j, i] = rect;
                    field[j, i] = "Свободно";
                    steps[i, j] = -2;
                }
            }
            Field.IsEnabled = false;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Action("Старт")) rects[goalheight, goalwidth].Fill = Brushes.Blue;
            Start.IsEnabled = false;
            start_column = goalwidth;
            start_row = goalheight;
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if(Action("Финиш")) rects[goalheight, goalwidth].Fill = Brushes.Red;
            Finish.IsEnabled = false;
            finish_column = goalwidth;
            finish_row = goalheight;
        }

        private void Road_Click(object sender, RoutedEventArgs e)
        {
            if(Action("Дорога")) rects[goalheight, goalwidth].Fill = Brushes.Gray;
        }

        private void Boloto_Click(object sender, RoutedEventArgs e)
        {
           if(Action("Болото")) rects[goalheight, goalwidth].Fill = Brushes.YellowGreen;
        }

        private void Sand_Click(object sender, RoutedEventArgs e)
        {
            if(Action("Песок")) rects[goalheight, goalwidth].Fill = Brushes.Yellow;
        }

        private void Wall_Click(object sender, RoutedEventArgs e)
        {
            if (Action("Стена")) rects[goalheight, goalwidth].Fill = Brushes.Black;
        }

        private void Counting_Click(object sender, RoutedEventArgs e)
        {
            ways = new PointCollection();
            steps[start_row, start_column] = 0;
            FullStep(start_row, start_column);
            while (koords.Count > 0)
            {
                FullStep(koords[0].Item1, koords[0].Item2);
                if (koords[0].Item1 == finish_row && koords[0].Item2 == finish_column) break;
                koords.RemoveAt(0);
            }
            if (steps[finish_row, finish_column] == -2) MessageBox.Show("Пути до финиша нет");
            point = new Point((2 * finish_column + 1) * InsideGRID.ActualWidth / width / 2, (2 * finish_row + 1) * InsideGRID.ActualHeight / height / 2);
            ways.Add(point);
            nextrow = finish_row;
            nextcolumn = finish_column;
            for (int i = 0; i < steps[finish_row, finish_column]; i++)
            {
                PointAdd(nextrow, nextcolumn, out nextrow, out nextcolumn);
            }
            Way.Points = ways;
            string text = $"------------ \nРазмеры поля: {width}x{height} \nКоординаты старта: {start_row}, {start_column} \nКоординаты финиша: {finish_row}, {finish_column} \nКоличество шагов оптимального маршрута: {steps[finish_row, finish_column]} \n";
            using (StreamWriter sw = new StreamWriter(@"koord.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLine(text);
            }
        }
        public void PointAdd(int i, int j, out int nexti, out int nextj)
        {
            nexti = nextj = -1;
            if (i - 1 >= 0 && steps[i - 1, j] == steps[i, j] - 1)
            {
                point = new Point((2 * j + 1) * InsideGRID.ActualWidth / width / 2, (2 * i - 1) * InsideGRID.ActualHeight / height / 2);
                ways.Add(point);
                nexti = i - 1;
                nextj = j;
                return;
            }
            if (i + 1 < height && steps[i + 1, j] == steps[i, j] - 1)
            {
                point = new Point((2 * j + 1) * InsideGRID.ActualWidth / width / 2, (2 * i + 3) * InsideGRID.ActualHeight / height / 2);
                ways.Add(point);
                nexti = i + 1;
                nextj = j;
                return;
            }
            if ( j + 1 < width && steps[i, j + 1] == steps[i, j] - 1)
            {
                point = new Point((2 * j + 3) * InsideGRID.ActualWidth / width / 2, (2 * i + 1) * InsideGRID.ActualHeight / height / 2);
                ways.Add(point);
                nexti = i;
                nextj = j + 1;
                return;
            }
            if (j - 1 >= 0 && steps[i, j - 1] == steps[i, j] - 1)
            {
                point = new Point((2 * j - 1) * InsideGRID.ActualWidth / width / 2, (2 * i + 1) * InsideGRID.ActualHeight / height / 2);
                ways.Add(point);
                nexti = i;
                nextj = j - 1;
                return;
            }
            return;
        }
        public void FullStep(int i, int j)
        {
            Choose(i - 1, j, i, j);
            Choose(i, j - 1, i, j);
            Choose(i + 1, j, i, j);
            Choose(i, j + 1, i, j);
        }
        public void Choose(int i, int j, int previ, int prevj)
        {
            if (i > height - 1 || j > width - 1 || i < 0 || j < 0 || field[previ, prevj] == "Стена" || steps[i, j] != -2) return;
            switch (field[i, j])
            {
                case "Болото":
                    steps[i, j] = steps[previ, prevj] + 3;
                    break;
                case "Песок":
                    steps[i, j] = steps[previ, prevj] + 2;
                    break;
                case "Свободно":
                    steps[i, j] = steps[previ, prevj] + 1;
                    break;
                case "Дорога":
                    steps[i, j] = steps[previ, prevj];
                    break;
                case "Финиш":
                    steps[i, j] = steps[previ, prevj] + 1;
                    break;
                case "Стена":
                    steps[i, j] = -1;
                    break;
            }
            var temp = new Tuple<int, int>(i, j);
            koords.Add(temp);
        }
        private void GridCtrl_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (sender != null)
            {
                var element = (UIElement)e.Source;
                goalwidth = Grid.GetColumn(element);
                goalheight = Grid.GetRow(element);
            }
        }
        public bool Action(string command)
        {
            if (field[goalheight, goalwidth] != "Старт" && field[goalheight, goalwidth] != "Финиш")
            {
                field[goalheight, goalwidth] = command;
                return true;
            }
            return false;
        }
    }
}
