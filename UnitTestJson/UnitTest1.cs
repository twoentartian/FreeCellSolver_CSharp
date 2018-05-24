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
	}
}
