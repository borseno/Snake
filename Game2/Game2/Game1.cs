using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Game2
{
    abstract class Entity : IComparable
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Entity(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Entity()
        {
            X = 0;
            Y = 0;
        }

        public int CompareTo(object a)
        {
            if (a is Entity entity)
            {
                if (this.X == entity.X)
                    return this.Y - entity.Y;
                return this.X - entity.X;
            }
            throw new InvalidCastException();
        }
        public override string ToString()
        {
            return this.GetType().Name + " coords: X = " + this.X + " Y = " + this.Y;
        }
    }

    class SnakePoint : Entity
    {
        public SnakePoint(int x, int y) : base(x, y) { }
        public SnakePoint() : base() { }
    }

    class Hash : Entity
    {
        public Hash(int x, int y) : base(x, y) { }
        public Hash() : base() { }
    }


    class Snake
    {
        public LinkedList<SnakePoint> SnakePoints { get; private set; }
        public char Direction { get; private set; } = 'd';

        private int _speed;
        public int Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                if (value >= 0 && value < 150)
                {
                    _speed = value;
                }
            }
        }

        public bool Move(char? direction = null, int? Speed = null)
        {
            if (Speed == null)
                Speed = _speed;

            if (direction == 'd')
            {
                SnakePoints.RemoveLast();
                SnakePoints.AddFirst(new SnakePoint(SnakePoints.First.Value.X + (int)Speed, SnakePoints.First.Value.Y));
                this.Direction = (char)direction;
            }
            else if (direction == 's')
            {
                SnakePoints.RemoveLast();
                SnakePoints.AddFirst(new SnakePoint(SnakePoints.First.Value.X, SnakePoints.First.Value.Y + (int)Speed));
                this.Direction = (char)direction;
            }
            else if (direction == 'a')
            {
                SnakePoints.RemoveLast();
                SnakePoints.AddFirst(new SnakePoint(SnakePoints.First.Value.X - (int)Speed, SnakePoints.First.Value.Y));
                this.Direction = (char)direction;
            }
            else if (direction == 'w')
            {
                SnakePoints.RemoveLast();
                SnakePoints.AddFirst(new SnakePoint(SnakePoints.First.Value.X, SnakePoints.First.Value.Y - (int)Speed));
                this.Direction = (char)direction;
            }
            else if (direction == null)
            {
                Move(this.Direction, Speed);
            }
            return true;
        }
        public void Eat(Hash hash)
        {
            SnakePoints.AddLast(new SnakePoint(SnakePoints.Last.Value.X, SnakePoints.Last.Value.Y));
            hash = null;
        }
        public Snake()
        {
            SnakePoints = new LinkedList<SnakePoint>();
            SnakePoints.AddLast(new SnakePoint(30, 30));
            SnakePoints.AddLast(new SnakePoint(30, 60));

            Direction = 's';
        }
        public Snake(int Speed)
        {
            this.Speed = Speed;
        }

        public Snake(int Speed, IEnumerable<SnakePoint> snakePoints) : this(Speed)
        {
            SnakePoints = new LinkedList<SnakePoint>(snakePoints);
        }
        public Snake(int Speed, params SnakePoint[] snakePoints) : this(Speed)
        {
            SnakePoints = new LinkedList<SnakePoint>(snakePoints);
        }
    }


    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Snake snake;
        Hash hash;
        Texture2D WhiteTexture;
        List<Keys> NextWay;

        int SnakeSpeed;
        int CurrentMoveTime = 0;
        readonly int MoveTime = 60;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 900,
                PreferredBackBufferHeight = 900
            };
            Content.RootDirectory = "Content";
            TargetElapsedTime = new System.TimeSpan(0, 0, 0, 0, 10);
        }

        protected override void Initialize()
        {
            Random rnd = new Random();

            SnakeSpeed = 30;

            hash = new Hash();
            SnakePoint first = new SnakePoint();
            foreach (var i in new Entity[] { hash, first } )
            {
                InitializeEntity(i);
            }
            SnakePoint second = rnd.Next(1, 2) == 1 ? // 50-50 : X or Y, where the snake extends
                new SnakePoint(first.X + SnakeSpeed, first.Y)
                : new SnakePoint(first.X, first.Y + SnakeSpeed);

            snake = new Snake(SnakeSpeed, first, second);

            NextWay = new List<Keys>();

            base.Initialize();
        }

        private void InitializeEntity(Entity entity)
        {
            Random rnd = new Random();

            int Width = graphics.PreferredBackBufferWidth;
            int Height = graphics.PreferredBackBufferHeight;

            entity.X = (rnd.Next(0, Width - SnakeSpeed) / SnakeSpeed) * SnakeSpeed;
            entity.Y = (rnd.Next(0, Height - SnakeSpeed) / SnakeSpeed) * SnakeSpeed;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            WhiteTexture = Content.Load<Texture2D>("white1x1");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            CurrentMoveTime += gameTime.ElapsedGameTime.Milliseconds;
            if (CurrentMoveTime >= MoveTime)
            {
                CurrentMoveTime = CurrentMoveTime - MoveTime;

                MoveSnake();

                bool EatenHash = SnakeEatsHashIfCan();

                if (EatenHash)
                {
                    GenerateNewHash();
                }

                ControleIfSnakeHitsWalls();

                base.Update(gameTime);
            }
            //else
            //{
            //    foreach (var i in from t in Keyboard.GetState().GetPressedKeys()
            //                      where new Keys[] { Keys.A, Keys.W, Keys.D, Keys.S }.Contains(t) // Like in SQL: t IN (A, W, D, S) =D
            //                      select t)
            //    {
            //        NextWay.Add(i);
            //    }
            //}
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach (var i in snake.SnakePoints)
                spriteBatch.Draw(WhiteTexture, new Vector2(i.X, i.Y), 
                    null, Color.Orange, 0, Vector2.Zero, SnakeSpeed, SpriteEffects.None, 1);

            spriteBatch.Draw(WhiteTexture, new Vector2(hash.X, hash.Y),
                null, Color.Black, 0, Vector2.Zero, SnakeSpeed, SpriteEffects.None, 1);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool IsContinious(Keys[] first, Keys[] second)
        {
            for (int i = 0; i < first.Length; i++)
            {
                for (int j = 0; j < second.Length; j++)
                {
                    if (first[i] == Keys.W || first[i] == Keys.S || first[i] == Keys.A || first[i] == Keys.D)
                    {
                        if (first[i] == second[j])
                            return true;
                    }
                }
            }
            return false;
        }

        private void ControleIfSnakeHitsWalls()
        {
            if (snake.SnakePoints.First.Value.X < 0)
                snake.SnakePoints.First.Value.X = graphics.PreferredBackBufferWidth - SnakeSpeed;
            if (snake.SnakePoints.First.Value.Y < 0)
                snake.SnakePoints.First.Value.Y = graphics.PreferredBackBufferHeight - SnakeSpeed;

            if (snake.SnakePoints.First.Value.X > graphics.PreferredBackBufferWidth - SnakeSpeed)
                snake.SnakePoints.First.Value.X = 0;
            if (snake.SnakePoints.First.Value.Y > graphics.PreferredBackBufferHeight - SnakeSpeed)
                snake.SnakePoints.First.Value.Y = 0;
        }

        private void MoveSnake()
        {
            bool MoveIsDone = false;

            if (NextWay.Count != 0) // There are keys left to handle
            {
                MoveIsDone = snake.Move(NextWay.Last().ToString().ToLower()[0]);
                NextWay.RemoveAt(NextWay.Count -1);
            }
            else
            {
                Keys[] keys = Keyboard.GetState().GetPressedKeys();

                int keysIterator;
                for (keysIterator = (keys.Length - 1); keysIterator >= 0 && !MoveIsDone; keysIterator--)
                {
                    if (keys[keysIterator] == Keys.W)
                        MoveIsDone = snake.Move('w');
                    else if (keys[keysIterator] == Keys.A)
                        MoveIsDone = snake.Move('a');
                    else if (keys[keysIterator] == Keys.S)
                        MoveIsDone = snake.Move('s');
                    else if (keys[keysIterator] == Keys.D)
                        MoveIsDone = snake.Move('d');
                }
                if (!MoveIsDone)
                    snake.Move();
                else
                {
                    for (; keysIterator >= 0; keysIterator--)
                    {
                        if (keys[keysIterator] == Keys.W || keys[keysIterator] == Keys.S
                            || keys[keysIterator] == Keys.A || keys[keysIterator] == Keys.D)
                            NextWay.Add(keys[keysIterator]); // Remain unhandled
                    }
                }
            }
        }

        private void GenerateNewHash()
        {
            Random rnd = new Random();

            int Width = graphics.PreferredBackBufferWidth;
            int Height = graphics.PreferredBackBufferHeight;

            hash = new Hash(
                (rnd.Next(0, Width - SnakeSpeed) / SnakeSpeed) * SnakeSpeed, (
                rnd.Next(0, Height - SnakeSpeed) / SnakeSpeed) * SnakeSpeed);
        }

        private bool SnakeEatsHashIfCan()
        {
            bool result = false;

            if (snake.SnakePoints.First.Value.X == hash.X
    && snake.SnakePoints.First.Value.Y == hash.Y)
            {
                snake.Eat(hash);
                result = true;
            }

            for (var i = snake.SnakePoints.First.Next; i != null; i = i.Next)
            {
                if (snake.SnakePoints.First.Value.X == i.Value.X
                    && snake.SnakePoints.First.Value.Y == i.Value.Y)
                    Exit();
                if (!result && i.Value.X == hash.X && i.Value.Y == hash.Y)
                {
                    snake.Eat(hash);
                    result = true;
                }
            }
            return result;
        }
    }
}
