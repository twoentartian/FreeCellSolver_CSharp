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
			set { _inferResultMapId = value; }
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
			InferResult startInferResult = new InferResult(desk);
			startInferResult.Id = InferResultMapId;
			InferResults.Add(startInferResult);
		}

		public LogicFlowChart StartInfer(int layer = 0)
		{
			LogicFlowChart outputFlowChart;
			if (layer == 0)
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
							outputFlowChart = new LogicFlowChart(false, $"Cannot solve in {LastInferLayer} layers");
							return outputFlowChart;
						}
					}
					List<InferResult> results = InferResults[index].Infer();
					foreach (InferResult result in results)
					{
						if (result.CurrentDesk.IsSolved())
						{
							outputFlowChart = new LogicFlowChart(true, $"Solved in {LastInferLayer} layers");
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
									InferResult parentResult = InferResults.Find(singleResult => singleResult.Id == tempInferResult.FromInferResultId);
									tempInferResult = parentResult;
								}
							}
							
							//TODO Assemble output flow chart
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
			else
			{
				for (int i = 0; i < layer; i++)
				{
					InferResults.Sort();
					int index = 0;
					while (InferResults[index].IsInferred)
					{
						index++;
						if (index == InferResults.Count)
						{
							outputFlowChart = new LogicFlowChart(false, $"Cannot solve in {LastInferLayer} layers");
							return outputFlowChart;
						}
					}
					List<InferResult> results = InferResults[index].Infer();
					foreach (InferResult result in results)
					{
						if (result.CurrentDesk.IsSolved())
						{
							outputFlowChart = new LogicFlowChart(true, $"Solved in {LastInferLayer} layers");
							//TODO Assemble output flow chart
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
				outputFlowChart = new LogicFlowChart(false, $"Cannot solve in {layer} layers");
				return outputFlowChart;
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

			//Check is there any cards to move to sorted card.
			for (int i = 0; i < CurrentDesk.AllCardOnDesk.ColoumCard.GetLength(0); i++)
			{
				Card tempcard = CurrentDesk.GetTheLastCardInfoInColoum(i);
				if (tempcard == null)
				{
					continue;
				}
				if (CurrentDesk.AddNewCardInSortedCard(tempcard))
				{
					Desk copyedDesk = CurrentDesk.DeepClone();
					copyedDesk.AddNewCardInSortedCard(tempcard, true);
					copyedDesk.RemoveCardInColoum(i);
					float winPercent = copyedDesk.CalculateWinPercent();
					outputResults.Add(new InferResult(copyedDesk, nextInferLayer, winPercent, $"Move {tempcard.Pretty()} to sorted card", Id));
				}
			}


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
