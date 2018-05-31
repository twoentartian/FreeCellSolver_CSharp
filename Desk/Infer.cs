using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskSpace
{
	public class InferManager
	{
		#region Singleton

		private InferManager()
		{
			
		}

		private static InferManager _instance;

		public static InferManager GetInstance()
		{
			return _instance ?? (_instance = new InferManager());
		}

		#endregion

		public List<InferResult> InferResults = new List<InferResult>();

		private int _inferResultMapId = 0;
		private int InferResultMapId
		{
			get
			{
				_inferResultMapId++;
				return _inferResultMapId-1;
			}
			set => _inferResultMapId = value;
		}

		private int _lastInferLayer;
		public int LastInferLayer => _lastInferLayer;

		private InferResult _lastInferResult;
		public InferResult LastInferResult => _lastInferResult;

		public void ClearInferData()
		{
			InferResults.Clear();
			InferResultMapId = 0;
		}

		public void SetStartDesk(Desk desk)
		{
			InferResult startInferResult = new InferResult(desk) {Id = InferResultMapId};
			InferResults.Add(startInferResult);
		}

		public LogicFlowChart StartInfer()
		{
			while (true)
			{
				InferResults.Sort();
				int index = 0;
				while (InferResults[index].IsInferred)
				{
					index++;
					if (index == InferResults.Count)
					{
						LogicFlowChart outputFlowChart = new LogicFlowChart(false, $"Cannot solve in {LastInferLayer} layers");
						return outputFlowChart;
					}
				}
				List<InferResult> results = InferResults[index].Infer();
				foreach (InferResult result in results)
				{
					if (result.CurrentDesk.IsSolved())
					{
						LogicFlowChart outputFlowChart = new LogicFlowChart(true, $"Solved in {LastInferLayer} layers");
						InferResult tempInferResult = _lastInferResult;
						while (true)
						{
							outputFlowChart.Procedures.Insert(0, tempInferResult);
							if (tempInferResult.FromInferResultId == -1)
							{
								break;
							}
							else
							{
								InferResult parentResult =
									InferResults.Find(singleResult => singleResult.Id == tempInferResult.FromInferResultId);
								tempInferResult = parentResult;
							}
						}
						return outputFlowChart;
					}
					else
					{
						bool isSame = false;
						foreach (var singleResult in InferResults)
						{
							if (result.InferLayer - singleResult.InferLayer > 8)
							{
								continue;
							}
							if (result.CurrentDesk.CheckSame(singleResult.CurrentDesk))
							{
								isSame = true;
								break;
							}
						}
						if (isSame)
						{
							continue;
						}
						_lastInferResult = result;
						_lastInferLayer = result.InferLayer;
						result.Id = InferResultMapId;
						InferResults.Add(result);
					}
				}
			}
		}
	}

	public class InferResult : IComparable<InferResult>
	{
		public InferResult(Desk startDesk)
		{
			CurrentDesk = startDesk;
			InferLayer = 0;
			_winpercent = 0;
			_fromInferResultId = -1;
		}

		private InferResult(Desk desk, int inferLayer, float winPercent, string moveMessage, int fromInferResultId)
		{
			CurrentDesk = desk;
			InferLayer = inferLayer;
			_winpercent = winPercent;
			_moveMessage = moveMessage;
			_fromInferResultId = fromInferResultId;
		}

		private InferResult()
		{
			
		}

		public int CompareTo(InferResult argResult)
		{
			return argResult.WinPercent.CompareTo(WinPercent);
		}

		#region Property

		public int Id;

		public readonly Desk CurrentDesk;

		public readonly int InferLayer;

		private float _winpercent;
		public float WinPercent => _winpercent;

		private bool _isInferred = false;
		public bool IsInferred => _isInferred;

		private string _moveMessage;
		public string MoveMessage => _moveMessage;

		private int _fromInferResultId;
		public int FromInferResultId => _fromInferResultId;

		#endregion

		public List<InferResult> Infer()
		{
			List<InferResult> outputResults = new List<InferResult>();
			int nextInferLayer = InferLayer + 1;
			_isInferred = true;

			Desk copyedDesk;

			//Check is there any coloum cards to move to sorted card.
			for (int i = 0; i < CurrentDesk.AllCardOnDesk.ColoumCard.GetLength(0); i++)
			{
				Card tempCard = CurrentDesk.GetLastCardInfoInColoum(i);
				if (tempCard == null)
				{
					continue;
				}
				if (CurrentDesk.AddNewCardInSortedCard(tempCard))
				{
					copyedDesk = CurrentDesk.DeepClone();
					copyedDesk.AddNewCardInSortedCard(tempCard, true);
					copyedDesk.RemoveLastCardInColoum(i);
					float winPercent = copyedDesk.CalculateWinPercent();
					outputResults.Add(new InferResult(copyedDesk, nextInferLayer, winPercent, $"Move {tempCard.Pretty()} to sorted card", Id));
				}
			}

			//Check is there any free cards to move to sorted card.
			for (int i = 0; i < CurrentDesk.AllCardOnDesk.FreeCard.Length; i++)
			{
				Card tempCard = CurrentDesk.GetCardInfoFreeCard(i);
				if (tempCard == null)
				{
					continue;
				}
				if (CurrentDesk.AddNewCardInSortedCard(tempCard))
				{
					copyedDesk = CurrentDesk.DeepClone();
					copyedDesk.AddNewCardInSortedCard(tempCard, true);
					copyedDesk.RemoveCardInFreeCard(i);
					float winPercent = copyedDesk.CalculateWinPercent();
					outputResults.Add(new InferResult(copyedDesk, nextInferLayer, winPercent, $"Move {tempCard.Pretty()} to sorted card", Id));
				}
			}

			//Check is there any cards which can be moved to another coloum
			copyedDesk = CurrentDesk.DeepClone();
			for (int sourceColoum = 0; sourceColoum < Config.NumberOfColoum; sourceColoum++)
			{
				if (CurrentDesk.GetLastCardInfoInColoum(sourceColoum) == null)
				{
					continue;
				}
				for (int sortedCardCounter = 0; sortedCardCounter <= CurrentDesk.GetSortedCardCountInColoum(sourceColoum); sortedCardCounter++)
				{
					for (int targetColoum = 0; targetColoum < Config.NumberOfColoum; targetColoum++)
					{
						if (sourceColoum != targetColoum)
						{

							if (copyedDesk.MoveCardToColoum(sourceColoum,sortedCardCounter,targetColoum))
							{
								float winPercent = copyedDesk.CalculateWinPercent();
								outputResults.Add(new InferResult(copyedDesk, nextInferLayer, winPercent,
									$"Move {CurrentDesk.GetCardInfo(sourceColoum, CurrentDesk.GetCardCountInColoum(sourceColoum) - sortedCardCounter - 1).Pretty()} to coloum {targetColoum}",
									Id));
								copyedDesk = CurrentDesk.DeepClone();
							}
						}
					}
				}
			}



			//Randomly move a card to free card
			//TODO

			//

			return outputResults;
		}
	}

	public class LogicFlowChart
	{
		public LogicFlowChart(bool isSolved, string message)
		{
			_isSolved = isSolved;
			_message = message;
		}

		private bool _isSolved;
		public bool IsSolved => _isSolved;

		private string _message;
		public string Message => _message;

		private List<InferResult> _procedures = new List<InferResult>();
		public List<InferResult> Procedures => _procedures;


		//TODO: ADD logic flow
	}
}
