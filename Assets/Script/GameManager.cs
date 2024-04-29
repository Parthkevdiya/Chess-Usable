using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {

		public static GameManager Instance { get; private set; }
		public enum Result { Playing, WhiteIsMated, BlackIsMated, Stalemate, Repetition, FiftyMoveRule, InsufficientMaterial }
		//public enum Difficulty { Easy , Medium , Hard }

		public event System.Action onPositionLoaded;
		public event System.Action<Move> onMoveMade;

		public enum PlayerType { Human, AI }

		public bool loadCustomPosition;
		public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";

		public PlayerType whitePlayerType;
		public PlayerType blackPlayerType;
		public AISettings aiSettings;
		public AISettings esaySetting;
		public AISettings mediumSetting;
		public AISettings hardSetting;
		public Color[] colors;


		[SerializeField] public GameObject upgradePawnPanel;
		[SerializeField] private Button knightUpgradeBtn , queenUpgradeBtn , rookUpgradeBtn , bishopUpgradeBtn;


        public bool useClocks;
		public Clock whiteClock;
		public Clock blackClock;
		public TMPro.TMP_Text aiDiagnosticsUI;
		public TMPro.TMP_Text resultUI;
        public TextMeshProUGUI difficultyText;

        public Result gameResult;
		DifficultyInstance.Difficulty gameDifficulty = DifficultyInstance.Difficulty.Hard;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;
		List<Move> gameMoves;
		BoardUI boardUI;

		public ulong zobristDebug;
		public Board board { get; private set; }
		Board searchBoard; // Duplicate version of board used for ai search
        private void Awake()
        {
			//gameDifficulty = DifficultyInstance.Instance.gameDifficulty;
			Instance = this;
        }
        void Start () {
			//Application.targetFrameRate = 60;

			if (useClocks) {
				whiteClock.isTurnToMove = false;
				blackClock.isTurnToMove = false;
			}

			boardUI = FindObjectOfType<BoardUI> ();
			gameMoves = new List<Move> ();
			board = new Board ();
			searchBoard = new Board ();
			aiSettings.diagnostics = new Search.SearchDiagnostics ();

			NewGame (whitePlayerType, blackPlayerType);

		}

		void Update () {
			zobristDebug = board.ZobristKey;

			if (gameResult == Result.Playing) {
				LogAIDiagnostics ();

				if (playerToMove != null)
				{
                    playerToMove.Update();
                }
				

				if (useClocks) {
					whiteClock.isTurnToMove = board.WhiteToMove;
					blackClock.isTurnToMove = !board.WhiteToMove;
				}
			}

		}

		public Player GetPlayerToMove()
		{
			return playerToMove;
		}
		public List<Move> GetGameMoves()
		{
			return gameMoves;
		}

		void OnMoveChosen (Move move) {

			if (playerToMove is AIPlayer)
			{
				StartCoroutine(WaitAndDoWorkOnMoveChosen(move, UnityEngine.Random.Range(0.1f, 0.2f)));
			}
			else
			{
                StartCoroutine(WaitAndDoWorkOnMoveChosen(move, 0));
            }
			
            
		}

		 IEnumerator WaitAndDoWorkOnMoveChosen(Move move , float time)
		{
			yield return new WaitForSeconds(time);
			int opponentColourIndex = 1 - board.ColourToMoveIndex;
            int moveFrom = move.StartSquare;
            int moveTo = move.TargetSquare;


            int capturedPieceType = Piece.PieceType(board.Square[moveTo]);
            int movePiece = board.Square[moveFrom];
            int movePieceType = Piece.PieceType(movePiece);

			GameMoveData.Instance.AddMoveInList(move);

			GameMoveData.Instance.AddMovePieceColorInList(board.ColourToMoveIndex);
			GameMoveData.Instance.AddDeathPieceColorInList(opponentColourIndex);
            
			GameMoveData.Instance.AddPeiceInList(movePieceType);
            GameMoveData.Instance.AddDeathPeiceInList(capturedPieceType);


            bool animateMove = playerToMove is AIPlayer;
			board.MakeMove (move);

            

            searchBoard.MakeMove (move);

			gameMoves.Add (move);
			onMoveMade?.Invoke (move);
			boardUI.OnMoveMade (board, move, true);

			


            NotifyPlayerToMove ();
		}

		public void NewGame (bool humanPlaysWhite) {
			boardUI.SetPerspective (humanPlaysWhite);
			NewGame ((humanPlaysWhite) ? PlayerType.Human : PlayerType.AI, (humanPlaysWhite) ? PlayerType.AI : PlayerType.Human);
		}

		public void NewComputerVersusComputerGame () {
			boardUI.SetPerspective (true);
			NewGame (PlayerType.AI, PlayerType.AI);
		}

		public void SetGameDifficulty(int difficultyLevel)
		{
			
			switch (difficultyLevel)
			{
				case 1:
                    gameDifficulty = DifficultyInstance.Difficulty.Easy;
                    break;

                case 2:
                    gameDifficulty = DifficultyInstance.Difficulty.Medium;
                    break;

                case 3:
                    gameDifficulty = DifficultyInstance.Difficulty.Hard;
                    break;
            }
		}

		public void QuitGame()
		{
			SceneManager.LoadScene("MainMenu");
		}

        void NewGame (PlayerType whitePlayerType, PlayerType blackPlayerType) {

			SetUpGameDifficulty();
            difficultyText.text = gameDifficulty.ToString();
            gameMoves.Clear ();
			if (loadCustomPosition) {
				board.LoadPosition (customPosition);
				searchBoard.LoadPosition (customPosition);
			} else {
				board.LoadStartPosition ();
				searchBoard.LoadStartPosition ();
			}
			onPositionLoaded?.Invoke ();
			boardUI.UpdatePosition (board);
			boardUI.ResetSquareColours ();

			CreatePlayer (ref whitePlayer, whitePlayerType);
			CreatePlayer (ref blackPlayer, blackPlayerType);

			gameResult = Result.Playing;
			PrintGameResult (gameResult);

			NotifyPlayerToMove ();

		}

		void SetUpGameDifficulty()
		{
			switch (gameDifficulty)
			{
				case DifficultyInstance.Difficulty.Easy:
					aiSettings = esaySetting;
					break;

				case DifficultyInstance.Difficulty.Medium:
					aiSettings = mediumSetting;
					break;

				case DifficultyInstance.Difficulty.Hard:
					aiSettings = hardSetting;
					break;
				default:
					aiSettings = mediumSetting;
					break;
			}
		}


        void LogAIDiagnostics () {
			string text = "";
			var d = aiSettings.diagnostics;
			//text += "AI Diagnostics";
			text += $"<color=#{ColorUtility.ToHtmlStringRGB(colors[3])}>Version 1.0\n";
			text += $"<color=#{ColorUtility.ToHtmlStringRGB(colors[0])}>Depth Searched: {d.lastCompletedDepth}";
			//text += $"\nPositions evaluated: {d.numPositionsEvaluated}";

			string evalString = "";
			if (d.isBook) {
				evalString = "Book";
			} else {
				float displayEval = d.eval / 100f;
				if (playerToMove is AIPlayer && !board.WhiteToMove) {
					displayEval = -displayEval;
				}
				evalString = ($"{displayEval:00.00}").Replace (",", ".");
				if (Search.IsMateScore (d.eval)) {
					evalString = $"mate in {Search.NumPlyToMateFromScore(d.eval)} ply";
				}
			}
			text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(colors[1])}>Eval: {evalString}";
			text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(colors[2])}>Move: {d.moveVal}";

			aiDiagnosticsUI.text = text;
		}

		void NotifyPlayerToMove () {
			gameResult = GetGameState ();
			PrintGameResult (gameResult);

			if (gameResult == Result.Playing) {
				playerToMove = (board.WhiteToMove) ? whitePlayer : blackPlayer;
				playerToMove.NotifyTurnToMove ();

			} else {
				Debug.Log ("Game Over");
			}
		}

		void PrintGameResult (Result result) {
			float subtitleSize = resultUI.fontSize * 0.75f;
			string subtitleSettings = $"<color=#787878> <size={subtitleSize}>";

			if (result == Result.Playing) {
				resultUI.text = "";
			} else if (result == Result.WhiteIsMated || result == Result.BlackIsMated) {
				resultUI.text = "Checkmate!";
				if (result == Result.WhiteIsMated)
				{
					boardUI.SetSquareColour(BoardRepresentation.CoordFromIndex(board.KingSquare[0]) , boardUI.boardTheme.lightSquaresSprite.kingDeathHighlight , boardUI.boardTheme.darkSquaresSprite.kingDeathHighlight);
				}
				else if (result == Result.BlackIsMated)
				{
                    boardUI.SetSquareColour(BoardRepresentation.CoordFromIndex(board.KingSquare[1]), boardUI.boardTheme.lightSquaresSprite.kingDeathHighlight, boardUI.boardTheme.darkSquaresSprite.kingDeathHighlight);
                }
			} else if (result == Result.FiftyMoveRule) {
				resultUI.text = "Draw";
				resultUI.text += subtitleSettings + "\n(50 move rule)";
			} else if (result == Result.Repetition) {
				resultUI.text = "Draw";
				resultUI.text += subtitleSettings + "\n(3-fold repetition)";
			} else if (result == Result.Stalemate) {
				resultUI.text = "Draw";
				resultUI.text += subtitleSettings + "\n(Stalemate)";
			} else if (result == Result.InsufficientMaterial) {
				resultUI.text = "Draw";
				resultUI.text += subtitleSettings + "\n(Insufficient material)";
			}
		}

		Result GetGameState () {
			MoveGenerator moveGenerator = new MoveGenerator ();
			var moves = moveGenerator.GenerateMoves (board);

			// Look for mate/stalemate
			if (moves.Count == 0) {
				if (moveGenerator.InCheck ()) {
					return (board.WhiteToMove) ? Result.WhiteIsMated : Result.BlackIsMated;
				}
				return Result.Stalemate;
			}

			// Fifty move rule
			if (board.fiftyMoveCounter >= 100) {
				return Result.FiftyMoveRule;
			}

			// Threefold repetition
			int repCount = board.RepetitionPositionHistory.Count ((x => x == board.ZobristKey));
			if (repCount == 3) {
				return Result.Repetition;
			}

			// Look for insufficient material (not all cases implemented yet)
			int numPawns = board.pawns[Board.WhiteIndex].Count + board.pawns[Board.BlackIndex].Count;
			int numRooks = board.rooks[Board.WhiteIndex].Count + board.rooks[Board.BlackIndex].Count;
			int numQueens = board.queens[Board.WhiteIndex].Count + board.queens[Board.BlackIndex].Count;
			int numKnights = board.knights[Board.WhiteIndex].Count + board.knights[Board.BlackIndex].Count;
			int numBishops = board.bishops[Board.WhiteIndex].Count + board.bishops[Board.BlackIndex].Count;

			if (numPawns + numRooks + numQueens == 0) {
				if (numKnights == 1 || numBishops == 1) {
					return Result.InsufficientMaterial;
				}
			}

			return Result.Playing;
		}

		void CreatePlayer (ref Player player, PlayerType playerType) {
			if (player != null) {
				player.onMoveChosen -= OnMoveChosen;
			}

			if (playerType == PlayerType.Human) {
				player = new HumanPlayer (board);
			} else {
				player = new AIPlayer (searchBoard, aiSettings);
			}
			player.onMoveChosen += OnMoveChosen;
		}

        public void OpenUpgradePawnPanel(Move move, Action action)
        {
            upgradePawnPanel.SetActive(true);

            knightUpgradeBtn.onClick.RemoveAllListeners();
            queenUpgradeBtn.onClick.RemoveAllListeners();
            rookUpgradeBtn.onClick.RemoveAllListeners();
            bishopUpgradeBtn.onClick.RemoveAllListeners();

            // going to generate new move

            knightUpgradeBtn.onClick.AddListener(() => 
			{
				Move newMove = new Move(move.StartSquare , move.TargetSquare , Move.Flag.PromoteToKnight);
				(playerToMove as HumanPlayer).MoveItForMe(newMove);
				action();
				upgradePawnPanel.SetActive(false);
			});

            queenUpgradeBtn.onClick.AddListener(() =>
            {
                Move newMove = new Move(move.StartSquare, move.TargetSquare, Move.Flag.PromoteToQueen);
                (playerToMove as HumanPlayer).MoveItForMe(newMove);
                action();
                upgradePawnPanel.SetActive(false);
            });

            rookUpgradeBtn.onClick.AddListener(() =>
            {
                Move newMove = new Move(move.StartSquare, move.TargetSquare, Move.Flag.PromoteToRook);
                (playerToMove as HumanPlayer).MoveItForMe(newMove);
                action();
                upgradePawnPanel.SetActive(false);
            });

            bishopUpgradeBtn.onClick.AddListener(() =>
            {
                Move newMove = new Move(move.StartSquare, move.TargetSquare, Move.Flag.PromoteToBishop);
                (playerToMove as HumanPlayer).MoveItForMe(newMove);
                action();
                upgradePawnPanel.SetActive(false);
            });
        }

    }
}