using System;
using static System.Console;

class ConsoleControl: IControl {
	public ControlResult Control() {
		ControlResult r = ControlResult.Nothing;
		while(Console.KeyAvailable) {
			var k = Console.ReadKey(true);
			switch(k.Key) {
			case ConsoleKey.Escape:
				return ControlResult.Exit;
			case ConsoleKey.LeftArrow:
				r = ControlResult.Left;
				break;
			case ConsoleKey.RightArrow:
				r = ControlResult.Right;
				break;
			case ConsoleKey.UpArrow:
				r = ControlResult.Up;
				break;
			case ConsoleKey.DownArrow:
				r = ControlResult.Down;
				break;
			}
		}
		return r;
	}
}

class ConsoleView: IView {
	public ConsoleView() {
		CursorVisible = false;
	}

	public void Clear() {
		Console.Clear();
	}
	public void Wall(int x, int y) {
		SetCursorPosition(x, y);
		Write('#');
	}
	public void Candy(int x, int y) {
		SetCursorPosition(x, y);
		Write('O');
	}
	public void Snake(Snake s) {
		foreach((int X, int Y) in s.body) {
			SetCursorPosition(X, Y);
			Write('+');
		}
	}

	public void Result(int? score) {
		CursorVisible = true;
		SetCursorPosition(0, 0);
		Clear();
		if(score.HasValue) {
			WriteLine($"score: {score.Value}");
		}
	}
}
