using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;

namespace DeskSpace
{
	public class NotEnoughCardException : Exception
	{
		
	}
	public class Desk
	{
		public Desk()
		{
			
		}

		#region Property

		public DeskCard AllCardOnDesk = new DeskCard();

		private int[] ColoumCardCounter = new int[8];

		private int _allCardCount;
		public int AllCardCount => _allCardCount;



		#endregion

		public string GetJson()
		{
			return JsonConvert.SerializeObject(AllCardOnDesk, Formatting.Indented);
		}

		public static Desk GetDeskFromJson(string json)
		{
			JsonSerializerSettings jsonSetting = new JsonSerializerSettings
			{
				CheckAdditionalContent = true,
				MissingMemberHandling = MissingMemberHandling.Error
			};
			DeskCard outputDeskCard = JsonConvert.DeserializeObject<DeskCard>(json, jsonSetting);
			Desk outputDesk = new Desk {AllCardOnDesk = outputDeskCard};
			return outputDesk;
		}

		public bool CheckSame(Desk argDesk) => CheckSame(this, argDesk);

		public static bool CheckSame(Desk argDesk1, Desk argDesk2)
		{
			return DeskCard.CheckSame(argDesk1.AllCardOnDesk, argDesk2.AllCardOnDesk);
		}

		public string Pretty()
		{
			return AllCardOnDesk.Pretty();
		}

		public void AddNewCardInColoum(int coloum, Card.Type type, int numberInt)
		{
			Card.Number number = (Card.Number)numberInt;
			AddNewCardInColoum(coloum, type, number);
		}

		public void AddNewCardInColoum(int coloum, Card.Type type, Card.Number number)
		{
			AllCardOnDesk.ColoumCard[coloum, ColoumCardCounter[coloum]] = new Card(type, number);
			ColoumCardCounter[coloum]++;
			_allCardCount++;
		}

		public void AddNewCardInColoum(int coloum, Card card)
		{
			AllCardOnDesk.ColoumCard[coloum, ColoumCardCounter[coloum]] = card;
			ColoumCardCounter[coloum]++;
			_allCardCount++;
		}

		public Card RemoveCardInColoum(int coloum)
		{
			Card outputCard;
			if (ColoumCardCounter[coloum] > 0)
			{
				ColoumCardCounter[coloum]--;
				outputCard = AllCardOnDesk.ColoumCard[coloum, ColoumCardCounter[ColoumCardCounter[coloum]]];
				AllCardOnDesk.ColoumCard[coloum, ColoumCardCounter[ColoumCardCounter[coloum]]] = null;
				_allCardCount--;
			}
			else
			{
				throw new NotEnoughCardException();
			}
			return outputCard;
		}

		public void CalculateParameters()
		{
			
		}
	}

	[Serializable]
	public class DeskCard
	{
		public DeskCard()
		{

		}

		#region Property

		public Card[] FreeCard = new Card[4];

		public Card[] SortedCard = new Card[4];

		public Card[,] ColoumCard = new Card[8, 26];

		#endregion

		public int GetEmptyLocationOnFreeCard()
		{
			return FreeCard.Count(t => t == null);
		}

		public int GetEmptyLocationOnColoumCard()
		{
			int output = 0;
			for (int i = 0; i < ColoumCard.GetLength(0); i++)
			{
				if (ColoumCard[i,0] == null)
				{
					output++;
				}
			}
			return output;
		}

		public int GetMaximumMovementNumber()
		{
			return (GetEmptyLocationOnColoumCard() + 1) * (GetEmptyLocationOnFreeCard() + 1);
		}

		public int CheckSortedInColoum(int coloum)
		{
			return 0;
		}

		public bool CheckSame(DeskCard argDeskCard) => CheckSame(this, argDeskCard);

		public static bool CheckSame(DeskCard argDeskCard1, DeskCard argDeskCard2)
		{
			for (int i = 0; i < argDeskCard1.FreeCard.Length; i++)
			{
				bool isSame = false;
				for (int j = 0; j < argDeskCard2.FreeCard.Length; j++)
				{
					if (Card.CheckSame(argDeskCard1.FreeCard[i], argDeskCard2.FreeCard[j]))
					{
						isSame = true;
						break;
					}
				}
				if (!isSame)
				{
					return false;
				}
			}

			for (int i = 0; i < argDeskCard1.SortedCard.Length; i++)
			{
				bool isSame = false;
				for (int j = 0; j < argDeskCard2.SortedCard.Length; j++)
				{
					if (Card.CheckSame(argDeskCard1.SortedCard[i], argDeskCard2.SortedCard[j]))
					{
						isSame = true;
						break;
					}
				}
				if (!isSame)
				{
					return false;
				}
			}

			for (int i = 0; i < argDeskCard1.ColoumCard.GetLength(0); i++)
			{
				bool isSame = false;
				for (int j = 0; j < argDeskCard2.ColoumCard.GetLength(0); j++)
				{
					bool IsColoumSame = true;
					for (int y = 0; y < argDeskCard1.ColoumCard.GetLength(1); y++)
					{
						if (!Card.CheckSame(argDeskCard1.ColoumCard[i, y], argDeskCard2.ColoumCard[j, y]))
						{
							IsColoumSame = false;
							break;
						}
					}
					if (IsColoumSame)
					{
						isSame = true;
					}
				}
				if (!isSame)
				{
					return false;
				}
			}

			return true;
		}

		public string Pretty()
		{
			StringBuilder sb = new StringBuilder();

			foreach (Card card in SortedCard)
			{
				sb.Append(card == null ? "   " : card.Pretty());
				sb.Append("  |  ");
			}
			sb.Append("\b\b\b\b\b ||| ");
			foreach (Card card in FreeCard)
			{
				sb.Append(card == null ? "   " : card.Pretty());
				sb.Append("  |  ");
			}
			sb.Append("\b\b\b\b\b     ");
			sb.Append(Environment.NewLine);
			sb.Append("--- --- --- --- --- --- --- --- --- --- --- --- --- --- ---");
			sb.Append(Environment.NewLine);

			for (int rowIndex = 0; rowIndex < ColoumCard.GetLength(1); rowIndex++)
			{
				bool IsCardInRow = false;
				for (int coloumIndex = 0; coloumIndex < ColoumCard.GetLength(0); coloumIndex++)
				{
					if (ColoumCard[coloumIndex, rowIndex] == null)
					{
						sb.Append("   ");
					}
					else
					{
						sb.Append(ColoumCard[coloumIndex, rowIndex].Pretty());
						IsCardInRow = true;
					}
					sb.Append("  |  ");
				}
				sb.Append("\b\b\b\b\b     ");
				sb.Append(Environment.NewLine);
				if (!IsCardInRow)
				{
					break;
				}
			}


			return sb.ToString();
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

		#region Property

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

		#endregion

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

		public string Pretty()
		{
			StringBuilder sb = new StringBuilder();
			if (CardType == Type.Club)
			{
				sb.Append("C");
			}
			else if (CardType == Type.Diamonds)
			{
				sb.Append("D");
			}
			else if (CardType == Type.Heart)
			{
				sb.Append("H");
			}
			else if (CardType == Type.Spade)
			{
				sb.Append("S");
			}
			else if (CardType == Type.Unknown)
			{
				sb.Append("U");
			}
			else
			{
				throw new NotImplementedException();
			}

			if (CardNumber >= Number.Arch && CardNumber <=Number.Nine)
			{
				sb.Append((int)CardNumber);
				sb.Append(" ");
			}
			else if (CardNumber == Number.Ten)
			{
				sb.Append((int)CardNumber);
			}
			else if (CardNumber == Number.Jack)
			{
				sb.Append("11");
			}
			else if (CardNumber == Number.Queen)
			{
				sb.Append("12");
			}
			else if (CardNumber == Number.King)
			{
				sb.Append("13");
			}
			else if (CardNumber == Number.Unknown)
			{
				sb.Append("U");
				sb.Append(" ");
			}
			else
			{
				throw new NotImplementedException();
			}

			return sb.ToString();
		}
	}
}
