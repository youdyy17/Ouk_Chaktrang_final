using UnityEngine;
using UnityEngine.UI;

public class King : BasePiece
{

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        // Base setup
        base.Setup(newTeamColor, newSpriteColor, newPieceManager);

        // King stuff
        mMovement = new Vector3Int(1, 1, 1);
        Sprite[] sprites = Resources.LoadAll<Sprite>("W");

        foreach (Sprite s in sprites)
        {
            if (s.name == "White_King")
            {
                GetComponent<Image>().sprite = s;
                break;
            }
        }
    }

    public override void Kill()
    {
        base.Kill();

        mPieceManager.mIsKingAlive = false;
    }

    protected override void CheckPathing()
    {
        // Normal pathing
        base.CheckPathing();

        // Debug: show cell states for one-row forward (by color) and diagonals
        {
            int cx = mCurrentCell.mBoardPosition.x;
            int cy = mCurrentCell.mBoardPosition.y;
            int forward = (mColor == Color.white) ? 1 : -1;

            // One-row forward and diagonals along forward direction
            (int x, int y)[] normals = new (int, int)[] { (cx, cy + forward), (cx - 1, cy + forward), (cx + 1, cy + forward) };
            foreach (var (tx, ty) in normals)
            {
                CellState st = mCurrentCell.mBoard.ValidateCell(tx, ty, this);
            }
        }


        // Special first move addition: knight-like forward jump (x±2, y+forward)
        // Example (white): from (4,1) add (2,2) and (6,2) on first move
        // Example (black): from (4,6) add (2,5) and (6,5) on first move
        // Disabled when it's this side's turn and the king is in check
        if (mIsFirstMove)
        {
            bool isThisSideTurn = (mPieceManager.mColorToMove == mColor);
            if (isThisSideTurn && mPieceManager.mColorToMoveInCheck)
                return; // block special move while in check

            int currentX = mCurrentCell.mBoardPosition.x;
            int currentY = mCurrentCell.mBoardPosition.y;
            int forward = (mColor == Color.white) ? 1 : -1;

            (int x, int y)[] specials = new (int, int)[] { (currentX - 2, currentY + forward), (currentX + 2, currentY + forward) };
            foreach (var (tx, ty) in specials)
            {
                CellState cellState = mCurrentCell.mBoard.ValidateCell(tx, ty, this);
                if (cellState == CellState.Free)
                {
                    mHighlightedCells.Add(mCurrentCell.mBoard.mAllCells[tx, ty]);
                }
            }
        }
    }
}