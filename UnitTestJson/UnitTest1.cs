using System;
using DeskSpace;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestJson
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestJson4Desk()
		{
			Desk tempDesk = new Desk();
			tempDesk.AllCardOnDesk.FreeCard[0] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk.AllCardOnDesk.SortedCard[0] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk.AllCardOnDesk.ColoumCard[0, 0] = new Card(Card.Type.Heart, Card.Number.Arch);
			string json = tempDesk.GetJson();
			Desk deserizlizedDeskJson = Desk.GetDeskFromJson(json);

			bool result = tempDesk.CheckSame(deserizlizedDeskJson);

			if (!result)
			{
				throw new Exception("Test for Json failed, the deserialized desk differs from the origin one");
			}
		}

		[TestMethod]
		public void TestPrettyCard()
		{
			Card tempCard = new Card
			{
				CardNumber = Card.Number.Arch,
				CardType = Card.Type.Club
			};

			if (tempCard.Pretty() != "C1 ")
			{
				throw new Exception("Test for pretty card club arch failed");
			}
		}

		[TestMethod]
		public void TestPrettyDesk()
		{
			Desk tempDesk = new Desk();
			tempDesk.AllCardOnDesk.FreeCard[0] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk.AllCardOnDesk.FreeCard[1] = new Card(Card.Type.Heart, Card.Number.Two);
			tempDesk.AllCardOnDesk.FreeCard[2] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk.AllCardOnDesk.FreeCard[3] = new Card(Card.Type.Heart, Card.Number.Four);

			tempDesk.AllCardOnDesk.SortedCard[0] = new Card(Card.Type.Heart, Card.Number.Five);
			tempDesk.AllCardOnDesk.SortedCard[1] = new Card(Card.Type.Heart, Card.Number.Six);
			tempDesk.AllCardOnDesk.SortedCard[2] = new Card(Card.Type.Heart, Card.Number.Seven);
			tempDesk.AllCardOnDesk.SortedCard[3] = new Card(Card.Type.Heart, Card.Number.Eight);

			int coloum = 0, row = 0;
			for (int i = 1; i <= 13; i++)
			{
				tempDesk.AllCardOnDesk.ColoumCard[coloum, row] = new Card(Card.Type.Diamonds, (Card.Number)i);
				coloum++;
				if (coloum == tempDesk.AllCardOnDesk.ColoumCard.GetLength(0))
				{
					row++;
					coloum = 0;
				}
			}

			for (int i = 1; i <= 13; i++)
			{
				tempDesk.AllCardOnDesk.ColoumCard[coloum, row] = new Card(Card.Type.Club, (Card.Number)i);
				coloum++;
				if (coloum == tempDesk.AllCardOnDesk.ColoumCard.GetLength(0))
				{
					row++;
					coloum = 0;
				}
			}

			for (int i = 1; i <= 13; i++)
			{
				tempDesk.AllCardOnDesk.ColoumCard[coloum, row] = new Card(Card.Type.Spade, (Card.Number)i);
				coloum++;
				if (coloum == tempDesk.AllCardOnDesk.ColoumCard.GetLength(0))
				{
					row++;
					coloum = 0;
				}
			}
			string pretty = tempDesk.Pretty();
			if (pretty != "H5   |  H6   |  H7   |  H8   |  \b\b\b\b\b ||| H1   |  H2   |  H3   |  H4   |  \b\b\b\b\b     \r\n--- --- --- --- --- --- --- --- --- --- --- --- --- --- ---\r\nD1   |  D2   |  D3   |  D4   |  D5   |  D6   |  D7   |  D8   |  \b\b\b\b\b     \r\nD9   |  D10  |  D11  |  D12  |  D13  |  C1   |  C2   |  C3   |  \b\b\b\b\b     \r\nC4   |  C5   |  C6   |  C7   |  C8   |  C9   |  C10  |  C11  |  \b\b\b\b\b     \r\nC12  |  C13  |  S1   |  S2   |  S3   |  S4   |  S5   |  S6   |  \b\b\b\b\b     \r\nS7   |  S8   |  S9   |  S10  |  S11  |  S12  |  S13  |       |  \b\b\b\b\b     \r\n     |       |       |       |       |       |       |       |  \b\b\b\b\b     \r\n")
			{
				throw new Exception("Test for pretty desk failed");
			}
			

		}

		[TestMethod]
		public void TestDeskSame()
		{
			Desk tempDesk1 = new Desk();
			tempDesk1.AllCardOnDesk.FreeCard[0] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk1.AllCardOnDesk.SortedCard[0] = new Card(Card.Type.Heart, Card.Number.Two);
			tempDesk1.AllCardOnDesk.ColoumCard[0, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[0, 1] = new Card(Card.Type.Heart, Card.Number.Four);

			Desk tempDesk2 = new Desk();
			tempDesk2.AllCardOnDesk.FreeCard[1] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk2.AllCardOnDesk.SortedCard[2] = new Card(Card.Type.Heart, Card.Number.Two);
			tempDesk2.AllCardOnDesk.ColoumCard[3, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk2.AllCardOnDesk.ColoumCard[3, 1] = new Card(Card.Type.Heart, Card.Number.Four);
	
			bool result = Desk.CheckSame(tempDesk1, tempDesk2);
			if (!result)
			{
				throw new Exception("Test for check desk same failed");
			}

			tempDesk2.AllCardOnDesk.ColoumCard[3, 2] = new Card(Card.Type.Heart, Card.Number.Four);

			result = Desk.CheckSame(tempDesk1, tempDesk2);
			if (result)
			{
				throw new Exception("Test for check desk same failed");
			}
		}

		[TestMethod]
		public void TestMaximumMoveCardNumber()
		{
			Desk tempDesk1 = new Desk();
			tempDesk1.AllCardOnDesk.FreeCard[0] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk1.AllCardOnDesk.ColoumCard[0, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[0, 1] = new Card(Card.Type.Heart, Card.Number.Four);

			int maximumMovement = tempDesk1.AllCardOnDesk.GetMaximumMovementNumber();

			if (maximumMovement != 4*8)
			{
				throw new Exception("Test for calculating the maximum card movement in a single move failed");
			}

			tempDesk1.AllCardOnDesk.FreeCard[1] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk1.AllCardOnDesk.ColoumCard[1, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[1, 1] = new Card(Card.Type.Heart, Card.Number.Four);

			maximumMovement = tempDesk1.AllCardOnDesk.GetMaximumMovementNumber();

			if (maximumMovement != 3 * 7)
			{
				throw new Exception("Test for calculating the maximum card movement in a single move failed");
			}

			tempDesk1.AllCardOnDesk.FreeCard[2] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk1.AllCardOnDesk.FreeCard[3] = new Card(Card.Type.Heart, Card.Number.Arch);
			tempDesk1.AllCardOnDesk.ColoumCard[2, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[3, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[4, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[5, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[6, 0] = new Card(Card.Type.Heart, Card.Number.Three);
			tempDesk1.AllCardOnDesk.ColoumCard[7, 0] = new Card(Card.Type.Heart, Card.Number.Three);

			maximumMovement = tempDesk1.AllCardOnDesk.GetMaximumMovementNumber();

			if (maximumMovement != 1 * 1)
			{
				throw new Exception("Test for calculating the maximum card movement in a single move failed");
			}

		}

		[TestMethod]
		public void TestAddOrRemoveCard()
		{
			Desk tempDesk1 = new Desk();
			Card tempCard = new Card(Card.Type.Diamonds, Card.Number.Arch);
			tempDesk1.AddNewCardInColoum(0, tempCard);

			if (tempDesk1.AllCardCount != 1)
			{
				throw new Exception("Test failed");
			}

			Card card = tempDesk1.RemoveCardInColoum(0);

			try
			{
				tempDesk1.RemoveCardInColoum(0);
				throw new Exception("Test Failed");
			}
			catch (NotEnoughCardException e)
			{

			}
			if (card.GetHashCode() != tempCard.GetHashCode())
			{
				throw new Exception("Test failed");
			}
		}
	}
}
