﻿using Newtonsoft.Json;
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

		private readonly int[] _coloumCardCounter = new int[Config.NumberOfColoum];

		private readonly int[] _coloumCardSortedCardCounter = new int[Config.NumberOfColoum];

		private int _allCardCount;
		public int AllCardCount => _allCardCount;

		#endregion

		#region JSON

		public string GetJson()
		{
			return JsonConvert.SerializeObject(AllCardOnDesk, Formatting.Indented);
		}

		/// <summary>
		/// Count card in each position, this function is only applied when invoking GetDeskFromJson().
		/// </summary>
		private void CalculateParameters()
		{
			for (int coloumIndex = 0; coloumIndex < AllCardOnDesk.ColoumCard.GetLength(0); coloumIndex++)
			{
				for (int i = 0; i < AllCardOnDesk.ColoumCard.GetLength(1); i++)
				{
					if (AllCardOnDesk.ColoumCard[coloumIndex, i] != null)
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
					_allCardCount++;
				}
			}

			CheckSortedCardInColoum();
		}

		/// <summary>
		/// Generate a desk target from json file.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static Desk GetDeskFromJson(string json)
		{
			JsonSerializerSettings jsonSetting = new JsonSerializerSettings
			{
				CheckAdditionalContent = true,
				MissingMemberHandling = MissingMemberHandling.Error
			};
			DeskCard outputDeskCard = JsonConvert.DeserializeObject<DeskCard>(json, jsonSetting);
			Desk outputDesk = new Desk { AllCardOnDesk = outputDeskCard };
			outputDesk.CalculateParameters();
			return outputDesk;
		}

		#endregion

		/// <summary>
		/// Check whether two desks are same.
		/// </summary>
		/// <param name="argDesk"></param>
		/// <returns></returns>
		public bool CheckSame(Desk argDesk) => CheckSame(this, argDesk);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="argDesk1"></param>
		/// <param name="argDesk2"></param>
		/// <returns></returns>
		public static bool CheckSame(Desk argDesk1, Desk argDesk2)
		{
			return DeskCard.CheckSame(argDesk1.AllCardOnDesk, argDesk2.AllCardOnDesk);
		}

		/// <summary>
		/// Draw the desk with a readable format.
		/// </summary>
		/// <returns></returns>
		public string Pretty()
		{
			return AllCardOnDesk.Pretty();
		}

		/// <summary>
		/// Calculate the percentage of winning.
		/// </summary>
		/// <returns></returns>
		public float CalculateWinPercent()
		{
			return 1 - (float) _allCardCount / (4 * 13) + (float) _coloumCardSortedCardCounter.Sum() / (4 * 12 * 2);
		}

		#region Coloum operation

		/// <summary>
		/// Check sorted card in single coloum
		/// </summary>
		/// <param name="coloum"></param>
		private void CheckSortedCardInColoum(int coloum)
		{
			Card card = GetLastCardInfoInColoum(coloum);
			_coloumCardSortedCardCounter[coloum] = 0;
			if (card== null)
			{
				return;
			}
			else
			{
				Card prevCard = AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum] - 1];
				for (int i = _coloumCardCounter[coloum] - 2; i >= 0; i--)
				{
					if (AllCardOnDesk.ColoumCard[coloum, i].CardColor() == Card.Color.Unknown || AllCardOnDesk.ColoumCard[coloum, i].CardNumber == Card.Number.Unknown)
					{
						throw new Exception("Impossible situation");
					}
					if (prevCard.CardColor() == Card.Color.Unknown || prevCard.CardNumber == Card.Number.Unknown)
					{
						throw new Exception("Impossible situation");
					}

					if (AllCardOnDesk.ColoumCard[coloum, i].CardColor() != prevCard.CardColor() && AllCardOnDesk.ColoumCard[coloum, i].CardNumber == prevCard.CardNumber + 1)
					{
						_coloumCardSortedCardCounter[coloum]++;
					}
					else
					{
						break;
					}

					prevCard = AllCardOnDesk.ColoumCard[coloum, i];
				}
			}
		}

		/// <summary>
		/// Check sorted card in all coloums
		/// </summary>
		private void CheckSortedCardInColoum()
		{
			for (int i = 0; i < Config.NumberOfColoum; i++)
			{
				CheckSortedCardInColoum(i);
			}
		}

		public void AddNewCardInColoum(int coloum, Card.Type type, int numberInt)
		{
			Card.Number number = (Card.Number)numberInt;
			AddNewCardInColoum(coloum, new Card(type, number));
		}

		public void AddNewCardInColoum(int coloum, Card.Type type, Card.Number number)
		{
			AddNewCardInColoum(coloum, new Card(type, number));
		}

		public void AddNewCardInColoum(int coloum, Card card)
		{
			AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]] = card;
			_coloumCardCounter[coloum]++;
			_allCardCount++;
			CheckSortedCardInColoum(coloum);
		}

		public bool MoveCardFromColoumToColoum(int sourceCardColoum, int sourceSortedCardCount, int targetColoum)
		{
			if (sourceSortedCardCount > AllCardOnDesk.GetMaximumMovementNumber())
			{
				return false;
			}
			if (GetLastCardInfoInColoum(sourceCardColoum) == null)
			{
				return false;
			}

			Card sourceCard = AllCardOnDesk.ColoumCard[sourceCardColoum, _coloumCardCounter[sourceCardColoum] - sourceSortedCardCount - 1];
			Card lastCard = GetLastCardInfoInColoum(targetColoum);
			if (Card.CanMove(lastCard, sourceCard))
			{
				for (int i = 0; i <= sourceSortedCardCount; i++)
				{
					//Add card in target coloum
					AllCardOnDesk.ColoumCard[targetColoum, _coloumCardCounter[targetColoum] + sourceSortedCardCount - i] =
						AllCardOnDesk.ColoumCard[sourceCardColoum, _coloumCardCounter[sourceCardColoum] - i - 1];

					//Remove card in source coloum
					AllCardOnDesk.ColoumCard[sourceCardColoum, _coloumCardCounter[sourceCardColoum] - i - 1] = null;
				}

				_coloumCardCounter[targetColoum] = _coloumCardCounter[targetColoum] + sourceSortedCardCount + 1;
				_coloumCardCounter[sourceCardColoum] = _coloumCardCounter[sourceCardColoum] - sourceSortedCardCount - 1;

				CheckSortedCardInColoum(sourceCardColoum);
				CheckSortedCardInColoum(targetColoum);
				return true;
			}
			else
			{
				return false;
			}

		}

		public bool MoveCardFromFreeToColoum(int coloum, Card card)
		{
			Card lastCard = GetLastCardInfoInColoum(coloum);
			if (Card.CanMove(lastCard, card))
			{
				AddNewCardInColoum(coloum, card);
				return true;
			}
			else
			{
				return false;
			}

			;
		}

		public Card RemoveLastCardInColoum(int coloum)
		{
			Card outputCard;
			if (_coloumCardCounter[coloum] > 0)
			{
				_coloumCardCounter[coloum]--;
				outputCard = AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]];
				AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum]] = null;
				_allCardCount--;
				CheckSortedCardInColoum(coloum);
			}
			else
			{
				throw new NotEnoughCardException("No card in the coloum");
			}
			return outputCard;
		}

		public Card GetLastCardInfoInColoum(int coloum)
		{
			if (_coloumCardCounter[coloum]==0)
			{
				return null;
			}
			return AllCardOnDesk.ColoumCard[coloum, _coloumCardCounter[coloum] - 1];
		}

		public Card GetCardInfo(int coloum, int location)
		{
			return AllCardOnDesk.ColoumCard[coloum, location];
		}

		public int GetSortedCardCountInColoum(int coloum)
		{
			return _coloumCardSortedCardCounter[coloum];
		}

		public int GetCardCountInColoum(int coloum)
		{
			return _coloumCardCounter[coloum];
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
			if (AllCardOnDesk.SortedCard[(int)card.CardType - 1] == null)
			{
				if (card.CardNumber == Card.Number.Arch)
				{
					if (change)
					{
						AllCardOnDesk.SortedCard[(int)card.CardType - 1] = card;
					}
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (card.CardNumber == Card.Number.Arch)
				{
					throw new Exception("Impossible situation");
				}
				else if (AllCardOnDesk.SortedCard[(int)card.CardType - 1].CardNumber == card.CardNumber - 1)
				{
					if (change)
					{
						AllCardOnDesk.SortedCard[(int)card.CardType - 1] = card;
					}
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Free card operation

		public bool AddNewCardInFreeCard(Card card)
		{
			for (int i = 0; i < AllCardOnDesk.FreeCard.Length; i++)
			{
				if (AllCardOnDesk.FreeCard[i] == null)
				{
					AllCardOnDesk.FreeCard[i] = card;
					_allCardCount++;
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
			_allCardCount--;
			return output;
		}

		#endregion

		/// <summary>
		/// Check whether the desk is solved.
		/// </summary>
		/// <returns></returns>
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

		public Card[] FreeCard = new Card[Config.NumberOfFreeCardPosition];

		public Card[] SortedCard = new Card[4];

		public Card[,] ColoumCard = new Card[Config.NumberOfColoum, 26];

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
	public class Card : ICloneable
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

		public object Clone()
		{
			return new Card(CardType, CardNumber);
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

		public static bool CanMove(Card parentCard, Card moveCard)
		{
			if (parentCard == null)
			{
				return true;
			}
			if (parentCard.CardNumber == Card.Number.Unknown || moveCard.CardNumber == Card.Number.Unknown)
			{
				throw new Exception("Logic error: unknow card number");
			}
			if (parentCard.CardColor() == Card.Color.Unknown || moveCard.CardColor() == Card.Color.Unknown)
			{
				throw new Exception("Logic error: unknow card color");
			}
			if (parentCard.CardColor() != moveCard.CardColor() && parentCard.CardNumber - 1 == moveCard.CardNumber)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
