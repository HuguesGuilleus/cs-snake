using System;

class Program {
	static void Main(string[] args) {
		new Board {
			Width =  Console.WindowWidth,
			Height = Console.WindowHeight,
		}.CrossWall().Play(new ConsoleView(), new ConsoleControl());
	}
}
