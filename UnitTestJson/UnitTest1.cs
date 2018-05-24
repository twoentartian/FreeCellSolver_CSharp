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
				throw new Exception("Test For Json Failed, the deserialized desk differs from the origin one");
			}
		}
	}
}
