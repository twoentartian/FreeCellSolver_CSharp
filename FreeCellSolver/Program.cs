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
		

		/*
		static void Main(string[] args)
		{
			Desk tempDesk1 = new Desk();
			for (int coloumIndex = 0; coloumIndex < 4; coloumIndex++)
			{
				for (Card.Number i = Card.Number.King; i >= Card.Number.Arch; i--)
				{
					tempDesk1.AddNewCardInColoum(coloumIndex, new Card((Card.Type)coloumIndex + 1, i));
				}
			}
			string json = tempDesk1.GetJson();
			using (var fs = File.Create(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "test.json"))
			{
				byte[] byteArray = Encoding.Default.GetBytes(json);
				fs.Write(byteArray, 0, byteArray.Length);
			}
		}
		*/
	}
}
