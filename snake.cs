using System;
using System.Threading;
using System.Collections.Generic;

interface IControl {
	/// The result of the user input.
	ControlResult Control();
}

/// The result of Controler.Control()
public enum ControlResult {
	/// The gamer doesn't want to do something.
	Nothing,
	/// The gamer want to exit.
	Exit,
	/// The user want go to left.
	Left,
	/// The user want to go the the right.
	Right,
	/// The user want to go the the top.
	Up,
	/// The user want to go the the bottom.
	Down,
}

/// A interface to diplay the board. At each round the method Clear then
/// Candy for each active candys then Snake if called.
interface IView {
	/// Clear the view.
	void Clear();
	/// Draw one part of the wall.
	void Wall(int x, int y);
	/// Display a candy.
	void Candy(int x, int y);
	/// Display the snake.
	void Snake(Snake s);
	/// Display the result at the end of the game.
	void Result(int? score);
}

class Wall {
	public int BeginX, BeginY, EndX, EndY;

	/// All points of the wall.
	public IEnumerator<(int X, int Y)> GetEnumerator() {
		for(int x = BeginX; x < EndX; x++) {
			for(int y = BeginY; y < EndY; y++) {
				yield return (x, y);
			}
		}
	}

	/// Return true if one of this coordonate is over the wall.
	public bool over(int x, int y) {
		return BeginX <= x && x < EndX &&
		       BeginY <= y && y < EndY;
	}
}

class Board {
	public int Width = 20;
	public int Height = 20;

	/// The walls: the snake and the candys must not be over.
	public List<Wall> Walls = new();

	/// Split the Board in four zone at each coin with a wross.
	public Board CrossWall() {
		int m;

		m = Width / 2;
		Walls.Add(new Wall {
			BeginX = m,
			EndX = m + 1,
			BeginY = 0,
			EndY = Height,
		});

		m = Height / 2;
		Walls.Add(new Wall {
			BeginX = 0,
			EndX = Width,
			BeginY = m,
			EndY = m + 1,
		});

		return this;
	}

	/// Return true if the coord is over a wall.
	public bool OnWall(int x, int y) {
		foreach(Wall w in Walls) {
			if(w.over(x, y)) {
				return true;
			}
		}
		return false;
	}

	/// The duration between each refresh 100ms
	static TimeSpan Refresh = new TimeSpan(100_000_0);

	/// Run one game with with board.
	public int? Play(IView view, IControl ctr) {
		Snake snake = new Snake(this);
		var candys = new Candy[5];
		for(int i = 0; i < candys.Length; i++) {
			candys[i] = new Candy(this);
		}

		while(true) {
			view.Clear();
			// Display the walls.
			foreach(Wall w in Walls) {
				foreach((int X, int Y) in w) {
					view.Wall(X, Y);
				}
			}

			// Regenarate and print candys.
			foreach(Candy c in candys) {
				c.Regenerate();
				if(c.Coord.HasValue) {
					view.Candy(c.Coord.Value.X, c.Coord.Value.Y);
				}
			}
			// Move the snake.
			ControlResult cr = ctr.Control();
			switch(cr) {
			case ControlResult.Exit:
				view.Result(null);
				return null;
			case ControlResult.Nothing:
				break;
			default:
				snake.SetDirection(cr);
				break;
			}
			if(snake.Move(candys)) {
				view.Result(snake.Result);
				return snake.Result;
			}
			view.Snake(snake);
			Thread.Sleep(Board.Refresh);
		}
	}
}

class Snake {
	static private int initSize = 3;

	/// The body of the snake, the head is the zero index.
	public List<(int X, int Y)> body = new();

	Board board;

	public Snake(Board b) {
		board = b;
		for(int i = initSize; i > 0; i--) {
			body.Add((X: 3 + i, Y: 2));
		}
	}

	/// The direction of the Snake.
	enum Direction {
		Left,
		Right,
		Up,
		Down,
	}
	/// The direction movement of the snake.
	Direction direction = Direction.Right;
	public void SetDirection(ControlResult cr) {
		switch(cr) {
		case ControlResult.Left:
			if(direction != Direction.Right) {
				direction = Direction.Left;
			}
			break;
		case ControlResult.Right:
			if(direction != Direction.Left) {
				direction = Direction.Right;
			}
			break;
		case ControlResult.Up:
			if(direction != Direction.Down) {
				direction = Direction.Up;
			}
			break;
		case ControlResult.Down:
			if(direction != Direction.Up) {
				direction = Direction.Down;
			}
			break;
		}
	}

	/// Move the snake and return true if the snake walk over itself.
	public bool Move(Candy[] candys) {
		(int X, int Y)last = body[body.Count - 1];
		// Move.
		for(int i = body.Count - 1; i > 0; i--) {
			body[i] = body[i - 1];
		}
		switch(direction) {
		case Direction.Left:
			body[0] = (body[0].X == 0 ? board.Width : body[0].X - 1, body[0].Y);
			break;
		case Direction.Right:
			body[0] = (body[0].X == board.Width ? 0 : body[0].X + 1, body[0].Y);
			break;
		case Direction.Up:
			body[0] = (body[0].X, body[0].Y == 0 ? board.Height : body[0].Y - 1);
			break;
		case Direction.Down:
			body[0] = (body[0].X, body[0].Y == board.Height ? 0 : body[0].Y + 1);
			break;
		}
		// Bump a wall
		if(board.OnWall(body[0].X, body[0].Y)) {
			return true;
		}
		// Eat candy.
		foreach(Candy c in candys) {
			if(c.Coord == body[0]) {
				c.Remove();
				body.Add(last);
				break;
			}
		}
		// Test if the snake walk on itself.
		return body.LastIndexOf(body[0]) != 0;
	}

	/// Result method returns the score from the snake length.
	public int Result {
		get {
			return body.Count - initSize;
		}
	}
}

/// One candy it can be active with coord or not. At each round, call
// Regenerate method to move or regenerate it according to the expiration.
class Candy {
	/// The coordonate, can be active or not.
	public (int X, int Y) ? Coord = null;
	Board board;

	/// The duration before move or regenration.
	DateTime expiration;
	static int LifeRand = 5_000_000_0;
	static int LifeMin = 3_000_000_0;
	private Random rand;

	/// Init a candy, it will after a random duration.
	public Candy(Board b) {
		board = b;
		rand = new Random();
		Remove();
	}

	/// Remove the candy and set a random duration before the regeneration.
	public void Remove() {
		Coord = null;
		expiration = DateTime.Now +
		             new TimeSpan(rand.Next(LifeRand + LifeMin));
	}

	/// If the expiration duration is over, move the candy (or regenerate it).
	public void Regenerate() {
		if(DateTime.Now > expiration) {
			expiration = DateTime.Now +
			             new TimeSpan(rand.Next(LifeRand) + LifeMin);
			do {
				Coord = (rand.Next(board.Width), rand.Next(board.Height));
			} while(board.OnWall(Coord.Value.X, Coord.Value.Y));
		}
	}
}
