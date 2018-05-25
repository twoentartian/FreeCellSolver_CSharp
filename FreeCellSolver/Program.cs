using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeskSpace;

namespace FreeCellSolver
{
	class Program
	{
		static void Main(string[] args)
		{
			CommandManager.GetInstance().ExecuteCommand("h");
			while (true)
			{
				Console.Write("Enter New Command:");
				string userInput = Console.ReadLine();
				Console.WriteLine();
				if (!string.IsNullOrWhiteSpace(userInput))
				{
					CommandManager.GetInstance().ExecuteCommand(userInput);
				}
			}
		}
	}
}
