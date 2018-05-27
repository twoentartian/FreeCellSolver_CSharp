using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DeskSpace;

namespace FreeCellSolver
{
	class Command
	{
		public virtual string GetHelp()
		{
			throw new NotImplementedException();
		}

		public virtual string GetName()
		{
			throw new NotImplementedException();
		}

		public virtual string GetCommand()
		{
			throw new NotImplementedException();
		}

		public virtual void Execute()
		{
			throw new NotImplementedException();
		}
	}

	#region HelpCommand

	class CommandPrintHelp : Command
	{
		#region Singleton

		private static CommandPrintHelp _instance;

		private CommandPrintHelp()
		{

		}

		public static CommandPrintHelp GetInstance()
		{
			return _instance ?? (_instance = new CommandPrintHelp());
		}

		#endregion

		public override string GetHelp()
		{
			return "Get this help.";
		}

		public override string GetName()
		{
			return "help";
		}

		public override string GetCommand()
		{
			return "h";
		}

		public override void Execute()
		{
			Console.WriteLine("Help:");
			foreach (var commandPair in CommandManager.GetInstance().CommandSet)
			{
				Command currentComand = commandPair.Value;
				Console.WriteLine($"Command: {currentComand.GetCommand()} ({currentComand.GetName()}) - {currentComand.GetHelp()}");
			}
		}
	}

	#endregion

	#region QuitCommand

	class CommandQuit : Command
	{
		#region Singleton

		private static CommandQuit _instance;

		private CommandQuit()
		{

		}

		public static CommandQuit GetInstance()
		{
			return _instance ?? (_instance = new CommandQuit());
		}

		#endregion

		public override string GetHelp()
		{
			return "Quit application.";
		}

		public override string GetName()
		{
			return "quit";
		}

		public override string GetCommand()
		{
			return "q";
		}

		public override void Execute()
		{
			Environment.Exit(0);
		}
	}

	#endregion

	#region GenerateSampleCommand

	class CommandGenerateSample : Command
	{
		#region Singleton

		private static CommandGenerateSample _instance;

		private CommandGenerateSample()
		{

		}

		public static CommandGenerateSample GetInstance()
		{
			return _instance ?? (_instance = new CommandGenerateSample());
		}

		#endregion

		public override string GetHelp()
		{
			return "Generate a sample json file to present the desk.";
		}

		public override string GetName()
		{
			return "generate sample";
		}

		public override string GetCommand()
		{
			return "gs";
		}

		public override void Execute()
		{
			string json = Ui.GetInstance().GenerateSampleDeskJson();
			using (var fs = File.Create(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Sample.json"))
			{
				byte[] byteArray = Encoding.Default.GetBytes(json);
				fs.Write(byteArray, 0, byteArray.Length);
			}
		}
	}

	#endregion

	#region Solve Desk Command

	class CommandSolveDesk : Command
	{
		#region Singleton

		private static CommandSolveDesk _instance;

		private CommandSolveDesk()
		{

		}

		public static CommandSolveDesk GetInstance()
		{
			return _instance ?? (_instance = new CommandSolveDesk());
		}

		#endregion

		public override string GetHelp()
		{
			return "Solve a desk from a json file.";
		}

		public override string GetName()
		{
			return "solve desk";
		}

		public override string GetCommand()
		{
			return "sd";
		}

		public override void Execute()
		{
			string location = CommandManager.GetInstance().RemainCommand.Trim();
			string json;
			try
			{
				json = File.ReadAllText(location);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine("File not found");
				return;
			}

			Desk targetDesk = Desk.GetDeskFromJson(json);
			Console.WriteLine(targetDesk.Pretty());

			// TODO Add solve desk
		}
	}

	#endregion

	class CommandManager
	{
		#region Singleton

		public static CommandManager GetInstance()
		{
			return _instance ?? (_instance = new CommandManager());
		}

		private static CommandManager _instance;

		private CommandManager()
		{
			InitDictionary();
		}

		#endregion

		public string RemainCommand;

		private void InitDictionary()
		{
			CommandSet.Add(CommandPrintHelp.GetInstance().GetCommand(), CommandPrintHelp.GetInstance());
			CommandSet.Add(CommandGenerateSample.GetInstance().GetCommand(),CommandGenerateSample.GetInstance());
			CommandSet.Add(CommandSolveDesk.GetInstance().GetCommand(), CommandSolveDesk.GetInstance());
			CommandSet.Add(CommandQuit.GetInstance().GetCommand(), CommandQuit.GetInstance());
		}

		public readonly Dictionary<string, Command> CommandSet = new Dictionary<string, Command>();

		public void ExecuteCommand(string command)
		{
			string[] items = command.Split(' ');
			RemainCommand = command.Remove(0, items[0].Length);
			try
			{
				CommandSet[items[0]].Execute();
			}
			catch (KeyNotFoundException e)
			{
				Console.WriteLine("No such command");
				GetInstance().ExecuteCommand("h");
				return;
			}
		}
	}
}
