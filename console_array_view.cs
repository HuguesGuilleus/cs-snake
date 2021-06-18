using System;
using System.Text;

class ConsoleArrayView: IView {
	public readonly BoardDimension dimension;

	private char[] screen;

	private char this[int x, int y] {
		set => this.screen[y * this.dimension.Width + x] = value;
	}

	public ConsoleArrayView(BoardDimension pdimension) {
		this.dimension = pdimension;
		this.screen = new char[this.dimension.Height * (this.dimension.Width + 0)];
		this.Clear();
		Console.CursorVisible = false;
	}

	public void Clear() {
		System.Array.Fill(this.screen, ' ');
	}
	public void Wall(int x, int y) {
		this[x, y] = '#';
	}
	public void Candy(int x, int y) {
		this[x, y] = 'O';
	}
	public void Snake(Snake s) {
		foreach((int X, int Y) in s.body) {
			this[X, Y] = '+';
		}
		Console.Write(this.screen);
	}

	public void Result(int? score) {
		Console.CursorVisible = true;
		Console.Clear();
		if(score.HasValue) {
			Console.WriteLine($"score: {score.Value}");
		}
	}
}
