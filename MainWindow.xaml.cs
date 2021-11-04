using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;


/// <summary>
/// список доработок:
///     1. сделать нормальные цвета        X
///     2. доделать Check()                X
///     3. сделать многопоточность
///     4. сделать бекап                   X
///     5. начать делать новую версию
/// </summary>
namespace AntsAI
{
    public partial class MainWindow : Window
    {
        abstract class Ant
        {
            private bool _TriggerCol1 = false;
            public bool TriggerCol1 { get => _TriggerCol1; set => _TriggerCol1 = value; }
            private bool _TriggerCol2 = false;
            public bool TriggerCol2 { get => _TriggerCol2; set => _TriggerCol2 = value; }
            private double _Vect;
            public double Vect { get => _Vect; set => _Vect = value; }
            private int[] _Steps;
            public int[] Steps { get => _Steps; }
            private double _Speed;
            public double Speed { get => _Speed; }
            private int _X;
            public int X { get => _X; set => _X = value; }
            private int _Y;
            public int Y { get => _Y; set => _Y = value; }
            private Ellipse _Ellipse;
            public Ellipse Ellipse { get => _Ellipse; }
            public Ant(int X, int Y, Ellipse Ellipse)
            {
                _Vect = App.Rnd.Next(360) * Math.PI / 180;
                _Steps = new int[2] { 0, 0 };
                //_Speed = App.Rnd.Next(3) + 2;
                _Speed = 4;
                _X = X;
                _Y = Y;
                _Ellipse = Ellipse;
            }
        }
        class Worker : Ant
        {
            //private bool _TriggerBase = false;
            //public bool TriggerBase { get => _TriggerBase; set => _TriggerBase = value; }
            private bool _Target;
            public bool Target { get => _Target; set => _Target = value; }
            private bool _State = false;
            public bool State { get => _State; set => _State = value; } 
            public Worker(int X, int Y, bool Target, Ellipse Ellipse) : base(X, Y, Ellipse)
            {
                _Target = Target;
            }
        }
        abstract class Base
        {
            private int _Radius;
            public int Radius { get => _Radius; }
            private int _X;
            public int X { get => _X; }
            private int _Y;
            public int Y { get => _Y; }
            private Ellipse _Ellipse;
            public Ellipse Ellipse { get => _Ellipse; }
            public Base(int X, int Y, int Radius, Ellipse Ellipse)
            {
                _X = X;
                _Y = Y;
                _Radius = Radius;
                _Ellipse = Ellipse;
            }

        }
        class Home : Base
        {
            public Home(int X, int Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse) { }
        }
        class Food : Base
        {
            public Food(int X, int Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse) { }
        }
        class Border : Base
        {
            public Border(int X, int Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse) { }
        }
        static class App
        {
            public static List<Ant  > Ants  = new List<Ant  >();
            public static List<Base > Bases = new List<Base >();
            public static List<int[]> Mas   = new List<int[]>();

            public static int AntsQuantity = 1500;
            public static int AntRadius    = 3;
            public static int BaseRadius   = 40;
            public static int SignalRadius = 32;

            public static int Counter = 0;

            public static bool HomeButtonTrigger   = true;
            public static bool FoodButtonTrigger   = false;
            public static bool BorderButtonTrigger = false;
            public static bool StartButtonTrigger  = true;

            public static SolidColorBrush Blue   = new SolidColorBrush(Color.FromRgb(81 , 77 , 255));
            public static SolidColorBrush Gray   = new SolidColorBrush(Color.FromRgb(112, 128, 144));
            public static SolidColorBrush Red    = new SolidColorBrush(Color.FromRgb(255, 67 , 67 ));
            public static SolidColorBrush White  = new SolidColorBrush(Color.FromRgb(226, 226, 226));

            public static DateTime Start;
            public static Random Rnd            = new Random         ();
            public static DispatcherTimer Timer = new DispatcherTimer();
            
        }
        public MainWindow()
        {
            InitializeComponent();

            App.Timer.Interval = TimeSpan.FromMilliseconds(20);
            App.Timer.Tick += PauseLoop;
            App.Timer.Start();
        }
        private void PauseLoop(object sender, object e)
        {
            App.Start = DateTime.Now;

            UpdateAnts();
            foreach (Ant Ant in App.Ants) Draw(Ant);

            TextFPS.Text = $"{(DateTime.Now - App.Start).Milliseconds}ms per frame";
            TextCounter.Text = $"Counter: {App.Counter}";
        }
        private void MainLoop(object sender, object e)
        {
            App.Start = DateTime.Now;

            UpdateAnts();
            foreach (Ant Ant in App.Ants) Draw(Ant);
            for (int i = 0; i < App.Ants.Count; i++) UpdateDir(App.Ants[i], i);
            foreach (Ant Ant in App.Ants) Check(Ant);
            for (int i = 0; i < App.Ants.Count; i++) ChangeDir(App.Ants[i], i);
            foreach (Ant Ant in App.Ants) Go(Ant);

            App.Mas.Clear();

            TextFPS.Text = $"{(DateTime.Now - App.Start).Milliseconds}ms per frame";
            TextCounter.Text = $"Counter: {App.Counter}";
        }
        private void Draw(Ant Ant)
        {
            Canvas.SetLeft(Ant.Ellipse, Ant.X);
            Canvas.SetTop(Ant.Ellipse, Ant.Y);
        }
        private void UpdateDir(Ant Ant, int i)
        {
            App.Mas.Add(new int[5] { Ant.X, Ant.Y, Ant.Steps[0] + App.SignalRadius, Ant.Steps[1] + App.SignalRadius, i });
        }
        private void Check(Ant Ant)
        {
            foreach (Base El in App.Bases)
            {
                if ((El.X - Ant.X) * (El.X - Ant.X) + 
                    (El.Y - Ant.Y) * (El.Y - Ant.Y) <= 
                    El.Radius * El.Radius)
                {
                    if (El is Border)
                    {
                        Ant.TriggerCol1 = true;

                        if (!Ant.TriggerCol2)
                        {
                            Ant.Vect = Math.Atan2(Ant.Y - El.Y, Ant.X - El.X);

                            Ant.TriggerCol2 = true;
                        }
                    }
                    else if (El is Home)
                    {
                        Ant.TriggerCol1 = true;

                        if (!Ant.TriggerCol2)
                        {
                            //Ant.Vect = Math.Atan2(Ant.Y - El.Y, Ant.X - Ant.Y);
                            Ant.Vect = Ant.Vect - Math.PI;
                            Ant.Steps[0] = 0;

                            Ant.TriggerCol2 = true;
                        }

                        if (Ant is Worker)
                        {
                            if (!((Worker)Ant).Target)
                            {
                                ((Worker)Ant).Target = true;
                                Ant.Ellipse.Fill = App.Red;

                                if (((Worker)Ant).State)
                                {
                                    ((Worker)Ant).State = false;
                                    App.Counter++;
                                }
                            }
                        }
                    }
                    else if (El is Food)
                    {
                        Ant.TriggerCol1 = true;

                        if (!Ant.TriggerCol2)
                        {
                            //double V1 = Math.Atan2(Ant.Y - El.Y, Ant.X - Ant.Y);
                            //double V2 = Ant.Vect - Math.PI;
                            Ant.Vect = Ant.Vect - Math.PI;
                            Ant.Steps[1] = 0;

                            Ant.TriggerCol2 = true;
                        }

                        if (Ant is Worker)
                        {
                            if (((Worker)Ant).Target)
                            {
                                ((Worker)Ant).Target = false;
                                Ant.Ellipse.Fill = App.Blue;

                                if (!((Worker)Ant).State)
                                {
                                    ((Worker)Ant).State = true;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            if (!Ant.TriggerCol1)
            {
                Ant.TriggerCol2 = false;
            }
            Ant.TriggerCol1 = false;
        }
        private void ChangeDir(Ant Ant, int i)
        {
            if (Ant.TriggerCol2) return;

            double TrV = 100000;
            bool Tr = false;

            foreach (int[] El in App.Mas)
            {
                if (i != El[4])
                {
                    if ((El[0] - Ant.X) * (El[0] - Ant.X) + 
                        (El[1] - Ant.Y) * (El[1] - Ant.Y) <= 
                        App.SignalRadius * App.SignalRadius)
                    {
                        if (El[2] < Ant.Steps[0])
                        {
                            Ant.Steps[0] = El[2];
                            if (Ant is Worker)
                            {
                                if (!((Worker)Ant).Target)
                                {
                                    TrV = Math.Atan2(El[1] - Ant.Y, El[0] - Ant.X);
                                    Tr = true;
                                }
                            }
                        }
                        if (El[3] < Ant.Steps[1])
                        {
                            Ant.Steps[1] = El[3];
                            if (Ant is Worker)
                            {
                                if (((Worker)Ant).Target)
                                {
                                    TrV = Math.Atan2(El[1] - Ant.Y, El[0] - Ant.X);
                                    Tr = true;
                                }
                            }
                        }
                    }
                }
            }
            if (Tr) Ant.Vect = TrV;
        }
        private void Go(Ant Ant)
        {
            Ant.X += (int)(Math.Cos(Ant.Vect) * Ant.Speed);
            Ant.Y += (int)(Math.Sin(Ant.Vect) * Ant.Speed);

            if (Ant.X > (int)MainCanvas.ActualWidth - 2 || Ant.X < 1) Ant.Vect = Math.PI - Ant.Vect;
            if (Ant.Y > (int)MainCanvas.ActualHeight - 2 || Ant.Y < 1) Ant.Vect = Math.PI * 2 - Ant.Vect;

            Ant.Steps[0]++;
            Ant.Steps[1]++;
        }
        private void UpdateAnts()
        {
            while (App.Ants.Count < App.AntsQuantity)
            {
                int X = App.Rnd.Next((int)MainCanvas.ActualWidth / 2) + (int)MainCanvas.ActualHeight / 4;
                int Y = App.Rnd.Next((int)MainCanvas.ActualHeight / 2) + (int)MainCanvas.ActualHeight / 4;

                bool Target = App.Rnd.Next(2) == 1 ? true : false;

                Ellipse EllipseAnt = new Ellipse() {
                    Width = App.AntRadius * 2,
                    Height = App.AntRadius * 2,
                    Fill = Target ? App.Red : App.Blue };

                MainCanvas.Children.Add(EllipseAnt);

                Canvas.SetLeft(EllipseAnt, X - App.AntRadius);
                Canvas.SetTop(EllipseAnt, Y - App.AntRadius);

                App.Ants.Add(new Worker(X, Y, true, EllipseAnt));
            }
            while (App.Ants.Count > App.AntsQuantity)
            {
                Ant Ant = App.Ants[0];

                MainCanvas.Children.Remove(Ant.Ellipse);

                App.Ants.Remove(Ant);
            }
            AntsText.Text = $"Ants: {App.Ants.Count} | {(int)AntsSlider.Value}";
        }
        private void AntsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int Value = (int)((Slider)sender).Value;

            App.AntsQuantity = Value;
        }

        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int Value = (int)((Slider)sender).Value;

            App.BaseRadius = Value;

            RadiusText.Text = $"Radius: {Value}";
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(this);

            foreach (Base B in App.Bases)
            {
                if (Math.Pow(B.X - p.X, 2) + Math.Pow(B.Y - p.Y, 2) <= Math.Pow(B.Radius, 2))
                {
                    MainCanvas.Children.Remove(B.Ellipse);

                    App.Bases.Remove(B);

                    return;
                }
            }
            Ellipse EllipseBase = new Ellipse()
            {
                Width = App.BaseRadius * 2,
                Height = App.BaseRadius * 2,

                Fill = App.HomeButtonTrigger ? App.Blue : 
                       App.FoodButtonTrigger ? App.Red : 
                       App.Gray
            };

            if (App.HomeButtonTrigger)
            {
                App.Bases.Add(
                        new Home((int)p.X, 
                                 (int)p.Y, 
                                 App.BaseRadius, 
                                 EllipseBase));
            }
            else if (App.FoodButtonTrigger)
            {
                App.Bases.Add(
                        new Food((int)p.X, 
                                 (int)p.Y, 
                                 App.BaseRadius, 
                                 EllipseBase));
            }
            else if (App.BorderButtonTrigger)
            {
                App.Bases.Add(
                        new Border((int)p.X, 
                                   (int)p.Y, 
                                   App.BaseRadius, 
                                   EllipseBase));
            }
            MainCanvas.Children.Add(EllipseBase);

            Canvas.SetLeft(EllipseBase, p.X - App.BaseRadius);
            Canvas.SetTop(EllipseBase, p.Y - App.BaseRadius);

            Canvas.SetZIndex(EllipseBase, -1);
        }

        private void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content)
            {
                case "Home":

                    App.HomeButtonTrigger   = true;
                    App.FoodButtonTrigger   = false;
                    App.BorderButtonTrigger = false;

                    break;

                case "Food":

                    App.HomeButtonTrigger   = false;
                    App.FoodButtonTrigger   = true;
                    App.BorderButtonTrigger = false;

                    break;

                case "Border":

                    App.HomeButtonTrigger   = false;
                    App.FoodButtonTrigger   = false;
                    App.BorderButtonTrigger = true;

                    break;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.StartButtonTrigger)
            {
                App.Timer.Tick -= PauseLoop;
                App.Timer.Tick += MainLoop;

                App.StartButtonTrigger = false;
            }
            else
            {
                App.Timer.Tick -= MainLoop;
                App.Timer.Tick += PauseLoop;

                App.StartButtonTrigger = true;
            }
        }
    }
}

