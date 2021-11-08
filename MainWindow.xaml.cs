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
            _Vect = AntsAIGame.Rnd.NextDouble() * Math.PI * 2;
            _Speed = 2 + AntsAIGame.Rnd.Next(2);
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
            _Speed = AntsAIGame.Rnd.NextDouble() + 0.2;
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
            _Speed = AntsAIGame.Rnd.NextDouble() + 0.2;
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
    class AntsAIGame
    {
        public List<Ant> Ants = new List<Ant>();
        public List<Base> Bases = new List<Base>();
        public List<Queen> Queens = new List<Queen>();
        public List<MovableFood> MovableFoods = new List<MovableFood>();
        public List<double[]> Mas = new List<double[]>();

        private int _AntsQuantity;
        public int AntsQuantity { get => _AntsQuantity; set => _AntsQuantity = value; }

        protected double AntRadius = 1.5;
        public int BaseRadius = 40;
        protected int SignalRadius = 32;

        public static int Width;
        public static int Height;

        protected int Counter = 0;

        public bool HomeButtonTrigger = true;
        public bool FoodButtonTrigger = false;
        public bool BorderButtonTrigger = false;
        public bool StartButtonTrigger = true;
        public bool HealthButtonTrigger = false;
        public static bool ModeButtonTrigger = true;

        protected SolidColorBrush HomeColor = new SolidColorBrush(Color.FromRgb(81, 77, 255));
        protected SolidColorBrush BorderColor = new SolidColorBrush(Color.FromRgb(112, 128, 144));
        protected SolidColorBrush ScoutColor = new SolidColorBrush(Color.FromRgb(28, 28, 56));
        protected SolidColorBrush FoodColor = new SolidColorBrush(Color.FromRgb(255, 67, 67));
        protected SolidColorBrush White = new SolidColorBrush(Color.FromRgb(226, 226, 226));

        public static Random Rnd = new Random();

        public static DispatcherTimer Timer = new DispatcherTimer();

        protected Stopwatch Time = new Stopwatch();

        public AntsAIGame() 
        {
            _AntsQuantity = 1500;
        }
        protected void Draw()
        {
            foreach (Ant Ant in Ants)
            {
                Canvas.SetLeft(Ant.Ellipse, Ant.X - AntRadius);
                Canvas.SetTop(Ant.Ellipse, Ant.Y - AntRadius);

                if (Ant is Worker)
                {
                    Ant.Ellipse.Fill = ((Worker)Ant).Target ? FoodColor : HomeColor;
                }
                else if (Ant is Scout)
                {
                    Ant.Ellipse.Fill = ((Scout)Ant).State ? HomeColor : ScoutColor;
                }
            }
            foreach (Base Base in Bases)
            {
                Canvas.SetLeft(Base.Ellipse, Base.X - Base.Radius);
                Canvas.SetTop(Base.Ellipse, Base.Y - Base.Radius);
            }

            if (ModeButtonTrigger) return;

            foreach (Queen Queen in Queens)
            {
                Canvas.SetLeft(Queen.Ellipse, Queen.X - Queen.Radius);
                Canvas.SetTop(Queen.Ellipse, Queen.Y - Queen.Radius);
            }
            foreach (MovableFood MovableFood in MovableFoods)
            {
                Canvas.SetLeft(MovableFood.Ellipse, MovableFood.X - MovableFood.Radius);
                Canvas.SetTop(MovableFood.Ellipse, MovableFood.Y - MovableFood.Radius);
            }
        }
        protected void UpdateDir(Ant Ant)
        {
            Mas.Add(new double[5] {
                    Ant.X,
                    Ant.Y,
                    Ant.Steps[0] + SignalRadius,
                    Ant.Steps[1] + SignalRadius,
                    Ant.Id });
        }
        protected void Check(Ant Ant)
        {
            foreach (Base El in Bases)
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
                                    Counter++;
                                }
                            }
                        }
                        else if (Ant is Scout)
                        {
                            if (((Scout)Ant).State)
                            {
                                ((Scout)Ant).State = false;

                                ((Home)El).Health++;
                                Counter++;
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
        protected void ChangeDir(Ant Ant)
        {
            if (Ant.TriggerCol2) return;

            foreach (double[] El in Mas)
            {
                if (Ant.Id != El[4])
                {
                    if ((El[0] - Ant.X) * (El[0] - Ant.X) +
                        (El[1] - Ant.Y) * (El[1] - Ant.Y) <=
                        SignalRadius * SignalRadius)
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
        protected void Go(Ant Ant)
        {
            Ant.X += Math.Cos(Ant.Vect + (Rnd.Next(6) - 3) * Math.PI / 180) * Ant.Speed;
            Ant.Y += Math.Sin(Ant.Vect + (Rnd.Next(6) - 3) * Math.PI / 180) * Ant.Speed;

            if (Ant.X > Width - 2 || Ant.X < 1)
            {
                Ant.Vect = Math.PI - Ant.Vect;
                Ant.X = Ant.X < 1 ? 1 : Width - 2;
            }
            if (Ant.Y > Height - 2 || Ant.Y < 1)
            {
                Ant.Vect = Math.PI * 2 - Ant.Vect;
                Ant.Y = Ant.Y < 1 ? 1 : Height - 2;
            }

            Ant.Steps[0]++;
            Ant.Steps[1]++;
        }
        protected void UpdateBases()
        {
            foreach (Base Base in Bases)
            {
                if (Base is Food)
                {
                    Base.Radius = ((Food)Base).Pieces / 10;

                    Base.Ellipse.Width = Base.Ellipse.Height = Base.Radius * 2;

                    if (((Food)Base).Pieces == 0) ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Base.Ellipse);
                }
                else if (Base is Home)
                {
                    ((Home)Base).Health--;

                    Base.Radius = ((Home)Base).Health / 10;

                    Base.Ellipse.Width = Base.Ellipse.Height = Base.Radius * 2;

                    if (((Home)Base).Health == 0) ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Base.Ellipse);
                }
            }
            Bases.RemoveAll((El) => El is Food ? ((Food)El).Pieces <= 0 : El is Home ? ((Home)El).Health <= 0 : false);
        }
        protected void UpdateAnts()
        {
            while (Ants.Count < AntsQuantity)
            {
                int X = Rnd.Next(Width / 2) + Width / 4;
                int Y = Rnd.Next(Height / 2) + Height / 4;

                int Target = Rnd.Next(19);

                Ellipse EllipseAnt = new Ellipse()
                {
                    Width = AntRadius * 2,
                    Height = AntRadius * 2,
                    Fill = Target < 9 ? FoodColor : Target < 18 ? HomeColor : ScoutColor
                };

                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Add(EllipseAnt);

                if (Target < 9) Ants.Add(new Worker(X, Y, false, EllipseAnt));

                else if (Target < 18) Ants.Add(new Worker(X, Y, true, EllipseAnt));

                else if (Target < 19) Ants.Add(new Scout(X, Y, EllipseAnt));
            }
            while (Ants.Count > AntsQuantity)
            {
                Ant Ant = Ants[0];

                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Ant.Ellipse);

                Ants.Remove(Ant);
            }
            if (ModeButtonTrigger) ((MainWindow)Application.Current.MainWindow).AntsText1.Text = $"Max Ants: {AntsQuantity} Ants: {Ants.Count}";

            else ((MainWindow)Application.Current.MainWindow).AntsText2.Text = $"Max Ants: {AntsQuantity} Ants: {Ants.Count}";
        }
        public void SpawnBase(Point p)
        {

            foreach (Base B in Bases)
            {
                if (Math.Pow(B.X - p.X, 2) + Math.Pow(B.Y - p.Y, 2) <= Math.Pow(B.Radius, 2))
                {
                    ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(B.Ellipse);

                    Bases.Remove(B);

                    return;
                }
            }
            Ellipse EllipseBase = new Ellipse()
            {
                Width = BaseRadius * 2,
                Height = BaseRadius * 2,
                Fill = HomeButtonTrigger ? HomeColor :
                       FoodButtonTrigger ? FoodColor :
                       BorderColor
            };

            if (HomeButtonTrigger)
            {
                Bases.Add(new Home(p.X, p.Y,BaseRadius, EllipseBase));
            }
            else if (FoodButtonTrigger)
            {
                Bases.Add(new Food(p.X, p.Y, BaseRadius,EllipseBase));
            }
            else if (BorderButtonTrigger)
            {
                Bases.Add(new Border(p.X, p.Y, BaseRadius, EllipseBase));
            }

            ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Add(EllipseBase);

            Canvas.SetLeft(EllipseBase, p.X - BaseRadius);
            Canvas.SetTop(EllipseBase, p.Y - BaseRadius);
        }
    }
    class Mode1 : AntsAIGame
    {
        public Mode1() : base() { }
        public void PauseLoop1(object sender, object e)
        {
            Rnd = new Random();

            Time.Restart();

            UpdateAnts();
            Draw();

            Time.Stop();

            ((MainWindow)Application.Current.MainWindow).TextFPS1.Text = $"{Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (Time.ElapsedMilliseconds < 1 ? 1 : Time.ElapsedMilliseconds)} FPS";
            ((MainWindow)Application.Current.MainWindow).TextCounter.Text = $"Count: {Counter}";
        }
        public void MainLoop1(object sender, object e)
        {
            Rnd = new Random();

            Time.Restart();

            UpdateAnts();
            if (HealthButtonTrigger) UpdateBases();

            Draw();

            foreach (Ant Ant in Ants) UpdateDir(Ant);

            Parallel.ForEach(Ants, Check);
            Parallel.ForEach(Ants, ChangeDir);
            Parallel.ForEach(Ants, Go);

            Mas.Clear();

            Time.Stop();

            ((MainWindow)Application.Current.MainWindow).TextFPS1.Text = $"{Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (Time.ElapsedMilliseconds < 1 ? 1 : Time.ElapsedMilliseconds)} FPS";
            ((MainWindow)Application.Current.MainWindow).TextCounter.Text = $"Count: {Counter}";
        }
    }
    class Mode2 : AntsAIGame
    {
        private int _QueensQuantity;
        public int QueensQuantity { get => _QueensQuantity; set => _QueensQuantity = value; }

        private int _MovableFoodsQuantity;
        public int MovableFoodsQuantity { get => _MovableFoodsQuantity; set => _MovableFoodsQuantity = value; }

        private int _NewQueen;
        public int NewQueen { get => _NewQueen; set => _NewQueen = value; }

        public Mode2() : base()
        {
            _MovableFoodsQuantity = 10;
            _QueensQuantity = 5;
            _NewQueen = 400;
        }
        public void PauseLoop2(object sender, object e)
        {
            Rnd = new Random();

            Time.Restart();

            UpdateAnts();
            UpdateBases();

            CreateQueen();
            SpawnMovableFood();

            Draw();

            Time.Stop();

            ((MainWindow)Application.Current.MainWindow).TextFPS2.Text = $"{Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (Time.ElapsedMilliseconds < 1 ? 1 : Time.ElapsedMilliseconds)} FPS";
        }
        public void MainLoop2(object sender, object e)
        {
            Rnd = new Random();

            Time.Restart();

            UpdateAnts();

            UpdateMovableFoods();
            SpawnMovableFood();

            CreateQueen();
            UpdateQueens();

            Parallel.ForEach(Queens, UpdateQueenVect);

            Draw();

            foreach (Ant Ant in Ants) UpdateDir(Ant);

            Parallel.ForEach(Ants, CheckMode2);
            Parallel.ForEach(Queens, QueenCheck);
            Parallel.ForEach(MovableFoods, MovableFoodCheck);

            Parallel.ForEach(Ants, ChangeDir);

            Parallel.ForEach(Ants, Go);
            Parallel.ForEach(Queens, QueenGo);
            Parallel.ForEach(MovableFoods, MovableFoodGo);

            Mas.Clear();

            Time.Stop();

            ((MainWindow)Application.Current.MainWindow).TextFPS2.Text = $"{Time.ElapsedMilliseconds} ms | " +
                            $"{1000 / (Time.ElapsedMilliseconds < 1 ? 1 : Time.ElapsedMilliseconds)} FPS";
        }
        protected void CreateQueen()
        {
            
            while (Queens.Count > QueensQuantity)
            {
                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Queens[0].Ellipse);
                Queens.RemoveAt(0);
            }
            if (Queens.Count == 0)
            {
                Ellipse Ellipse = new Ellipse() { Width = BaseRadius, Height = BaseRadius, Fill = HomeColor };
                Queen Queen = new Queen(Width / 2, Height / 2, Ellipse);

                Queens.Add(Queen);

                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Add(Ellipse);

                Canvas.SetLeft(Ellipse, Queen.X - 20);
                Canvas.SetTop(Ellipse, Queen.Y - 20);
            }
            foreach (Ant Ant in Ants)
            {
                if (Ant.Steps[0] > NewQueen && Queens.Count < QueensQuantity)
                {
                    Ellipse Ellipse = new Ellipse() { Width = 40, Height = 40, Fill = HomeColor };

                    Queens.Add(new Queen(Ant.X, Ant.Y, Ellipse));

                    ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Add(Ellipse);
                    ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Ant.Ellipse);
                }
            }
            Ants.RemoveAll((El) => El.Steps[0] > 500);

            ((MainWindow)Application.Current.MainWindow).QueensText.Text = $"Max Queens: {QueensQuantity} Queens: {Queens.Count}";
        }
        protected void UpdateMovableFoods()
        {
            foreach (MovableFood MovableFood in MovableFoods)
            {
                MovableFood.Radius = MovableFood.Pieces / 20;

                MovableFood.Ellipse.Width = MovableFood.Ellipse.Height = MovableFood.Radius * 2;

                if (MovableFood.Pieces <= 0) ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(MovableFood.Ellipse);
            }
            MovableFoods.RemoveAll((El) => El.Pieces <= 0);
        }
        protected void UpdateQueens()
        {
            foreach (Queen Queen in Queens)
            {
                Queen.Health--;

                Queen.Radius = Queen.Health / 20;

                Queen.Ellipse.Width = Queen.Ellipse.Height = Queen.Radius * 2;

                if (Queen.Health <= 0) ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(Queen.Ellipse);
            }
            Queens.RemoveAll((El) => El.Health <= 0);
        }
        protected void SpawnMovableFood()
        {
            while (MovableFoods.Count < MovableFoodsQuantity)
            {

                Ellipse Ellipse = new Ellipse { Width = BaseRadius, Height = BaseRadius, Fill = FoodColor };

                MovableFoods.Add(new MovableFood(
                                        Rnd.Next(Width - 48) + 24,
                                        Rnd.Next(Height - 48) + 24,
                                        Ellipse));
                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Add(Ellipse);
            }
            while (MovableFoods.Count > MovableFoodsQuantity)
            {
                ((MainWindow)Application.Current.MainWindow).MainCanvas.Children.Remove(MovableFoods[0].Ellipse);
                MovableFoods.RemoveAt(0);
            }

            ((MainWindow)Application.Current.MainWindow).MovableFoodsText.Text = $"Food: {MovableFoodsQuantity}";
        }
        protected void UpdateQueenVect(Queen Queen)
        {
            double Len = 0;

            double X = Width / 2;
            double Y = Height / 2;

            foreach (MovableFood El in MovableFoods)
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
        protected void CheckMode2(Ant Ant)
        {
            foreach (MovableFood El in MovableFoods)
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

            foreach (Queen El in Queens)
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
                                Counter++;
                            }
                        }
                    }
                    else if (Ant is Scout)
                    {
                        if (((Scout)Ant).State)
                        {
                            ((Scout)Ant).State = false;

                            El.Health++;
                            Counter++;
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
        protected void QueenCheck(Queen El)
        {
            foreach (Queen Queen in Queens)
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
            foreach (MovableFood MF in MovableFoods)
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
        protected void MovableFoodCheck(MovableFood El)
        {
            foreach (Queen Queen in Queens)
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
            foreach (MovableFood MF in MovableFoods)
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
        protected void QueenGo(Queen Queen)
        {
            Queen.X += Math.Cos(Queen.Vect + (Rnd.Next(6) - 3) * Math.PI / 180) * Queen.Speed;
            Queen.Y += Math.Sin(Queen.Vect + (Rnd.Next(6) - 3) * Math.PI / 180) * Queen.Speed;

            if (Queen.X + Queen.Radius > Width - 2 || Queen.X - Queen.Radius < 1)
            {
                Queen.Vect = Math.PI - Queen.Vect;
                Queen.X = Queen.X - Queen.Radius < 1 ? Queen.Radius + 1 : Width - Queen.Radius - 1;
            }
            if (Queen.Y + Queen.Radius > Height - 2 || Queen.Y - Queen.Radius < 1)
            {
                Queen.Vect = Math.PI * 2 - Queen.Vect;
                Queen.Y = Queen.Y - Queen.Radius < 1 ? Queen.Radius + 1 : Height - Queen.Radius - 1;
            }
        }
        protected void MovableFoodGo(MovableFood MovableFood)
        {
            MovableFood.X += Math.Cos(MovableFood.Vect + (Rnd.Next(4) - 2) * Math.PI / 180) * MovableFood.Speed;
            MovableFood.Y += Math.Sin(MovableFood.Vect + (Rnd.Next(4) - 2) * Math.PI / 180) * MovableFood.Speed;

            if (MovableFood.X + MovableFood.Radius > Width - 1 ||
                MovableFood.X - MovableFood.Radius < 1)
            {
                MovableFood.Vect = Math.PI - MovableFood.Vect;

                MovableFood.X = MovableFood.X - MovableFood.Radius < 1 ?
                                    MovableFood.Radius + 1 :
                                    Width - MovableFood.Radius - 1;
            }
            if (MovableFood.Y + MovableFood.Radius > Height - 1 ||
                MovableFood.Y - MovableFood.Radius < 1)
            {
                MovableFood.Vect = Math.PI * 2 - MovableFood.Vect;

                MovableFood.Y = MovableFood.Y - MovableFood.Radius < 1 ?
                                    MovableFood.Radius + 1 :
                                    Height - MovableFood.Radius - 1;
            }
        }
    }
    public partial class MainWindow : Window
    {
        private AntsAIGame Game = new AntsAIGame();

        public Random Rnd = new Random();
        public MainWindow()
        {
            InitializeComponent();

            ChangeMode();
        }
        private void ChangeMode()
        {
            AntsAIGame.Timer.Stop();

            MainCanvas.Children.Clear();

            AntsAIGame.Timer = new DispatcherTimer();

            AntsAIGame.Timer.Interval = TimeSpan.FromMilliseconds(20);

            AntsAIGame.Timer.Tick += UpdateSize;

            if (AntsAIGame.ModeButtonTrigger)
            {
                Game = new Mode1();

                AntsAIGame.Timer.Tick += ((Mode1)Game).PauseLoop1;

                AntsSlider.Value = Game.AntsQuantity;
                RadiusSlider.Value = Game.BaseRadius;

                Mode2Grid.Visibility = Visibility.Collapsed;
                Mode1Grid.Visibility = Visibility.Visible;
            }
            else
            {
                Game = new Mode2();

                AntsAIGame.Timer.Tick += ((Mode2)Game).PauseLoop2;

                AntsSlider2.Value = Game.AntsQuantity;
                QueensSlider.Value = ((Mode2)Game).QueensQuantity;
                NewQueenSlider.Value = ((Mode2)Game).NewQueen;
                MovableFoodsSlider.Value = ((Mode2)Game).MovableFoodsQuantity;

                Mode1Grid.Visibility = Visibility.Collapsed;
                Mode2Grid.Visibility = Visibility.Visible;
            }

            AntsAIGame.Timer.Start();
        }
        private void AntsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Game.AntsQuantity = (int)((Slider)sender).Value;
        }
        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int Value = (int)((Slider)sender).Value;

            Game.BaseRadius = Value;

            RadiusText.Text = $"Radius: {Value}";
        }
        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AntsAIGame.ModeButtonTrigger) Game.SpawnBase(e.GetPosition(this));
        }
        private void BaseButton_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content)
            {
                case "Home":

                    Game.HomeButtonTrigger = true;
                    Game.FoodButtonTrigger = false;
                    Game.BorderButtonTrigger = false;

                    break;

                case "Food":

                    Game.HomeButtonTrigger = false;
                    Game.FoodButtonTrigger = true;
                    Game.BorderButtonTrigger = false;

                    break;

                case "Border":

                    Game.HomeButtonTrigger = false;
                    Game.FoodButtonTrigger = false;
                    Game.BorderButtonTrigger = true;

                    break;
            }
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (Game.StartButtonTrigger)
            {
                if (AntsAIGame.ModeButtonTrigger)
                {
                    AntsAIGame.Timer.Tick -= ((Mode1)Game).PauseLoop1;
                    AntsAIGame.Timer.Tick += ((Mode1)Game).MainLoop1;
                }
                else
                {
                    AntsAIGame.Timer.Tick -= ((Mode2)Game).PauseLoop2;
                    AntsAIGame.Timer.Tick += ((Mode2)Game).MainLoop2;
                }

                Game.StartButtonTrigger = false;
            }
            else
            {
                if (AntsAIGame.ModeButtonTrigger)
                {
                    AntsAIGame.Timer.Tick -= ((Mode1)Game).MainLoop1;
                    AntsAIGame.Timer.Tick += ((Mode1)Game).PauseLoop1;
                }
                else
                {
                    AntsAIGame.Timer.Tick -= ((Mode2)Game).MainLoop2;
                    AntsAIGame.Timer.Tick += ((Mode2)Game).PauseLoop2;
                }

                Game.StartButtonTrigger = true;
            }
        }
        private void ModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Game.StartButtonTrigger) StartButton_Click(null, null);

            AntsAIGame.ModeButtonTrigger = !AntsAIGame.ModeButtonTrigger;

            ChangeMode();
        }
        private void HealthButton_Click(object sender, RoutedEventArgs e)
        {
            if (AntsAIGame.ModeButtonTrigger)
            {
                Game.HealthButtonTrigger = !Game.HealthButtonTrigger;

                foreach (Base Base in Game.Bases)
                {
                    if (Base is Home) ((Home)Base).Health = 10 * (int)Base.Ellipse.Width / 2;

                    else if (Base is Food) ((Food)Base).Pieces = 10 * (int)Base.Ellipse.Width / 2;
                }
            }
        }
        private void UpdateSize(object sender, object e)
        {
            AntsAIGame.Width = (int)MainCanvas.ActualWidth;
            AntsAIGame.Height = (int)MainCanvas.ActualHeight;
        }
        private void QueensSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AntsAIGame.ModeButtonTrigger) ((Mode2)Game).QueensQuantity = (int)((Slider)sender).Value;
        }
        private void MovableFoodsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!AntsAIGame.ModeButtonTrigger) ((Mode2)Game).MovableFoodsQuantity = (int)((Slider)sender).Value;
        }
        private void NewQueenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (!AntsAIGame.ModeButtonTrigger)
            {
                ((Mode2)Game).NewQueen = (int)((Slider)sender).Value;

                NewQueenText.Text = $"Steps to Queen: {((Mode2)Game).NewQueen}";
            }
        }
    }
}