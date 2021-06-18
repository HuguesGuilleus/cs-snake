using System;

class Program {
	static void Main(string[] args) {
		var dimension = new BoardDimension {
			Width = Console.WindowWidth,
			Height = Console.WindowHeight - 1,
		};

		new Board(dimension).Play(
			new ConsoleArrayView(dimension),
			new ConsoleControl()
			);
	}
}
