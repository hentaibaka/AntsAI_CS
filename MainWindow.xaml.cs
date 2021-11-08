using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

//спавн и смерть муравьёв
//сделать инструкцию
//сделать чтобы королевы держались подальше друг от друга
namespace AntsAI
{
    abstract class Movable
    {
        private static int _IdS = 0;

        private int _Id;
        public int Id { get => _Id; }

        private bool _TriggerCol1 = false;
        public bool TriggerCol1 { get => _TriggerCol1; set => _TriggerCol1 = value; }

        private bool _TriggerCol2 = false;
        public bool TriggerCol2 { get => _TriggerCol2; set => _TriggerCol2 = value; }

        private double _Vect;
        public double Vect { get => _Vect; set => _Vect = value; }

        protected double _Speed;
        public double Speed { get => _Speed; set => _Speed = value; }

        private double _X;
        public double X { get => _X; set => _X = value; }

        private double _Y;
        public double Y { get => _Y; set => _Y = value; }

        private Ellipse _Ellipse;
        public Ellipse Ellipse { get => _Ellipse; }
        public Movable(double X, double Y, Ellipse Ellipse)
        {
            _Id = _IdS++;
            _Vect = App1.Rnd.NextDouble() * Math.PI * 2;
            _Speed = 2 + App1.Rnd.Next(2);
            _X = X;
            _Y = Y;
            _Ellipse = Ellipse;
        }
    }
    abstract class Ant : Movable
    {
        private int[] _Steps;
        public int[] Steps { get => _Steps; }

        public Ant(double X, double Y, Ellipse Ellipse) : base(X, Y, Ellipse)
        {
            _Steps = new int[2] { 0, 0 };
        }
    }
    class Worker : Ant
    {
        private bool _Target;
        public bool Target { get => _Target; set => _Target = value; }

        private bool _State = false;
        public bool State { get => _State; set => _State = value; }

        public Worker(double X, double Y, bool Target, Ellipse Ellipse) : base(X, Y, Ellipse)
        {
            _Target = Target;
        }
    }
    class Scout : Ant
    {
        private bool _State = false;
        public bool State { get => _State; set => _State = value; }

        public Scout(double X, double Y, Ellipse Ellipse) : base(X, Y, Ellipse) { }
    }
    class Queen : Movable
    {
        private int _Health;
        public int Health { get => _Health; set => _Health = value; }

        private int _Radius;
        public int Radius { get => _Radius; set => _Radius = value < 5 ? 5 : value > 50 ? 50 : value; }
        public Queen(double X, double Y, Ellipse Ellipse) : base(X, Y, Ellipse)
        {
            _Health = 400;
            _Radius = 20;
            _Speed = App1.Rnd.NextDouble() + 0.2;
        }
    }
    class MovableFood : Movable
    {
        private int _Pieces;
        public int Pieces { get => _Pieces; set => _Pieces = value; }

        private int _Radius;
        public int Radius { get => _Radius; set => _Radius = value < 5 ? 5 : value > 50 ? 50 : value; }

        public MovableFood(int X, int Y, Ellipse Ellipse) : base(X, Y, Ellipse)
        {
            _Pieces = 400;
            _Radius = 20;
            _Speed = App1.Rnd.NextDouble() + 0.2;
        }
    }
    abstract class Base
    {
        private int _Radius;
        public int Radius { get => _Radius; set => _Radius = value; }

        private double _X;
        public double X { get => _X; }

        private double _Y;
        public double Y { get => _Y; }

        private Ellipse _Ellipse;
        public Ellipse Ellipse { get => _Ellipse; }

        public Base(double X, double Y, int Radius, Ellipse Ellipse)
        {
            _X = X;
            _Y = Y;
            _Radius = Radius;
            _Ellipse = Ellipse;
        }

    }
    class Home : Base
    {
        private int _Health;
        public int Health { get => _Health; set => _Health = value > 1001 ? _Health : value; }

        public Home(double X, double Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse)
        {
            _Health = Radius * 10 + 1;
        }
    }
    class Food : Base
    {
        private int _Pieces;
        public int Pieces { get => _Pieces; set => _Pieces = value; }

        public Food(double X, double Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse)
        {
            _Pieces = Radius * 10;
        }
    }
    class Border : Base
    {
        public Border(double X, double Y, int Radius, Ellipse Ellipse) : base(X, Y, Radius, Ellipse) { }
    }
    static class App1
    {
        public static List<Ant> Ants = new List<Ant>();
        public static List<Base> Bases = new List<Base>();
        public static List<Queen> Queens = new List<Queen>();
        public static List<MovableFood> MovableFoods = new List<MovableFood>();
        public static List<double[]> Mas = new List<double[]>();

        public static int AntsQuantity = 1500;
        public static int QueensQuantity = 5;
        public static int MovableFoodsQuantity = 10;
        public static int NewQueen = 400;
        public static double AntRadius = 1.5;
        public static int BaseRadius = 40;
        public static int SignalRadius = 32;

        public static int Width;
        public static int Height;

        public static int Counter = 0;

        public static bool HomeButtonTrigger = true;
        public static bool FoodButtonTrigger = false;
        public static bool BorderButtonTrigger = false;
        public static bool StartButtonTrigger = true;
        public static bool HealthButtonTrigger = false;
        public static bool ModeButtonTrigger = true;

        public static SolidColorBrush HomeColor = new SolidColorBrush(Color.FromRgb(81, 77, 255));
        public static SolidColorBrush BorderColor = new SolidColorBrush(Color.FromRgb(112, 128, 144));
        public static SolidColorBrush ScoutColor = new SolidColorBrush(Color.FromRgb(28, 28, 56));
        public static SolidColorBrush FoodColor = new SolidColorBrush(Color.FromRgb(255, 67, 67));
        public static SolidColorBrush White = new SolidColorBrush(Color.FromRgb(226, 226, 226));

        public static Random Rnd = new Random();
        public static DispatcherTimer Timer = new DispatcherTimer();
        public static Stopwatch Time = new Stopwatch();

    }
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ChangeMode();
        }
        private void ChangeMode()
        {
            App1.Timer.Stop();

            App1.Ants.Clear();
            App1.Bases.Clear();
            App1.Queens.Clear();
            App1.MovableFoods.Clear();
            App1.Mas.Clear();

            MainCanvas.Children.Clear();

            App1.AntsQuantity = 1500;
            App1.QueensQuantity = 5;
            App1.MovableFoodsQuantity = 10;
            App1.NewQueen = 400;
            App1.Counter = 0;

            App1.HomeButtonTrigger = true;
            App1.FoodButtonTrigger = false;
            App1.BorderButtonTrigger = false;
            App1.StartButtonTrigger = true;
            App1.HealthButtonTrigger = false;

            App1.Timer = new DispatcherTimer();

            App1.Timer.Interval = TimeSpan.FromMilliseconds(10);

            if (App1.ModeButtonTrigger)
            {
                Mode2Grid.Visibility = Visibility.Collapsed;
                Mode1Grid.Visibility = Visibility.Visible;

                App1.Timer.Tick += PauseLoop1;
            }
            else
            {
                Mode1Grid.Visibility = Visibility.Collapsed;
                Mode2Grid.Visibility = Visibility.Visible;

                App1.Timer.Tick += PauseLoop2;
            }

            App1.Timer.Start();

        }
        private void PauseLoop2(object sender, object e)
        {
            App1.Rnd = new Random();

            App1.Time.Restart();

            UpdateSize();

            UpdateAnts();
            UpdateBases();

            CreateQueen();
            SpawnMovableFood();

            Draw();

            App1.Time.Stop();

            TextFPS2.Text = $"{App1.Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (App1.Time.ElapsedMilliseconds < 1 ? 1 : App1.Time.ElapsedMilliseconds)} FPS";
        }
        private void MainLoop2(object sender, object e)
        {
            App1.Rnd = new Random();

            App1.Time.Restart();

            UpdateSize();

            UpdateAnts();

            UpdateMovableFoods();
            SpawnMovableFood();

            CreateQueen();
            UpdateQueens();

            Parallel.ForEach(App1.Queens, UpdateQueenVect);

            Draw();

            foreach (Ant Ant in App1.Ants) UpdateDir(Ant);

            Parallel.ForEach(App1.Ants, CheckMode2);
            Parallel.ForEach(App1.Queens, QueenCheck);
            Parallel.ForEach(App1.MovableFoods, MovableFoodCheck);

            Parallel.ForEach(App1.Ants, ChangeDir);

            Parallel.ForEach(App1.Ants, Go);
            Parallel.ForEach(App1.Queens, QueenGo);
            Parallel.ForEach(App1.MovableFoods, MovableFoodGo);

            App1.Mas.Clear();

            App1.Time.Stop();

            TextFPS2.Text = $"{App1.Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (App1.Time.ElapsedMilliseconds < 1 ? 1 : App1.Time.ElapsedMilliseconds)} FPS";
        }
        private void CreateQueen()
        {
            while (App1.Queens.Count > App1.QueensQuantity)
            {
                MainCanvas.Children.Remove(App1.Queens[0].Ellipse);
                App1.Queens.RemoveAt(0);
            }
            if (App1.Queens.Count == 0)
            {
                Ellipse Ellipse = new Ellipse() { Width = 40, Height = 40, Fill = App1.HomeColor };
                Queen Queen = new Queen((int)MainCanvas.ActualWidth / 2,
                                        (int)MainCanvas.ActualHeight / 2,
                                        Ellipse);

                App1.Queens.Add(Queen);

                MainCanvas.Children.Add(Ellipse);

                Canvas.SetLeft(Ellipse, Queen.X - 20);
                Canvas.SetTop(Ellipse, Queen.Y - 20);
            }
            foreach (Ant Ant in App1.Ants)
            {
                if (Ant.Steps[0] > App1.NewQueen && App1.Queens.Count < App1.QueensQuantity)
                {
                    Ellipse Ellipse = new Ellipse() { Width = 40, Height = 40, Fill = App1.HomeColor };

                    App1.Queens.Add(new Queen(Ant.X, Ant.Y, Ellipse));

                    MainCanvas.Children.Add(Ellipse);
                    MainCanvas.Children.Remove(Ant.Ellipse);
                }
            }
            App1.Ants.RemoveAll((El) => El.Steps[0] > 500);

            QueensText.Text = $"Max Queens: {App1.QueensQuantity} Queens: {App1.Queens.Count}";
        }
        private void UpdateMovableFoods()
        {
            foreach (MovableFood MovableFood in App1.MovableFoods)
            {
                MovableFood.Radius = MovableFood.Pieces / 20;

                MovableFood.Ellipse.Width = MovableFood.Ellipse.Height = MovableFood.Radius * 2;

                if (MovableFood.Pieces <= 0) MainCanvas.Children.Remove(MovableFood.Ellipse);
            }
            App1.MovableFoods.RemoveAll((El) => El.Pieces <= 0);
        }
        private void UpdateQueens()
        {
            foreach (Queen Queen in App1.Queens)
            {
                Queen.Health--;

                Queen.Radius = Queen.Health / 20;
            
                Queen.Ellipse.Width = Queen.Ellipse.Height = Queen.Radius * 2;

                if (Queen.Health <= 0) MainCanvas.Children.Remove(Queen.Ellipse);
            }
            App1.Queens.RemoveAll((El) => El.Health <= 0);
        }
        private void SpawnMovableFood()
        {
            while (App1.MovableFoods.Count < App1.MovableFoodsQuantity)
            { 

                Ellipse Ellipse = new Ellipse { Width = 40, Height = 40, Fill = App1.FoodColor };

                App1.MovableFoods.Add(new MovableFood(
                                        App1.Rnd.Next(App1.Width - 48) + 24, 
                                        App1.Rnd.Next(App1.Height - 48) + 24, 
                                        Ellipse));
                MainCanvas.Children.Add(Ellipse);
            }
            while (App1.MovableFoods.Count > App1.MovableFoodsQuantity)
            {
                MainCanvas.Children.Remove(App1.MovableFoods[0].Ellipse);
                App1.MovableFoods.RemoveAt(0);
            }

            MovableFoodsText.Text = $"Food: {App1.MovableFoodsQuantity}";
        }
        private void UpdateQueenVect(Queen Queen)
        {
            double Len = 0;
            double X = App1.Width / 2;
            double Y = App1.Height / 2;
            foreach (MovableFood El in App1.MovableFoods)
            {
                double Len1 = (Queen.X - El.X) * (Queen.X - El.X) + (Queen.Y - El.Y) * (Queen.Y - El.Y);

                if (Len1 > Len)
                {
                    Len = Len1;
                    X = El.X;
                    Y = El.Y;
                }
            }
            if (Len != 10000) Queen.Vect = Math.Atan2(Y - Queen.Y, X - Queen.X);
        }
        private void CheckMode2(Ant Ant)
        {
            foreach (MovableFood El in App1.MovableFoods)
            {
                if ((El.X - Ant.X) * (El.X - Ant.X) +
                    (El.Y - Ant.Y) * (El.Y - Ant.Y) <=
                    El.Radius * El.Radius)
                {
                    Ant.TriggerCol1 = true;

                    if (!Ant.TriggerCol2)
                    {
                        Ant.Vect = Ant.Vect - Math.PI;
                        Ant.Steps[1] = 0;

                        Ant.TriggerCol2 = true;
                    }

                    if (Ant is Worker)
                    {
                        if (((Worker)Ant).Target)
                        {
                            ((Worker)Ant).Target = false;

                            if (!((Worker)Ant).State)
                            {
                                ((Worker)Ant).State = true;
                                El.Pieces--;
                            }
                        }
                    }
                    else if (Ant is Scout)
                    {
                        if (!((Scout)Ant).State)
                        {
                            ((Scout)Ant).State = true;
                            El.Pieces--;
                        }
                    }
                    break;
                }
            }

            foreach (Queen El in App1.Queens)
            {
                if ((El.X - Ant.X) * (El.X - Ant.X) +
                    (El.Y - Ant.Y) * (El.Y - Ant.Y) <=
                    El.Radius * El.Radius)
                {
                    Ant.TriggerCol1 = true;

                    Ant.Steps[0] = 0;

                    if (!Ant.TriggerCol2)
                    {
                        if (Ant is Worker)
                        {
                            Ant.Vect = Ant.Vect - Math.PI;
                        }
                        else if (Ant is Scout)
                        {
                            Ant.Vect = Math.Atan2(Ant.Y - El.Y, Ant.X - El.X);
                        }
                        Ant.TriggerCol2 = true;
                    }

                    if (Ant is Worker)
                    {
                        if (!((Worker)Ant).Target)
                        {
                            ((Worker)Ant).Target = true;

                            if (((Worker)Ant).State)
                            {
                                ((Worker)Ant).State = false;

                                El.Health++;
                                App1.Counter++;
                            }
                        }
                    }
                    else if (Ant is Scout)
                    {
                        if (((Scout)Ant).State)
                        {
                            ((Scout)Ant).State = false;

                            El.Health++;
                            App1.Counter++;
                        }
                    }
                }
            }
            if (!Ant.TriggerCol1)
            {
                Ant.TriggerCol2 = false;
            }
            Ant.TriggerCol1 = false;
        }
        private void QueenCheck(Queen El) 
        {
            foreach (Queen Queen in App1.Queens)
            {
                if (Queen.Id != El.Id &&
                    (Queen.X - El.X) * (Queen.X - El.X) +
                    (Queen.Y - El.Y) * (Queen.Y - El.Y) <=
                    (Queen.Radius + El.Radius) * (Queen.Radius + El.Radius))
                {
                    El.TriggerCol1 = true;

                    if (!El.TriggerCol2)
                    {
                        El.Vect = Math.Atan2(El.Y - Queen.Y, El.X - Queen.X);

                        El.TriggerCol2 = true;
                    }
                    break;
                }
            }
            foreach (MovableFood MF in App1.MovableFoods)
            {
                if (MF.Id != El.Id && 
                    (MF.X - El.X) * (MF.X - El.X) +
                    (MF.Y - El.Y) * (MF.Y - El.Y) <=
                    (MF.Radius + El.Radius) * (MF.Radius + El.Radius))
                {
                    El.TriggerCol1 = true;

                    if (!El.TriggerCol2)
                    {
                        El.Vect = Math.Atan2(El.Y - MF.Y, El.X - MF.X);

                        El.TriggerCol2 = true;
                    }
                    break;
                }
            }
            if (!El.TriggerCol1)
            {
                El.TriggerCol2 = false;
            }
            El.TriggerCol1 = false;
        }
        private void MovableFoodCheck(MovableFood El)
        {
            foreach (Queen Queen in App1.Queens)
            {
                if (Queen.Id != El.Id && 
                    (Queen.X - El.X) * (Queen.X - El.X) +
                    (Queen.Y - El.Y) * (Queen.Y - El.Y) <=
                    (Queen.Radius + El.Radius) * (Queen.Radius + El.Radius))
                {
                    El.TriggerCol1 = true;

                    if (!El.TriggerCol2)
                    {
                        El.Vect = Math.Atan2(El.Y - Queen.Y, El.X - Queen.X);

                        El.TriggerCol2 = true;
                    }
                    break;
                }
            }
            foreach (MovableFood MF in App1.MovableFoods)
            {
                if (MF.Id != El.Id && 
                    (MF.X - El.X) * (MF.X - El.X) +
                    (MF.Y - El.Y) * (MF.Y - El.Y) <=
                    (MF.Radius + El.Radius) * (MF.Radius + El.Radius))
                {
                    El.TriggerCol1 = true;

                    if (!El.TriggerCol2)
                    {
                        El.Vect = Math.Atan2(El.Y - MF.Y, El.X - MF.X);

                        El.TriggerCol2 = true;
                    }
                    break;
                }
            }
            if (!El.TriggerCol1)
            {
                El.TriggerCol2 = false;
            }
            El.TriggerCol1 = false;
        }
        private void QueenGo(Queen Queen)
        {
            Queen.X += Math.Cos(Queen.Vect + (App1.Rnd.Next(6) - 3) * Math.PI / 180) * Queen.Speed;
            Queen.Y += Math.Sin(Queen.Vect + (App1.Rnd.Next(6) - 3) * Math.PI / 180) * Queen.Speed;

            if (Queen.X  + Queen.Radius > App1.Width - 2 || Queen.X - Queen.Radius < 1)
            {
                Queen.Vect = Math.PI - Queen.Vect;
                Queen.X = Queen.X - Queen.Radius < 1 ? Queen.Radius + 1 :
                                    App1.Width - Queen.Radius - 1;
            }
            if (Queen.Y  + Queen.Radius > App1.Height - 2 || Queen.Y - Queen.Radius < 1)
            {
                Queen.Vect = Math.PI * 2 - Queen.Vect;
                Queen.Y = Queen.Y - Queen.Radius < 1 ? Queen.Radius + 1 : 
                                    App1.Height - Queen.Radius - 1;
            }
        }
        private void MovableFoodGo(MovableFood MovableFood)
        {
            MovableFood.X += Math.Cos(MovableFood.Vect + (App1.Rnd.Next(4) - 2) * Math.PI / 180) * MovableFood.Speed;
            MovableFood.Y += Math.Sin(MovableFood.Vect + (App1.Rnd.Next(4) - 2) * Math.PI / 180) * MovableFood.Speed;

            if (MovableFood.X + MovableFood.Radius > App1.Width - 1 || 
                MovableFood.X - MovableFood.Radius < 1)
            {
                MovableFood.Vect = Math.PI - MovableFood.Vect;

                MovableFood.X = MovableFood.X - MovableFood.Radius < 1 ? 
                                    MovableFood.Radius + 1 :
                                    App1.Width - MovableFood.Radius - 1;
            }
            if (MovableFood.Y + MovableFood.Radius > App1.Height - 1 || 
                MovableFood.Y - MovableFood.Radius < 1)
            {
                MovableFood.Vect = Math.PI * 2 - MovableFood.Vect;

                MovableFood.Y = MovableFood.Y - MovableFood.Radius < 1 ?
                                    MovableFood.Radius + 1 :
                                    App1.Height - MovableFood.Radius - 1;
            }
        }
        private void PauseLoop1(object sender, object e)
        {
            App1.Rnd = new Random();

            App1.Time.Restart();

            UpdateAnts();
            Draw();

            App1.Time.Stop();

            TextFPS1.Text = $"{App1.Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (App1.Time.ElapsedMilliseconds < 1 ? 1 : App1.Time.ElapsedMilliseconds)} FPS";
            TextCounter.Text = $"Count: {App1.Counter}";
        }
        private void MainLoop1(object sender, object e)
        {
            App1.Rnd = new Random();

            App1.Time.Restart();

            UpdateSize();
            UpdateAnts();
            if (App1.HealthButtonTrigger) UpdateBases();

            Draw();

            foreach (Ant Ant in App1.Ants) UpdateDir(Ant);

            Parallel.ForEach(App1.Ants, Check);
            Parallel.ForEach(App1.Ants, ChangeDir);
            Parallel.ForEach(App1.Ants, Go);

            App1.Mas.Clear();

            App1.Time.Stop();

            TextFPS1.Text = $"{App1.Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (App1.Time.ElapsedMilliseconds < 1 ? 1 : App1.Time.ElapsedMilliseconds)} FPS";
            TextCounter.Text = $"Count: {App1.Counter}";
        }
        private void Draw()
        {
            foreach (Ant Ant in App1.Ants)
            {
                Canvas.SetLeft(Ant.Ellipse, Ant.X - App1.AntRadius);
                Canvas.SetTop(Ant.Ellipse, Ant.Y - App1.AntRadius);

                if (Ant is Worker)
                {
                    Ant.Ellipse.Fill = ((Worker)Ant).Target ? App1.FoodColor : App1.HomeColor;
                }
                else if (Ant is Scout)
                {
                    Ant.Ellipse.Fill = ((Scout)Ant).State ? App1.HomeColor : App1.ScoutColor;
                }
            }
            foreach (Base Base in App1.Bases)
            {
                Canvas.SetLeft(Base.Ellipse, Base.X - Base.Radius);
                Canvas.SetTop(Base.Ellipse, Base.Y - Base.Radius);
            }

            if (App1.ModeButtonTrigger) return;

            foreach (Queen Queen in App1.Queens)
            {
                Canvas.SetLeft(Queen.Ellipse, Queen.X - Queen.Radius);
                Canvas.SetTop(Queen.Ellipse, Queen.Y - Queen.Radius);
            }
            foreach (MovableFood MovableFood in App1.MovableFoods)
            {
                Canvas.SetLeft(MovableFood.Ellipse, MovableFood.X - MovableFood.Radius);
                Canvas.SetTop(MovableFood.Ellipse, MovableFood.Y - MovableFood.Radius);
            }
        }
        private void UpdateDir(Ant Ant)
        {
            App1.Mas.Add(new double[5] {
                    Ant.X,
                    Ant.Y,
                    Ant.Steps[0] + App1.SignalRadius,
                    Ant.Steps[1] + App1.SignalRadius,
                    Ant.Id });
        }
        private void Check(Ant Ant)
        {
            foreach (Base El in App1.Bases)
            {
                if ((El.X - Ant.X) * (El.X - Ant.X) +
                    (El.Y - Ant.Y) * (El.Y - Ant.Y) <=
                    El.Radius * El.Radius)
                {
                    if (El is Border)
                    {
                        Ant.TriggerCol1 = true;
                        Ant.TriggerCol2 = true;

                        Ant.Vect = Math.Atan2(Ant.Y - El.Y, Ant.X - El.X);
                    }
                    else if (El is Home)
                    {
                        Ant.TriggerCol1 = true;

                        Ant.Steps[0] = 0;

                        if (!Ant.TriggerCol2)
                        {
                            if (Ant is Worker)
                            {
                                Ant.Vect = Ant.Vect - Math.PI;
                            }
                            else if (Ant is Scout)
                            {
                                Ant.Vect = Math.Atan2(Ant.Y - El.Y, Ant.X - El.X);
                            }
                            Ant.TriggerCol2 = true;
                        }

                        if (Ant is Worker)
                        {
                            if (!((Worker)Ant).Target)
                            {
                                ((Worker)Ant).Target = true;

                                if (((Worker)Ant).State)
                                {
                                    ((Worker)Ant).State = false;

                                    ((Home)El).Health++;
                                    App1.Counter++;
                                }
                            }
                        }
                        else if (Ant is Scout)
                        {
                            if (((Scout)Ant).State)
                            {
                                ((Scout)Ant).State = false;

                                ((Home)El).Health++;
                                App1.Counter++;
                            }
                        }
                    }
                    else if (El is Food)
                    {
                        Ant.TriggerCol1 = true;

                        if (!Ant.TriggerCol2)
                        {
                            Ant.Vect = Ant.Vect - Math.PI;
                            Ant.Steps[1] = 0;

                            Ant.TriggerCol2 = true;
                        }

                        if (Ant is Worker)
                        {
                            if (((Worker)Ant).Target)
                            {
                                ((Worker)Ant).Target = false;

                                if (!((Worker)Ant).State)
                                {
                                    ((Worker)Ant).State = true;

                                    ((Food)El).Pieces--;
                                }
                            }
                        }
                        else if (Ant is Scout)
                        {
                            if (!((Scout)Ant).State)
                            {
                                ((Scout)Ant).State = true;

                                ((Food)El).Pieces--;
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
        private void ChangeDir(Ant Ant)
        {
            if (Ant.TriggerCol2) return;

            foreach (double[] El in App1.Mas)
            {
                if (Ant.Id != El[4])
                {
                    if ((El[0] - Ant.X) * (El[0] - Ant.X) +
                        (El[1] - Ant.Y) * (El[1] - Ant.Y) <=
                        App1.SignalRadius * App1.SignalRadius)
                    {
                        if (El[2] < Ant.Steps[0])
                        {
                            Ant.Steps[0] = (int)El[2];

                            if (Ant is Worker)
                            {
                                if (!((Worker)Ant).Target)
                                {
                                    Ant.Vect = Math.Atan2(El[1] - Ant.Y, El[0] - Ant.X);
                                }
                            }
                            else if (Ant is Scout)
                            {
                                if (((Scout)Ant).State)
                                {
                                    Ant.Vect = Math.Atan2(El[1] - Ant.Y, El[0] - Ant.X);
                                }
                            }
                        }
                        if (El[3] < Ant.Steps[1])
                        {
                            Ant.Steps[1] = (int)El[3];

                            if (Ant is Worker)
                            {
                                if (((Worker)Ant).Target)
                                {
                                    Ant.Vect = Math.Atan2(El[1] - Ant.Y, El[0] - Ant.X);
                                }
                            }
                        }
                    }
                }
            }
        }
        private void Go(Ant Ant)
        {
            Ant.X += Math.Cos(Ant.Vect + (App1.Rnd.Next(6) - 3) * Math.PI / 180) * Ant.Speed;
            Ant.Y += Math.Sin(Ant.Vect + (App1.Rnd.Next(6) - 3) * Math.PI / 180) * Ant.Speed;

            if (Ant.X > App1.Width - 2 || Ant.X < 1)
            {
                Ant.Vect = Math.PI - Ant.Vect;
                Ant.X = Ant.X < 1 ? 1 : App1.Width - 2;
            }
            if (Ant.Y > App1.Height - 2 || Ant.Y < 1)
            {
                Ant.Vect = Math.PI * 2 - Ant.Vect;
                Ant.Y = Ant.Y < 1 ? 1 : App1.Height - 2;
            }

            Ant.Steps[0]++;
            Ant.Steps[1]++;
        }
        private void UpdateBases()
        {
            foreach (Base Base in App1.Bases)
            {
                if (Base is Food)
                {
                    Base.Radius = ((Food)Base).Pieces / 10;

                    Base.Ellipse.Width = Base.Ellipse.Height = Base.Radius * 2;

                    if (((Food)Base).Pieces == 0) MainCanvas.Children.Remove(Base.Ellipse);
                }
                else if (Base is Home)
                {
                    ((Home)Base).Health--;

                    Base.Radius = ((Home)Base).Health / 10;

                    Base.Ellipse.Width = Base.Ellipse.Height = Base.Radius * 2;

                    if (((Home)Base).Health == 0) MainCanvas.Children.Remove(Base.Ellipse);
                }
            }
            App1.Bases.RemoveAll((El) => El is Food ? ((Food)El).Pieces <= 0 : El is Home ? ((Home)El).Health <= 0 : false);
        }
        private void UpdateAnts()
        {
            while (App1.Ants.Count < App1.AntsQuantity)
            {
                int X = App1.Rnd.Next((int)MainCanvas.ActualWidth / 2) + (int)MainCanvas.ActualWidth / 4;
                int Y = App1.Rnd.Next((int)MainCanvas.ActualHeight / 2) + (int)MainCanvas.ActualHeight / 4;

                int Target = App1.Rnd.Next(19);

                Ellipse EllipseAnt = new Ellipse() {
                    Width = App1.AntRadius * 2,
                    Height = App1.AntRadius * 2,
                    Fill = Target < 9 ? App1.FoodColor : Target < 18 ? App1.HomeColor : App1.ScoutColor };

                MainCanvas.Children.Add(EllipseAnt);

                if (Target < 9) App1.Ants.Add(new Worker(X, Y, false, EllipseAnt));
                else if (Target < 18) App1.Ants.Add(new Worker(X, Y, true, EllipseAnt));
                else if (Target < 19) App1.Ants.Add(new Scout(X, Y, EllipseAnt));
            }
            while (App1.Ants.Count > App1.AntsQuantity)
            {
                Ant Ant = App1.Ants[0];

                MainCanvas.Children.Remove(Ant.Ellipse);

                App1.Ants.Remove(Ant);
            }
            if (App1.ModeButtonTrigger) AntsText1.Text = $"Max Ants: {App1.AntsQuantity} Ants: {App1.Ants.Count}";
            else AntsText2.Text = $"Max Ants: {App1.AntsQuantity} Ants: {App1.Ants.Count}";
        }
        private void AntsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App1.AntsQuantity = (int)((Slider)sender).Value;
        }
        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int Value = (int)((Slider)sender).Value;

            App1.BaseRadius = Value;

            RadiusText.Text = $"Radius: {Value}";
        }
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!App1.ModeButtonTrigger) return;

            Point p = e.GetPosition(this);

            foreach (Base B in App1.Bases)
            {
                if (Math.Pow(B.X - p.X, 2) + Math.Pow(B.Y - p.Y, 2) <= Math.Pow(B.Radius, 2))
                {
                    MainCanvas.Children.Remove(B.Ellipse);

                    App1.Bases.Remove(B);

                    return;
                }
            }
            Ellipse EllipseBase = new Ellipse()
            {
                Width = App1.BaseRadius * 2,
                Height = App1.BaseRadius * 2,

                Fill = App1.HomeButtonTrigger ? App1.HomeColor :
                       App1.FoodButtonTrigger ? App1.FoodColor :
                       App1.BorderColor
            };

            if (App1.HomeButtonTrigger)
            {
                App1.Bases.Add(
                        new Home((int)p.X,
                                 (int)p.Y,
                                 App1.BaseRadius,
                                 EllipseBase));
            }
            else if (App1.FoodButtonTrigger)
            {
                App1.Bases.Add(
                        new Food((int)p.X,
                                 (int)p.Y,
                                 App1.BaseRadius,
                                 EllipseBase));
            }
            else if (App1.BorderButtonTrigger)
            {
                App1.Bases.Add(
                        new Border((int)p.X,
                                   (int)p.Y,
                                   App1.BaseRadius,
                                   EllipseBase));
            }
            MainCanvas.Children.Add(EllipseBase);

            Canvas.SetLeft(EllipseBase, p.X - App1.BaseRadius);
            Canvas.SetTop(EllipseBase, p.Y - App1.BaseRadius);

            Canvas.SetZIndex(EllipseBase, -1);
        }
        private void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content)
            {
                case "Home":

                    App1.HomeButtonTrigger = true;
                    App1.FoodButtonTrigger = false;
                    App1.BorderButtonTrigger = false;

                    break;

                case "Food":

                    App1.HomeButtonTrigger = false;
                    App1.FoodButtonTrigger = true;
                    App1.BorderButtonTrigger = false;

                    break;

                case "Border":

                    App1.HomeButtonTrigger = false;
                    App1.FoodButtonTrigger = false;
                    App1.BorderButtonTrigger = true;

                    break;
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (App1.StartButtonTrigger)
            {
                if (App1.ModeButtonTrigger)
                {
                    App1.Timer.Tick -= PauseLoop1;
                    App1.Timer.Tick += MainLoop1;
                }
                else
                {
                    App1.Timer.Tick -= PauseLoop2;
                    App1.Timer.Tick += MainLoop2;
                }

                App1.StartButtonTrigger = false;
            }
            else
            {
                if (App1.ModeButtonTrigger)
                {
                    App1.Timer.Tick -= MainLoop1;
                    App1.Timer.Tick += PauseLoop1;
                }
                else
                {
                    App1.Timer.Tick -= MainLoop2;
                    App1.Timer.Tick += PauseLoop2;
                }

                App1.StartButtonTrigger = true;
            }
        }
        private void ModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (App1.StartButtonTrigger) StartButton_Click(null, null);

            App1.ModeButtonTrigger = !App1.ModeButtonTrigger;

            ChangeMode();
        }
        private void HealthButton_Click(object sender, RoutedEventArgs e)
        {
            App1.HealthButtonTrigger = !App1.HealthButtonTrigger;

            foreach (Base Base in App1.Bases)
            {
                if (Base is Home) ((Home)Base).Health = 10 * (int)Base.Ellipse.Width / 2;

                else if (Base is Food) ((Food)Base).Pieces = 10 * (int)Base.Ellipse.Width / 2;
            }
        }
        private void UpdateSize()
        {
            App1.Width = (int)MainCanvas.ActualWidth;
            App1.Height = (int)MainCanvas.ActualHeight;
        }
        private void QueensSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App1.QueensQuantity = (int)((Slider)sender).Value;
        }
        private void MovableFoodsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App1.MovableFoodsQuantity = (int)((Slider)sender).Value;
        }
        private void NewQueenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            App1.NewQueen = (int)((Slider)sender).Value;
            NewQueenText.Text = $"Steps to Queen: {App1.NewQueen}";
        }
    }
}