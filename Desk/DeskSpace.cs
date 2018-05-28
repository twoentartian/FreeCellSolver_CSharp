using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DeskSpace
{
	public class NotEnoughCardException : Exception
	{
		public NotEnoughCardException(string info) : base(info)
		{
				
		}
	}

	public class ErrorInDeskException : Exception
	{
		public ErrorInDeskException(string info) : base(info)
		{
			
		}
	}

	[Serializable]
	public class Desk
	{
		public Desk()
		{
			
		}

		public Desk DeepClone()
		{
			using (Stream objectStream = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(objectStream, this);
				objectStream.Seek(0, SeekOrigin.Begin);
				return formatter.Deserialize(objectStream) as Desk;
			}
		}

		#region Property

		public DeskCard AllCardOnDesk = new DeskCard();

		private readonly int[] _coloumCardCounter = new int[8];

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
			outputDesk.CalculateParameters();
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

		public float CalculateWinPercent()
		{
			return 1 - (float)_allCardCount / (4 * 13);
		}

		#region Coloum operation

		public void AddNewCardInColoum(int coloum, Card.Type type, int numberInt)
		{
			Card.Number number = (Card.Number)numberInt;
			AddNewCardInColoum(coloum, type, number);
		}

		public void AddNewCardInColoum(int coloum, Card.Type type, Card.Number number)
		{
			AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]] = new Card(type, number);
			_coloumCardCounter[coloum]++;
			_allCardCount++;
		}

		public void AddNewCardInColoum(int coloum, Card card)
		{
			AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]] = card;
			_coloumCardCounter[coloum]++;
			_allCardCount++;
		}

		public Card RemoveCardInColoum(int coloum)
		{
			Card outputCard;
			if (_coloumCardCounter[coloum] > 0)
			{
				_coloumCardCounter[coloum]--;
				outputCard = AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]];
				AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]] = null;
				_allCardCount--;
			}
			else
			{
				throw new NotEnoughCardException("Not enought space in coloum, try increase the coloum size in source.");
			}
			return outputCard;
		}

		public Card GetTheLastCardInfoInColoum(int coloum)
		{
			if (_coloumCardCounter[coloum]==0)
			{
				return null;
			}
			return AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum] - 1];
		}

		#endregion

		#region Sorted card operation

		/// <summary>
		/// Move a card to sorted card, return false if this card violate game rule.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="change"></param>
		/// <returns></returns>
		public bool AddNewCardInSortedCard(Card card, bool change = false)
		{
			if (card.CardNumber == Card.Number.Arch)
			{
				if (change)
				{
					AllCardOnDesk.SortedCard[(int)card.CardType - 1] = card;
				}
				return true;
			}
			else if (AllCardOnDesk.SortedCard[(int)card.CardType - 1].CardNumber == card.CardNumber-1)
			{
				if (change)
				{
					AllCardOnDesk.SortedCard[(int)card.CardType - 1] = card;
				}
				return true;
			}
			return false;
		}

		#endregion

		#region Free card operation

		private int _freeCardCounter = 0;

		public bool AddNewCardInFreeCard(Card card)
		{
			for (int i = 0; i < AllCardOnDesk.FreeCard.Length; i++)
			{
				if (AllCardOnDesk.FreeCard[i] == null)
				{
					AllCardOnDesk.FreeCard[i] = card;
					_freeCardCounter++;
					return true;
				}
			}
			return false;
		}

		public Card GetCardInfoFreeCard(int loc)
		{
			return AllCardOnDesk.FreeCard[loc];	
		}

		public Card RemoveCardInFreeCard(int loc)
		{
			Card output = AllCardOnDesk.FreeCard[loc];
			AllCardOnDesk.FreeCard[loc] = null;
			return output;
		}

		#endregion

		private void CalculateParameters()
		{
			for (int coloumIndex = 0; coloumIndex < AllCardOnDesk.ColoumCard.GetLength(0); coloumIndex++)
			{
				for (int i = 0; i < AllCardOnDesk.ColoumCard.GetLength(1); i++)
				{
					if (AllCardOnDesk.ColoumCard[coloumIndex,i] != null)
					{
						_coloumCardCounter[coloumIndex]++;
						_allCardCount++;
					}
				}
			}

			for (int i = 0; i < AllCardOnDesk.FreeCard.Length; i++)
			{
				if (AllCardOnDesk.FreeCard[i] != null)
				{
					_freeCardCounter++;
					_allCardCount++;
				}
			}
		}

		public bool IsSolved()
		{
			for (int i = 0; i < AllCardOnDesk.SortedCard.Length; i++)
			{
				if (AllCardOnDesk.SortedCard[i] == null)
				{
					return false;
				}
				if (AllCardOnDesk.SortedCard[i].CardNumber != Card.Number.King)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// If the desk from json file does not match game rule (for example, two club 10), the function throws exceptions.
		/// </summary>
		public void CheckError()
		{
			bool[] isAppear = new bool[13 * 4];
			foreach (Card t in AllCardOnDesk.SortedCard)
			{
				if (t != null)
				{
					for (int j = (int)(t.CardType - 1) * 13; j <= (int)t.CardNumber - 1 + (int)(t.CardType - 1) * 13; j++)
					{
						isAppear[j] = true;
					}
				}
			}

			foreach (Card t in AllCardOnDesk.FreeCard)
			{
				if (t != null)
				{
					int index = t.GetId();
					if (!isAppear[index])
					{
						// Card has not appear
						isAppear[index] = true;
					}
					else
					{
						throw new ErrorInDeskException($"Error in free card: {t.Pretty()}");
					}
				}
			}

			for (int coloumIndex = 0; coloumIndex < AllCardOnDesk.ColoumCard.GetLength(0); coloumIndex++)
			{
				for (int i = 0; i < AllCardOnDesk.ColoumCard.GetLength(1); i++)
				{
					if (AllCardOnDesk.ColoumCard[coloumIndex, i] != null)
					{
						int index = AllCardOnDesk.ColoumCard[coloumIndex, i].GetId();
						if (!isAppear[index])
						{
							// Card has not appear
							isAppear[index] = true;
						}
						else
						{
							throw new ErrorInDeskException($"Error in coloum card: coloum-{coloumIndex + 1} index-{i + 1}");
						}
					}
				}
			}

			foreach (bool t in isAppear)
			{
				if (!t)
				{
					throw new ErrorInDeskException("No all card appear in desk");
				}
			}
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
			//TODO
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

		public Color CardColor()
		{
			return _cardColor;
		}

		/// <summary>
		/// Definition of type
		/// Unknown=0, Diamonds=1, Heart=2, Spade=3, Club=4
		/// </summary>
		public enum Type
		{
			Unknown = 0,
			Diamonds = 1,
			Heart = 2,
			Spade = 3,
			Club = 4
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
			return (argCard1.CardType == argCard2.CardType && argCard1.CardNumber == argCard2.CardNumber);
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

		public int GetId()
		{
			return (int)CardNumber - 1 + 13 * ((int) CardType - 1);
		}
	}
}
