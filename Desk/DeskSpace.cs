using Newtonsoft.Json;
using System;

namespace DeskSpace
{
	[Serializable]
	public class Desk
	{
		public Desk()
		{
			
		}

		public DeskCard AllCardOnDesk = new DeskCard();

		public string GetJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public static Desk GetDeskFromJson(string json)
		{
			JsonSerializerSettings jsonSetting = new JsonSerializerSettings
			{
				CheckAdditionalContent = true,
				MissingMemberHandling = MissingMemberHandling.Error
			};
			Desk outputDesk = JsonConvert.DeserializeObject<Desk>(json, jsonSetting);
			return outputDesk;
		}

		public bool CheckSame(Desk argDesk) => CheckSame(this, argDesk);

		public static bool CheckSame(Desk argDesk1, Desk argDesk2)
		{
			return DeskCard.CheckSame(argDesk1.AllCardOnDesk, argDesk2.AllCardOnDesk);
		}
	}

	[Serializable]
	public class DeskCard
	{
		public DeskCard()
		{

		}

		public Card[] FreeCard = new Card[4];

		public Card[] SortedCard = new Card[4];

		public Card[,] ColoumCard = new Card[8, 26];

		public bool CheckSame(DeskCard argDeskCard) => CheckSame(this, argDeskCard);

		public static bool CheckSame(DeskCard argDeskCard1, DeskCard argDeskCard2)
		{
			for (int i = 0; i < argDeskCard1.FreeCard.Length; i++)
			{
				if(!Card.CheckSame(argDeskCard1.FreeCard[i], argDeskCard2.FreeCard[i]))
				{
					return false;
				}
			}
			for (int i = 0; i < argDeskCard1.SortedCard.Length; i++)
			{
				if (!Card.CheckSame(argDeskCard1.SortedCard[i], argDeskCard2.SortedCard[i]))
				{
					return false;
				}
			}
			for (int x = 0; x < argDeskCard1.ColoumCard.GetLength(0); x++)
			{
				for (int y = 0; y < argDeskCard1.ColoumCard.GetLength(1); y++)
				{
					if (!Card.CheckSame(argDeskCard1.ColoumCard[x, y], argDeskCard2.ColoumCard[x, y]))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	[Serializable]
	public class Card
	{
		public Card()
		{
			CardNumber = Number.Unknown;
			CardType = Type.Unknown;
		}

		public Card(Type argType, Number argNumber)
		{
			CardNumber = argNumber;
			CardType = argType;
		}

		public enum Color
		{
			Unknown,
			Red,
			Black
		}

		private Color _cardColor;

		public Color CardColor
		{
			get => _cardColor;
			set => _cardColor = value;
		}

		/// <summary>
		/// Definition of type
		/// Unknown=0, Diamonds=1, Heart=2, Spade=3, Club=4
		/// </summary>
		public enum Type
		{
			Unknown,
			Diamonds,
			Heart,
			Spade,
			Club
		}

		private Type _cardType;

		public Type CardType
		{
			get => _cardType;
			set
			{
				_cardType = value;
				if (value == Type.Spade || value == Type.Club)
				{
					_cardColor = Color.Black;
				}
				else if (value == Type.Diamonds || value == Type.Heart)
				{
					_cardColor = Color.Red;
				}
				else if (value == Type.Unknown)
				{
					_cardColor = Color.Unknown;
				}
				else
				{
					throw new NotImplementedException();
				}
			}
		}

		public enum Number
		{
			Unknown = 0,
			Arch = 1,
			Two = 2,
			Three = 3,
			Four = 4,
			Five = 5,
			Six = 6,
			Seven = 7,
			Eight = 8,
			Nine = 9,
			Ten = 10,
			Jack = 11,
			Queen = 12,
			King = 13
		}

		private Number _cardNumber;

		public Number CardNumber
		{
			get => _cardNumber;
			set => _cardNumber = value;
		}

		public bool CheckSame(Card argCard) => CheckSame(this, argCard);

		public static bool CheckSame(Card argCard1, Card argCard2)
		{
			if (argCard1 == null && argCard2 == null)
			{
				return true;
			}
			if (argCard1 == null || argCard2 == null)
			{
				return false;
			}
			return (argCard1.CardType == argCard2.CardType || argCard1.CardNumber == argCard2.CardNumber);
		}
	}
}
