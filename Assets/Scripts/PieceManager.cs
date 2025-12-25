using System;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    [HideInInspector]
    public bool mIsKingAlive = true;

    public bool mGameOver = false;

    // Color to move and whether that side is currently in check
    public Color mColorToMove = Color.white;
    public bool mColorToMoveInCheck = false;

    public GameObject mPiecePrefab;

    public TimerController mTimerController;
    public StatusOverlay mStatusOverlay; // optional UI overlay for status images

    [Header("UI Control")]
    public bool controlStatusExternally = false;

    public event System.Action OnCheck;
    public event System.Action OnCheckmate;
    public event System.Action<Color> OnCheckmateWithWinner;
    public event System.Action OnStalemate;
    public event System.Action OnClearStatus;

    private List<BasePiece> mWhitePieces = null;
    private List<BasePiece> mBlackPieces = null;
    private List<BasePiece> mPromotedPieces = new List<BasePiece>();

    private string[] wPieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "K", "Q", "B", "KN", "R"
    };

    private string[] bPieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R"
    };

    private Dictionary<string, Type> mPieceLibrary = new Dictionary<string, Type>()
    {
        {"P",  typeof(Pawn)},
        {"R",  typeof(Rook)},
        {"KN", typeof(Knight)},
        {"B",  typeof(Bishop)},
        {"K",  typeof(King)},
        {"Q",  typeof(Queen)},
        {"PP", typeof(Treybok)},
    };

    // 🔹 ADDED: scale per piece type
    private Dictionary<Type, Vector3> mPieceScales = new Dictionary<Type, Vector3>()
    {
        { typeof(Pawn),   new Vector3(0.9f, 0.9f, 1f) },
        { typeof(Rook),   new Vector3(0.88f, 0.88f, 1f) },
        { typeof(Knight), new Vector3(0.76f, 1.17f, 1f) },
        { typeof(Bishop), new Vector3(0.70f, 1.05f, 1f) },
        { typeof(Queen),  new Vector3(0.53f, 1.11f, 1f) },
        { typeof(Treybok), new Vector3(0.9f, 0.9f, 1f) },
        { typeof(King),   new Vector3(0.82f,1.29f,1f) },
    };

    public void Setup(Board board)
    {
        if (mTimerController == null)
            mTimerController = GetComponent<TimerController>();
        
        // Auto-find StatusOverlay if not assigned
        if (mStatusOverlay == null)
            mStatusOverlay = FindFirstObjectByType<StatusOverlay>();
        
        mWhitePieces = CreatePieces(Color.white, new Color32(255, 255, 255, 255), wPieceOrder);

        mBlackPieces = CreatePieces(
            Color.black,
            new Color32(193, 154, 107, 255),
            bPieceOrder
        );

        PlacePieces(2, 0, mWhitePieces, board);
        PlacePieces(5, 7, mBlackPieces, board);

        // SwitchSides(Color.black);
    }

    // 🔹 MODIFIED: added pieceOrder parameter
    private List<BasePiece> CreatePieces(Color teamColor, Color32 spriteColor, string[] pieceOrder)
    {
        List<BasePiece> newPieces = new List<BasePiece>();

        for (int i = 0; i < pieceOrder.Length; i++)
        {
            string key = pieceOrder[i];
            Type pieceType = mPieceLibrary[key];

            BasePiece newPiece = CreatePiece(pieceType);
            newPieces.Add(newPiece);

            newPiece.Setup(teamColor, spriteColor, this);
        }

        return newPieces;
    }

    // 🔹 MODIFIED (scale applied, no other logic touched)
    private BasePiece CreatePiece(Type pieceType)
    {
        GameObject newPieceObject = Instantiate(mPiecePrefab);
        newPieceObject.transform.SetParent(transform);
        newPieceObject.transform.localRotation = Quaternion.identity;

        BasePiece newPiece = (BasePiece)newPieceObject.AddComponent(pieceType);

        // Apply scale based on piece type
        if (mPieceScales.TryGetValue(pieceType, out Vector3 scale))
            newPieceObject.transform.localScale = scale;
        else
            newPieceObject.transform.localScale = Vector3.one;

        return newPiece;
    }

    private void PlacePieces(int pawnRow, int royaltyRow, List<BasePiece> pieces, Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            pieces[i].Place(board.mAllCells[i, pawnRow]);
            pieces[i + 8].Place(board.mAllCells[i, royaltyRow]);
        }
    }

    private void SetInteractive(List<BasePiece> allPieces, bool value)
    {
        foreach (BasePiece piece in allPieces)
            piece.enabled = value;
    }

    private void SetAllInteractive(bool value)
    {
        SetInteractive(mWhitePieces, value);
        SetInteractive(mBlackPieces, value);
        foreach (var piece in mPromotedPieces)
            piece.enabled = value;
    }

    public void SwitchSides(Color color)
    {
        if (mGameOver)
            return;

        if (!mIsKingAlive)
        {
            ResetPieces();
            mIsKingAlive = true;
            color = Color.black;
        }

        bool isBlackTurn = color == Color.white ? true : false;

        SetInteractive(mWhitePieces, !isBlackTurn);
        SetInteractive(mBlackPieces, isBlackTurn);

        foreach (BasePiece piece in mPromotedPieces)
        {
            bool isBlackPiece = piece.mColor != Color.white ? true : false;
            bool isPartOfTeam = isBlackPiece == true ? isBlackTurn : !isBlackTurn;
            piece.enabled = isPartOfTeam;
        }

        // Evaluate board state for the next side to move
        Color nextToMove = isBlackTurn ? Color.black : Color.white;
        var endState = EvaluateEndState(nextToMove);

        // Cache turn info to avoid recursive check computations in pathing
        mColorToMove = nextToMove;
        mColorToMoveInCheck = (endState == EndState.Check || endState == EndState.Checkmate);

        // If in check, restrict interactable pieces according to rules
        if (endState == EndState.Check)
            ApplyCheckTurnConstraints(nextToMove);

        Color winnerColor = Color.clear;

        // Emit events for external controllers
        switch (endState)
        {
            case EndState.Checkmate:
                mGameOver = true;
                mTimerController?.StopTimers();
                SetAllInteractive(false);

                // If nextToMove is checkmated, the other side wins.
                winnerColor = (nextToMove == Color.white) ? Color.black : Color.white;
                OnCheckmateWithWinner?.Invoke(winnerColor);
                OnCheckmate?.Invoke();
                
                // Play checkmate sound (overrides pending move sound)
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.ClearPendingSound();
                    GameSoundManager.Instance.PlayCheckmateSound();
                }
                break;
            case EndState.Stalemate:
                mGameOver = true;
                mTimerController?.StopTimers();
                SetAllInteractive(false);
                OnStalemate?.Invoke();
                
                // Play draw sound (overrides pending move sound)
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.ClearPendingSound();
                    GameSoundManager.Instance.PlayDrawSound();
                }
                break;
            case EndState.Check:
                OnCheck?.Invoke();
                
                // Play check sound (overrides pending move sound)
                if (GameSoundManager.Instance != null)
                {
                    GameSoundManager.Instance.ClearPendingSound();
                    GameSoundManager.Instance.PlayCheckSound();
                }
                break;
            default:
                OnClearStatus?.Invoke();
                
                // No check - play the pending move/capture sound
                if (GameSoundManager.Instance != null)
                    GameSoundManager.Instance.PlayPendingMoveSoundIfNoCheck();
                break;
        }

        // Show UI overlay internally only if not controlled externally
        if (!controlStatusExternally)
        {
            if (mStatusOverlay == null)
            {
                Debug.LogWarning("PieceManager: mStatusOverlay is null! Trying to find it...");
                mStatusOverlay = FindFirstObjectByType<StatusOverlay>();
            }
            
            if (mStatusOverlay != null)
            {
                switch (endState)
                {
                    case EndState.Checkmate:
                        Debug.Log("PieceManager: Showing Checkmate/Win UI");
                        if (winnerColor != Color.clear)
                            mStatusOverlay.ShowWin(winnerColor);
                        else
                            mStatusOverlay.ShowCheckmate();
                        break;
                    case EndState.Stalemate:
                        Debug.Log("PieceManager: Showing Draw UI");
                        mStatusOverlay.ShowDraw();
                        break;
                    case EndState.Check:
                        mStatusOverlay.ShowCheck();
                        break;
                    default:
                        mStatusOverlay.HideAll();
                        break;
                }
            }
            else
            {
                Debug.LogError("PieceManager: Could not find StatusOverlay!");
            }
        }

        // Only advance turn/timers if the game is not over.
        if (!mGameOver)
            mTimerController?.SwitchTurn();
    }

    private IEnumerable<BasePiece> GetAllPieces(Color color)
    {
        foreach (var p in (color == Color.white ? mWhitePieces : mBlackPieces))
            yield return p;
        foreach (var p in mPromotedPieces)
            if (p.mColor == color) yield return p;
    }

    // Allow only legal responders while in check: king moves always; if single check, also blocks/captures
    private void ApplyCheckTurnConstraints(Color toMove)
    {
        var analysis = AnalyzeCheckResponses(toMove);

        // Build allowed set
        var allowed = new HashSet<BasePiece>();
        foreach (var mv in analysis.kingMoves) allowed.Add(mv.piece);
        if (!analysis.isDoubleCheck)
        {
            foreach (var mv in analysis.captureMoves) allowed.Add(mv.piece);
            foreach (var mv in analysis.blockMoves) allowed.Add(mv.piece);
        }

        // Enable only allowed pieces for the side to move; disable others
        foreach (var p in GetAllPieces(toMove))
            p.enabled = allowed.Contains(p);
    }

    public void ResetPieces()
    {
        mGameOver = false;
        mIsKingAlive = true;

        foreach (BasePiece piece in mPromotedPieces)
        {
            piece.Kill();
            Destroy(piece.gameObject);
        }

        mPromotedPieces.Clear();

        foreach (BasePiece piece in mWhitePieces)
            piece.Reset();

        foreach (BasePiece piece in mBlackPieces)
            piece.Reset();

        // Reset turn to white (white moves first)
        mColorToMove = Color.white;
        mColorToMoveInCheck = false;

        // Enable white pieces and disable black pieces
        SetInteractive(mWhitePieces, true);
        SetInteractive(mBlackPieces, false);

        // Clear any status overlay
        OnClearStatus?.Invoke();
    }

    public void PromotePiece(Pawn pawn, Cell cell, Color teamColor, Color spriteColor)
    {
        pawn.Kill();

        BasePiece promotedPiece = CreatePiece(typeof(Treybok));
        promotedPiece.Setup(teamColor, spriteColor, this);
        promotedPiece.Place(cell);

        mPromotedPieces.Add(promotedPiece);
    }

    // --- Check/Attack/Mate/Stalemate Helpers ---
    private List<BasePiece> GetPieces(Color color)
    {
        List<BasePiece> result = new List<BasePiece>();
        result.AddRange(color == Color.white ? mWhitePieces : mBlackPieces);
        result.AddRange(mPromotedPieces.FindAll(p => p.mColor == color));
        return result;
    }

    public Vector2Int GetKingPosition(Color color)
    {
        foreach (var piece in GetPieces(color))
        {
            if (piece is King && piece.gameObject.activeSelf && piece.CurrentCell != null)
                return piece.CurrentCell.mBoardPosition;
        }
        // Fallback: not found
        return new Vector2Int(-1, -1);
    }

    public bool IsSquareAttacked(Vector2Int square, Color byColor, BasePiece ignorePiece = null)
    {
        var attackers = GetPieces(byColor);
        foreach (var piece in attackers)
        {
            if (ignorePiece != null && ReferenceEquals(piece, ignorePiece))
                continue;
            if (!piece.gameObject.activeSelf || piece.CurrentCell == null)
                continue;

            // Pawn: attack diagonals only based on color forward
            if (piece is Pawn)
            {
                int forward = (piece.mColor == Color.white) ? 1 : -1;
                int x = piece.CurrentCell.mBoardPosition.x;
                int y = piece.CurrentCell.mBoardPosition.y;
                Vector2Int a1 = new Vector2Int(x - 1, y + forward);
                Vector2Int a2 = new Vector2Int(x + 1, y + forward);
                if (a1 == square || a2 == square)
                {
                    var state = piece.CurrentCell.mBoard.ValidateCell(square.x, square.y, piece);
                    if (state != CellState.Friendly && state != CellState.OutOfBounds)
                        return true;
                }
                continue;
            }

            // Knight and others: use pathing which includes captures
            piece.GeneratePathing();
            bool attacks = piece.GetHighlightedCells().Exists(c => c.mBoardPosition == square);
            piece.ClearCells();
            if (attacks)
                return true;
        }
        return false;
    }

    // List all pieces of byColor that attack the given square
    public List<BasePiece> GetAttackersOnSquare(Vector2Int square, Color byColor, BasePiece ignorePiece = null)
    {
        var attackers = new List<BasePiece>();
        foreach (var piece in GetPieces(byColor))
        {
            if (ignorePiece != null && ReferenceEquals(piece, ignorePiece))
                continue;
            if (!piece.gameObject.activeSelf || piece.CurrentCell == null)
                continue;

            if (piece is Pawn)
            {
                int forward = (piece.mColor == Color.white) ? 1 : -1;
                int x = piece.CurrentCell.mBoardPosition.x;
                int y = piece.CurrentCell.mBoardPosition.y;
                Vector2Int a1 = new Vector2Int(x - 1, y + forward);
                Vector2Int a2 = new Vector2Int(x + 1, y + forward);
                if (a1 == square || a2 == square)
                {
                    var state = piece.CurrentCell.mBoard.ValidateCell(square.x, square.y, piece);
                    if (state != CellState.Friendly && state != CellState.OutOfBounds)
                        attackers.Add(piece);
                }
                continue;
            }

            piece.GeneratePathing();
            bool attacks = piece.GetHighlightedCells().Exists(c => c.mBoardPosition == square);
            piece.ClearCells();
            if (attacks)
                attackers.Add(piece);
        }
        return attackers;
    }

    public bool IsInCheck(Color color, BasePiece ignorePiece = null)
    {
        Vector2Int kingPos = GetKingPosition(color);
        if (kingPos.x < 0) return false; // no king found
        Color opponent = (color == Color.white) ? Color.black : Color.white;
        return IsSquareAttacked(kingPos, opponent, ignorePiece);
    }

    // Return list of enemy pieces currently checking the given color's king
    public List<BasePiece> GetCheckingPieces(Color kingColor)
    {
        Vector2Int kingPos = GetKingPosition(kingColor);
        if (kingPos.x < 0) return new List<BasePiece>();
        Color opponent = (kingColor == Color.white) ? Color.black : Color.white;
        return GetAttackersOnSquare(kingPos, opponent);
    }

    public bool IsLegalMove(BasePiece piece, Cell target)
    {
        // Simulate move in-place (no transforms)
        Cell from = piece.CurrentCell;
        BasePiece captured = target.mCurrentPiece;

        // Apply
        from.mCurrentPiece = null;
        target.mCurrentPiece = piece;
        piece.SetCurrentCell(target);

        // If this is a capture, ignore the captured piece during check detection.
        // Otherwise the captured piece would still be considered an attacker during simulation.
        bool leavesInCheck = IsInCheck(piece.mColor, captured);

        // Revert
        piece.SetCurrentCell(from);
        target.mCurrentPiece = captured;
        from.mCurrentPiece = piece;

        return !leavesInCheck;
    }

    public List<(BasePiece piece, Cell target)> EnumerateLegalMoves(Color forColor)
    {
        var legal = new List<(BasePiece, Cell)>();
        foreach (var piece in GetPieces(forColor))
        {
            if (!piece.gameObject.activeSelf || piece.CurrentCell == null)
                continue;

            piece.GeneratePathing();
            foreach (var cell in piece.GetHighlightedCells())
            {
                if (IsLegalMove(piece, cell))
                    legal.Add((piece, cell));
            }
            piece.ClearCells();
        }
        return legal;
    }

    // Helper: squares strictly between two positions for sliders
    private static IEnumerable<Vector2Int> SquaresBetween(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;
        int sx = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int sy = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // straight or diagonal only
        if (!(dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy)))
            yield break;

        int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        for (int i = 1; i < steps; i++)
            yield return new Vector2Int(from.x + i * sx, from.y + i * sy);
    }

    // Analysis result for check responses
    public class CheckAnalysis
    {
        public List<BasePiece> attackers = new List<BasePiece>();
        public bool isDoubleCheck = false;
        public List<(BasePiece piece, Cell target)> kingMoves = new List<(BasePiece, Cell)>();
        public List<(BasePiece piece, Cell target)> captureMoves = new List<(BasePiece, Cell)>();
        public List<(BasePiece piece, Cell target)> blockMoves = new List<(BasePiece, Cell)>();
    }

    // Compute legal responses when the given color is in check
    public CheckAnalysis AnalyzeCheckResponses(Color color)
    {
        var result = new CheckAnalysis();

        // Detect attackers
        result.attackers = GetCheckingPieces(color);
        result.isDoubleCheck = result.attackers.Count >= 2;

        // If not in check, nothing to analyze
        if (result.attackers.Count == 0)
            return result;

        // Enumerate all legal moves for the side
        var legal = EnumerateLegalMoves(color);

        // King safe moves (includes captures that land on safe squares)
        foreach (var mv in legal)
        {
            if (mv.piece is King)
                result.kingMoves.Add(mv);
        }

        // If double check: only king moves are legal responses
        if (result.isDoubleCheck)
            return result;

        // Single check: classify captures and blocks
        var attacker = result.attackers[0];
        var attackerPos = attacker.CurrentCell.mBoardPosition;
        var kingPos = GetKingPosition(color);

        // Precompute block squares for sliding attackers
        var blockSquares = new HashSet<Vector2Int>();
        if (attacker is Rook || attacker is Bishop || attacker is Queen)
        {
            foreach (var sq in SquaresBetween(attackerPos, kingPos))
                blockSquares.Add(sq);
        }

        foreach (var mv in legal)
        {
            if (mv.piece is King) continue; // already categorized

            // Capture attacker
            if (mv.target.mCurrentPiece == attacker)
            {
                result.captureMoves.Add(mv);
                continue;
            }

            // Blocker move (only for sliders)
            if (blockSquares.Count > 0 && blockSquares.Contains(mv.target.mBoardPosition))
            {
                result.blockMoves.Add(mv);
            }
                mGameOver = false;
        }

        return result;
    }

    public enum EndState { None, Check, Checkmate, Stalemate }

    public EndState EvaluateEndState(Color toMoveColor)
    {
        bool inCheck = IsInCheck(toMoveColor);
        var legal = EnumerateLegalMoves(toMoveColor);

        if (inCheck && legal.Count == 0) return EndState.Checkmate;
        if (!inCheck && legal.Count == 0) return EndState.Stalemate;
        if (inCheck) return EndState.Check;
        return EndState.None;
    }
}
