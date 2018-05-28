using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeskSpace;

namespace FreeCellSolver
{
	class Ui
	{
		#region Singleton

		private static Ui _instance;

		private Ui()
		{

		}

		public static Ui GetInstance()
		{
			return _instance ?? (_instance = new Ui());
		}

		#endregion

		public string GenerateSampleDeskJson()
		{
			Desk sampleDesk = new Desk();
			int coloum = 0, row = 0;
			for (Card.Type type = Card.Type.Diamonds; type <= Card.Type.Club; type++)
			{
				for (int i = 1; i <= 13; i++)
				{
					sampleDesk.AddNewCardInColoum(coloum, new Card(type, (Card.Number)i));
					coloum++;
					if (coloum == sampleDesk.AllCardOnDesk.ColoumCard.GetLength(0))
					{
						row++;
						coloum = 0;
					}
				}
			}
			Console.WriteLine(sampleDesk.Pretty());
			return sampleDesk.GetJson();
		}
	}
}
