using Chess;
using Chess.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMoveData : MonoBehaviour
{
    [SerializeField] private DeathPieceImages whiteDethPiceImages;
    [SerializeField] private DeathPieceImages blackDethPiceImages;

    public enum PieceType
    {
        None,
        King,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
    }

    public enum ColorType
    {
        White,
        Black,
    }
    public static GameMoveData Instance { get; private set; }

    private List<Move> gameMoves = new List<Move>();

    private List<PieceType> movePiecesType = new List<PieceType>();
    private List<PieceType> deathPiecesType = new List<PieceType>();

    private List<ColorType> movePiecesTypeColor = new List<ColorType>();
    private List<ColorType> deathPiecesTypeColor = new List<ColorType>();

    [SerializeField] private List<GamePieceMoveData> gameMoveDatas;
    private void Awake()
    {
        Instance = this;
        Debug.Log(Instance);
    }

    public List<Move> GetGameMovesList()
    {
        return GameManager.Instance.GetGameMoves();
    }

    public void AddMoveInList(Move move)
    {
        gameMoves.Add(move);
    }
    public void AddPeiceInList(int pieceIndex)
    {
        movePiecesType.Add(GetPieceFromNumber(pieceIndex));
    }

    public void AddDeathPeiceInList(int pieceIndex)
    {
        deathPiecesType.Add(GetPieceFromNumber(pieceIndex));
        OnPieceDeath();
        AddLastDataInContainer();
    }

    public void AddMovePieceColorInList(int colorIndex)
    {
        movePiecesTypeColor.Add(GetColorFromIndex(colorIndex));
    }

    public void AddDeathPieceColorInList(int colorIndex)
    {
        deathPiecesTypeColor.Add(GetColorFromIndex(colorIndex));
    }

    public void OnPieceDeath()
    {
        int lastIndex = deathPiecesType.Count - 1;
        if (deathPiecesType[lastIndex] != PieceType.None)
        {
            ColorType colorType = deathPiecesTypeColor[lastIndex];
            switch (deathPiecesType[lastIndex])
            {
                case PieceType.King:
                    // King Never Dies But Just For Record
                    if (colorType == ColorType.White)
                    {
                        whiteDethPiceImages.king.gameObject.SetActive(true);
                    }
                    else if(colorType == ColorType.Black) 
                    {
                        blackDethPiceImages.king.gameObject.SetActive(true);
                    }
                    break;

                case PieceType.Queen:
                    if (colorType == ColorType.White)
                    {
                        whiteDethPiceImages.queen.gameObject.SetActive(true);
                    }
                    else if (colorType == ColorType.Black)
                    {
                        blackDethPiceImages.queen.gameObject.SetActive(true);
                    }
                    break;

                case PieceType.Pawn:
                    GetFirstInActivePawnOfColor(colorType).SetActive(true);
                    break;

                case PieceType.Knight:
                    GetFirstInActiveKnightOfColor(colorType).SetActive(true);
                    break;

                case PieceType.Bishop:
                    GetFirstInActiveBishopOfColor(colorType).SetActive(true);
                    break;

                case PieceType.Rook:
                    GetFirstInActiveRookOfColor(colorType).SetActive(true);
                    break;

            }
        }
    }

    public void AddLastDataInContainer()
    {
        int lastIndex = gameMoves.Count - 1;
        GamePieceMoveData gameMoveData = new GamePieceMoveData();
        gameMoveData.moveNumber = (lastIndex + 1).ToString();

        gameMoveData.movedFrom = BoardRepresentation.SquareNameFromIndex(gameMoves[lastIndex].StartSquare);
        gameMoveData.movedTo = BoardRepresentation.SquareNameFromIndex(gameMoves[lastIndex].TargetSquare);

        gameMoveData.movedPeice = movePiecesType[lastIndex];
        gameMoveData.toPiece = deathPiecesType[lastIndex];

        gameMoveData.movedPieceColor = movePiecesTypeColor[lastIndex];
        gameMoveData.toPieceColor = deathPiecesTypeColor[lastIndex];

        gameMoveDatas.Add(gameMoveData);
    }

    public GameObject GetFirstInActivePawnOfColor(ColorType colorType)
    {
        if (colorType == ColorType.White)
        {
            for (int i = 0; i < whiteDethPiceImages.pawn.Length; i++)
            {
                if (whiteDethPiceImages.pawn[i].gameObject.active == false)
                {
                    return whiteDethPiceImages.pawn[i].gameObject;
                }
            }
        }
        else if (colorType == ColorType.Black)
        {
            for (int i = 0; i < blackDethPiceImages.pawn.Length; i++)
            {
                if (blackDethPiceImages.pawn[i].gameObject.active == false)
                {
                    return blackDethPiceImages.pawn[i].gameObject;
                }
            }
        }
        return null;
    }

    public GameObject GetFirstInActiveKnightOfColor(ColorType colorType)
    {
        if (colorType == ColorType.White)
        {
            for (int i = 0; i < whiteDethPiceImages.knight.Length; i++)
            {
                if (whiteDethPiceImages.knight[i].gameObject.active == false)
                {
                    return whiteDethPiceImages.knight[i].gameObject;
                }
            }
        }
        else if (colorType == ColorType.Black)
        {
            for (int i = 0; i < blackDethPiceImages.knight.Length; i++)
            {
                if (blackDethPiceImages.knight[i].gameObject.active == false)
                {
                    return blackDethPiceImages.knight[i].gameObject;
                }
            }
        }
        return null;
    }

    public GameObject GetFirstInActiveBishopOfColor(ColorType colorType)
    {
        if (colorType == ColorType.White)
        {
            for (int i = 0; i < whiteDethPiceImages.bishop.Length; i++)
            {
                if (whiteDethPiceImages.bishop[i].gameObject.active == false)
                {
                    return whiteDethPiceImages.bishop[i].gameObject;
                }
            }
        }
        else if (colorType == ColorType.Black)
        {
            for (int i = 0; i < blackDethPiceImages.bishop.Length; i++)
            {
                if (blackDethPiceImages.bishop[i].gameObject.active == false)
                {
                    return blackDethPiceImages.bishop[i].gameObject;
                }
            }
        }
        return null;
    }

    public GameObject GetFirstInActiveRookOfColor(ColorType colorType)
    {
        if (colorType == ColorType.White)
        {
            for (int i = 0; i < whiteDethPiceImages.rook.Length; i++)
            {
                if (whiteDethPiceImages.rook[i].gameObject.active == false)
                {
                    return whiteDethPiceImages.rook[i].gameObject;
                }
            }
        }
        else if (colorType == ColorType.Black)
        {
            for (int i = 0; i < blackDethPiceImages.rook.Length; i++)
            {
                if (blackDethPiceImages.rook[i].gameObject.active == false)
                {
                    return blackDethPiceImages.rook[i].gameObject;
                }
            }
        }
        return null;
    }

    public Move GetMoveOfMoveNumber(int moveNumber)
    {
        return GetGameMovesList()[moveNumber];
    }

    public PieceType GetPieceFromNumber(int pieceNumber)
    {
        switch (pieceNumber)
        {
            case 0:
                return PieceType.None;
            case 1:
                return PieceType.King;
            case 2:
                return PieceType.Pawn;
            case 3:
                return PieceType.Knight;
            case 5:
                return PieceType.Bishop;
            case 6:
                return PieceType.Rook;
            case 7:
                return PieceType.Queen;
            default:
                return PieceType.None;
        }
    }

    public ColorType GetColorFromIndex(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0:
                return ColorType.White;
            case 1: 
                return ColorType.Black;
            default:
                return ColorType.Black;
        }
    }


}

[Serializable]
public class DeathPieceImages
{
    public Image king;
    public Image queen;
    public Image[] pawn;
    public Image[] knight;
    public Image[] bishop;
    public Image[] rook;

}

[Serializable]
public class GamePieceMoveData
{
    public string moveNumber;
    public string movedFrom, movedTo;
    public GameMoveData.PieceType movedPeice , toPiece;
    public GameMoveData.ColorType movedPieceColor , toPieceColor;
}
