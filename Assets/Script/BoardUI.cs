using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chess.Game {
	public class BoardUI : MonoBehaviour {
		public PieceTheme pieceTheme;
		public BoardTheme boardTheme;
		public bool showLegalMoves;
		public Shader colorShader;

		public bool whiteIsBottom = true;

		Image[, ] squareRenderers;
		Image[, ] squarePieceRenderers;
		Move lastMadeMove;
		MoveGenerator moveGenerator;

		const float pieceDepth = -0.1f;
		const float pieceDragDepth = -0.2f;

		[SerializeField] private GameObject quardImage;



        [SerializeField] GraphicRaycaster m_Raycaster;
        PointerEventData m_PointerEventData;
        [SerializeField] EventSystem m_EventSystem;

        void Start()
        {
            /*//Fetch the Raycaster from the GameObject (the Canvas)
            m_Raycaster = GetComponent<GraphicRaycaster>();
            //Fetch the Event System from the Scene
            m_EventSystem = GetComponent<EventSystem>();*/
        }

        void Awake () {
			moveGenerator = new MoveGenerator ();
			CreateBoardUI ();

		}

		public void HighlightLegalMoves (Board board, Coord fromSquare) {
			if (showLegalMoves) {

				var moves = moveGenerator.GenerateMoves (board);

				for (int i = 0; i < moves.Count; i++) {
					Move move = moves[i];
					if (move.StartSquare == BoardRepresentation.IndexFromCoord (fromSquare)) {
						Coord coord = BoardRepresentation.CoordFromIndex (move.TargetSquare);
						
						if (board.Square[move.TargetSquare] != 0)
						{
							SetSquareColour(coord, boardTheme.lightSquaresSprite.deathHighlight, boardTheme.darkSquaresSprite.deathHighlight);
							Debug.Log("Can Capture" + board.Square[move.TargetSquare]);
						}
						else
						{
                            SetSquareColour(coord, boardTheme.lightSquaresSprite.legal, boardTheme.darkSquaresSprite.legal);
                            Debug.Log("No Body's Here" + board.Square[move.TargetSquare]);
                        }
					}
				}
			}
		}

		public void DragPiece (Coord pieceCoord, Vector2 mousePos) {
			squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = new Vector3 (mousePos.x, mousePos.y, pieceDragDepth);
		}

		public void ResetPiecePosition (Coord pieceCoord) {
			Vector3 pos = Vector3.zero;
			squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.localPosition = pos;
		}

		public void SelectSquare (Coord coord) {
			SetSquareColour (coord, boardTheme.lightSquaresSprite.selected, boardTheme.darkSquaresSprite.selected);
		}

		public void DeselectSquare (Coord coord) {
			//BoardTheme.SquareColours colours = (coord.IsLightSquare ()) ? boardTheme.lightSquares : boardTheme.darkSquares;
			//squareMaterials[coord.file, coord.rank].color = colours.normal;
			ResetSquareColours ();
		}

		public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out Coord selectedCoord)
		{
            int file = (int)(mouseWorld.x + 4);
            int rank = (int)(mouseWorld.y + 4);
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
			//Set the Pointer Event Position to that of the mouse position
			m_PointerEventData.position = Input.mousePosition;

			//Create a list of Raycast Results
			List<RaycastResult> results = new List<RaycastResult>();

			//Raycast using the Graphics Raycaster and mouse click position
			m_Raycaster.Raycast(m_PointerEventData, results);

			//For every result returned, output the name of the GameObject on the Canvas hit by the Ray
			foreach (RaycastResult result in results)
			{
				if (result.gameObject.transform.tag.Equals("Square"))
				{
					GetFileAndRank(result.gameObject.name , out int fileVal , out int rankVal);
					file = rankVal;
					rank = fileVal;
					
				}
			}

			if (!whiteIsBottom)
			{
				file = 7 - file;
				rank = 7 - rank;
			}
			selectedCoord = new Coord(file, rank);
			return file >= 0 && file < 8 && rank >= 0 && rank < 8;
		}

		public void GetFileAndRank(string str , out int file , out int rank)
		{
			file = 0;
			rank = 0;
			string ch = str[0].ToString();
			switch (ch)
			{
				case "a":
					rank = 0;
					break;
                case "b":
                    rank = 1;
                    break;
                case "c":
                    rank = 2;
                    break;
                case "d":
                    rank = 3;
                    break;
                case "e":
                    rank = 4;
                    break;
                case "f":
                    rank = 5;
                    break;
                case "g":
                    rank = 6;
                    break;
                case "h":
                    rank = 7;
                    break;
            }
			file = int.Parse(str[1].ToString()) - 1;
		}

        public void UpdatePosition (Board board) {
			for (int rank = 0; rank < 8; rank++) {
				for (int file = 0; file < 8; file++) {
					Coord coord = new Coord (file, rank);
					int piece = board.Square[BoardRepresentation.IndexFromCoord (coord.fileIndex, coord.rankIndex)];
					squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite (piece);
					//squarePieceRenderers[file, rank].transform.position = PositionFromCoord (file, rank, pieceDepth);
					squarePieceRenderers[file, rank].transform.localPosition = Vector3.zero;

                    if (squarePieceRenderers[file, rank].sprite == null)
					{
						squarePieceRenderers[file, rank].color = new Color32(255, 255, 255, 0);
					}
					else
					{
                        squarePieceRenderers[file, rank].color = new Color32(255, 255, 255, 255);
					}
				}
			}

		}

		public void OnMoveMade (Board board, Move move, bool animate = false) {
			lastMadeMove = move;
			if (animate) {
				StartCoroutine (AnimateMove (move, board));
			} else {
				UpdatePosition (board);
				ResetSquareColours ();
			}
		}


/*		IEnumerator AnimateMove(Move_ChessAi move, Board_ChessAi board)
		{
			float t = 0;
			const float moveAnimDuration = 0.25f;
			Coord_ChessAi startCoord = BoardRepresentation_ChessAi.CoordFromIndex(move.StartSquare);
			Coord_ChessAi targetCoord = BoardRepresentation_ChessAi.CoordFromIndex(move.TargetSquare);
			Transform pieceT = squarePieceRenderers[startCoord.fileIndex, startCoord.rankIndex].transform;

			//Vector3 startPos = PositionFromCoord(startCoord);
			//Vector3 targetPos = PositionFromCoord(targetCoord);

			//Vector3 startPos = squareRenderers[startCoord.fileIndex, startCoord.rankIndex].transform.position;
			//Vector3 targetPos = squareRenderers[targetCoord.fileIndex, targetCoord.rankIndex].transform.position;

			Vector3 startPos = pieceT.TransformPoint(pieceT.position);
			Vector3 targetPos = squareRenderers[targetCoord.fileIndex, targetCoord.rankIndex].transform.position;

			if (GameManager_ChessAi.Instance.GetPlayerToMove() is AIPlayer_ChessAi)
			{
				SetSquareColour(BoardRepresentation_ChessAi.CoordFromIndex(move.StartSquare), boardTheme.lightSquaresSprite.moveFromHighlight, boardTheme.darkSquaresSprite.moveFromHighlight);
			}


			while (t <= 1)
			{
				yield return null;
				t += Time.deltaTime * 1 / moveAnimDuration;
				pieceT.position = Vector3.Lerp(startPos, targetPos, t);
			}
			UpdatePosition(board);
			if (GameManager_ChessAi.Instance.gameResult == GameManager_ChessAi.Result.Playing)
			{
				ResetSquareColours();
			}

			pieceT.position = startPos;

			if (preCircleObjClone != null)
			{
				preCircleObjClone.transform.localScale = new Vector3(1f, 1f, 1f);
				Destroy(preCircleObjClone);
			}
		}
*/

		IEnumerator AnimateMove (Move move, Board board) {
			float t = 0;
			const float moveAnimDuration = 0.15f;
			Coord startCoord = BoardRepresentation.CoordFromIndex (move.StartSquare);
			Coord targetCoord = BoardRepresentation.CoordFromIndex (move.TargetSquare);
			Transform pieceT = squarePieceRenderers[startCoord.fileIndex, startCoord.rankIndex].transform;


            Vector3 startPos = squareRenderers[startCoord.fileIndex, startCoord.rankIndex].transform.position;
			Vector3 targetPos = squareRenderers[targetCoord.fileIndex, targetCoord.rankIndex].transform.position;
			
		

            /*Vector3 startPos = PositionFromCoord (startCoord);
			Vector3 targetPos = PositionFromCoord (targetCoord);*/

            /*if (GameManager.Instance.GetPlayerToMove() is AIPlayer)
			{
                SetSquareColour(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquaresSprite.moveFromHighlight, boardTheme.darkSquaresSprite.moveFromHighlight);
            }*/


            while (t <= 1) {
				yield return null;
				t += Time.deltaTime * 1 / moveAnimDuration;
				pieceT.position = Vector3.Lerp (startPos, targetPos, t);
			}
			UpdatePosition (board);
			if (GameManager.Instance.gameResult == GameManager.Result.Playing)
			{
				ResetSquareColours();
			}
			
			pieceT.position = startPos;
		}

		void HighlightMove(Move move)
		{
			if (GameManager.Instance.GetPlayerToMove() is AIPlayer)
			{
				SetSquareColour(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquaresSprite.moveFromHighlight, boardTheme.darkSquaresSprite.moveFromHighlight);
				SetSquareColour(BoardRepresentation.CoordFromIndex(move.TargetSquare), boardTheme.lightSquaresSprite.moveToHighlight, boardTheme.darkSquaresSprite.moveToHighlight);

			}
		}

		void CreateBoardUI () {

			Shader squareShader = Shader.Find("Unlit/Texture");
			squareRenderers = new Image[8, 8];
			squarePieceRenderers = new Image[8, 8];

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
                    // Create square
                    //Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                    Image square = new GameObject("Piece").AddComponent<Image>();
					square.rectTransform.parent = transform;
					square.name = BoardRepresentation.SquareNameFromCoordinate(file, rank);
					square.rectTransform.position = PositionFromCoord(file, rank, 0);
                    square.rectTransform.sizeDelta = new Vector2(225, 225);
                    square.rectTransform.localScale = Vector3.one;
                    square.rectTransform.localPosition = new Vector3((float)-787.5 + file * 225, (float)-787.5 + rank * 225 , -225);
					square.rectTransform.tag = "Square";
                    Material squareMaterial = new Material(squareShader);

                    squareRenderers[file, rank] = square.gameObject.GetComponent<Image>();
					//squareRenderers[file, rank].material = squareMaterial;

					// Create piece sprite renderer for current square
					Image pieceRenderer = new GameObject("Piece").AddComponent<Image>();
					Canvas canvas = pieceRenderer.rectTransform.gameObject.AddComponent<Canvas>();
					StartCoroutine(WaitAndOveerideSorting( () => { canvas.overrideSorting = true; }));
					canvas.sortingOrder = 2;
					pieceRenderer.rectTransform.parent = square.rectTransform;
					pieceRenderer.rectTransform.localScale = Vector3.one;
					pieceRenderer.rectTransform.sizeDelta = new Vector2(225, 225);
					pieceRenderer.rectTransform.localPosition = Vector3.zero;
					pieceRenderer.preserveAspect = true;
					pieceRenderer.raycastTarget = false;
					squarePieceRenderers[file, rank] = pieceRenderer;
				}
			}

			ResetSquareColours ();
		}

		IEnumerator WaitAndOveerideSorting(Action action)
		{
			yield return new WaitForSeconds(0.01f);
			action();
		}

		void ResetSquarePositions () {
			for (int rank = 0; rank < 8; rank++) {
				for (int file = 0; file < 8; file++) {
					if (file == 0 && rank == 0) {
						//Debug.Log (squarePieceRenderers[file, rank].gameObject.name + "  " + PositionFromCoord (file, rank, pieceDepth));
					}
					//squarePieceRenderers[file, rank].transform.position = PositionFromCoord (file, rank, pieceDepth);
					squareRenderers[file, rank].transform.position = PositionFromCoord (file, rank, 0);
                    //squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
                    squarePieceRenderers[file, rank].transform.localPosition = Vector3.zero;
				}
			}

			if (!lastMadeMove.IsInvalid) {
				HighlightMove (lastMadeMove);
			}
		}

		public void SetPerspective (bool whitePOV) {
			whiteIsBottom = whitePOV;
			ResetSquarePositions ();

		}

		public void ResetSquareColours (bool highlight = true) {
			for (int rank = 0; rank < 8; rank++) {
				for (int file = 0; file < 8; file++) {
					SetSquareColour (new Coord (file, rank), boardTheme.lightSquaresSprite.normal, boardTheme.darkSquaresSprite.normal);
				}
			}
			if (highlight) {
				if (!lastMadeMove.IsInvalid) {
					HighlightMove (lastMadeMove);
				}
			}
		}

		public void SetSquareColour (Coord square, Sprite lightCol, Sprite darkCol) {
			squareRenderers[square.fileIndex, square.rankIndex].sprite = (square.IsLightSquare ()) ? lightCol : darkCol;
		}

		public Vector3 PositionFromCoord (int file, int rank, float depth = 0) {
			if (whiteIsBottom) {
				return new Vector3 (-3.5f + file, -3.5f + rank, depth);
			}
			return new Vector3 (-3.5f + 7 - file, 7 - rank - 3.5f, depth);

		}

		public Vector3 PositionFromCoord (Coord coord, float depth = 0) {
			return PositionFromCoord (coord.fileIndex, coord.rankIndex, depth);
		}

	}
}